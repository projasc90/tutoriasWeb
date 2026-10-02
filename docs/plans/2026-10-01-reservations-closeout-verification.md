# Verification — Fase 0 del cierre de deuda de reservas

**Fecha:** 2026-10-01
**Plan:** [plan](./2026-10-01-reservations-closeout-plan.md)

## Gates ejecutados

| Comando | Resultado |
|---------|-----------|
| `cd api && dotnet build` | ✅ 0 errores |
| `cd api && dotnet test` | ✅ **75/75 verdes** (60 previos + 6 Fase 0 + 3 expiración perezosa + 2 pago con monedero + 4 cierre de sesión) |
| `cd web && npm run lint` | ✅ 0 errores, 3 warnings pre-existentes (archivos no tocados) |
| `cd web && npx tsc --noEmit` | ✅ 0 errores |
| `dotnet ef database update` | ✅ `20261001215535_AddSlotsAndReservations` aplicada |

## Cambios por tarea

| Tarea | Evidencia |
|-------|-----------|
| 0.2 Tests TDD | `ReservationServiceTests`: `Cancel_en_PendingPayment_no_credita_wallet`, `CreateReservation_devuelve_el_receptor_configurado`; `SlotAvailabilityServiceTests` (4: vacío, pasado, recorte, retorno) |
| 0.3 Índice duplicado | `AuraLearnDbContext`: solo `ux_slots_tutor_start` (unique) |
| 0.4 Application | `SlotAvailabilityService` nuevo; `ReservationService` con `SinpeConfig` inyectada (quita 2 hardcodes) + fix de crédito + sin `IUserRepository` muerto |
| 0.5 Infrastructure | `SlotRepository.EnsureAheadInRangeAsync`: pre-consulta + insert en lote + reintento individual tolerante a carrera (elimina el fake `IsUniqueConstraintViolation`) |
| 0.6 Wiring | `SlotsController` ensure-ahead; `TutorsController.GetById` nuevo; `AdminPaymentsController` con DI por ctor (sin service-locator); `Program.cs` registra `SlotAvailabilityService` |
| 0.7 Migración | Regenerada por tooling: `20261001215535_*.cs` + `.Designer.cs` + snapshot sincronizado; bloque ROLLBACK MANUAL añadido a mano al final |
| 0.9 BD | psql: tablas `slots`/`reservations`/`wallet_entries` creadas; `ux_reservations_active_slot` = `CREATE UNIQUE INDEX ... WHERE (status = ANY (ARRAY[1, 2]))` ✅; 13 tutores |
| 0.10 Frontend | `fetchTutor` en `lib/api.ts`; `useTutor` consume `GET /api/tutors/{id}`; copy de cancelación en `/mis-tutorias` distingue PendingPayment |

## Evidencia funcional end-to-end (API real + psql)

`dotnet run` + curl + psql contra `auralearn_dev`:

| Verificación | Resultado |
|--------------|-----------|
| `GET /health` | ✅ Healthy |
| `GET /api/tutors/{id}/slots` (1ª llamada) | ✅ 143 slots generados y persistidos (ensure-ahead, 14 días de horizonte) |
| `GET /api/tutors/{id}/slots` (2ª llamada) | ✅ 143 slots — idempotente, no duplicó |
| `SELECT count(*) FROM slots` (1 tutor) | ✅ 143 |
| `GET /api/tutors?pageSize=50` | ✅ 12 tutores, todos con `nextSlotAt` (ensure-ahead batch por página) |
| `SELECT count(*) FROM slots` tras catálogo | ✅ 1716 (12 × 143) |
| `GET /api/tutors/{id}` | ✅ 200 `{name, nextSlotAt: 2026-10-02T14:00:00Z}` |
| `GET /api/tutors/{id inexistente}` | ✅ 404 |
| Slots iniciales | ✅ `2026-10-02T14:00` (08:00 CR del día siguiente — zona CR en generación, UTC en BD) |

## Fase 1 — Smoke E2E a nivel API (`dotnet run` + curl + psql)

Flujo feliz y negativos, contra `auralearn_dev` (usuario Admin creado vía register + `UPDATE users SET role=3`, solo dev):

| # | Escenario | Resultado |
|---|-----------|-----------|
| 1 | `POST /api/auth/register` estudiante | ✅ 201, rol Estudiante |
| 2 | `POST /api/reservations` slot ≥12 h | ✅ 201 `PendingPayment`, PaymentInfo con receptor `88888888` (de config) y expiración +60 min |
| 3 | Replay con la **misma** idempotencyKey | ✅ 200 (misma reserva) |
| 4 | Otro estudiante reserva el mismo slot | ✅ 409 `SLOT_ALREADY_RESERVED` (guardián: índice parcial de BD) |
| 5 | Comprobante con monto incorrecto (9999) | ✅ 422 `VALIDATION_FAILED` |
| 6 | Comprobante válido | ✅ 202, `PendingPayment` con nº confirmación |
| 7 | Re-submit del mismo nº a la misma reserva | ✅ 202 (idempotente benigno) |
| 8 | `GET /api/admin/payments` (rol Admin) | ✅ 200 con 1 pendiente (nombre estudiante/tutor, conf, precio) |
| 9 | `PATCH /api/admin/payments/{id}` approve | ✅ 204 → reserva `Confirmed` (2) |
| 10 | `PATCH …/cancel` Confirmed con ≥12 h | ✅ 200 → `Cancelled` (5) |
| 11 | `GET /api/wallet` tras cancelar | ✅ saldo **14500** (crédito completo) |
| 12 | Re-cancelar la misma reserva | ✅ 409 `CANNOT_CANCEL_IN_TERMINAL_STATE` |
| 13 | Cancelar una reserva en `PendingPayment` (nunca pagó) | ✅ 200 y saldo **0** — fix de Fase 0 verificado en BD real |
| 14 | Expiración perezosa: `expires_at` forzado al pasado (SQL dev) y leer la reserva | ✅ pasa a `Expired` (4) **persistido en BD** |
| 15 | Reservar el slot liberado por la expiración | ✅ 201 (índice parcial deja de bloquear) |

**Bug crítico encontrado y corregido durante el smoke:** la expiración perezosa (ADR-005: "al leer/crear") **no estaba implementada** — la reserva vencida quedaba en `PendingPayment` bloqueando el slot para siempre vía el índice parcial. Corrección con TDD (3 tests rojos primero): helper `TryExpireIfDueAsync` en `ReservationService` aplicado en `CreateAsync` (libera y permite nueva reserva), `GetByIdAsync`, `SubmitComprobanteAsync` y `GetByStudentAsync` (barrido al listar). Regla: **NO expira** una `PendingPayment` con comprobante en cola (el dinero ya se transfirió; decide el Admin). Suite final: **69/69 verdes**. Verificación empírica: pasos 14–15.

## Fase 2 — Pago con saldo del monedero (solo pago total)

Contra API real (crédito de ₡20000 al estudiante vía SQL dev):

| # | Escenario | Resultado |
|---|-----------|-----------|
| 1 | `POST /api/reservations` con `useWalletBalance: true` y saldo 20000 | ✅ 201 `Confirmed` (2), **sin** `paymentInfo`, sin cola admin |
| 2 | `GET /api/wallet` después | ✅ saldo **5500** (débito completo −14500) |
| 3 | Saldo insuficiente | ✅ fallback automático al flujo SINPE (201 `PendingPayment` con PaymentInfo) |

Hallazgo durante el smoke: el primer intento dio **FK 23503** — la reserva debe persistirse ANTES del débito (el `WalletEntry` lleva FK a la reserva). Corregido y testado (tests 71/71).

ADR-006 registrado en `.ai/ADR_LOG.md`. Frontend: checkbox "Pagar con monedero (₡X)" en `/tutores/[id]` (deshabilitado si saldo < precio) y navegación directa a `/mis-tutorias` cuando la reserva nace confirmada.

## Fase 3 — Estado `Completed` con liquidación (tutor manual)

Contra API real (usuario promedio a rol Tutor + vínculo con Dr. Carlos Solano vía SQL dev):

| # | Escenario | Resultado |
|---|-----------|-----------|
| 1 | `GET /api/reservations/tutor-sessions` (rol Tutor) | ✅ 200 — 6 sesiones del tutor con nombre del estudiante |
| 2 | `PATCH /api/reservations/{id}/complete` con slot aún futuro | ✅ 422 `SESSION_NOT_ENDED` (protegido; probado con slot movido al pasado por SQL dev para el flujo feliz) |
| 3 | `PATCH …/complete` de la sesión Confirmed terminada | ✅ 204 → `Completed` (6) |
| 4 | `GET /api/wallet` del tutor después | ✅ saldo **14500** (reason `TutorPayout`, retención hasta cerrar) |

**Bug de contrato corregido:** `ReservationDto.TutorId` venía rellenado con `StudentId` (mapeo erróneo en `ToDto`, codificado en un test). Corregido a `r.Slot.TutorId` + nuevo campo `StudentName`; mirror frontend actualizado. Suite: **75/75 verdes**.

Frontend: página nueva `/tutor/sesiones` (guard rol Tutor; botón "Completar sesión" en las confirmadas ya terminadas; banner "Liquidado" en completadas) + hook `use-tutor-sessions`.

Nota: verificación pendiente a nivel navegador (UI): el flujo /tutores/[id] → /checkout/[id] → /admin/pagos → /mis-tutorias con polling. El equivalente API está cubierto; smoke de UI a ejecutar por el usuario o con el servidor de dev.

## Rework UI — Pantallas Stitch 02 (perfil) y 03 (checkout) replicadas

**Solicitud del usuario:** la sección de pago debe igualar la pantalla Stitch 03 ("Checkout y Confirmación") y el perfil + motor de reserva debe igualar la Stitch 02 ("Perfil del Tutor y Motor de Reserva"), conservando la tipografía/tokens establecidos desde el inicio (Plus Jakarta Sans + MD3; sin fuentes nuevas).

Componentes co-localizados creados:

- `web/components/checkout/checkout-stepper.tsx` — stepper 3 pasos + "Sesión Segura · SSL".
- `web/components/checkout/timer-banner.tsx` — banner ámbar "Slot reservado temporalmente" con cuenta regresiva.
- `web/components/checkout/booking-details.tsx` — "Detalles de la Clase": materia, horario CR, modalidad Meet+Pizarra, nota pedagógica (300 chars).
- `web/components/checkout/payment-method-card.tsx` — "Método de Pago": tabs SINPE (activo: receptor copiable, monto exacto, emisor +506, nº referencia) / Tarjeta / IBAN (próximamente, sin pasarela integrada).
- `web/components/checkout/order-summary.tsx` — sticky: tutor snapshot (verificado + rating), resumen de orden, total, CTA "Confirmar y Pagar ₡X", políticas de cancelación **12 h** (el mock decía 6 h; corregido a la regla real del glosario) y garantía.
- `web/components/checkout/terminal-views.tsx` — estados confirmado/rechazado/expirado/cancelado (paso 3 del stepper).
- `web/components/tutor/tutor-profile-header.tsx` — header del perfil: avatar + "En Línea", chips verificación, KPIs (rating, asistencia, respuesta, horas), feature pills.
- `web/components/tutor/subject-cards.tsx` — "Cursos y Materias Especializadas": cards seleccionables (código + precio /h) que sincronizan la reserva.
- `web/components/tutor/booking-engine.tsx` — motor sticky: 1. Modalidad (pack próximamente) · 2. Tira de días (orden real por fecha, key determinista `en-CA` en zona CR) · 3. Horarios agrupados Mañana/Tarde/Noche con contador "N libres" · cita seleccionada · subtotal · toggle monedero · CTA · garantía · métodos aceptados.

Evidencia (navegador real, sesión Smoke Estudiante):

| Verificación | Resultado |
|--------------|-----------|
| Perfil: header Stitch (En Línea, KPIs, pills) + pedagogía + materias + credenciales + reseñas | ✅ |
| Motor: días ordenados cronológicamente (VIE 02 → DOM 04 → LUN 05 → MAR 06 → MIÉ 07 → JUE 08) | ✅ |
| Click MAR 06 → "4 libres" → TARDE 14/15/16/17 → cita "martes, 06 de octubre, 14:00" | ✅ |
| CTA "Continuar al Pago / Checkout" → navegó a `/checkout/{id}` | ✅ |
| Checkout: stepper + timer 59:00 + detalles clase + método pago (receptor copiable ₡14,500) + resumen sticky | ✅ |
| Tipografía: Plus Jakarta Sans + tokens MD3 en toda la pantalla (sin fuentes nuevas) | ✅ |

Problemas resueltos en el rework: (1) keys de día inconsistentes entre grupo y selección → unificadas a `YYYY-MM-DD` (`en-CA` determinista, zona CR); (2) días sin orden → sort por la key; (3) estado de día dependiente del slot → `selectedDayKey` derivado con fallback al primer día; (4) `MaterialIcon` no soporta `title` → `aria-label`; (5) animación `animate-ping` impedía clicks estables en Playwright (solo afecta el test harness, no al usuario).

## Rework UI 2 — Panel del Profesor (Stitch 05) + tipografía Roboto global

**Solicitud del usuario:** usar **Roboto** como tipografía del producto y replicar la pantalla "Panel del Profesor – Gestor de Disponibilidad" (screen `640dfa8a4a58467f894d145a4bdab2b4`, descargado a `design/stitch/screens/05-panel-disponibilidad.html/png`).

### Tipografía Roboto global
- `web/app/layout.tsx`: `Roboto` de `next/font/google` (400/500/700/900) con variable `--font-roboto`.
- `web/app/globals.css`: los 2 tokens `--font-sans` (typography + :root) ahora resuelven a `var(--font-roboto, "Roboto")`. Se corrigió una referencia circular pre-existente en la línea 91 (`var(--font-sans)` dentro de su propia definición). Plus Jakarta Sans fuera.

### Gestor de Disponibilidad (replica Stitch 05)
- `web/components/tutor/availability/availability-chrome.tsx` — header (badge "Panel Académico Docente", display-lg, ribbon "Capacidad Semanal"/"Ocupación Actual"), toolbar (zona horaria GMT-6, "Sincronizar ahora"), tabs (Semanal Recurrente activo / Excepciones).
- `web/components/tutor/availability/day-column.tsx` — columna de día: "N cupos libres", badge Activo/Cerrado, franjas con hora CR editable inline + "cupos abiertos" + "Recurrente", acciones "Franja horaria" / "Copiar a otros días"; día sin franjas = empty state "Día de descanso" + "Habilitar día".
- `web/components/tutor/availability/save-action-bar.tsx` — barra sticky inferior: estado (sincronizada / cambios sin guardar / guardando / éxito), "Descartar Cambios" + "Guardar Cambios de Disponibilidad".
- Página `web/app/tutor/disponibilidad/page.tsx` reescrita: dirty-tracking contra baseline, descarte, copia de franjas a todos los días, auto-offset al añadir (evita duplicados exactos que el backend rechaza), reglas informativas (12 h / 60 min / 2 semanas) y panel de calendarios externos (próximamente).

Evidencia (navegador real, tutor del smoke):

| Verificación | Resultado |
|--------------|-----------|
| Header + métricas (capacidad 8 h al definir 2 franjas) + toolbar + tabs | ✅ |
| "Habilitar día" Lunes → franja 08:00-12:00 activa con "4 cupos abiertos" | ✅ |
| "Franja horaria" → segunda franja auto-offset 12:00-16:00 (sin solape) | ✅ |
| "Guardar Cambios" → 204 y banner "¡Cambios Guardados Exitosamente!" | ✅ |
| psql: 2 reglas persistidas (`weekday=1, 08:00/12:00` + `12:00/16:00`) | ✅ |
| Roboto cargado en toda la app (chips/stepper/panel) | ✅ |

Problema resuelto: duplicado exacto de franja → 422 `AVAILABILITY_OVERLAP` (nuevo error específico del backend, mensaje en español en el cliente HTTP); la UI ahora auto-offseteada la nueva franja después de la última.

## Problemas resueltos durante la ejecución

1. **Ciclo de regeneración de la migración:** `dotnet ef migrations remove` reportaba `Done` pero no eliminaba los archivos (quedaban `.cs` huérfanos y el snapshot sin revertir en el primer intento). Solución: `remove --force` + `rm` manual de residuos + `git checkout` del snapshot para partir de un estado limpio, y luego un único `migrations add`.
2. **Índice borrado de más:** en la primera edición del `DbContext` se eliminaron ambos índices de slots (incluido el único `ux_slots_tutor_start`, guardián contra doble franja). Detectado al auditar la migración generada; restaurado y regenerado.
3. **Falso-positivo `has-pending-model-changes`:** tras generar la migración, EF seguía reportando cambios pendientes de solo-datos (`UpdateData` del seed `subjects`, 24 ops UP+DOWN). Es el mismo falso-positivo de EF 10 + `HasData` documentado en `.github/errors/backend.md` (suprimido vía `PendingModelChangesWarning`); el esquema está completo.
4. **`migrations script` no refleja el archivo final:** el `remove` posterior no aplicado dejaba el tooling apuntando a archivos borrados; solventado regenerando y aplicando la migración en el mismo ciclo.

## Riesgos residuales

- Sin test de integración de concurrencia contra Postgres real (Testcontainers no configurado) — el reintento individual del `EnsureAheadInRangeAsync` tolera la carrera, pero el guardián real sigue siendo el índice único.
- `AddRangeIgnoringDuplicatesAsync` eliminado de `ISlotRepository`: breaking interno (solo lo usaba `SlotAvailabilityService`).
- Expiración de `PendingPayment` sigue perezosa (sin worker) — por diseño (ADR-005).
