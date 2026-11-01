# Verificación — Reutilizar las variables de entorno de Neon y Blob

**Fecha:** 2026-11-01

## Testing gate

```bash
cd api && dotnet build && dotnet test
```

| Comprobación | Resultado |
|---|---|
| `dotnet build` | ✅ 0 warnings, 0 errores |
| `dotnet test` | ✅ **181/181** (161 previos + 20 nuevos) |

## Smoke de resolución de conexión

Ejecutado con el binario compilado y `--no-launch-profile` (para que apliquen las
variables de entorno como en el contenedor), contra el Postgres local
(`auralearn_dev`).

| Caso | Variables | Log de arranque | `/health` | `/api/tutors` |
|---|---|---|---|---|
| 1 | `PGHOST`+`PGPORT`+`PGUSER`+`PGDATABASE`+`PGPASSWORD` | `… desde PGHOST/PGUSER/PGDATABASE` | `200 Healthy` | 200 con datos |
| 2 | `DATABASE_URL=postgresql://…@localhost:5432/auralearn_dev` | `… desde DATABASE_URL` | `200 Healthy` | 200 con datos |
| 3 | Sobrescritura explícita (dev local) | `… desde ConnectionStrings:Default` | — | — |

`/health/live` respondió `200` en ambos casos (liveness sin tocar la BD).

## Smoke de credenciales de Blob

| Caso | Variables | Resultado |
|---|---|---|
| Token + store coincidentes | `BLOB_READ_WRITE_TOKEN=vercel_blob_rw_BJe6tww3JlHPu28Y_…` y `BLOB_STORE_ID=store_BJe6tww3JlHPu28Y` | Arranca y registra `Archivos en Vercel Blob (store BJe6tww3JlHPu28Y)` — prefijo `store_` eliminado |
| Store distinto al del token | `BLOB_STORE_ID=store_OtroStoreDistinto` + token de `BJe6…` | ❌ Aborta el **arranque** con `BLOB_STORE_ID (OtroStoreDistinto) no coincide con el store del BLOB_READ_WRITE_TOKEN (BJe6tww3JlHPu28Y)` |
| Solo `BLOB_STORE_ID`, sin token | — | `InvalidOperationException` que nombra `BLOB_READ_WRITE_TOKEN` y aclara que OIDC no basta |

> Los smokes usaron un token ficticio con el mismo store id: valida el parseo y
> el fail-fast **sin** exponer el secreto real de `.env.local`.

## Casos cubiertos por tests unitarios (20 nuevos)

- Prioridad de fuentes (`ConnectionStrings__Default` > `DATABASE_URL` > `POSTGRES_URL` > `PG*`).
- URI de Neon: `sslmode`, `channel_binding`, password URL-encoded (`%40`/`%3B`), puerto por defecto, `pgbouncer=true` ignorado.
- Modo migraciones: prefiere `DATABASE_URL_UNPOOLED`, acepta `POSTGRES_URL_NON_POOLING`, usa `PGHOST_UNPOOLED`, cae a pooled si no hay directa.
- `SslMode=Require` automático con host `.neon.tech`; `Prefer` en local.
- Fallo rápido sin fuentes; mensaje que nombra las variables y no filtra valores.
- Store id con y sin `store_`, token con y sin prefijo en el id, discrepancia, token con formato inválido, modo del store.

## Pendiente de verificación manual (requiere credenciales reales)

- [ ] `/health` en el proyecto Vercel `api` con las variables de la integración Neon (sin `ConnectionStrings__Default`).
- [ ] Subida real de un PDF por la ruta de subida directa y `GET …/download` como Admin.
- [ ] Job `migrate` con `NEON_CONNECTION_STRING` apuntando a la URL **directa**.
