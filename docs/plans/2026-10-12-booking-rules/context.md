# Contexto — Reglas de reserva definidas por Admin (booking rules)

**Fecha:** 2026-10-12 · **Versión objetivo:** 0.11.0

## Problema

La tarjeta "Reglas de Reserva e Intervalos" del gestor de disponibilidad (`/tutor/disponibilidad`) muestra valores **hardcodeados en la UI**: anticipación 12 h, duración de slots 60 min y ventana de 2 semanas. Las reglas reales viven como constantes de código en el backend (`ReservationService.CancelAsync` con `TimeSpan.FromHours(12)` inline, `PaymentWindow = 60 min`, `TutorService`/`SlotsController` con `AddDays(14)`). El Admin no tiene forma de definir ni modificar ninguna regla (no existe tabla, DTO ni endpoint de settings de plataforma).

Además, la regla de 12 h está duplicada como número mágico en el frontend: `mis-tutorias` (`hoursUntil(...) >= 12` para el crédito al monedero) y textos de checkout (`order-summary.tsx`, `terminal-views.tsx`).

## Decisión del usuario (2026-10-12)

- **Modelo:** 4 reglas **tipadas y opcionales** (nullable) en una fila única `booking_rules`. Si el Admin deja una sin definir → esa tarjeta **no se muestra** en `/tutor/disponibilidad` y la lógica backend usa su valor default.
- **Reglas:** ① anticipación mínima de cancelación (12 h) ② ventana de pago SINPE (60 min) ③ duración de slots (60 min) ④ ventana máxima de reserva (14 días).
- **Lógica "segura":** cancelación, ventana de pago y horizonte leen BD con fallback a los valores actuales. La **duración de slot se persiste pero sigue sin efecto** (acoplada a granularidad de 1 h de franjas, índice único `(tutor_id, start_at)` y UI de disponibilidad — deuda registrada).
- **UI admin:** nueva página `/admin/configuracion` con formulario editable + enlace en el menú Admin.
- **Duplicados:** `mis-tutorias` y checkout leen la regla de BD con fallback 12 h. La FAQ de la landing queda estática (marketing).
- **Seed:** fila única sembrada con 12/60/60/14 para que el Admin vea los valores actuales precargados y pueda modificarlos o vaciarlos.

## Referencias de patrón

- Settings con GET/PUT + códigos de error: `AvailabilityService` / `AvailabilityController` (`AvailabilityErrors` UPPER_SNAKE, 422 con `error`).
- Repositorio: `TutorAvailabilityRepository` (patrón puertos de Application).
- Frontend: `use-availability.ts` (hook ApiState), `admin/tutores/page.tsx` (guard de rol), formularios nativos con `useState`.
