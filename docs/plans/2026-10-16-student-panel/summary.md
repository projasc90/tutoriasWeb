# Summary — Panel estudiante + búsqueda pública + login en reserva (2026-10-16)

## Qué se construyó

1. **Búsqueda de tutores sin login** (ya existía, se verificó y no se rompió): `GET /api/tutors`, `GET /api/tutors/{id}` y `GET /api/tutors/{id}/slots` son públicos; `/tutores` y la landing consumen la API real sin sesión.
2. **Login exigido al reservar, con retorno al flujo**: el backend ya devolvía 401; ahora la UI en `/tutores/[id]` muestra banner + CTA "Inicia sesión para reservar" que lleva a `/login?redirect=/tutores/{id}&slot={slotId}`; tras autenticar (o registrarse) el usuario vuelve a la página del tutor con su horario preseleccionado.
3. **Panel del estudiante** (`/estudiante/*` con subnav Reservas · Monedero): reservas con cancelación/reprogramación según `booking_rules` y monedero con saldo + explicación de créditos. `/mis-tutorias` y `/panel` redirigen a la nueva ruta; `PanelMenu` actualizado.
4. **Panel Admin de estudiantes**: `GET /api/admin/users` (solo Admin, filtro por rol + búsqueda + paginado) y página `/admin/estudiantes` con búsqueda con debounce, filtro de rol y paginación.
5. **Reservas restringidas al rol Estudiante**: `Create`/`SubmitComprobante`/`Cancel` con `[Authorize(Roles = "Estudiante")]` (un Tutor/Admin ya no puede reservar).
6. **Landing con tutores reales**: sección Mentors consume `GET /api/tutors` (client component, fetch en runtime); "Ver Perfil" navega al perfil. Mock `MENTORS` archivado.
7. **Rutas del nav verificadas**: eliminado el enlace roto "Para Universidades" (`#`); `Logo` apunta a `/`.

## Archivos clave

- Backend: `ReservationsController.cs` (roles), `AdminUsersController.cs` (nuevo), `AdminUserService.cs` (nuevo), `AdminUserDto.cs` (nuevo), `IUserRepository.cs` + `UserRepository.cs` (SearchAsync), `AdminUserServiceTests.cs` (nuevo, 5 tests).
- Frontend: `lib/redirect.ts` (nuevo), `app/login/page.tsx`, `app/registro/page.tsx`, `app/tutores/[id]/page.tsx`, `components/tutor/booking-engine.tsx`, `app/estudiante/*` (nuevo), `components/estudiante/student-subnav.tsx` (nuevo), `app/mis-tutorias/page.tsx` (redirect), `app/panel/page.tsx`, `components/landing/currency-toggle.tsx`, `app/admin/estudiantes/page.tsx` (nuevo), `hooks/use-admin-users.ts` (nuevo), `lib/api.ts`, `components/landing/mentors.tsx`.

## Resultado de gates

- Backend: build 0 errores · **149/149 tests** (5 nuevos).
- Frontend: lint 0 errores · tsc limpio · build 21 rutas (3 nuevas).

## Deudas / siguientes pasos

- Smoke manual E2E con servidores dev (pendiente al momento del cierre; gates automatizados en verde).
- Historial de movimientos del monedero (`GET /api/wallet/movements`) — fuera de alcance por decisión del usuario.
- El claim `email` no se mapea en `/api/auth/me` (deuda preexistente documentada en `lib/api.ts`).
- Versión: `0.11.0` → `0.12.0` (bump registrado en `.ai/version-changes.md`).
