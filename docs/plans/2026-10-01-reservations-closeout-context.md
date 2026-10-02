# Contexto — Cierre de deuda de reservas (migración, smoke E2E, monedero, Completed, disponibilidad)

**Fecha:** 2026-10-01
**Plan:** [plan](./2026-10-01-reservations-closeout-plan.md)
**Estado previo:** v0.5.0 (plan `2026-09-29-slots-reservations` cerrado)

## Alcance

Ejecutar la deuda explícita del plan anterior en fases:

- **Fase 0 — Deuda técnica que BLOQUEA el smoke E2E** (descubierta en exploración):
  1. `SlotGenerationService.GenerateSlots` y `AddRangeIgnoringDuplicatesAsync` **no se invocan en código productivo** → tras migrar, la tabla `slots` queda vacía y `GET /tutors/{id}/slots` devuelve `[]`. El ensure-ahead de ADR-005 no está cableado.
  2. La migración `AddSlotsAndReservations` fue escrita a mano: sin `Designer.cs` y el snapshot NO contiene slots/reservations/wallet_entries → la próxima migración con tooling generaría un diff destructivo. Regenerar con `dotnet ef` antes de aplicar.
  3. Índice duplicado en `AuraLearnDbContext` (`ux_slots_tutor_start` + `ix_slots_tutor_start` sobre las mismas columnas): el único ya sirve consultas por prefijo → eliminar el redundante.
  4. `SinpeConfig` registrada en DI pero `ReservationService` hardcodea `"88888888"` (2 sitios).
  5. **Bug de negocio:** `CancelAsync` acredita monedero también en `PendingPayment` (el estudiante nunca pagó). Solo `Confirmed` + ≥12 h debe acreditar.
  6. `IsUniqueConstraintViolation` en `SlotRepository` devuelve `true` incondicionalmente (mascara cualquier `DbUpdateException`) + insert uno-por-uno (O(n) round-trips).
  7. `GET /api/tutors/{id}` no existe: `useTutor` trae el catálogo con `pageSize:50` y busca en memoria. `TutorService.GetByIdAsync` ya existe.
  8. `AdminPaymentsController` usa service-locator (`HttpContext.RequestServices.GetService`) en vez de DI por ctor.

- **Fase 1 — Smoke E2E manual** del flujo completo (registro → reserva → comprobante → admin aprueba → cancelar ≥12 h → monedero) + negativos.
- **Fase 2 — Pagar con saldo del monedero (solo pago total):** saldo ≥ precio → `Confirmed` directo con débito negativo, sin comprobante ni cola admin; saldo insuficiente → fallback a flujo SINPE normal. TDD.
- **Fase 3 — Estado `Completed` con liquidación (tutor manual):** `PATCH /api/reservations/{id}/complete` por el tutor dueño + `WalletEntry` a favor del tutor. TDD. El enum `Completed` y `ReservationStateMachine.CanComplete(Confirmed)` ya existen.
- **Fase 4 — Disponibilidad semanal del tutor (reglas + honoring):** entidad `TutorAvailability` (franjas por día), migración con rollback manual, endpoints `GET/PUT /api/tutors/me/availability`, `SlotGenerationService` respeta reglas (sin reglas → default 08:00–20:00 CR). TDD.
- **Fase 5 — Reprogramación = cancelar + crear (UX):** sin endpoint nuevo; botón "Reprogramar" en `/mis-tutorias` → modal → cancelar → navegar al perfil del tutor.

## Decisiones (alineadas con el usuario, 2026-09-30)

| # | Decisión | Elección |
|---|----------|----------|
| 1 | Assets Stitch | Ya descargados en `design/stitch/screens/` (4 pares HTML+PNG); se consideran válidos. No re-ejecutar el script. |
| 2 | Fase 0 | Completa: regenerar migración + limpiar índice + ensure-ahead + SinpeConfig real + `GET /tutors/{id}`. |
| 3 | Monedero al pagar | **Solo pago total** (sin pago parcial). Débito atómico con lock; ADR-006. |
| 4 | Completed | **Tutor manual** (no lazy). Liquidación = crédito al ledger del tutor al completar. |
| 5 | Disponibilidad | **Reglas semanales + honoring** (no simple on/off). |
| 6 | Reprogramación | **Cancelar + crear nueva** (se descarta PATCH reschedule). Actualizar skill y glosario. |

## Restricciones

- TDD obligatorio en pagos/slots/reservas: test primero, cada invariante = test.
- Migraciones con tooling EF + bloque `// ROLLBACK MANUAL` comentado en el mismo archivo.
- UTC en BD; zona `America/Costa_Rica` solo en presentación (ADR-005 vigente).
- El frontend nunca valida reglas de negocio.
- Sin workers (la expiración sigue perezosa, ADR-005).
- No loguear PII financiera.

## Referencias

- Skills: `slots-reservations`, `sinpe-payments`, `database-migrations`, `backend-testing`
- ADRs vigentes: 004 (confirmación manual SINPE), 005 (UTC + ensure-ahead)
- Estado previo: `.ai/CURRENT_STATE.md` (v0.5.0)
