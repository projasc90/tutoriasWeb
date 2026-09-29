---
id: current-state
priority: 1
loadWhen: always
maxLines: 60
---

# Estado Actual — AuraLearn

**Versión backend:** solución `AuraLearn.sln` .NET 10 · **Versión frontend:** `0.4.2` (SEMVER en `web/package.json`)

## Completado recientemente

- **Panel admin de aprobación** (`docs/plans/2026-09-29-admin-panel/`): página `/admin/tutores` protegida por rol Admin (guard client-side), cola de postulaciones vía `useAdminApplications`, tarjeta con datos del postulante, rechazo con motivo obligatorio (modal ≥10 chars) contra `PATCH {id}/verify`. Smoke E2E: login admin, cola visible, rechazo validado, `Rejected` + `rejection_reason` persistidos en BD. Bump 0.4.2.

- **Onboarding del Profesor full-stack** (`docs/plans/2026-09-28-tutor-onboarding/`): wizard `/postular` de 4 pasos (cuenta → perfil → cursos/tarifa → revisión) + confirmación ≤48 h. Backend: `POST /api/tutor-applications` (202 PendingReview), `GET /api/tutor-applications/status`, cola admin `GET /api/admin/tutor-applications` y `PATCH {id}/verify` (approve/reject con motivo obligatorio, rol Admin). Vínculo User↔Tutor (`tutors.user_id` FK cascade + unique) y `rejection_reason` vía migración `AddTutorUserId` (rollback manual). 18 tests TDD (25/25). Smoke E2E 19 verificaciones. Bump 0.4.0.
- **Auth implementada con TDD** (`docs/plans/2026-09-28-auth-endpoints/`): `POST /api/auth/register` (201, rol Estudiante, hash PBKDF2 con PasswordHasher), `POST /api/auth/login` (JWT con claims sub/email/role/name, exp 8h), `GET /api/auth/me` (protegido con Bearer). Migración `AddUsersTable` con rollback manual + índice unique en email. 7 tests de auth escritos ANTES de la implementación; gate 12/12 verdes; smoke completo (201/409/400/200/401).
- **Auth UI en el frontend** (`docs/plans/2026-09-28-auth-ui/`): páginas `/login` y `/registro` conectadas a los endpoints reales, `AuthProvider`/`useAuth` (sesión global con expiración JWT), `SiteHeader` con sesión ("Hola, {nombre}" + cerrar sesión), `fetchMe` en `lib/api.ts`. Smoke E2E completo: login, registro, logout, persistencia en recarga. El wizard `/postular` reutiliza el mismo storage (`auralearn.auth`) con `toSession`.
- **Backend .NET implementado** (`docs/plans/2026-09-28-api-scaffold/`, ADR-003): Clean Architecture, EF Core + PostgreSQL, seed 12 tutores, `GET /api/tutors` con filtros/paginación.
- Extraídos los mocks a módulos dedicados (`docs/plans/2026-09-28-extract-mock-data/`).
- Sistema de memoria externa y gobernanza (ADR-001).

## Pendientes inmediatos

- Aprobar un tutor real en producción (el smoke del panel solo rechazó datos de prueba).
- Refresh tokens con rotación + rate limiting en login (deuda de seguridad del plan auth); cookies httpOnly vs localStorage (ADR).
- Upload de atestados (storage con URLs firmadas) según skill `tutor-verification`; notificaciones por email al tutor.
- Motor de slots/reservas para reemplazar el `nextSlot` demo del backend.
- Conectar `level`/`availability` (requiere campos nuevos en backend); migrar sort al backend con paginación UI.
- Docker/compose para reproducibilidad dev; Playwright para E2E.
- Portal público en `portal/` (reservado).

## Contexto rápido de arquitectura

- **Flujo de postulación:** wizard `/postular` → `registerUser`/sesión localStorage (`lib/auth.ts`) → `useTutorApplication` → `POST /api/tutor-applications` → fila `Tutor` con `UserId` + `PendingReview` → admin aprueba/rechaza → `Verified` (visible en catálogo) o `Rejected`+motivo (re-postulación permitida).
- **Flujo de datos `/tutores`:** UI (`app/tutores/page.tsx`) → `useTutors` (hook con estados/abort/debounce) → `lib/api.ts` (fetch tipado) → `GET /api/tutors` (backend .NET) → PostgreSQL. Filtros server-side; sort client-side.
- **Flujo de sesión (frontend):** `AuthProvider`/`useAuth` (restauración al montar con expiración) → `lib/auth.ts` (localStorage `auralearn.auth`, `decodeJwt`) → `lib/api.ts` (`loginUser`/`registerUser`/`fetchMe`) → `/api/auth/*` (backend). Header reactivo: "Hola, {nombre}" + cerrar sesión.
- **Auth backend:** `AuthService` (hash PBKDF2 con `PasswordHasher<User>`, validación FluentValidation) → `JwtTokenService` (claims sub/email/role/name, exp 8h) → tabla `users` con email único. Endpoints `/api/auth/{register,login,me}`.
- **Backend (`api/`, activo):** `AuraLearn.Api` → `AuraLearn.Application` → `AuraLearn.Domain`; `AuraLearn.Infrastructure` (DbContext Npgsql, repos, migraciones). BD `auralearn_dev` local con 12 tutores y tabla `users`.
- **Frontend (`web/`, activo):** Next.js 16.3.6 App Router + React 19.2.8 + TS 5 + Tailwind 4 + shadcn/ui base-nova. `MENTORS` de la landing sigue estático (marketing).
- Tokens de diseño: Material 3 light en `web/app/globals.css` (`@theme inline`); tipografías MD3; fuente Plus Jakarta Sans.
