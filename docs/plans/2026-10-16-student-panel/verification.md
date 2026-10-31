# Verificación — Panel estudiante + búsqueda pública + login en reserva (2026-10-16)

## Gates

| Gate | Comando | Resultado |
|---|---|---|
| Backend build | `cd api && dotnet build` | ✅ 0 errores, 0 advertencias |
| Backend tests | `cd api && dotnet test` | ✅ 149/149 (144 previos + 5 nuevos de `AdminUserServiceTests`) |
| Frontend lint | `cd web && npm run lint` | ✅ 0 errores (10 warnings preexistentes) |
| Frontend tipos | `npx tsc --noEmit` | ✅ limpio |
| Frontend build | `npm run build` | ✅ 21 rutas (3 nuevas: `/admin/estudiantes`, `/estudiante/reservas`, `/estudiante/monedero`) |

## Correcciones durante la verificación

1. `AdminUsersController` sin `using AuraLearn.Application.Interfaces` → CS0246; añadido.
2. Test `AdminUserServiceTests`: `[Theory]` sin `Task` → añadido.
3. ESLint `react-hooks/set-state-in-effect` (6 errores): setState síncrono en effects de `login`, `registro`, `tutores/[id]` (×2) y `use-admin-users` → patrón IIFE async con `await Promise.resolve()` (GOTCHA-008) o setState dentro del `setTimeout` del debounce.
4. `@next/next/no-html-link-for-pages`: `Logo` con `<a href="/">` → `<Link href="/">`.
5. `/` prerenderizada estática con fetch server-side congelaba los tutores destacados en build-time → `Mentors` convertido a client component (fetch en runtime, skeleton de carga).

## Cambios por fase

### Fase A — Backend
- `ReservationsController`: `[Authorize(Roles = "Estudiante")]` en `Create`, `SubmitComprobante`, `Cancel`.
- `AdminUserDto`, `AdminUserSearchCriteria`, `AdminUserSearchCriteriaValidator` (MaxPageSize 100), `AdminUserService`, `AdminUsersController` (`GET /api/admin/users?query&role&page&pageSize`).
- `IUserRepository.SearchAsync` + implementación EF (`AsNoTracking`, filtro rol, query sobre Email/FullName lowercase, orden CreatedAt desc, paginado).
- Sin migración (no cambia esquema).

### Fase B — Login con retorno + CTA
- `web/lib/redirect.ts`: `readInternalRedirect` (solo rutas internas, anti open-redirect) + `DEFAULT_LOGIN_REDIRECT`.
- `/login` y `/registro`: leen `?redirect=`, aviso contextual "Inicia sesión para continuar con tu reserva", push al redirect tras éxito, el redirect se propaga al link de registro.
- `/tutores/[id]`: banner + CTA "Inicia sesión para reservar" cuando `authState.status === "anonymous"`; el CTA navega a `/login?redirect=/tutores/{id}&slot={slotId}`; al retornar, el slot del query se preselecciona cuando el catálogo carga.

### Fase C — Panel estudiante
- `web/app/estudiante/layout.tsx` + `StudentSubnav` (Reservas · Monedero).
- `/estudiante/reservas`: contenido migrado de `/mis-tutorias` (cards, modal de cancelación con `noticeHoras` de `booking_rules`, reprogramación).
- `/estudiante/monedero`: saldo + explicación de créditos de cancelación.
- `/mis-tutorias` → redirect a `/estudiante/reservas`; `/panel` Estudiante → `/estudiante/reservas`; `PanelMenu` Estudiante con 2 items; enlaces internos actualizados (`tutor/disponibilidad`, `checkout/terminal-views`, `tutores/[id]` post-pago con saldo).

### Fase D — Admin estudiantes
- `fetchAdminUsers` en `web/lib/api.ts` (401/403 con mensajes accionables).
- `useAdminUsers` (patrón `use-tutors`: debounce 300 ms, AbortController, reload).
- `/admin/estudiantes`: guard Admin, búsqueda con debounce, filtro por rol (Todos/Estudiante/Tutor/Admin), paginación, estados loading/error/empty. Entrada en `PanelMenu` Admin.

### Fase E — Landing + nav
- `Mentors` con datos reales (`fetchTutors`, 4 destacados): foto o iniciales con gradiente, materias como badges, `nextSlotAt` con `formatSlotEsCr`, precio con `formatCrc`, "Ver Perfil" → `Link` a `/tutores/{id}` (antes sin navegación). Mock `MENTORS` archivado en `.ai/archive/`.
- Nav: eliminado "Para Universidades" (`#` roto); `Logo` → `/`.

## Pendiente (smoke manual)
- Flujo completo con servidores dev: buscar sin login → slot → CTA → login → retorno con slot → reserva → checkout SINPE.
- `/admin/estudiantes` con usuario Admin real (requiere `UPDATE users SET role=3` + re-login; ver gotcha en memoria del repo).
