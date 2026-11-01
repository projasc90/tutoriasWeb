# Summary — Reutilizar las variables de entorno de Neon y Blob

**Fecha:** 2026-11-01 · **Versión:** 0.13.1 · **ADR:** 010

## Qué se hizo

1. **Conexión a BD sin variables propias** (`Infrastructure/Configuration/DatabaseConnectionString.cs`).
   El API resuelve la primera fuente disponible entre `ConnectionStrings__Default`
   (override), `DATABASE_URL` / `POSTGRES_URL` (pooled) y `PGHOST`+`PGUSER`+`PGDATABASE`+`PGPASSWORD`,
   y en modo migraciones prefiere `DATABASE_URL_UNPOOLED` / `POSTGRES_URL_NON_POOLING`.
   Normaliza las URIs `postgresql://` de Neon a `keyword=value` (Npgsql no las
   acepta), decodifica usuario/contraseña, mapea `sslmode`, `channel_binding`,
   `connect_timeout`, `application_name`, `options` y `search_path`, e ignora
   parámetros del proveedor. `SslMode=Require` automático en hosts `.neon.tech`.

2. **Credenciales de Blob con los nombres de Vercel** (`Infrastructure/Configuration/VercelBlobConfiguration.cs`).
   `BLOB_READ_WRITE_TOKEN` + `BLOB_STORE_ID`, los mismos que `web/.env.local`. El
   store id se normaliza (acepta `store_<id>`) y pasa a ser la fuente autoritativa;
   si discrepa con el del token el arranque falla con ambos ids en el mensaje.
   `VercelBlobFileStorage` recibe el id explícito y abandona el `Split('_')[3]`
   que devolvía `store` con tokens que incluyen el prefijo.

3. **Fail-fast real.** La factory de `AddSingleton<IFileStorage>` es perezosa: la
   validación se movió antes de `builder.Build()`, así una credencial inválida
   detiene el despliegue en vez de aparecer como 403 en la primera subida
   (comportamiento anterior, comprobado). El log de arranque indica el origen de
   la conexión (`[auralearn] Conexión a PostgreSQL desde DATABASE_URL`) y el store
   de Blob, sin exponer valores.

4. **Tests.** `api/tests` ahora referencia `AuraLearn.Infrastructure`; **20 tests
   nuevos** (181 en total). Se alineó EF Core a 10.0.12 (el `MSB3277` que
   introdujo la referencia) y se cambió `Microsoft.AspNetCore.Identity` 2.3.13 por
   `Microsoft.Extensions.Identity.Core` 10.0.12, eliminando los advisories
   NU1902/NU1903 que arrastraba. `dotnet build` queda en **0 warnings**.

5. **Documentación.** ADR-010; `deploy-guide.md` (variables del proyecto `api` sin
   `ConnectionStrings__Default`, secret de CI con URL directa, nota de borrar la
   variable antigua); `infra.instructions.md`; `api/Dockerfile`; `web/.env.example`.

## Resultado

- Un único lugar donde configurar la BD (la integración de Neon) y el store de
  archivos (la conexión del Blob store): ni `ConnectionStrings__Default` ni
  variables propias son necesarias en producción.
- Dev local, `docker-compose` y CI siguen funcionando igual: el override sigue
  teniendo la máxima prioridad.
- Corregido de paso un bug latente: la guía pedía pasar una URI `postgresql://`
  a Npgsql, que la rechaza.

## Deudas y advertencias

- `VERCEL_OIDC_TOKEN` no se usa: `handleUpload` y el cliente REST .NET necesitan
  credencial de escritura. Migrar requiere `handleUploadPresigned` + emisión de
  URLs firmadas en .NET.
- `BLOB_READ_WRITE_TOKEN` sigue siendo estático de larga vida; su rotación rompe
  subidas en curso.
- El log de arranque menciona el store id (público, aparece en las URLs) pero
  nunca el token ni la cadena de conexión.
