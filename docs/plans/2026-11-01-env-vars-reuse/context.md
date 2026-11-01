# Contexto — Reutilizar las variables de entorno de Neon y Blob

**Fecha:** 2026-11-01

## Solicitud

El usuario ya tiene, inyectadas por la integración Neon ↔ Vercel, las variables
`DATABASE_URL`, `DATABASE_URL_UNPOOLED`, `PGHOST`, `PGUSER`, `PGDATABASE`,
`PGPASSWORD` y los alias `POSTGRES_*`; y para archivos, `BLOB_STORE_ID` +
`BLOB_READ_WRITE_TOKEN`. Pide que el API las use **tal cual** en vez de mantener
variables propias duplicadas (`ConnectionStrings__Default`).

Decisiones tomadas con el usuario:

1. Enfoque **código con fallback** (no referencias `$VAR` del panel de Vercel).
2. Migraciones por conexión **directa** (unpooled); runtime **pooled**.
3. `ConnectionStrings__Default` sigue siendo el override de mayor prioridad
   (dev local, docker-compose, GitHub Actions).

## Estado del código antes del cambio

- `Program.cs`: `options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))`.
  Única fuente: `ConnectionStrings__Default` o `appsettings.Development.json`.
- `VercelBlobFileStorage`: ctor `(token, access, httpClient)`; el store id se
  derivaba del token con `token.Split('_')[3]`, ignorando `BLOB_STORE_ID`.
- La factory de DI (`AddSingleton<IFileStorage>`) es **perezosa**: la validación
  de credenciales no ocurría al arrancar, sino en la primera subida (403 opacos).
- `api/tests/AuraLearn.Tests` no referenciaba `AuraLearn.Infrastructure`.

## Hallazgos verificados

| Hallazgo | Consecuencia |
|---|---|
| Npgsql **solo** acepta `keyword=value;` | La guía y el secret de CI usaban URIs `postgresql://…` → fallo al arrancar |
| Npgsql lee `PGUSER`/`PGPASSWORD` del entorno, pero **no** `PGHOST`/`PGDATABASE` | Hay que componer la cadena a mano |
| Neon exige TLS; el host pooled contiene `-pooler` | `SslMode=Require` automático cuando el host es `.neon.tech` |
| `BLOB_STORE_ID` llega como `store_<id>` | La URL del blob es `<id>.private.blob.vercel-storage.com`: hay que quitar el prefijo |
| `handleUpload` (web) exige `BLOB_READ_WRITE_TOKEN`; OIDC no firma client tokens | Se descarta `VERCEL_OIDC_TOKEN` para el API y para la ruta de subida |
| El token puede venir con o sin prefijo `store_` en su id | `Split('_')[3]` devolvía `store` en ese caso → parseo endurecido |
| `api/tests` referenciaba `Microsoft.AspNetCore.Identity` 2.3.13 (legacy) | Arrastraba `System.Security.Cryptography.Xml/Pkcs` con advisories NU1902/NU1903 |

## Fuera de alcance

- `web/`: no usa base de datos (sin Prisma/pg) y ya leía ambas variables de Blob.
- Rotación de secretos, cambio de proveedor de BD, referencias `$VAR` de Vercel.
