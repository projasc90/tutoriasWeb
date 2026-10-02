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

## ERR-BE-005 — Ciclo remove→add de migraciones deja la migración vacía (EF 10)

- **Nombre:** migración `add` tras `remove` contiene solo `UpdateData` del seed (sin `CreateTable`/`CreateIndex`)
- **Síntoma:** el archivo `*_AddSlotsAndReservations.cs` (188 líneas) no tiene ninguna `CreateTable`; la BD migra pero no crea tablas. Además `dotnet ef migrations remove` reporta `Done` sin borrar los archivos `.cs`/`.Designer.cs`.
- **Causa:** en EF 10.0.12, `migrations remove` revierte el snapshot pero a veces no elimina los archivos; el siguiente `add` parte del snapshot ya revertido y solo genera el diff de datos seed. Combinado con `remove --force` sobre una migración ya aplicada, se producen residuos.
- **Solución:** ciclo canónico: (1) `dotnet ef migrations remove --force`; (2) verificar con `ls` que no queden archivos de la migración — borrar manualmente los residuos; (3) `git checkout -- *ModelSnapshot.cs` solo si el remove corrompió el snapshot; (4) un único `migrations add`; (5) auditar el archivo con `grep -c CreateTable` + índices esperados ANTES de `database update`. Nunca asumir que `Done` = archivos limpiados.
- **Contexto:** 2026-10-01 — regeneración de `AddSlotsAndReservations` (Fase 0 del cierre de reservas); docenas de ciclos residuales depurados.

## ERR-BE-006 — ExecuteDelete de slots viola FK de reservas terminales (23503)

- **Nombre:** `NpgsqlException 23503: update or delete on table "slots" violates foreign key constraint "fk_reservations_slots_slot_id"`
- **Síntoma:** `GET /api/tutors/{id}/slots` responde 500 tras definir franjas de disponibilidad (la purga del ensure-ahead borra slots fuera de franjas).
- **Causa:** la FK `reservations → slots` es `onDelete: Restrict`; el filtro de purga excluía solo los slots con reserva **activa** (status IN (1,2)), pero los slots con reserva **terminal** (Expired/Cancelled/Rejected) también referencian el slot y bloquean el `DELETE`.
- **Solución:** la purga solo elimina slots **sin ninguna reserva** (ni activa ni terminal): `Except(slotsConReserva)` sin filtrar status. El historial se conserva (auditoría); un slot con reservas termina de existir lógicamente — deja de ser reservable porque las terminales no bloquean, y el horario futuro lo repueblan las franjas del generador.
- **Contexto:** 2026-10-01 — smoke del honoring (Fase 4 del cierre de reservas); detectado al probar la UI con el API levantado.
