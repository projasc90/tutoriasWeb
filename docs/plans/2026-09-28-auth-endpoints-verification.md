# Verification — Auth endpoints (register/login/me) con JWT + roles

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-auth-endpoints-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `dotnet build` | ✅ 0 errores |
| `dotnet test` | ✅ **12/12 pasados** (5 previos + 7 nuevos de auth, escritos ANTES de la implementación) |
| `dotnet ef migrations add AddUsersTable` + `database update` | ✅ aplicada a `auralearn_dev` |

## Smoke (API en localhost:5037)

| Escenario | Resultado |
|-----------|-----------|
| `POST /api/auth/register` válido | ✅ 201 con `AuthResponse` (id, email, fullName, role, token) — sin hash expuesto |
| Register con email duplicado | ✅ 409 Conflict |
| Register con email inválido | ✅ 400 BadRequest |
| Register con password corta | ✅ 400 BadRequest |
| `POST /api/auth/login` válido | ✅ 200 con token JWT |
| Login con password incorrecta | ✅ 401 Unauthorized |
| Login con email desconocido | ✅ 401 Unauthorized |
| `GET /api/auth/me` con Bearer | ✅ 200 (id, role, fullName) |
| `/me` sin token | ✅ 401 |
| Claims del JWT decodificados | ✅ `sub` (id), `email`, `role: Estudiante`, `name`, `exp` (+8h), `iss`/`aud` AuraLearn.Api |
| SQL en logs | ✅ `SELECT ... FROM users WHERE email = @email LIMIT 1` + `INSERT INTO users` |

## Evidencia estructural

- **TDD real:** `AuthServiceTests.cs` (7 tests) creado antes que `AuthService`; cubre hashing, duplicado, validación, login válido/inválido, email desconocido.
- Hash de contraseñas: `PasswordHasher<User>` (PBKDF2, built-in de ASP.NET — sin dependencia nueva).
- Migración `20260929050529_AddUsersTable` con `// ROLLBACK MANUAL` completo + índice unique `ux_users_email` (verificado con `\d users`).
- Contraseñas jamás en respuestas; solo `AuthResponse` con token.

## Problemas resueltos durante la implementación

1. **CS0234 `Microsoft.AspNetCore` no existe en Application** → añadido `Microsoft.Extensions.Identity.Core` (IPasswordHasher sin traer todo Identity).
2. **CS9035 `required member PasswordHash`** → asignar en el inicializador (`PasswordHash = string.Empty`) y luego sobreescribir con el hash real.
3. **CS0234 `System.IdentityModel` en Infrastructure** → añadido paquete `System.IdentityModel.Tokens.Jwt`.
4. **Orden de inicialización en Program.cs** (jwtKey usado antes de declararse) → reordenado el wiring.
5. **`AddUsersTable` con UpdateData de subjects** → no-ops benignos de normalización del seed (documentado en el rollback manual de la migración).

## Riesgos residuales

- **Sin refresh tokens** (deuda documentada): el token expira a las 8h y no hay renovación.
- **Sin rate limiting en login** (protección brute-force pendiente — deuda de seguridad).
- `/me` devuelve id/role/fullName de claims; el email claim no se mapea automáticamente (deuda menor de ClaimTypes).
- El frontend aún no consume auth (tarea propia).

> Nota: esta verificación no sustituye al testing gate (`dotnet build && dotnet test` ejecutados con evidencia arriba).
