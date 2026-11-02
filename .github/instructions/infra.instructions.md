---
applyTo: ".github/workflows/**, **/Dockerfile, **/*.bicep, **/*.yaml, **/*.yml, **/docker-compose*"
description: Infraestructura, CI/CD y despliegue de AuraLearn (VPS/Azure/AWS)
---

# Infra / CI-CD — AuraLearn

## Topología de despliegue

| Componente | Destino |
|------------|---------|
| Frontend `web/` | **Vercel** (Next.js nativo, Root Directory `web`, `web/vercel.json`) |
| Backend `api/` | **Vercel Container Runtime** (Root Directory `api`, Framework Preset `Container`, construye `api/Dockerfile` — ADR-009) |
| PostgreSQL | **Neon** (connection string por variable de entorno) o Postgres en compose/VPS |
| Archivos (documentos/fotos) | **Vercel Blob** (store privado) vía `VercelBlobFileStorage`; disco local solo en desarrollo |
| Portal `portal/` (futuro) | Vercel/Netlify, independiente de `web/` |

> ⚠️ Vercel **sí** ejecuta contenedores (Container Runtime, Beta en todos los
> planes) — ADR-009 supersede la premisa contraria de ADR-008. Railway, Azure
> Container Apps y VPS con compose siguen siendo alternativas válidas; el
> `Dockerfile` es portable entre ellas.

## Reglas generales

1. **Secretos nunca en el repo.** CI usa secrets del proveedor (GitHub Actions Secrets / Vercel env); prod usa variables de entorno, Key Vault o equivalentes.
2. Configuración por entorno: `NODE_ENV`, `ASPNETCORE_ENVIRONMENT`; connection strings solo por variable de entorno.
3. Los workflows de GitHub Actions viven en `.github/workflows/`; pipelines declarativos, sin scripts imperfativos largos inline.
4. IaC (si aplica) en Bicep/Terraform versionado; los recursos efímeros se destruyen.
5. **Límite de 4.5 MB por request** en Vercel Functions: cualquier subida de
   archivos debe usar subida directa al store + `POST .../attach`, no multipart
   contra el API. No subir `MaxSizeBytes` sin revisar este límite.

## Backend (contenedor en Vercel — ADR-009)

- Imagen multi-stage `api/Dockerfile` (SDK 10 build → aspnet 10 runtime), usuario no-root, `entrypoint.sh` con `PORT` dinámico. No usar imágenes *chiseled*: no traen shell y el entrypoint lo necesita.
- **Definir `PORT=8080` en el proyecto de Vercel.** Vercel enruta a 80 por defecto mientras la base ASP.NET escucha en 8080 → 502 en todas las rutas. `EXPOSE` no influye en el routing.
- `ASPNETCORE_URLS` lo fija el entrypoint: es la única fuente de binding. `Program.cs` solo añade `app.Urls` como fallback si esa variable falta (evita doble registro en Kestrel).
- Migraciones: **no** se ejecutan en el arranque del servicio desplegado (Vercel no permite sobreescribir el comando). El job `migrate` de `.github/workflows/api-ci.yml` corre `ApplyMigrations=true` one-shot contra Neon. `Down()` + rollback manual comentado en cada migración.
- Variables del servicio: `Jwt__Key`, `Jwt__Issuer`, `Cors__Origins` (dominios separados por `;`), `BLOB_READ_WRITE_TOKEN`, `BLOB_STORE_ID`, `ASPNETCORE_ENVIRONMENT=Production`, `PORT=8080`.
- **La conexión a BD no se configura a mano:** el API resuelve la primera disponible entre `ConnectionStrings__Default` (override) → `DATABASE_URL` → `POSTGRES_URL` → `PGHOST`+`PGUSER`+`PGDATABASE`+`PGPASSWORD`, que son las que inyecta la integración Neon ↔ Vercel. Npgsql **no** acepta URIs `postgresql://`, así que la normalización a `keyword=value` la hace `DatabaseConnectionString`. En modo migraciones prefiere `DATABASE_URL_UNPOOLED` / `POSTGRES_URL_NON_POOLING` (DDL por PgBouncer es frágil).
- **Blob:** `BLOB_READ_WRITE_TOKEN` + `BLOB_STORE_ID` son los mismos nombres que usa `web/`; el id se acepta con o sin prefijo `store_` y si no coincide con el del token el arranque **falla** (fail-fast al validar la configuración, no en la primera subida). OIDC (`VERCEL_OIDC_TOKEN`) no sirve: `handleUpload` y el cliente REST .NET necesitan credencial de escritura.
- `Storage:Provider` (`Local` | `VercelBlob`) fuerza la implementación; sin él se infiere por la presencia de `BLOB_READ_WRITE_TOKEN` o `BLOB_STORE_ID`. Si se fuerza `VercelBlob` sin token, el arranque falla a propósito.
- Volumen/disco: el filesystem de Vercel es efímero (solo `/tmp`, 500 MB). Nunca guardar archivos ahí — usar Blob.
- Backups: Neon incluye PITR según plan; probar restauración al menos una vez.

## CI/CD mínimo esperado

- `web`: `.github/workflows/web-ci.yml` (lint + tsc + build en cada PR); deploy automático a preview en PR y a producción en merge a main vía integración Git de Vercel.
- `api`: `.github/workflows/api-ci.yml` — build + tests en cada PR; en push a main **o ejecución manual** (`workflow_dispatch`): build + tests + job `migrate` (migraciones one-shot contra Neon). El contenedor lo construye y despliega Vercel con su integración Git; el workflow **no** publica imágenes. Secretos: `NEON_CONNECTION_STRING` o `DATABASE_URL_UNPOOLED` (cualquiera de los dos, Repository o Environment del entorno `production`), inyectado como `DATABASE_URL_UNPOOLED`; debe ser la URL **directa** — el paso aborta si detecta `-pooler.` en el host, porque el DDL a través de PgBouncer cuelga el lock de `__EFMigrationsHistory`.

## Verificación

- Tras desplegar: healthcheck del backend, smoke del frontend (200 en rutas clave), y registro de la incidencia si algo falla.
- Health del backend: `/health` es **readiness** (proceso + PostgreSQL, `503` si la BD no responde) y `/health/live` es **liveness** (sin dependencias). Un monitor de proceso debe usar `/health/live` para no despertar Neon en cada ping.
- Cualquier cambio de infraestructura significativo → ADR + entrada en `.ai/STACK.md`.
