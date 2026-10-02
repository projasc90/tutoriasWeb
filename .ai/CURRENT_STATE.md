---
id: current-state
priority: 1
loadWhen: always
maxLines: 60
---

# Estado Actual — AuraLearn

**Versión backend:** solución `AuraLearn.slnx` .NET 10 · **Versión frontend:** `0.6.0` (SEMVER en `web/package.json`)

## Completado recientemente

- **Cierre de deuda de reservas** (`docs/plans/2026-10-01-reservations-closeout/`): 6 fases ejecutadas. **Fase 0** desbloqueó el smoke: migración `AddSlotsAndReservations` regenerada con tooling (snapshot sincronizado, índice duplicado fuera), ensure-ahead cableado (ADR-005) vía `SlotAvailabilityService` + `EnsureAheadInRangeAsync` (pre-consulta + batch + reintento de carrera), `SinpeConfig` real (adiós `"88888888"` hardcode), fix: cancelar `PendingPayment` no acredita monedero, `GET /api/tutors/{id}` + `fetchTutor`/`useTutor` reales, AdminPayments con DI por ctor. **Fase 1** smoke E2E API (15 escenarios) → **bug arreglado:** expiración perezosa no existía → `TryExpireIfDueAsync` (no expira con comprobante en cola; libera slot al vencer). **Fase 2** pago con monedero (ADR-006): `useWalletBalance`, `TryDebitAsync` atómico (FOR UPDATE + SUM en lock), `WalletPayment` débito negativo, fallback SINPE si no alcanza; reserva persiste ANTES del débito (FK 23503 del smoke); toggle en `/tutores/[id]` → Confirmed va directo a `/mis-tutorias`. **Fase 3** `Completed` con liquidación: `PATCH {id}/complete` (tutor dueño, `EndAt <= now` → 422 sino), `TutorPayout`, `GET reservations/tutor-sessions`, fix de `TutorId`/`StudentName` en DTO; `/tutor/sesiones` + hook. **Fase 4** disponibilidad semanal: `TutorAvailability` + migración con rollback manual, honoring en el generador (overload con reglas; sin reglas → default 08-20 CR), purga de slots fuera de franjas (conserva con reserva activa), `GET/PUT /api/tutors/me/availability` (204/422); `/tutor/disponibilidad` + hook. **Fase 5** reprogramación = cancelar+crear (sin reschedule, decisión usuario): botón en `/mis-tutorias`; skill `slots-reservations` y GLOSSARY actualizados. Tests **79/79**; gates backend/frontend verdes (13 rutas, 2 nuevas). Bump 0.6.0.
- **Motor de slots y reservas + pago SINPE** (`2026-09-29-slots-reservations/`): máquina de estados, validador SINPE, índice único parcial `(slot_id) WHERE status IN (1,2)`, cola admin, checkout con polling, `/mis-tutorias`. Bump 0.5.0.
- **Panel admin** (`2026-09-29-admin-panel/`) · **Onboarding tutor** (`2026-09-28-tutor-onboarding/`, 0.4.0) · **Auth TDD** (`2026-09-28-auth-endpoints/`, 0.3.0) · **Auth UI** (`2026-09-28-auth-ui/`) · **Backend .NET 10** (`2026-09-28-api-scaffold/`, ADR-003) · mocks extraídos · gobernanza (ADR-001/002).

## Pendientes inmediatos

- Smoke E2E en navegador (UI real con polling) — el flujo API está cubierto a nivel curl (15 escenarios).
- Firma digital SINPE real (requiere API del banco; ADR-004 manual vigente).
- Test de concurrencia contra Postgres real (Testcontainers sin configurar) para débito y doble-booking.
- Refresh tokens + rate limiting en login; ADR cookies httpOnly vs localStorage.
- Pizarra digital realtime para sesiones confirmadas; notificaciones email; upload de atestados con URLs firmadas.
- Docker/compose; Playwright E2E; portal público `portal/`.

## Contexto rápido de arquitectura

- **Flujo de reserva:** `/tutores/[id]` → slots (ensure-ahead genera 14 días) → `useReservation.create` (`useWalletBalance` opcional: saldo ≥ precio → `Confirmed` directo; sino `PaymentInfo`) → `/checkout/{id}` (SINPE 60 min, polling 3 s) → comprobante → `/admin/pagos` → approve/reject (motivo ≥10 chars) → `Confirmed` → tutor la completa tras `EndAt` (`PATCH complete` → liquidación `TutorPayout`); cancelar ≥12 h acredita `CancellationCredit`.
- **Exclusividad del slot:** índice único parcial `(slot_id) WHERE status IN (1,2)` en BD; la expiración perezosa (`TryExpireIfDueAsync`) libera al leer/crear.
- **Disponibilidad:** con franjas en `tutor_availability`, solo se generan slots dentro de ellas y se purgan los que quedan fuera (sin reserva activa); sin reglas → default CR 08-20 con comida 12-13.
- **Tiempo:** UTC en BD (`timestamptz`); `America/Costa_Rica` solo en generación y presentación (`lib/time.ts`).
- **Roles y auth:** JWT claims `sub/email/role/name` (role con URI largo .NET — el bearer lo remapea), exp 8 h; `[Authorize(Roles=…)]`; roles Estudiante/Tutor/Admin.
- **Backend (`api/`):** `Api` → `Application` → `Domain`; `Infrastructure` (DbContext Npgsql, repos, migraciones con rollback manual). BD `auralearn_dev`: tutors(13), users, slots, reservations, wallet_entries, tutor_availability.
- **Frontend (`web/`):** Next.js 16.3.6 App Router + React 19 + TS 5 + Tailwind 4 + base-nova. 13 rutas. Tokens MD3 en `globals.css`; fuente Plus Jakarta Sans.
- **Exclusividad del slot:** índice único parcial `(slot_id) WHERE status IN (1,2)` en la tabla `reservations` — no depende de validación en servicio.
- **Tiempo:** UTC en BD (`timestamptz`); `TimeZoneInfo("America/Costa_Rica")` solo en `SlotGenerationService`; frontend formatea con `Intl` + `America/Costa_Rica` (`lib/time.ts`).
- **Roles y auth:** JWT con claims `sub/email/role/name`, exp 8 h. `[Authorize]` simple y `[Authorize(Roles="Admin")]`. Roles `Estudiante`, `Tutor`, `Admin`.
- **Backend (`api/`, activo):** `AuraLearn.Api` → `AuraLearn.Application` → `AuraLearn.Domain`; `AuraLearn.Infrastructure` (DbContext Npgsql, repos, migraciones). BD `auralearn_dev` local con 12 tutores, `users`, `slots`, `reservations`, `wallet_entries`.
- **Frontend (`web/`, activo):** Next.js 16.3.6 App Router + React 19.2.8 + TS 5 + Tailwind 4 + shadcn/ui base-nova. `MENTORS` de la landing sigue estático (marketing).
- **Tokens de diseño:** Material 3 light en `web/app/globals.css` (`@theme inline`); tipografías MD3; fuente Plus Jakarta Sans.
