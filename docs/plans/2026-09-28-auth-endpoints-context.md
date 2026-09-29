# Context — Auth endpoints (register/login/me) con JWT + roles

**Fecha:** 2026-09-28
**Plan asociado:** [plan](./2026-09-28-auth-endpoints-plan.md)

## Alcance

Primera implementación de autenticación del backend .NET:

- Entidad `User` (email único, hash de contraseña, rol).
- `POST /api/auth/register` — alta de usuario (rol Estudiante por defecto).
- `POST /api/auth/login` — emisión de JWT con claims (sub, email, role).
- `GET /api/auth/me` — datos del usuario autenticado (protegido con Bearer).
- Migración `AddUsersTable` con rollback manual + índice único en email.
- **TDD obligatorio (test-first)**: dominio auth/seguridad según `PROTOCOLS.md` §3.

## Decisiones

1. **Hash de contraseñas:** `PasswordHasher<User>` de ASP.NET Core (built-in, PBKDF2). Sin dependencias nuevas (regla del repo: no librerías sin ADR).
2. **JWT:** reutiliza la config ya existente en `Program.cs` (`Jwt:Key`, issuer). Claims: `sub` (id), `email`, `role` (string del enum).
3. **Roles:** `Role.Estudiante` por defecto en register; `Tutor`/`Admin` se asignan por flujos posteriores (verificación de tutores, admin).
4. **Sin refresh tokens** en esta tarea (deuda documentada).
5. **El frontend NO se conecta** en esta tarea (auth UI será tarea propia).
6. **Rate limiting en login:** pendiente (protección brute-force) — deuda de seguridad documentada.

## Restricciones

- Validación con FluentValidation (email formato, password mínimo 8, nombre requerido).
- Errores: 409 email duplicado, 401 credenciales inválidas, 400 validación.
- Nunca devolver el hash de contraseña en respuestas.
- Código en inglés, comentarios en español.

## Dudas cerradas

- **Paquete JWT:** `System.IdentityModel.Tokens.Jwt` (viene transitivo con JwtBearer ya instalado; verificar).
- **Puerto de smoke:** `localhost:5037` (launchSettings).
- **Bump:** backend no usa SEMVER propio; se registra en `version-changes.md` como `feat` de la v0.2.x.
