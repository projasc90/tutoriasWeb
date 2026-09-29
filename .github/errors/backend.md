# Catálogo de errores — Backend (api/)

> Se llenará al implementar el proyecto .NET. Cada error nuevo resuelto se agrega automáticamente aquí.
> Formato: nombre · mensaje o síntoma observable · causa · solución · contexto/fecha.

---

## ERR-BE-001 — Migración sin rollback manual (reserva preventiva)

- **Nombre:** migración EF sin sección de rollback manual
- **Síntoma:** al necesitar revertir en producción, no hay pasos documentados y solo existe el `Down()` del tooling.
- **Causa:** asumir que el rollback automático de EF es suficiente.
- **Solución:** toda migración incluye al final `// ROLLBACK MANUAL` con los pasos inversos (SQL/comandos); ver template `.github/templates/migration-with-rollback.md`. Nunca asumir rollback automático.
- **Contexto:** 2026-09-27 — regla establecida en la gobernanza inicial; primer caso real en `20260928075820_InitialCreate.cs` (2026-09-28).

## ERR-BE-002 — PendingModelChangesWarning bloquea `dotnet ef database update` (EF 10)

- **Nombre:** `PendingModelChangesWarning` con seed `HasData` estático
- **Síntoma:** `dotnet ef database update` falla con: *"The model for context 'AuraLearnDbContext' changes each time it is built. This is usually caused by dynamic values used in a 'HasData' call"* — incluso con seed 100% estático (Guid.Parse, DateTime fijo) y migración recién generada.
- **Causa:** falso-positivo conocido de EF Core 10: el warning se dispara cuando el modelo en runtime difiere del snapshot por detalles internos (p. ej. orden de propiedades o mapeo de colecciones), no solo por valores dinámicos. `dotnet ef migrations has-pending-model-changes` lo confirma aunque el modelo sea estable.
- **Solución:** verificar primero con `dotnet ef migrations script --idempotent` que el SQL generado es correcto (el modelo real es estable). Si es falso-positivo, suprimir explícitamente en el DbContext:
  ```csharp
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      => optionsBuilder.ConfigureWarnings(w =>
          w.Ignore(RelationalEventId.PendingModelChangesWarning));
  ```
  (requiere `using Microsoft.EntityFrameworkCore.Diagnostics;`). Si el warning es real (valores dinámicos en HasData), corregir el seed con valores estáticos.
- **Contexto:** 2026-09-28 — encontrado al aplicar `InitialCreate` del scaffold; el modelo era estable (script idempotente correcto con `created_at` mapeado).

## ERR-BE-003 — Migraciones duplicadas tras regeneración fallida

- **Nombre:** `CS0111: el tipo 'InitialCreate' ya define un miembro denominado 'Up'`
- **Síntoma:** el build falla con definiciones duplicadas de `Up`/`Down` en `Migrations/`; hay dos archivos `*_InitialCreate.cs` con timestamps distintos.
- **Causa:** regenerar la migración con `dotnet ef migrations add` cuando la anterior quedó a medias (snapshot desincronizado tras `migrations remove --force` + fallo de `database update`).
- **Solución:** limpiar **todos** los archivos de `Migrations/*.cs` (incluido `AuraLearnDbContextModelSnapshot.cs`) y regenerar una única vez: `rm -f Migrations/*.cs && dotnet ef migrations add <Nombre>`. Verificar con `dotnet build` antes de `database update`.
- **Contexto:** 2026-09-28 — durante el scaffold; tres generaciones de `InitialCreate` quedaron mezcladas.

## ERR-BE-004 — CS1705 mezcla de versiones de EF Core entre proyectos

- **Nombre:** `El ensamblado 'AuraLearn.Infrastructure' usa 'Microsoft.EntityFrameworkCore, Version=10.0.12.0' que tiene una versión superior a la referenciada (10.0.4.0)`
- **Síntoma:** el build del proyecto Api falla con CS1705 al consumir Infrastructure.
- **Causa:** `dotnet add package Microsoft.EntityFrameworkCore.Design` en Infrastructure trajo EF 10.0.12 transitivamente, mientras Api referenciaba EF 10.0.4 (versión base del runtime .NET 10.0.401).
- **Solución:** alinear las versiones añadiendo el mismo paquete `Microsoft.EntityFrameworkCore.Design 10.0.12` (con `PrivateAssets=all`) al proyecto Api, o fijar explícitamente la misma versión de EF en todos los csproj que la usen.
- **Contexto:** 2026-09-28 — durante el scaffold.
