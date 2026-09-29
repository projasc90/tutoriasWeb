---
id: version-changes
priority: 4
loadWhen: bump-de-version-o-cierre-de-tarea
maxLines: 70
---

# Version Changes — AuraLearn

Changelog por versión. Clasificación permitida: `feat`, `fix`, `refactor`, `db`, `test`, `docs`.
> Histórico antiguo NO es carga automática (ver `CONTEXT_BUDGET.md`). Mantener detallada solo la versión actual.

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
