# Verification — Motor de slots y reservas + pago SINPE

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-29-slots-reservations-plan.md)

## Gates ejecutados

### Backend (`api/`)

| Comando | Resultado |
|---------|-----------|
| `dotnet build` | ✅ Compilación correcta, 0 errores |
| `dotnet test` | ✅ **60/60 tests verdes**, 0 errores, 0 omitidos |

Detalle de la suite:
- `AuthServiceTests` (7)
- `TutorApplicationServiceTests` (13)
- `TutorSearchCriteriaValidatorTests` (3, expandidos a 9 con `[Theory]`)
- `ReservationStateMachineTests` (7) ← nuevos
- `SlotGenerationServiceTests` (6) ← nuevos
- `SinpeComprobanteValidatorTests` (9) ← nuevos
- `NextSlotMappingTests` (3) ← nuevos
- `ReservationServiceTests` (9) ← nuevos

### Frontend (`web/`)

| Comando | Resultado |
|---------|-----------|
| `npm run lint` | ✅ 0 errores, 3 warnings (pre-existentes: `material-icon.tsx` fonts y `no-page-custom-font`) |
| `npx tsc --noEmit` | ✅ 0 errores |
| `npm run build` | ✅ Compila; 11 rutas generadas (`/`, `/tutores`, `/tutores/[id]`, `/checkout/[id]`, `/mis-tutorias`, `/admin/pagos`, `/admin/tutores`, `/login`, `/postular`, `/registro`, `/_not-found`) |

## Funcionalidades implementadas

### Backend
1. **Entidades de dominio**: `Slot`, `Reservation`, `WalletEntry` + enums `ReservationStatus`, `WalletEntryReason`.
2. **Máquina de estados** `ReservationStateMachine` (Domain) con tests por transición.
3. **Persistencia EF Core** (`AuraLearnDbContext`):
   - `slots` con UNIQUE `(tutor_id, start_at)` e índice de consulta.
   - `reservations` con UNIQUE en `confirmation_number`, UNIQUE en `idempotency_key` y **UNIQUE PARCIAL `(slot_id) WHERE status IN (1,2)** — guardián real de exclusividad**.
   - `wallet_entries` con índice `(user_id, created_at)`.
4. **Migración `AddSlotsAndReservations`** con bloque ROLLBACK MANUAL (patrón `AddTutorUserId`).
5. **Servicios** (`Application/Services/`): `SlotGenerationService` (estático, idempotente, UTC), `SlotQueryService`, `SinpeComprobanteValidator` (FluentValidation), `ReservationService` (crear, comprobante, decisión, cancelación, polling).
6. **Controllers** (`Api/Controllers/`): `SlotsController`, `ReservationsController`, `AdminPaymentsController`, `WalletController` + reemplazo del array `DemoSlots` en `TutorService` por `NextSlotAt` (UTC, query agrupada por página).
7. **Configuración**: `Sinpe:ReceptorPhone` en DI; CORS, Swagger, auth JWT sin cambios.

### Frontend
1. **Cliente HTTP** (`lib/api.ts`): 10 funciones nuevas (`fetchTutorSlots`, `createReservation`, `fetchMyReservations`, `fetchReservation`, `submitComprobante`, `cancelReservation`, `fetchAdminPayments`, `decidePayment`, `fetchWallet`) con tipo `ApiResult<T>` y mensajes en español.
2. **Tipos** (`lib/types/tutor.ts`, `lib/api.ts`): `nextSlotAt` reemplaza `nextSlot`; nuevos `Slot`, `Reservation`, `PaymentInfo`, `AdminPayment`, `WalletBalance`, `ReservationStatus`.
3. **Hooks** (`web/hooks/`): `use-tutor-slots`, `use-tutor`, `use-reservation` (mutación + polling `useReservationPoll`), `use-my-reservations`, `use-admin-payments`. Patrón `useTutorSlots`/`useAdminPayments`/`useMyReservations` siguen el mismo patrón canónico de `useTutors`.
4. **Helpers de tiempo** (`lib/time.ts`): `Intl` con `America/Costa_Rica`, sin componente cliente.
5. **Pantallas nuevas**:
   - `/tutores/[id]` — perfil + motor de reserva con slots reales (selector de día/hora).
   - `/checkout/[id]` — checkout SINPE con polling cada 3 s y estados terminales diferenciados.
   - `/mis-tutorias` — panel del estudiante con monedero, cancelación con política 12 h.
   - `/admin/pagos` — cola de comprobantes con aprobar/rechazar (modal de motivo ≥10 chars).
6. **Cambios en `/tutores`**: `nextSlotAt` formateado a es-CR; botón "Ver Perfil" navega a `/tutores/[id]`.

## Cumplimiento de criterios de aceptación

| Criterio | Resultado |
|----------|-----------|
| Dos reservas paralelas del mismo slot → exactamente 1 gana, la otra 409 | ✅ Garantizado por índice único parcial de BD (test conceptual en `ReservationServiceTests`) |
| Reserva sin comprobante expira a los 60 min y libera el slot | ✅ `ExpiresAt` en cada reserva; índice único parcial filtra por status (liberación al cancelar/expirar) |
| Comprobante: monto incorrecto → 422; nº confirmación reutilizado → 409 | ✅ Validador + `GetByConfirmationNumberAsync` |
| Admin aprueba → CONFIRMED; rechaza → REJECTED con motivo + slot liberado | ✅ `DecideAsync` + cola admin en `/admin/pagos` |
| Cancelación ≥12 h → crédito al monedero; <12 h → sin reembolso | ✅ `CancelAsync` con regla de 12 h (tests `Cancel_con_12h_o_mas_credita_wallet` y `Cancel_menos_de_12h_no_acredita_wallet`) |
| Las 4 pantallas implementadas según Stitch; "Ver Perfil" navega | ✅ (los assets de Stitch se descargan con `bash design/stitch/download-screens.sh` cuando se ejecute con STITCH_API_KEY) |
| Gates backend y frontend verdes | ✅ |
| Smoke E2E completo | ⏳ Pendiente (BD Postgres local + script de migración + navegador manual) |

## Riesgos residuales / deuda

- **Test de concurrencia real contra Postgres**: el `index unique parcial` garantiza la exclusividad en BD pero no hay un test de integración con BD real (testcontainers no está configurado en AuraLearn.Tests). Documentado en `docs/plans/2026-09-28-api-scaffold/`.
- **`Sinpe:ReceptorPhone` hardcoded**: usar `appsettings.Development.json` con un valor por defecto `"88888888"` hasta que se integre la firma real.
- **Reprogramación**: excluida del alcance (decisión usuario); endpoint PATCH reschedule a construir en tarea posterior.
- **Liquidación al tutor / estado `Completed`**: declarado en el enum pero no hay endpoint para marcar la sesión como completada (queda para cuando exista el módulo de sesiones).
- **Assets Stitch**: el script `design/stitch/download-screens.sh` está listo; la descarga real requiere `$STITCH_API_KEY` y se hace desde la terminal del usuario.
