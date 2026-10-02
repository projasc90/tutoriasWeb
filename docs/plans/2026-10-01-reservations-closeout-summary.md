# Summary — Cierre de deuda de reservas

**Fecha:** 2026-10-01
**Plan:** [plan](./2026-10-01-reservations-closeout-plan.md)
**Verificación:** [verification](./2026-10-01-reservations-closeout-verification.md)

## Qué cambió

### Fase 0 — Desbloqueo del smoke E2E (deuda técnica)
- Migración `AddSlotsAndReservations` regenerada **con tooling EF** (`20261001215535_*` con Designer y snapshot sincronizado; la versión manual `20260930000000_*` fue eliminada).
- Ensure-ahead (ADR-005) **cableado por primera vez**: `SlotAvailabilityService` (Application) + `SlotRepository.EnsureAheadInRangeAsync` (Infrastructure): pre-consulta de existentes → generación de faltantes → inserción por lotes con reintento tolerante a carrera. Se eliminó el `IsUniqueConstraintViolation` falso (`return true`) y el insert O(n) round-trips.
- Índice no-único redundante de slots eliminado (el único `(tutor_id, start_at)` cubre las consultas por prefijo).
- `SinpeConfig` real en `ReservationService` (2 hardcodes `"88888888"` fuera) + `Sinpe:ReceptorPhone` en `appsettings.Development.json`.
- Fix de negocio: **cancelar una `PendingPayment` ya no acredita monedero** (solo `Confirmed` + ≥12 h).
- `GET /api/tutors/{id}` nuevo (200/404); `fetchTutor` + `useTutor` consumen el endpoint (adiós al truco `pageSize:50`).
- `AdminPaymentsController` con DI por ctor (eliminado el service-locator `HttpContext.RequestServices`).

### Fase 1 — Smoke E2E a nivel API (15 escenarios)
Flujo feliz completo verificado (registro → reserva 201 → replay 200 → doble-booking 409 → comprobante 202/422 → admin 204 → Confirmed → cancelar ≥12 h → monedero 14500 → re-cancelar 409) + expiración perezosa y liberación del slot.

- **Bug crítico arreglado durante el smoke:** la expiración perezosa declarada en ADR-005 **no estaba implementada** (las vencidas bloqueaban el slot para siempre). `TryExpireIfDueAsync` con TDD en `CreateAsync`/`GetByIdAsync`/`SubmitComprobanteAsync`/`GetByStudentAsync`. Regla nueva: una `PendingPayment` **con** comprobante en cola NO expira (el dinero ya se transfirió; decide el Admin).

### Fase 2 — Pagar con saldo del monedero (solo pago total, ADR-006)
- `CreateReservationRequest.UseWalletBalance`; `IWalletRepository.TryDebitAsync` atómico (transacción + `SELECT … FOR UPDATE` sobre `users` + SUM en lock; race-safe); `WalletEntryReason.WalletPayment` (débito negativo).
- Saldo ≥ precio → `Confirmed` directo, sin comprobante ni cola del Admin. Saldo insuficiente → fallback automático a SINPE.
- Orden corregido durante el smoke (FK 23503): la reserva se persiste **antes** del débito.
- Frontend: checkbox "Pagar con monedero (₡X)" en `/tutores/[id]` (deshabilitado si no alcanza); reserva confirmada navega directo a `/mis-tutorias`.

### Fase 3 — Estado `Completed` con liquidación (tutor manual)
- `PATCH /api/reservations/{id}/complete` (rol Tutor **dueño**, solo `Confirmed`, solo con `EndAt <= now` → 422 `SESSION_NOT_ENDED`); `WalletEntryReason.TutorPayout` (+precio al ledger del tutor; retención hasta cerrar).
- `GET /api/reservations/tutor-sessions` (sesiones del tutor autenticado).
- Fix de contrato: `ReservationDto.TutorId` venía con `StudentId` (mapeo erróneo codificado en un test) → corregido; campo nuevo `StudentName`.
- Frontend: página `/tutor/sesiones` (guard rol Tutor; "Completar sesión"; banner "Liquidado") + `use-tutor-sessions`.

### Fase 4 — Disponibilidad semanal del tutor (reglas + honoring)
- Entidad `TutorAvailability` (weekday 0-6, franja local CR `TimeOnly`), migración `AddTutorAvailability` con tooling + ROLLBACK MANUAL, unique `(tutor_id, weekday, start_local)`.
- Honoring: overload `SlotGenerationService.GenerateSlots(…, rules)` (solo franjas definidas, hora a hora; sin reglas → default 08-20 CR con comida excluida).
- **Purga de honorabilidad** (hallazgo del smoke): los slots ya generados fuera de las nuevas franjas se **eliminan** al asegurar-ahead, salvo que tengan reserva activa (sesión comprometida).
- `GET/PUT /api/tutors/me/availability` (rol Tutor dueño; reemplazo completo transaccional; validación de franjas/solapes/granularidad 1 h → 422).
- Frontend: página `/tutor/disponibilidad` (editor por día con checkboxes + `<input type="time">`) + `use-availability`.

### Fase 5 — Reprogramación = cancelar + crear (decisión del usuario)
- Sin endpoint nuevo: botón "Reprogramar" en `/mis-tutorias` (reservas `Confirmed` con ≥12 h) que cancela (acreditando monedero) y navega al perfil del tutor para elegir nuevo horario.
- Skill `slots-reservations` (API del repo vigente + decisión) y GLOSSARY actualizados.

## Qué NO cambió
- ADR-004 vigente: la aprobación de comprobantes sigue manual por Admin (firma digital real pendiente de la API del banco).
- `Mentors` de la landing sigue estático (marketing).
- JWT: sin refresh tokens ni rate limiting (deuda seguridad).
- Notificaciones email, upload de atestados, Playwright E2E, Docker/compose, portal público: sin cambios.

## Evidencia de gates

| Gate | Resultado |
|------|-----------|
| `cd api && dotnet build` | ✅ 0 errores |
| `cd api && dotnet test` | ✅ **79/79** (60 previos + 19 nuevos TDD) |
| `dotnet ef database update` | ✅ 2 migraciones aplicadas (`…AddSlotsAndReservations`, `AddTutorAvailability`) |
| `cd web && npm run lint` | ✅ 0 errores, 3 warnings pre-existentes |
| `cd web && npx tsc --noEmit` | ✅ 0 errores |
| `cd web && npm run build` | ✅ **13 rutas** (2 nuevas: `/tutor/sesiones`, `/tutor/disponibilidad`) |

## Deuda explícita restante

1. **Firma digital SINPE real** (requiere API del banco; ADR-004 manual sigue).
2. **Pago parcial con monedero** (SINPE por el remanente) — excluido por decisión del usuario.
3. **Test de concurrencia contra Postgres real** (Testcontainers no configurado): el débito y el índice parcial son race-safe por diseño, sin test de integración.
4. **Smoke E2E en navegador** (UI con polling): el flujo API está cubierto; la verificación visual del usuario es recomendable antes de desplegar.
5. Refresh tokens + rate limiting + cookies httpOnly (ADR pendiente).
6. Notificaciones email; upload de atestados con URLs firmadas; portal público.

## Próximos pasos sugeridos

1. Smoke E2E en navegador del flujo completo (usuario real: estudiante + admin + tutor).
2. ADR de cookies httpOnly vs localStorage y refresh tokens.
3. Testcontainers para el test de carrera del débito y el doble-booking real.
4. Pizarra digital realtime (skill `realtime-whiteboard` ya tiene la base) para sesiones confirmadas.
