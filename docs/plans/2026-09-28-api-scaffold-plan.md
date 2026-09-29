# Plan — Scaffold backend .NET en api/

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-api-scaffold-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Solución + 4 proyectos + referencias | `api/AuraLearn.sln`, 4 `.csproj` | pendiente |
| 2 | Paquetes NuGet | csproj de cada proyecto | pendiente |
| 3 | Domain: entidades y enums | `Domain/Entities/Tutor.cs`, `Domain/Enums/{Role,VerificationStatus}.cs` | pendiente |
| 4 | Infrastructure: DbContext | `Infrastructure/Persistence/AuraLearnDbContext.cs` | pendiente |
| 5 | Migración inicial con rollback manual | `api/Migrations/` | pendiente |
| 6 | Application: DTO + interfaz + servicio | `Application/{Dto,Interfaces,Services}/` | pendiente |
| 7 | Api: Program.cs + controller + appsettings | `Api/{Program.cs,Controllers/,appsettings*.json}` | pendiente |
| 8 | Tests xUnit mínimo | `api/tests/AuraLearn.Tests/` | pendiente |
| 9 | Gate: `dotnet build && dotnet test` | — | pendiente |
| 10 | Smoke: run + curl | — | pendiente |
| 11 | Cierre: verification, summary, ADR-003, `.ai/` | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `api/AuraLearn.sln`, `api/AuraLearn.Api/**`, `api/AuraLearn.Application/**`, `api/AuraLearn.Domain/**`, `api/AuraLearn.Infrastructure/**`, `api/tests/AuraLearn.Tests/**`, `api/Migrations/*_InitialCreate.cs`
- **Eliminado:** `api/.gitkeep` (la carpeta ya no está vacía)
- **Actualizados (cierre):** `.ai/STACK.md` (versiones reales del csproj), `.ai/PROJECT_MAP.md`, `.ai/ADR_LOG.md` (ADR-003), `.ai/CURRENT_STATE.md`, `.ai/ACTIVE_MEMORY.md`, `.ai/version-changes.md`

## Criterios de aceptación

1. `cd api && dotnet build` → 0 errores; `dotnet test` → verdes.
2. `dotnet run` → `/health` responde 200; `GET /api/tutors?university=UCR` devuelve JSON filtrado con paginación.
3. Migración `InitialCreate` con sección `// ROLLBACK MANUAL` completa.
4. Sin secretos en `appsettings.json`; `appsettings.Development.json` ignorado por git.
5. ADR-003 registrado; `.ai/STACK.md` con versiones reales de los paquetes instalados.
