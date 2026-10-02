# Plan — Motor de slots y reservas + pago SINPE

**Fecha:** 2026-09-29
**Contexto:** [context](./2026-09-29-slots-reservations-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 0 | Assets Stitch (script + descarga 4 pantallas) | `design/stitch/download-screens.sh`, `design/stitch/screens/*` | pendiente |
| 1 | Entidades Domain + enums | `api/AuraLearn.Domain/Entities/{Slot,Reservation,WalletEntry}.cs`, `Enums/{ReservationStatus,WalletEntryReason}.cs` | pendiente |
| 2 | Config EF + migración con rollback manual | `AuraLearnDbContext.cs`, `Migrations/AddSlotsAndReservations*` | pendiente |
| 3 | Tests TDD (rojos primero) | `api/tests/AuraLearn.Tests/*Tests.cs` | pendiente |
| 4 | Puertos + servicios Application | `Application/Interfaces/*`, `Application/Services/*`, `Application/Dto/ReservationDto.cs` | pendiente |
| 5 | Controllers + reemplazo DemoSlots | `Api/Controllers/*`, `TutorService.cs`, `TutorDto.cs`, `Program.cs` | pendiente |
| 6 | Test de concurrencia (Postgres real) | `api/tests/AuraLearn.Tests/ReservationConcurrencyTests.cs` | pendiente |
| 7 | Gate backend | `cd api && dotnet build && dotnet test` | pendiente |
| 8 | Cliente HTTP + hooks frontend | `web/lib/api.ts`, `web/lib/types/tutor.ts`, `web/hooks/*` | pendiente |
| 9 | Directorio + perfil tutor (Stitch 01+02) | `web/app/tutores/page.tsx`, `web/app/tutores/[id]/page.tsx` | pendiente |
| 10 | Checkout SINPE + admin pagos (Stitch 03) | `web/app/checkout/[id]/page.tsx`, `web/app/admin/pagos/page.tsx` | pendiente |
| 11 | Mis-tutorías + monedero (Stitch 04) | `web/app/mis-tutorias/page.tsx` | pendiente |
| 12 | Gate frontend | `cd web && npm run lint && npx tsc --noEmit && npm run build` | pendiente |
| 13 | Smoke E2E flujo completo | — | pendiente |
| 14 | Cierre: verification, summary, `.ai/`, bump 0.5.0 | `docs/plans/*`, `.ai/*`, `web/package.json` | pendiente |

## Contrato de API (nuevo)

- `GET /api/tutors/{id}` → 200 TutorDto, 404
- `GET /api/tutors/{id}/slots?from&to` → 200 `[{id, startAt, endAt}]` solo disponibles
- `POST /api/reservations` [Estudiante] `{slotId, idempotencyKey}` → 201 + paymentInfo; 409 SLOT_ALREADY_RESERVED; 422 slot pasado/propio; replay → 200
- `GET /api/reservations` [Authorize] → reservas del estudiante
- `GET /api/reservations/{id}` [dueño o Admin] → detalle/estado
- `POST /api/reservations/{id}/comprobante` [dueño] `{confirmationNumber, amountCrc, phone?}` → 202; 409 COMPROBANTE_REUTILIZADO/RESERVA_NO_PENDIENTE; 422 MONTO_NO_COINCIDE/RECEPTOR_INCORRECTO/VENTANA_INVALIDA
- `PATCH /api/reservations/{id}/cancel` [dueño] → 200; ≥12h acredita monedero; <12h sin reembolso; 409 terminal
- `GET /api/admin/payments` [Admin] → cola de comprobantes
- `PATCH /api/admin/payments/{reservationId}` [Admin] `{decision, reason?}` → 204; reject exige motivo
- `GET /api/wallet` [Authorize] → `{balanceCrc}`
- `TutorDto.NextSlot` (string demo) → `NextSlotAt` (DateTime UTC)

## Modelo de datos

- `slots`: id, tutor_id FK cascade, start_at timestamptz, end_at. UNIQUE (tutor_id, start_at).
- `reservations`: id, slot_id FK, student_id FK, status, price_crc, idempotency_key, expires_at, confirmation_number (unique nullable), comprobante_amount_crc?, comprobante_phone?, comprobante_submitted_at?, decision_reason?, confirmed_at?, cancelled_at?, created_at. **Índice único parcial (slot_id) WHERE status IN (PendingPayment, Confirmed)**.
- `wallet_entries`: id, user_id FK, amount_crc, reason, reservation_id FK nullable, created_at.
- Enums: `ReservationStatus {PendingPayment=1, Confirmed=2, Rejected=3, Expired=4, Cancelled=5, Completed=6}`, `WalletEntryReason {CancellationCredit=1}`.
- Máquina de estados centralizada `ReservationStateMachine` (Domain) con tests por transición.

## Criterios de aceptación

1. Dos reservas paralelas del mismo slot → exactamente 1 gana (201), la otra 409; cancelar libera el slot.
2. Reserva sin comprobante expira a los 60 min y el slot vuelve a ser reservable.
3. Comprobante: monto incorrecto → 422; nº confirmación reutilizado → 409; Admin aprueba → CONFIRMED; rechaza → REJECTED con motivo y slot liberado.
4. Cancelación ≥12 h → crédito al monedero; <12 h → sin reembolso.
5. Las 4 pantallas implementadas según Stitch; "Ver Perfil" navega a `/tutores/[id]`.
6. Gates backend y frontend verdes; smoke E2E completo.
