---
id: version-changes
priority: 4
loadWhen: bump-de-version-o-cierre-de-tarea
maxLines: 70
---

# Version Changes — AuraLearn

Changelog por versión. Clasificación permitida: `feat`, `fix`, `refactor`, `db`, `test`, `docs`.
> Histórico antiguo NO es carga automática (ver `CONTEXT_BUDGET.md`). Mantener detallada solo la versión actual.

## v0.6.0 — 2026-10-01

- **feat:** cierre de deuda de reservas — monedero al pagar, cierre de sesión con liquidación, disponibilidad semanal del tutor, expiración perezosa y reprogramación cancelar+crear. Plan `docs/plans/2026-10-01-reservations-closeout/`.
  - **Fase 0 (desbloqueo):** migración `AddSlotsAndReservations` regenerada con tooling EF (Designer + snapshot sincronizado, índice duplicado eliminado); ensure-ahead cableado (ADR-005) en `SlotsController` + `TutorService` vía nuevo `SlotAvailabilityService` y `SlotRepository.EnsureAheadInRangeAsync` (pre-consulta + insert batch + purga de slots fuera de franjas sin reserva activa); `SinpeConfig` inyectada (quita 2 hardcodes); fix de negocio: cancelar una `PendingPayment` no acredita monedero; endpoint `GET /api/tutors/{id}` (200/404) + `fetchTutor`/`useTutor` reales; `AdminPaymentsController` con DI por ctor (sin service-locator).
  - **Fase 1 (smoke E2E API, 15 escenarios):** reserva 201/replay 200/doble-booking 409 (guardián índice parcial)/comprobante 202+422/admin 204→Confirmed/cancelar ≥12 h → monedero 14500/cancelar PendingPayment → 0. **Bug arreglado:** la expiración perezosa no existía → `TryExpireIfDueAsync` con TDD (no expira la que tiene comprobante en cola; libera el slot al vencer).
  - **Fase 2 (pago con monedero, ADR-006):** solo pago total — `useWalletBalance` + `IWalletRepository.TryDebitAsync` atómico (FOR UPDATE + SUM en lock; race-safe) + `WalletEntryReason.WalletPayment` (débito negativo); saldo insuficiente → fallback SINPE. La reserva se persiste ANTES del débito (FK 23503 detectado en smoke). Frontend: checkbox "Pagar con monedero" en `/tutores/[id]`; Confirmed → directo a `/mis-tutorias`.
  - **Fase 3 (Completed + liquidación):** `PATCH /api/reservations/{id}/complete` (rol Tutor dueño, solo `Confirmed` con `EndAt <= now`) + `WalletEntryReason.TutorPayout` (retención hasta cerrar); `GET /api/reservations/tutor-sessions`; fix de contrato: `ReservationDto.TutorId` venía con `StudentId` + nuevo `StudentName`. Frontend: `/tutor/sesiones` (guard Tutor, completar, banner liquidado) + hook `use-tutor-sessions`.
  - **Fase 4 (disponibilidad semanal):** entidad `TutorAvailability` (weekday + franja local CR), migración `AddTutorAvailability` (rollback manual), honoring en `SlotGenerationService` (overload con reglas; sin reglas → default 08-20 CR con comida excluida), purga de slots no reservables conservando los con reserva activa, `GET/PUT /api/tutors/me/availability` (204/422). Frontend: `/tutor/disponibilidad` (editor por día con `<input type="time">`) + hook `use-availability`.
  - **Fase 5 (reprogramación):** decisión cancelar+crear (sin PATCH reschedule); botón "Reprogramar" en `/mis-tutorias` para Confirmed ≥12 h → navega al perfil del tutor; skill `slots-reservations` y GLOSSARY actualizados.
  - **Tests:** 79/79 (60 previos + 19 nuevos TDD). **Gates:** `dotnet build`/`test` verdes; `lint`/`tsc`/`build` verdes (13 rutas, 2 nuevas).
  - **Deuda explícita restante:** firma digital SINPE real (ADR-004 sigue vigente); pago parcial con monedero excluido por decisión; test de concurrencia contra Postgres real (Testcontainers sin configurar); refresh tokens/rate limiting; notificaciones email; upload de atestados; Playwright E2E; smoke de UI manual del flujo completo pendiente del usuario.

## v0.5.0 — 2026-09-29

- **feat:** motor de slots y reservas + pago SINPE end-to-end (reemplaza el `nextSlot` demo).
  - **Backend:** entidades `Slot`/`Reservation`/`WalletEntry` + enums; `ReservationStateMachine` (Domain) con tests por transición; `SlotGenerationService` (idempotente, UTC, zona CR via `TimeZoneInfo`); `SinpeComprobanteValidator` (monto exacto, nº confirmación único, receptor, ventana); `ReservationService` (crear idempotente, comprobante, decidir, cancelar con regla 12 h + monedero); **índice único parcial `(slot_id) WHERE status IN (1,2)`** como guardián de exclusividad; migración `AddSlotsAndReservations` con bloque ROLLBACK MANUAL. Endpoints: `GET /api/tutors/{id}/slots`, `POST /api/reservations` (idempotent), `GET /api/reservations`, `GET /api/reservations/{id}`, `POST /api/reservations/{id}/comprobante`, `PATCH /api/reservations/{id}/cancel`, `GET /api/admin/payments`, `PATCH /api/admin/payments/{reservationId}` (rol Admin, motivo ≥10 chars), `GET /api/wallet`. Config `Sinpe:ReceptorPhone` (default `88888888`).
  - **Frontend:** `nextSlotAt` reemplaza `nextSlot`; tipos `Slot`/`Reservation`/`PaymentInfo`/`AdminPayment`/`WalletBalance`/`ReservationStatus`; 10 funciones nuevas en `lib/api.ts`; 4 hooks (`use-tutor-slots`, `use-reservation` + `useReservationPoll`, `use-my-reservations`, `use-admin-payments`); helpers `lib/time.ts` (`Intl` + `America/Costa_Rica`); 4 pantallas nuevas (`/tutores/[id]`, `/checkout/[id]`, `/mis-tutorias`, `/admin/pagos`); `/tutores` actualizado con formato es-CR + Link al perfil.
  - **Tests:** 34 tests nuevos en backend (Reservas/Slots/SINPE/StateMachine/NextSlot) — suite 60/60 verde. TDD obligatorio: cada invariante = test (incluida la máquina de estados).
  - **Gates:** `dotnet build`/`dotnet test` verdes; `npm run lint`/`tsc --noEmit`/`npm run build` verdes (11 rutas generadas).
  - **Deuda explícita:** reprogramación (PATCH reschedule) excluida; CRUD de disponibilidad del tutor (el concepto está en Stitch 02, no implementado); firma digital real del comprobante sustituida por confirmación manual admin (ADR-004); expiración de `PendingPayment` perezosa (60 min, sin workers); aplicar saldo del monedero al pagar (consumir con monto negativo); estado `Completed` con liquidación al tutor; notificaciones por email.
  - **ADRs nuevos:** 004 (confirmación manual SINPE por Admin) y 005 (UTC en BD, zona CR en presentación, slots ensure-ahead). Plan `docs/plans/2026-09-29-slots-reservations/`.

## v0.4.2 — 2026-09-29

- **feat:** panel admin de aprobación de tutores: página `/admin/tutores` protegida por rol Admin (guard client-side), cola de postulaciones (`fetchAdminApplications`), rechazo con motivo obligatorio (modal) y aprobación contra `PATCH {id}/verify`, recarga automática de la cola. Smoke E2E: login admin → cola visible → rechazo validado → `Rejected` + `rejection_reason` en BD. Plan `docs/plans/2026-09-29-admin-panel/`.

## v0.4.1 — 2026-09-29

- **feat:** auth UI en el frontend conectada al backend real: páginas `/login` y `/registro` (validación client-side espejo, estados loading/error), contexto global de sesión `AuthProvider`/`useAuth` (login/register/logout, restauración con validación de expiración JWT), `fetchMe` en `lib/api.ts`, `SiteHeader` con sesión ("Hola, {nombre}" + cerrar sesión). Smoke E2E: login, registro, logout y persistencia en recarga; usuario creado desde la UI en BD. Plan `docs/plans/2026-09-28-auth-ui/`.

## v0.4.0 — 2026-09-29

- **feat:** onboarding del profesor full-stack. Frontend: wizard `/postular` de 4 pasos (cuenta → perfil académico → cursos/tarifa → revisión) + confirmación ≤48 h (`components/onboarding/`, `hooks/use-tutor-application.ts`, `lib/auth.ts`); CTAs "Postular como Docente"/"Conviértete en Tutor" enlazados. Backend: `POST /api/tutor-applications` (202), `GET /api/tutor-applications/status`, `GET /api/admin/tutor-applications` + `PATCH {id}/verify` (rol Admin, rechazo con motivo obligatorio); `Tutor.UserId` (FK cascade + unique) y `RejectionReason`; migración `AddTutorUserId` con rollback manual. 18 tests TDD (25/25); smoke E2E 19 verificaciones. Plan `docs/plans/2026-09-28-tutor-onboarding/`.

## v0.3.0 — 2026-09-28

- **feat:** autenticación en el backend (TDD obligatorio, tests first): `POST /api/auth/register` (hash PBKDF2 con PasswordHasher, rol Estudiante), `POST /api/auth/login` (JWT con claims sub/email/role/name, exp 8h), `GET /api/auth/me` (protegido). Entidad `User` con email único; migración `AddUsersTable` con rollback manual. 7 tests de auth (12/12 total). Paquetes: Microsoft.Extensions.Identity.Core, System.IdentityModel.Tokens.Jwt. Plan `docs/plans/2026-09-28-auth-endpoints/`.

## v0.2.1 — 2026-09-28

- **feat:** primera integración real frontend↔backend: el directorio `/tutores` consume `GET /api/tutors` vía nuevo `web/lib/api.ts` (cliente HTTP tipado) y `web/hooks/use-tutors.ts` (estados, AbortController, debounce). Mock `TUTORS` eliminado; tipo `Tutor` alineado al DTO real; sort client-side en `lib/filters.ts`; `LoadingGrid`/`ErrorState` nuevos; `.env.example` + `.env.local` para `NEXT_PUBLIC_API_URL`. Plan `docs/plans/2026-09-28-connect-frontend-api/`.

## v0.2.0 — 2026-09-28

- **feat:** backend .NET 10 implementado en `api/` (ADR-003): solución Clean Architecture 4 proyectos, EF Core + PostgreSQL con migración `InitialCreate` (rollback manual incluido), seed de 12 tutores, JWT configurado, `GET /api/tutors` con filtros/paginación, healthcheck `/health`, Swagger dev, CORS localhost:3000. 5 tests xUnit. Gate backend verde. BD local `auralearn_dev` creada.

## v0.1.1 — 2026-09-28

- **refactor:** extraídos los mocks de datos fuera de los componentes: `TUTORS` + constantes de filtro a `web/lib/data/tutors.ts`, `MENTORS` a `web/lib/data/mentors.ts`, tipos (`Tutor`, `FilterState`, `SortOption`, `Mentor`) a `web/lib/types/`, y lógica de filtrado/orden a funciones puras en `web/lib/filters.ts` (`applyFilters`/`applySort`). Comportamiento idéntico; plan cerrado en `docs/plans/2026-09-28-extract-mock-data/`.

## v0.1.0 — 2026-09-27

- **feat:** landing pública en `web/` con secciones (hero, trust-strip, rigor, disciplinas, mentores, pasos, testimonios, tutor-cta, FAQ, final-cta, footer) y header con toggle CRC/USD.
- **feat:** directorio `/tutores` con búsqueda, filtros (universidad, nivel, rating, precio, disponibilidad) y orden sobre datos mock.
- **feat:** sistema de diseño base-nova sobre `@base-ui/react` con tokens Material 3 en `app/globals.css` y Material Symbols.
- **docs:** sistema de memoria externa y gobernanza del agente: `.ai/`, `.github/copilot-instructions.md`, `.github/instructions/`, `.github/skills/`, `.github/agents/`, `.github/prompts/`, `.github/errors/`, `.github/templates/`, `docs/plans/`.
- **docs:** ADR-001 (gobernanza) y ADR-002 (sistema de componentes).

## Histórico previo

(vacío — este es el registro inicial)
