# Plan — Reglas de reserva definidas por Admin (booking rules)

**Fecha:** 2026-10-12 · **Contexto:** [context](./context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Entidad `BookingRules` (fila única, 4 reglas nullable + auditoría) | `api/AuraLearn.Domain/Entities/BookingRules.cs` | ✅ |
| 2 | DbSet + config fluent + seed 12/60/60/14 + migración con rollback manual | `AuraLearnDbContext.cs`, `Migrations/AddBookingRules*` | ✅ |
| 3 | Puerto `IBookingRulesRepository` + impl EF (upsert fila única) | `Application/Interfaces/IBookingRulesRepository.cs`, `Infrastructure/Persistence/BookingRulesRepository.cs` | ✅ |
| 4 | Tests TDD servicio (get/update/rangos/auditoría) + tests rotos | `api/tests/AuraLearn.Tests/BookingRulesServiceTests.cs`, `ReservationServiceTests.cs`, `ReservationCompletionTests.cs` | ✅ |
| 5 | DTO + defaults + validator + `BookingRulesService` | `Application/Dto/BookingRulesDto.cs`, `Application/Services/BookingRulesService.cs` | ✅ |
| 6 | Controller (GET público + PUT Admin) + DI en Program | `Api/Controllers/BookingRulesController.cs`, `Program.cs` | ✅ |
| 7 | Reglas gobiernan lógica: cancelación (notice), ventana de pago, horizonte | `ReservationService.cs`, `TutorService.cs`, `SlotsController.cs` | ✅ |
| 8 | Gate backend | `cd api && dotnet build && dotnet test` | ✅ 144/144 |
| 9 | api.ts: tipo + defaults + fetch/update + hook `use-booking-rules` | `web/lib/api.ts`, `web/hooks/use-booking-rules.ts` | ✅ |
| 10 | Cards dinámicas en disponibilidad (sin regla → no se muestra) | `web/app/tutor/disponibilidad/page.tsx` | ✅ |
| 11 | Página `/admin/configuracion` + form + menú Admin | `web/app/admin/configuracion/page.tsx`, `web/components/admin/booking-rules-form.tsx`, `web/components/landing/currency-toggle.tsx` | ✅ |
| 12 | Duplicados leen BD: mis-tutorias + checkout | `web/app/mis-tutorias/page.tsx`, `web/components/checkout/order-summary.tsx`, `web/components/checkout/terminal-views.tsx` | ✅ |
| 13 | Gate frontend | `cd web && npm run lint && npx tsc --noEmit && npm run build` | ✅ 18 rutas |
| 14 | Verificación: migración, curl (GET/PUT/403/422), navegador | — | ✅ |
| 15 | Cierre: verification + summary, bump 0.11.0, `.ai/` | `docs/plans/*`, `.ai/*`, `web/package.json` | ✅ |

## Contrato de API (nuevo)

- `GET /api/booking-rules` [AllowAnonymous] → 200 `BookingRulesDto` `{cancellationNoticeHours?, paymentWindowMinutes?, slotDurationMinutes?, bookingHorizonDays?, updatedAt}` (null = sin definir).
- `PUT /api/admin/booking-rules` [Admin] → 200 `BookingRulesDto`; 422 `BOOKING_RULES_INVALID_CANCELLATION_NOTICE` (0–168 h) / `BOOKING_RULES_INVALID_PAYMENT_WINDOW` (15–240 min) / `BOOKING_RULES_INVALID_SLOT_DURATION` (15–180 min) / `BOOKING_RULES_INVALID_HORIZON` (1–56 días); 401/403.

## Modelo de datos

- `booking_rules` (fila única, id fijo `00000000-0000-0000-0000-000000000001`): `cancellation_notice_hours?`, `payment_window_minutes?`, `slot_duration_minutes?`, `booking_horizon_days?`, `updated_at`, `updated_by_user_id?` (sin FK: auditoría informativa).
- Seed `HasData`: 12 / 60 / 60 / 14.
- Fallback en Application: `BookingRulesDefaults` (mismos valores) cuando la regla quedó `null` o la fila no existe.

## Criterios de aceptación

1. `GET /api/booking-rules` público devuelve la fila seed (12/60/60/14) sin token.
2. Admin edita por `PUT /api/admin/booking-rules` y el cambio persiste tras reload; un JWT sin rol Admin recibe 403.
3. En `/tutor/disponibilidad` las 3 tarjetas muestran los valores de BD; si una regla queda sin definir, su tarjeta desaparece.
4. Cancelación usa la anticipación de BD (regla 24 h: con 20 h restantes no acredita; con 30 h sí; sin fila → fallback 12 h).
5. La ventana de pago SINPE y el horizonte ensure-ahead usan BD con fallback 60 min / 14 días.
6. `mis-tutorias` y checkout reflejan la anticipación definida; textos con el valor dinámico.
7. Gates backend (build+test) y frontend (lint+tsc+build) verdes.

## Alcance excluido (deuda)

- Duración de slot sin efecto real en la generación (persistida como configuración declarativa).
- FAQ de la landing sigue con texto estático (marketing).
- Caché de reglas e historial de cambios (solo `updated_at`/`updated_by_user_id`).
