# Summary — Reglas de reserva definidas por Admin (booking rules)

**Fecha:** 2026-10-12 · **Versión:** 0.11.0 · **Estado:** ✅ cerrado

## Qué se construyó

Las "Reglas de Reserva e Intervalos" del gestor de disponibilidad del tutor dejaron de estar hardcodeadas: ahora viven en la tabla `booking_rules` (fila única, definidas por el Admin) y el Admin las edita desde la nueva página `/admin/configuracion`.

- **Modelo:** 4 reglas **opcionales** (nullable) — anticipación de cancelación (12 h), ventana de pago SINPE (60 min), duración de slots (60 min), horizonte de reserva (14 días). Seed con los valores actuales; regla `null` = "sin definir" → no se muestra al tutor y la lógica usa su default (`BookingRulesDefaults`).
- **Backend:** entidad `BookingRules` + migración `AddBookingRules` (rollback manual), `IBookingRulesRepository`/`BookingRulesRepository` (upsert fila única), `BookingRulesService` + `UpdateBookingRulesValidator` (rangos por campo), `GET /api/booking-rules` público y `PUT /api/admin/booking-rules` `[Admin]` con errores `BOOKING_RULES_INVALID_*`.
- **Lógica gobernada por BD:** `ReservationService.CancelAsync` (anticipación), ventana de pago en `CreateAsync` (`ExpiresAt`), horizonte ensure-ahead en `TutorService` y `SlotsController`. La **duración de slot se persiste pero sigue sin efecto** (acoplada a granularidad de franjas e índices — deuda).
- **Frontend:** `BookingRules`/`BOOKING_RULES_DEFAULTS`/`fetchBookingRules`/`updateBookingRules` en `api.ts`, hook `use-booking-rules`, tarjetas dinámicas en `/tutor/disponibilidad` (con nota "solo lectura"), página `/admin/configuracion` + `BookingRulesForm` (input vacío = sin definir, validación espejo del backend), enlace "Reglas de reserva" en el menú Admin, y `mis-tutorias`/checkout leen la regla de cancelación y la ventana de pago de BD (FAQ queda estática).

## Verificación

Gates verdes: backend **144/144** tests (23 nuevos TDD), lint/tsc/build frontend (18 rutas). Migración aplicada y fila seed verificada. Smoke curl: GET público 200, PUT Admin 200, PUT Estudiante 403, PUT inválido 422 con código específico. Navegador: tarjetas con valores de BD y guard de la página admin. Detalle en [verification](./verification.md).

## Decisiones

- Reglas tipadas opcionales (no tabla clave→valor): tipado fuerte para gobernar la lógica y UI condicional simple.
- `updated_by_user_id` **sin FK**: auditoría informativa (eliminar un admin no rompe el historial).
- Fallback en Application (`BookingRulesDefaults`), no en Domain: los defaults son política de plataforma, no invariante de dominio.
- Duración de slot declarativa: cambiarla de verdad exige tocar granularidad de franjas, índice único `(tutor_id, start_at)` y UI — se registra como deuda en lugar de romper el motor.

## Deuda / siguientes pasos

- Efecto real de `slotDurationMinutes` en la generación (requiere ADR: granularidad de franjas + índices + UI).
- Caché de reglas (hoy 1 query por operación; barata pero sin invalidación).
- FAQ de la landing sigue con "12 horas" estático (marketing).
- Usuario Admin de prueba `admin.test@auralearn.dev` creado para el smoke — eliminar tras revisión.
