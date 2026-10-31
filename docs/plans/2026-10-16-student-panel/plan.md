# Plan — Panel estudiante + búsqueda pública + login en reserva (2026-10-16)

## Fase A — Backend (`api/`)
1. **Restringir reservas a Estudiante**: `[Authorize(Roles = "Estudiante")]` en `Create`, `SubmitComprobante`, `Cancel` de `ReservationsController`. `GetMine`/`GetById` quedan `[Authorize]` general; `GetTutorSessions` ya es Tutor.
2. **Listado admin de estudiantes**:
   - `AdminUserDto(Id, Email, FullName, Role, CreatedAt)` en `Application/Dto/`.
   - `IUserRepository.SearchAsync(Role? role, string? query, int page, int pageSize, CancellationToken)` → `PagedResult<AdminUserDto>`; implementar en `UserRepository` (EF `AsNoTracking`, filtro rol + query sobre Email/FullName, orden CreatedAt desc).
   - Nuevo `AdminUsersController` → `GET /api/admin/users` `[Authorize(Roles = "Admin")]` (patrón `AdminTutorApplicationsController`).
   - Tests TDD-ligeros con substitutes (patrón `AuthServiceTests`). Sin migración.

## Fase B — Frontend: login con retorno + CTA de reserva (`web/`)
3. **`/login` y `/registro` con `?redirect=`**: leer `useSearchParams()`, validar ruta interna (`/...` y no `//`), tras éxito `router.push(redirect || "/tutores")`.
4. **CTA de reserva en `/tutores/[id]`**: si `authState.status === "anonymous"` → banner "Inicia sesión para reservar" con botón a `/login?redirect=/tutores/{id}&slot={slotId}`; al retornar, auto-seleccionar el slot desde el query. Mensaje inline, tokens MD3.

## Fase C — Panel del estudiante (`web/`)
5. `web/app/estudiante/layout.tsx` (SiteHeader + StudentSubnav) + `web/components/estudiante/student-subnav.tsx` (espejo de `tutor-subnav.tsx`).
6. `/estudiante/reservas`: migra el contenido de `/mis-tutorias` (cards, modal de cancelación con `noticeHoras`, reprogramación). `/estudiante/monedero`: saldo + explicación de créditos de cancelación. Guard client-side por rol Estudiante.
7. `/mis-tutorias` → redirect a `/estudiante/reservas`; `/panel` Estudiante → `/estudiante/reservas`; `PanelMenu` Estudiante con 2 items.

## Fase D — Panel Admin de estudiantes (`web/`)
8. `fetchAdminUsers(params, token)` en `web/lib/api.ts` + hook `useAdminUsers` (patrón `use-tutors.ts`).
9. `web/app/admin/estudiantes/page.tsx` (patrón `/admin/tutores`: badge, guard, búsqueda con debounce, listado paginado) + entrada en `PanelMenu` Admin.

## Fase E — Landing: Mentors real + rutas
10. `Mentors` async server component con `fetchTutors({page:1, pageSize:4})`; mapeo `Tutor`→card con `formatSlotEsCr`/`formatCrc`, foto si `photoUrl`; "Ver Perfil" → `Link` a `/tutores/{id}`. Archivar mock `MENTORS`.
11. Corregir "Para Universidades" (`#`) y `Logo` (`#`) en `currency-toggle.tsx`; validar `NAV_ITEMS`.

## Fase F — Verificación
1. `cd api && dotnet build && dotnet test`
2. `cd web && npm run lint && npx tsc --noEmit` + `npm run build`
3. Smoke manual: búsqueda sin login → slot → CTA login → retorno con slot → reserva → checkout; admin lista estudiantes; landing con tutores reales.

## Criterios de aceptación
- Buscar tutores y ver detalle/slots sin sesión (ya funciona; no romperlo).
- Reservar sin sesión muestra CTA claro; tras login retorna a `/tutores/{id}` con el slot preseleccionado.
- Panel del estudiante con Reservas + Monedero accesible desde `/panel` y `PanelMenu`.
- Admin puede listar/buscar estudiantes (`GET /api/admin/users` + `/admin/estudiantes`).
- Solo rol Estudiante puede crear/submitir/cancelar reservas (403 para otros roles).
- Landing muestra tutores reales; nav sin enlaces rotos.
- Gates verdes: `dotnet build && dotnet test`, `npm run lint && npx tsc --noEmit`, `npm run build`.
