# Summary — Auth endpoints (register/login/me) con JWT + roles

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-auth-endpoints-plan.md)

## Qué cambió

- **Domain:** `Entities/User.cs` (email único, PasswordHash, Role con default Estudiante).
- **Application:** `Dto/AuthDto.cs` (RegisterRequest, LoginRequest, AuthResponse), `Interfaces/IUserRepository.cs` (+`IJwtTokenService`), `Services/AuthService.cs` (+`RegisterRequestValidator`, `AuthResult<T>`).
- **Infrastructure:** `Persistence/UserRepository.cs`, `Persistence/JwtTokenService.cs` (claims sub/email/role/name, exp 8h).
- **Api:** `Controllers/AuthController.cs` (register/login/me) + wiring en `Program.cs` (PasswordHasher, AuthService, JwtTokenService).
- **Migración:** `20260929050529_AddUsersTable` con rollback manual + índice unique en email; aplicada a `auralearn_dev`.
- **Tests:** `AuthServiceTests.cs` — 7 tests escritos **antes** de la implementación (TDD obligatorio en auth).
- **Paquetes nuevos:** `Microsoft.Extensions.Identity.Core` (Application), `System.IdentityModel.Tokens.Jwt` (Infrastructure), `NSubstitute` (tests).

## Qué NO cambió

- El frontend no consume auth todavía (UI de login/registro será tarea propia).
- Sin refresh tokens; sin rate limiting en login; sin verificación de email.
- `GET /api/tutors` sigue público (catálogo sin auth), como está diseñado.

## Endpoints disponibles

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/register` | — | Alta de usuario (rol Estudiante) + JWT |
| POST | `/api/auth/login` | — | Autenticación + JWT |
| GET | `/api/auth/me` | Bearer | Datos del usuario autenticado |

## Deuda / próximos pasos

1. **Refresh tokens** con rotación (seguridad).
2. **Rate limiting en login** (protección brute-force — deuda de seguridad).
3. **Auth UI en el frontend** (login/registro + guard de rutas + contexto de sesión).
4. Mapear el claim `email` en `/me` (minor).
5. Flujo de verificación de tutores (skill `tutor-verification`) para asignar rol Tutor.

## Verificación

Ver [verification](./2026-09-28-auth-endpoints-verification.md): build ✅, 12/12 tests ✅, smoke completo (201/409/400/200/401) con claims JWT decodificados.
