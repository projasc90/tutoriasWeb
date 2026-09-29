# Plan — Auth endpoints (register/login/me) con JWT + roles

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-auth-endpoints-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Entidad User (Domain) | `Domain/Entities/User.cs` | pendiente |
| 2 | **Tests FIRST** (TDD obligatorio) | `tests/AuraLearn.Tests/AuthServiceTests.cs` | pendiente |
| 3 | Application: DTOs + puertos + AuthService + validators | `Application/{Dto/AuthDto.cs,Interfaces/{IUserRepository,IJwtTokenService}.cs,Services/AuthService.cs}` | pendiente |
| 4 | Infrastructure: UserRepository + JwtTokenService | `Persistence/{UserRepository,JwtTokenService}.cs` | pendiente |
| 5 | Migración AddUsersTable + rollback manual | `Infrastructure/Migrations/` | pendiente |
| 6 | Api: AuthController + wiring | `Api/Controllers/AuthController.cs`, `Program.cs` | pendiente |
| 7 | Gate: build + test | — | pendiente |
| 8 | Smoke: register/login/me + errores | — | pendiente |
| 9 | Cierre: verification, summary, `.ai/` | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `Domain/Entities/User.cs`, `Application/Dto/AuthDto.cs`, `Application/Interfaces/{IUserRepository,IJwtTokenService}.cs`, `Application/Services/AuthService.cs`, `Infrastructure/Persistence/{UserRepository,JwtTokenService}.cs`, `Migrations/*_AddUsersTable.cs`, `Api/Controllers/AuthController.cs`, `tests/AuraLearn.Tests/AuthServiceTests.cs`
- **Modificados:** `Infrastructure/Persistence/AuraLearnDbContext.cs` (DbSet + config), `Api/Program.cs` (wiring)

## Criterios de aceptación

1. Tests de auth escritos **antes** de la implementación y verdes al final.
2. `register` → 201 con AuthResponse (sin hash); email duplicado → 409.
3. `login` válido → 200 con JWT (claims sub/email/role); password incorrecta → 401.
4. `me` con Bearer válido → 200 con datos del usuario; sin token → 401.
5. Migración con `// ROLLBACK MANUAL` + unique index en email.
6. `dotnet build` 0 errores; `dotnet test` verdes.
