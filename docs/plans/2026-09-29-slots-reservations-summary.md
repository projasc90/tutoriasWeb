# Summary — Motor de slots y reservas + pago SINPE

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-29-slots-reservations-plan.md)
**Verificación:** [verification](./2026-09-29-slots-reservations-verification.md)

## Qué cambió

### Backend (`api/`)
- **Dominio** (`AuraLearn.Domain`): entidades `Slot`, `Reservation`, `WalletEntry`; enums `ReservationStatus`, `WalletEntryReason`; `ReservationStateMachine` (transiciones centralizadas).
- **Application** (`AuraLearn.Application`): puertos `ISlotRepository`, `IReservationRepository`, `IWalletRepository`; servicios `SlotGenerationService` (estático, idempotente), `SlotQueryService`, `SinpeComprobanteValidator`, `ReservationService`. DTOs `SlotDto`, `ReservationDto`, `PaymentInfoDto`, `AdminPaymentDto`, `WalletBalanceDto`, `SinpeComprobanteDto`, `SinpeConfig`, `ReservationResult<T>`. `TutorService.SearchAsync` ya no usa el array `DemoSlots`: `NextSlotAt` viene de slots reales (UTC, query agrupada).
- **Infrastructure**: `SlotRepository`, `ReservationRepository`, `WalletRepository`. `AuraLearnDbContext` configura snake_case, índices únicos y FK cascade. Migración `AddSlotsAndReservations` con bloque ROLLBACK MANUAL.
- **API**: `SlotsController`, `ReservationsController` (helper `GetUserId` reusado), `AdminPaymentsController`, `WalletController`.

### Frontend (`web/`)
- **Cliente HTTP** (`lib/api.ts`): 10 funciones nuevas; tipo `Tutor.nextSlotAt` reemplaza a `nextSlot`.
- **Tipos**: `Slot`, `Reservation`, `PaymentInfo`, `AdminPayment`, `WalletBalance`, `ReservationStatus` (union de strings), `SinpeConfig`.
- **Hooks**: `use-tutor-slots`, `use-tutor`, `use-reservation` (con `useReservationPoll`), `use-my-reservations`, `use-admin-payments`.
- **Helpers**: `lib/time.ts` (formateo es-CR con `Intl` + `America/Costa_Rica`).
- **Pantallas nuevas**: `/tutores/[id]`, `/checkout/[id]`, `/mis-tutorias`, `/admin/pagos`.
- **Cambios**: `/tutores` formatea `nextSlotAt` a es-CR y enlaza a `/tutores/[id]`.

### Documentación
- **ADRs nuevos**:
  - ADR-004: Confirmación manual de pagos SINPE por Admin (en `.ai/ADR_LOG.md`).
  - ADR-005: Slots ensure-ahead + UTC en BD, zona CR solo en presentación.
- **Plans**: contexto, plan, verificación y summary (este archivo).

## Qué NO cambió (deuda explícita)

- Reprogramación (PATCH reschedule) — excluida por decisión del usuario.
- CRUD de disponibilidad semanal del tutor — tarea posterior.
- Validación de firma digital real del comprobante (sustituida por ADR-004).
- Liquidación al tutor / estado `Completed` automático.
- Notificaciones por email al subir comprobante / cambiar estado.
- Playwright E2E (no configurado).
- Cookies httpOnly + refresh tokens (deuda del plan auth).
- Subir atestados del tutor (storage con URLs firmadas).

## Próximos pasos

1. **Aplicar migración** (`dotnet ef database update`) y verificar `dotnet ef migrations script` para producción.
2. **Ejecutar smoke E2E** del flujo completo: registro → reserva → comprobante → admin aprueba → cancelar ≥12 h → monedero.
3. **Descargar assets Stitch** con `bash design/stitch/download-screens.sh` cuando se tenga `STITCH_API_KEY`.
4. **Configurar `Sinpe:ReceptorPhone`** real (producción) y reemplazar el default `"88888888"` en `appsettings.Development.json`.
5. **Tickets de seguimiento**:
   - Reprogramación con regla 12 h.
   - CRUD de disponibilidad del tutor (panel `/tutores/[id]/disponibilidad`).
   - Integración real de la firma digital SINPE (cuando exista la API del banco).
   - Aplicar saldo del monedero al pagar (consumir `WalletEntry` con monto negativo).
   - Estado `Completed` con liquidación al tutor.

## He actualizado los archivos de contexto en `.ai/`
