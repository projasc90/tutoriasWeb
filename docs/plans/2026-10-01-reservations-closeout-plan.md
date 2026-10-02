# Plan — Cierre de deuda de reservas

**Fecha:** 2026-10-01
**Contexto:** [context](./2026-10-01-reservations-closeout-context.md)

## Tareas atómicas (por fase)

| # | Fase | Tarea | Archivos | Estado |
|---|------|-------|----------|--------|
| 0.1 | 0 | Artefactos context + plan | `docs/plans/2026-10-01-reservations-closeout-{context,plan}.md` | ✅ |
| 0.2 | 0 | Tests TDD (rojos): cancel en PendingPayment no credita; receptor configurable; SlotAvailabilityService | `api/tests/AuraLearn.Tests/{ReservationServiceTests,SlotAvailabilityServiceTests}.cs` | ✅ |
| 0.3 | 0 | Quitar índice duplicado de slots | `api/AuraLearn.Infrastructure/Persistence/AuraLearnDbContext.cs` | ✅ |
| 0.4 | 0 | Application: `SlotAvailabilityService` + `SinpeConfig` en `ReservationService` + fix crédito + limpiar `IUserRepository` muerto | `Application/Services/SlotAvailabilityService.cs`, `ReservationService.cs` | ✅ |
| 0.5 | 0 | Infrastructure: `EnsureAheadInRangeAsync` reemplaza `AddRangeIgnoringDuplicatesAsync` (pre-query + batch + carrera benigna) | `Application/Interfaces/ISlotRepository.cs`, `Infrastructure/Persistence/SlotRepository.cs` | ✅ |
| 0.6 | 0 | Wiring ensure-ahead + `GET /api/tutors/{id}` + DI limpia en AdminPayments | `Api/Controllers/{SlotsController,TutorsController,AdminPaymentsController}.cs`, `Program.cs` | ✅ |
| 0.7 | 0 | Regenerar migración con tooling + ROLLBACK MANUAL | `api/AuraLearn.Infrastructure/Migrations/*AddSlotsAndReservations*`, `AuraLearnDbContextModelSnapshot.cs` | ✅ |
| 0.8 | 0 | Gate backend: `dotnet build && dotnet test` | — | ✅ 66/66 |
| 0.9 | 0 | `dotnet ef database update` + verificación psql (tablas + índice parcial + seed de slots) | — | ✅ |
| 0.10 | 0 | Frontend: `fetchTutor`, `useTutor` consume el endpoint, copy de cancelación en `/mis-tutorias` | `web/lib/api.ts`, `web/hooks/use-tutor.ts`, `web/app/mis-tutorias/page.tsx` | ✅ |
| 0.11 | 0 | Gate frontend: `npm run lint && npx tsc --noEmit` | — | ✅ |
| 1 | 1 | Smoke E2E manual flujo completo + negativos (reserva → comprobante → admin → cancelar ≥12 h → monedero) | — | ✅ API (15 escenarios) |
| 2.1 | 2 | TDD débito atómico: `TryDebitAsync` + `WalletEntryReason.WalletPayment` + `CreateReservationRequest.UseWalletBalance` + `Confirmed` directo | `WalletRepository.cs`, `IWalletRepository.cs`, `ReservationService.cs`, `ReservationDto.cs`, `WalletEntryReason.cs` | ✅ 71/71 |
| 2.2 | 2 | Frontend toggle "Pagar con monedero" | `web/lib/api.ts`, `web/app/tutores/[id]/page.tsx` | ✅ |
| 2.3 | 2 | ADR-006 (pago con saldo = sin admin, débito atómico) | `.ai/ADR_LOG.md` | ✅ |
| 3.1 | 3 | TDD `CompleteAsync` + `WalletEntryReason.TutorPayout` + endpoint `PATCH {id}/complete` (tutor dueño, `EndAt <= now`) + `GetByTutorUserAsync` | `ReservationService.cs`, `ReservationsController.cs`, `ReservationRepository.cs`, `WalletEntryReason.cs` | ✅ 75/75 |
| 3.2 | 3 | Frontend `/tutor/sesiones` (guard Tutor, completar sesión, liquidación visible) | `web/lib/api.ts`, `web/hooks/use-tutor-sessions.ts`, `web/app/tutor/sesiones/page.tsx` | ✅ |
| 4.1 | 4 | Entidad `TutorAvailability` + config EF + migración `AddTutorAvailability` (rollback manual) | `Domain/Entities/TutorAvailability.cs`, `AuraLearnDbContext.cs`, `Migrations/*` | ⏳ |
| 4.2 | 4 | Honoring: overload de `SlotGenerationService` con reglas + tests | `SlotGenerationService.cs`, tests | ⏳ |
| 4.3 | 4 | Endpoints `GET/PUT /api/tutors/me/availability` (rol Tutor dueño) | `AvailabilityController.cs`, puertos + impl | ⏳ |
| 4.4 | 4 | Frontend `/tutor/disponibilidad` (editor por día) | `web/app/tutor/disponibilidad/page.tsx`, `web/hooks/use-availability.ts` | ⏳ |
| 5 | 5 | Reprogramación UX (cancelar + crear) + actualizar skill `slots-reservations` + GLOSSARY | `web/app/mis-tutorias/page.tsx`, `.github/skills/slots-reservations/SKILL.md`, `.ai/GLOSSARY.md` | ⏳ |
| 6 | 6 | Cierre: gates completos, `verification` + `summary`, `.ai/` (bump 0.6.0) | `docs/plans/*`, `.ai/*`, `web/package.json` | ⏳ |

## Contrato de API (nuevo en este plan)

- `GET /api/tutors/{id}` → 200 TutorDto · 404 *(Fase 0)*
- `POST /api/reservations` `{slotId, idempotencyKey, useWalletBalance?}` → 201 Confirmed (pago con saldo) · 201 PendingPayment (flujo SINPE) · 409 · 422 *(Fase 2)*
- `PATCH /api/reservations/{id}/complete` [Tutor dueño] → 204 · 404 · 403 · 409 (no Confirmed) · 422 (sesión no terminada) *(Fase 3)*
- `GET /api/tutors/me/availability` [Tutor] → 200 reglas · `PUT` reemplazo completo → 204 · 422 (validación) *(Fase 4)*

## Criterios de aceptación

**Fase 0**
1. `dotnet ef migrations list` muestra la migración regenerada con `Designer.cs`; el snapshot incluye slots/reservations/wallet_entries.
2. El `Up()` generado crea `ux_reservations_active_slot` parcial (`status IN (1,2)`) y NO crea `ix_slots_tutor_start` redundante.
3. `dotnet test` verde (existentes + nuevos: cancel PendingPayment sin crédito, receptor de config, EnsureAhead).
4. Tras `database update`, `GET /api/tutors/{id}/slots` devuelve slots reales (ensure-ahead generó y persistió).
5. `GET /api/tutors/{id}` responde 200/404.

**Fase 1** — checklist smoke documentado en `verification.md` con evidencia psql (estados y movimientos de wallet correctos).

**Fases 2–5** — tests TDD verdes por invariante + smoke manual; liquidación visible en monedero del tutor; slots generados respetan franjas definidas.

## Fuera de alcance (explícito)

Firma digital SINPE real (ADR-004 sigue) · pago parcial con monedero · refresh tokens/rate limiting/cookies httpOnly · notificaciones email · upload atestados · Playwright E2E · Docker/compose · portal público.
