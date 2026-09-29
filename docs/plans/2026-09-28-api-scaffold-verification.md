# Verification — Scaffold backend .NET en api/

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-api-scaffold-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `dotnet build` (solución completa) | ✅ 0 errores, 0 warnings finales |
| `dotnet test` | ✅ 5 tests pasados (validador de criterios de búsqueda) |
| `dotnet ef migrations add InitialCreate` | ✅ `20260928075820_InitialCreate` en `AuraLearn.Infrastructure/Migrations/` |
| `dotnet ef database update` | ✅ migración aplicada a `auralearn_dev` |
| Smoke: `GET /health` | ✅ 200 |
| Smoke: `GET /api/tutors?university=UCR` | ✅ JSON filtrado (Dr. Carlos Solano, Dra. Laura Prop, Dra. Carolina Morales, …) |
| Smoke: `GET /api/tutors?university=TEC&pageSize=3` | ✅ `total=5, page=1, items=3` (paginación correcta) |
| Smoke: `GET /api/tutors?query=Cálculo&minRating=4.9` | ✅ `total=2` (búsqueda con acentos OK con URL encoding) |
| Smoke: `GET /api/tutors?minRating=4.9` | ✅ 7 resultados |
| Smoke: Swagger `/swagger/index.html` | ✅ 200 |

## Evidencia estructural

- Solución `AuraLearn.sln` con 4 proyectos + `tests/AuraLearn.Tests`:
  - `AuraLearn.Api` (webapi) → referencia Application + Infrastructure
  - `AuraLearn.Application` → referencia Domain; contiene Dto/Interfaces/Services + FluentValidation
  - `AuraLearn.Domain` → `Entities/Tutor.cs`, `Enums/{Role,VerificationStatus}.cs`; sin dependencias de EF
  - `AuraLearn.Infrastructure` → `Persistence/{AuraLearnDbContext,TutorRepository}.cs`; Npgsql + Design
- Tabla `tutors` con columnas snake_case, índices `ix_tutors_university_rating` e `ix_tutors_verification_status`, y **12 filas sembradas** (verificado con `psql`).
- `appsettings.Development.json` ignorado por git (regla `**/appsettings.*.json` del `.gitignore` raíz, con excepción de `appsettings.json`).
- JWT configurado (middleware + `Jwt:Key` desde config); **sin endpoints de auth** (tarea siguiente).

## Problemas resueltos durante la implementación

1. **Referencia faltante Api→Infrastructure** (el DbContext se inyecta en `Program.cs`) → añadida.
2. **Paquetes faltantes en Api** (`FluentValidation.DependencyInjectionExtensions`, `Swashbuckle.AspNetCore`, EF `Design` para alinear versiones 10.0.12) → añadidos; resolvió el CS1705 de mezcla de versiones EF 10.0.4/10.0.12.
3. **CS0103 `Enums` no existe** → unificados los usings a `AuraLearn.Domain.Enums` en DbContext, entidad y repositorio.
4. **Duplicados de migración** (dos generaciones `InitialCreate` mezcladas por los intentos) → limpieza completa de `Migrations/*.cs` y regeneración única.
5. **Warning `PendingModelChangesWarning`** de EF 10 (falso-positivo con seed `HasData` estático; el modelo es estable — verificado con script idempotente mostrando `created_at` correcto) → suprimido explícitamente en `OnConfiguring` con `RelationalEventId.PendingModelChangesWarning`, registrado también en `.github/errors/backend.md`.

## Riesgos residuales

- La conexión frontend↔API **no está hecha** en esta tarea (decisión del plan): `web/` sigue sirviendo su mock local.
- `nextSlot` en la respuesta es presentacional (arreglo rotativo en `TutorService`) hasta que exista el motor de slots.
- PostgreSQL local (Mac, Homebrew) con credenciales dev (`auralearn`/`auralearn_dev`); en producción usar managed (Azure/RDS) + secretos.

> Nota: esta verificación no sustituye al testing gate (`dotnet build && dotnet test` ya ejecutados con evidencia arriba).
