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

## ERR-BE-007 — 42P01 `relation "tutors" does not exist` en el API desplegado

- **Nombre:** esquema ausente en la base a la que apunta el runtime
- **Síntoma:** `GET /api/tutors` responde 500 con `Npgsql.PostgresException 42P01: relation "tutors" does not exist`, mientras `/health` responde `200 Healthy`. El log de arranque dice `[auralearn] Conexión a PostgreSQL desde ConnectionStrings:Default`.
- **Causa:** `/health` solo ejecuta `SELECT 1`, así que valida la conectividad pero no el esquema. La base en uso no tenía migraciones aplicadas. Tres escenarios: (1) el comando manual de la guía corrió **sin** `ASPNETCORE_ENVIRONMENT=Production`, de modo que `dotnet run` cargó `appsettings.Development.json` y migró el Postgres **local** (`ConnectionStrings:Default` tiene prioridad sobre `DATABASE_URL_UNPOOLED`); (2) el job `migrate` no corrió o falló; (3) `ConnectionStrings__Default` en Vercel apunta a otra base/branch que la que se migra.
- **Solución:** ejecutar las migraciones con `ASPNETCORE_ENVIRONMENT=Production` y la URL **directa** de Neon, comprobando que el log diga `desde DATABASE_URL_UNPOOLED (<host>.neon.tech/<db>)`. En Vercel, **borrar** `ConnectionStrings__Default` para que gane `DATABASE_URL` de la integración (ADR-010). Verificar el esquema con `psql "<url>" -c '\dt'`. Desde 0.13.1 el arranque registra host/base y avisa si hay migraciones pendientes (`[auralearn] AVISO: N migraciones pendientes`).
- **Confirmación (2026-11-02):** con la integración de Neon conectada, el log pasó a `desde DATABASE_URL (ep-….neon.tech/neondb)` y el aviso `12 migraciones pendientes` señaló el problema sin esperar al 500. La causa restante fue que el job `migrate` no se había ejecutado: solo corre en `push` a `main` **con cambios en `api/**`**, así que un push que toca solo `web/` no lo dispara. Ahora el workflow tiene `workflow_dispatch` para lanzarlo a mano.
- **Contexto:** 2026-11-01 — primer despliegue real en Vercel Container.

## ERR-BE-008 — `libgssapi_krb5.so.2` no encontrado al conectar con Npgsql

- **Nombre:** `Cannot load library libgssapi_krb5.so.2` en cada conexión nueva
- **Síntoma:** en el log del contenedor aparece `Error: libgssapi_krb5.so.2: cannot open shared object file: No such file or directory` repetido por request. **No es fatal**: las consultas se ejecutan correctamente.
- **Causa:** Npgsql negocia cifrado GSSAPI por defecto (`GSS Encryption Mode=Prefer`) y la imagen `mcr.microsoft.com/dotnet/aspnet:10.0` no incluye esa librería. Neon y cualquier Postgres gestionado usan TLS + SCRAM, así que GSS no aporta nada aquí.
- **Solución:** desactivar el valor por defecto en el resolutor de conexión (`DatabaseConnectionString.DisableUnusedGss`): `if (builder.GssEncryptionMode == GssEncryptionMode.Prefer) builder.GssEncryptionMode = GssEncryptionMode.Disable;`. Se respeta `Require` si alguien lo pide. No instalar paquetes Kerberos en la imagen.
- **Contexto:** 2026-11-01 — log del primer deploy; verificado en tests (`DatabaseConnectionStringTests`).

## ERR-BE-009 — El job `migrate` recibe el secret vacío

- **Nombre:** `El secret NEON_CONNECTION_STRING está vacío` en el job `migrate` de `api-ci`
- **Síntoma:** el job arranca, compila y falla en el último paso en ~1 s con `::error::Secret ausente` (antes: `Process completed with exit code 1` sin más pistas). El repo sí tiene las migraciones en `api/Migrations/`.
- **Causa más frecuente:** el secret no existe con ese nombre. Otras: está creado en la pestaña **Variables** (`secrets.X` no lee `vars.X`), o en otro repo, o es un *Environment secret* de un entorno distinto al declarado por el job.
- **Solución:** crear `NEON_CONNECTION_STRING` **o** `DATABASE_URL_UNPOOLED` como *Repository secret* (también vale *Environment secret* del entorno `production`, que el job declara) con la URL **directa** de Neon. Diagnosticar con `gh secret list --repo <owner>/<repo>`. Reintentar con *Actions → api-ci → Run workflow* (`workflow_dispatch`), sin necesidad de un commit.
- **Related:** el paso valida además que el host no contenga `-pooler.` y aborta antes de tocar la BD (ver ERR-BE-007). Alternativa siempre disponible: migrar en local con `ASPNETCORE_ENVIRONMENT=Production` + `ApplyMigrations=true`.
- **Contexto:** 2026-11-02 — primer run manual del job tras desbloquear la facturación de Actions.
