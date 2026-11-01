# Plan — Reutilizar las variables de entorno de Neon y Blob

**Fecha:** 2026-11-01 · **ADR:** 010

## Objetivo

Que `api/` resuelva su configuración desde las variables estándar de los
proveedores, sin copias propias ni cambios manuales en el panel de Vercel.

## Diseño

### 1. Conexión a PostgreSQL — `Infrastructure/Configuration/DatabaseConnectionString.cs`

`Resolve(explicitConnectionString, preferUnpooled, read)` → `record(Value, Source)`.

Prioridad:

| # | Runtime | Modo migraciones (`ApplyMigrations=true`) |
|---|---|---|
| 1 | `ConnectionStrings__Default` | `ConnectionStrings__Default` |
| 2 | `DATABASE_URL` | `DATABASE_URL_UNPOOLED` |
| 3 | `POSTGRES_URL` | `POSTGRES_URL_NON_POOLING` |
| 4 | `PGHOST`+`PGPORT`+`PGDATABASE`+`PGUSER`+`PGPASSWORD` | `PGHOST_UNPOOLED`+`PG*` |
| 5 | — | cae a la cadena pooled (2–4) |

`Normalize` convierte URIs `postgres(ql)://` a `keyword=value` con
`NpgsqlConnectionStringBuilder`: decodifica usuario/contraseña (`Uri.UnescapeDataString`),
mapea `sslmode`, `channel_binding`, `connect_timeout`, `application_name`,
`options` y `search_path`, e **ignora** parámetros del proveedor (`pgbouncer`,
`supa`…). Nunca se registra el valor: solo `Source`.

### 2. Credenciales de Blob — `Infrastructure/Configuration/VercelBlobConfiguration.cs`

`Resolve(read)` → `record(Token, StoreId, Access)`.

- Token: `BLOB_READ_WRITE_TOKEN` (alias de compatibilidad `Storage:VercelBlobToken`).
- StoreId: `BLOB_STORE_ID` normalizado (`store_` opcional) → si falta, derivado
  del token cortando por el **último** `_` y tolerando el prefijo.
- Si ambos existen y **discrepan** → `InvalidOperationException` (los ids no son
  secretos, aparecen en las URLs: el mensaje los muestra).
- `VercelBlobFileStorage` recibe el store id explícito y delega en el mismo
  resolutor (`ResolveStoreId`), eliminando el `Split('_')[3]`.

### 3. Fail-fast en `Program.cs`

- `isMigrationMode` se calcula **antes** de `AddDbContext` (cambia la preferencia pooled/unpooled).
- Log de arranque: `[auralearn] Conexión a PostgreSQL desde <Source>`.
- `UsesBlobStorage(configuration)` + `VercelBlobConfiguration.Resolve` se ejecutan
  **antes** de `builder.Build()`, porque la factory de `AddSingleton<IFileStorage>`
  es perezosa y la configuración inválida solo aparecía en la primera subida.
- Log: `[auralearn] Archivos en Vercel Blob (store <id>)`.

### 4. Tests

- `api/tests` referencia `AuraLearn.Infrastructure`.
- 20 tests nuevos: `DatabaseConnectionStringTests` (13) y `VercelBlobConfigurationTests` (10).
- Alineación de versiones EF Core a 10.0.12 (elimina `MSB3277` que introdujo la
  nueva referencia) y sustitución de `Microsoft.AspNetCore.Identity` 2.3.13 por
  `Microsoft.Extensions.Identity.Core` 10.0.12 (elimina NU1902/NU1903).

## Riesgos y mitigación

| Riesgo | Mitigación |
|---|---|
| Un parámetro de la URI se ignora y la conexión falla | Mapas explícitos + `pgbouncer=true` no es opción de conexión; verificado contra Postgres real |
| `ConnectionStrings__Default` vieja en Vercel gana sobre `DATABASE_URL` | Guía de despliegue indica borrarla; el log de arranque muestra el origen |
| Cambio de comportamiento en dev local | Override tiene la máxima prioridad: `appsettings.Development.json` y compose siguen igual |
| El token no incluye el store id (formato futuro) | `BLOB_STORE_ID` es la fuente autoritativa; `Storage__Provider` sigue disponible |
