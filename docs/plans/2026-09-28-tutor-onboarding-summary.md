# Summary — Onboarding del Profesor (postulación a tutor)

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-28-tutor-onboarding-plan.md) · **Verificación:** [verification](./2026-09-28-tutor-onboarding-verification.md)

## Qué cambió

### Backend (`api/`)
- **Domain:** `Tutor` gana `UserId` (FK nullable a `users`, 1:1) y `RejectionReason`.
- **Application:** `TutorApplicationDto.cs` (4 DTOs + `TutorApplicationResult<T>`), `SubmitTutorApplicationValidator`, `TutorApplicationService` (submit/status/listPending/decide), puertos ampliados (`ITutorRepository`: GetByUserId/GetById/GetPending/Add/Update; `IUserRepository`: GetById).
- **Infrastructure:** implementación de los nuevos métodos; migración `AddTutorUserId` (a mano, con Designer + Snapshot y `// ROLLBACK MANUAL`).
- **Api:** `TutorApplicationsController` (`POST` 202, `GET status` 404/200, `[Authorize]`) y `AdminTutorApplicationsController` (`GET` cola, `PATCH {id}/verify` 204/400/404, `[Authorize(Roles="Admin")]`); wiring en `Program.cs`.
- **Tests:** 18 tests xUnit+NSubstitute escritos ANTES del servicio (TDD). Total suite: 25/25.

### Frontend (`web/`)
- **Nueva ruta `/postular`** (server component con metadata es_CR) con columna de propuesta de valor + wizard client.
- **Wizard 4 pasos** (`components/onboarding/`): `onboarding-wizard.tsx` (orquestador con sesión en localStorage, chequeo de postulación existente al montar), `step-account` (registro vía `POST /api/auth/register`), `step-profile` (título/universidad/bio), `step-subjects` (chips de materias + tarifa CRC con USD derivado), `step-review` (resumen + declaración jurada), `confirmation` (timeline ≤48 h), `shared` (Stepper + Field).
- **Datos:** `lib/types/tutor-application.ts` (espejo DTOs), `lib/api.ts` (+`registerUser`, `loginUser`, `submitTutorApplication`, `fetchTutorApplicationStatus`, `AUTH_STORAGE_KEY`), `lib/auth.ts` (sesión mínima localStorage), `hooks/use-tutor-application.ts` (mutación idle/submitting/error/success).
- **CTAs enlazados:** "Postular como Docente" (`tutor-cta.tsx`) y "Conviértete en Tutor" (`SiteHeader`) → `/postular`.
- **Versión:** `web/package.json` 0.2.1 → **0.4.0**.

## Qué NO cambió

- `GET /api/tutors` y el directorio `/tutores` (el filtro `Verified` ya excluía pendientes).
- `AuthService` (rol Estudiante hardcodeado se mantiene; el rol Tutor lo otorga el admin al aprobar — decisión D2).
- Sin upload de documentos (D4), sin UI admin (D5), sin contexto global de sesión (D7).

## Deuda registrada

1. `AuraLearn.Api.http` obsoleto (template weatherforecast).
2. Sin tests de integración HTTP (Testcontainers) para los controllers nuevos.
3. Sesión en `localStorage` sin expiración/refresh (deuda del plan auth).
4. El wizard no consulta `GET /api/auth/me` para validar el token guardado (confía en el 401 del endpoint).
5. Pantalla Stitch de referencia no descargada en `design/stitch/` (la UI siguió el design system del repo).

## Próximos pasos sugeridos

1. Panel admin de aprobación (UI) consumiendo los endpoints nuevos.
2. Auth UI global (login/registro) + contexto de sesión; migrar el wizard a ese contexto.
3. Upload de atestados (storage con URLs firmadas) según skill `tutor-verification`.
4. Notificación por email al aprobar/rechazar.
