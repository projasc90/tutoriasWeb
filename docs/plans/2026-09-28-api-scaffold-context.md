# Context — Scaffold backend .NET en api/

**Fecha:** 2026-09-28
**Plan asociado:** [plan](./2026-09-28-api-scaffold-plan.md)

## Alcance

Primera implementación del backend .NET en `api/` (hoy reservada con `.gitkeep`):

- Solución `AuraLearn.sln` con Clean Architecture: `AuraLearn.Api`, `AuraLearn.Application`, `AuraLearn.Domain`, `AuraLearn.Infrastructure`.
- EF Core + PostgreSQL (Npgsql) con migración inicial en `api/Migrations/` (rollback manual obligatorio).
- JWT configurado (middleware + appsettings) **sin endpoints de auth aún**.
- `GET /api/tutors` con filtros (query, university, minRating, precio) y paginación, sirviendo los 12 tutores del directorio vía seed.
- Healthcheck `/health`, Swagger en dev, CORS para `localhost:3000`.
- Proyecto de tests xUnit mínimo (`AuraLearn.Tests`).

## Decisiones

1. **.NET 10** (SDK 10.0.401 ya instalado en el sistema, verificado).
2. **Seed en `OnModelCreating` (HasData):** el endpoint sirve datos reales tras `dotnet ef database update`, sin depender de scripts externos.
3. **JWT sin endpoints de auth:** el middleware y la config quedan listos; el login es la tarea siguiente.
4. **El frontend NO se conecta en esta tarea:** separación de cambios; la integración será tarea propia (template `data-hook.md`).
5. **FluentValidation:** paquete moderno `FluentValidation` (no el deprecated `FluentValidation.AspNetCore`).
6. **Nombres snake_case en BD** (convención PostgreSQL) vía configuración de entidades.
7. **ADR-003** registrará la estructura de la solución.

## Restricciones

- Dependencias apuntan hacia `Domain` (Api→Application→Domain; Infrastructure→Application+Domain).
- Sin secretos en `appsettings.json`; connection string local en `appsettings.Development.json` (ignorado por git).
- Migración con `// ROLLBACK MANUAL` completo (regla del repo).
- Código en inglés, comentarios en español.

## Dudas cerradas

- **PostgreSQL local:** se verificará al llegar a la migración (Docker/Homebrew); si no hay servidor, se pedirá al usuario.
- **Puerto:** Kestrel en `http://localhost:5000` (coincide con el README del repo).
- **Paginación:** `page`/`pageSize` con límite máximo de 50 por página.
