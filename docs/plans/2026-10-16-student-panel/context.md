# Contexto — Panel estudiante + búsqueda pública + login en reserva (2026-10-16)

## Solicitud del usuario

1. Crear panel de administración de estudiantes.
2. La búsqueda de tutores debe funcionar sin estar en el login.
3. Al buscar y seleccionar, al momento de reservar el espacio debe solicitar el login para generar la sesión en el panel.

Aclaraciones del usuario (Q&A):
- "Panel de administración de estudiantes" = **ambos**: panel propio del estudiante + gestión de estudiantes en el panel Admin.
- Secciones del panel estudiante: **Reservas + Monedero** (sin historial de movimientos ni card de próxima sesión).
- UX de reserva sin sesión: **CTA + redirect de retorno** (`/login?redirect=...` con slot preservado).
- **Sí** restringir la creación de reservas al rol Estudiante en el backend.
- **Sí** conectar la sección Mentors de la landing a la API real.
- Además: **verificar las rutas del sitio de la página principal** (nav de la landing).

## Hallazgos de investigación (subagentes Explore)

### Ya existe (no rehacer)
- **Búsqueda pública**: `GET /api/tutors` y `GET /api/tutors/{id}` en `TutorsController` sin `[Authorize]`; `GET /api/tutors/{tutorId}/slots` con `[AllowAnonymous]` explícito. Frontend `/tutores` consume API real vía `useTutors`/`fetchTutors` sin token. **La búsqueda sin login ya funciona.**
- **Login obligatorio para reservar**: `ReservationsController` tiene `[Authorize]` a nivel de clase → `POST /api/reservations` sin token devuelve 401.
- **"Mis reservas"**: `GET /api/reservations` → `ReservationService.GetByStudentAsync`; frontend `fetchMyReservations` + `use-my-reservations.ts`.
- **Monedero**: `GET /api/wallet` → `WalletBalanceDto(BalanceCrc)`; frontend `fetchWallet`.
- **Roles**: `Role.cs` — `Estudiante = 1, Tutor = 2, Admin = 3`; registro siempre crea `Estudiante`.
- **Plantilla de panel**: `web/app/tutor/layout.tsx` + `TutorSubnav` (`web/components/tutor/profile/tutor-subnav.tsx`); guard client-side por página; `/panel` redirige por rol; `PanelMenu` (en `web/components/landing/currency-toggle.tsx`) construye menú por rol.

### Gaps a cubrir
| Gap | Dónde |
|---|---|
| Botón de reserva deshabilitado sin aviso ni CTA de login en `/tutores/[id]` | `web/app/tutores/[id]/page.tsx` + `web/components/tutor/booking-engine.tsx` |
| Login no retorna a la página de origen (push hardcodeado a `/tutores`) | `web/app/login/page.tsx`, `web/app/registro/page.tsx` |
| No existe panel del estudiante (solo `/mis-tutorias` lista plana) | nuevo `web/app/estudiante/` |
| No existe endpoint de listado de usuarios para Admin | nuevo `AdminUsersController` + `IUserRepository.SearchAsync` |
| Sección `Mentors` de la landing usa mock `MENTORS` | `web/components/landing/mentors.tsx`, `web/lib/data/mentors.ts` |
| Enlace "Para Universidades" → `#` (roto); `Logo` → `#` | `web/components/landing/currency-toggle.tsx` |
| Botón "Ver Perfil" de MentorCard sin navegación | `web/components/landing/mentors.tsx` |
| Cualquier usuario autenticado (Tutor/Admin) puede reservar | `ReservationsController.Create/SubmitComprobante/Cancel` |

### Datos clave para el mapeo Mentors → API
`TutorDto(Id, Name, Credentials, Specialization?, University, Rating, Reviews, Subjects[], PriceCrc, PriceUsd, Bio, Featured, NextSlotAt?, PhotoUrl?)`. Formateadores existentes: `formatSlotEsCr` (`web/lib/time.ts`), `formatCrc`. La landing (`web/app/page.tsx`) es Server Component → `Mentors` puede ser async server component con `fetchTutors({page:1, pageSize:4})`.

### Patrones de test
- `ReservationServiceTests.cs` construye el servicio a mano con substitutes (⚠️ actualizar también `ReservationCompletionTests.cs` si cambia el constructor).
- `AuthServiceTests.cs`: 7 `[Fact]`, mock de `IUserRepository`/`IJwtTokenService`.
- 144 tests actuales.

## Restricciones de gobernanza
- Backend: controllers finos, FluentValidation en inputs, `[Authorize]` por roles, async end-to-end con CancellationToken.
- Frontend: tokens MD3, `MaterialIcon`, estados `idle|loading|error|success`, `ApiResult<T>`, mensajes inline sin toasts, server component por defecto.
- Sin migración (no cambia esquema). Sin librerías nuevas.
