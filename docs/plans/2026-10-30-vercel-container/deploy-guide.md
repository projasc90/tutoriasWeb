# Despliegue — API .NET en Vercel Container + web + Neon + Vercel Blob

> Guía vigente (ADR-009). Reemplaza `docs/plans/2026-10-16-deploy-containers/deploy-guide.md`.

## Arquitectura

```
┌──────────────────────────┐      HTTPS      ┌───────────────────────────┐
│ Vercel · proyecto "web"  │ ──────────────► │ Vercel · proyecto "api"   │
│ Next.js (Root: web/)     │                 │ ASP.NET Core (Root: api/) │
│                          │                 │ Container Runtime         │
│  /api/blob/upload        │                 └─────────────┬─────────────┘
│  (firma tokens de subida)│                               │ Npgsql/SSL
└────────────┬─────────────┘                               ▼
             │ subida directa                     ┌──────────────────┐
             ▼                                    │ Neon (Postgres)  │
   ┌──────────────────────┐                       └──────────────────┘
   │ Vercel Blob (privado)│ ◄──── lectura vía API (PII solo Admin)
   └──────────────────────┘
```

**Por qué así:** Vercel Functions limita el cuerpo de cada request a **4.5 MB** y
nuestros documentos admiten 5 MB. El navegador sube el archivo directo al store
y el API solo recibe la referencia. Además el filesystem de Vercel es efímero:
los archivos no pueden vivir en disco.

## Paso 0 — Prerrequisitos

- Repo **personal** en GitHub (Hobby no conecta repos de organizaciones).
- Cuenta de Vercel con acceso al repo.
- Base Neon con la connection string **pooled** (`...-pooler...?sslmode=require`).

## Paso 1 — Store de Vercel Blob

1. En el proyecto **web** (se crea en el paso 3): *Storage → Create → Blob*.
2. Nombre `auralearn-docs`, acceso **Private**.
   ⚠️ El modo de acceso **no se puede cambiar** después de crear el store.
3. Conectar el store a **ambos** proyectos (`web` y `api`), entornos
   *Production*, *Preview* y *Development*.
4. Anotar `BLOB_READ_WRITE_TOKEN` (formato `vercel_blob_rw_<storeId>_<secreto>`).
   Al conectar el store al proyecto, Vercel también inyecta `BLOB_STORE_ID`
   (`store_<id>`): el API lo normaliza y lo usa en la URL del blob. Son los
   mismos nombres que el frontend, así que no hay variables duplicadas.

> Un único store privado basta: las fotos de perfil también se sirven a través
> del API (`GET /api/tutors/{id}/photo`), no por URL pública directa.

## Paso 2 — Proyecto Vercel `api`

1. *Add New Project* → importar el repo.
2. **Root Directory = `api`**.
3. **Framework Preset = `Container`**. Si Vercel detectó otro, ejecutar
   `vercel project update --framework container --yes` o renombrar el archivo a
   `Dockerfile.vercel`.
4. Variables de entorno (Production y Preview):

| Variable | Valor |
|---|---|
| `PORT` | `8080` ← **crítico**; sin esto todas las rutas devuelven 502 |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Jwt__Key` | `openssl rand -base64 48` |
| `Jwt__Issuer` | `AuraLearn.Api` |
| `Cors__Origins` | `https://<proyecto-web>.vercel.app` (separar con `;` si hay más) |
| `BLOB_STORE_ID` | id del store del paso 1 (conectar el store lo inyecta solo) |
| `BLOB_READ_WRITE_TOKEN` | token del paso 1 |
| `Storage__Provider` | `VercelBlob` (opcional; sin él se infiere del token) |

> **La conexión a Neon no se configura a mano.** Al conectar la integración de
> Neon al proyecto, Vercel inyecta `DATABASE_URL` (pooled) y `DATABASE_URL_UNPOOLED`
> además de `PGHOST`/`PGUSER`/`PGDATABASE`/`PGPASSWORD`. El API toma la primera que
> exista y la normaliza a `keyword=value` (Npgsql no acepta URIs). `ConnectionStrings__Default`
> sigue funcionando como override de mayor prioridad si algún día hace falta
> apuntar a otra base: no hace falta definirla para producción.
>
> Si el proyecto ya tiene una `ConnectionStrings__Default` antigua, **bórrala**:
> gana sobre `DATABASE_URL` y puede apuntar a una base obsoleta.

5. Deploy y verificar: `curl https://<api>.vercel.app/health` → `Healthy`.

> **Dos rutas de salud:**
> - `/health` → *readiness*: proceso vivo **y** PostgreSQL accesible (`503` si la BD no responde).
> - `/health/live` → *liveness*: solo el proceso responde, sin tocar la BD.
>
> Usa `/health/live` para un monitor que solo vigila el proceso (no despierta
> Neon) y `/health` para decidir si la instancia puede servir tráfico.

> El contenedor escucha en `8080` porque la base ASP.NET fija
> `ASPNETCORE_HTTP_PORTS=8080`, mientras Vercel enruta a `80` por defecto.
> `EXPOSE` en el Dockerfile no influye en el routing.
>
> **Sobre el usuario del contenedor:** el Dockerfile usa `USER app`, el usuario
> no-root que ya traen las imágenes oficiales de .NET 8+ (UID 1654). No crear
> usuarios con `adduser`/`addgroup`: ese paquete no está en las imágenes slim y
> el build falla con `addgroup: not found` (exit 127) — ver GOTCHA-013.

## Paso 3 — Proyecto Vercel `web`

1. *Add New Project* → mismo repo, **Root Directory = `web`**.
2. Framework detectado: Next.js (usa `web/vercel.json`).
3. Variables (Production y Preview):

| Variable | Valor |
|---|---|
| `NEXT_PUBLIC_API_URL` | `https://<api>.vercel.app` (paso 2) |
| `BLOB_STORE_ID` | id del store del paso 1 (lo inyecta la conexión del store) |
| `BLOB_READ_WRITE_TOKEN` | token del paso 1 (lo usa `/api/blob/upload`) |

> Son los mismos nombres que lee el API: el store se configura una sola vez y
> ambos proyectos apuntan a él. Sin el token, `/api/blob/upload` responde `501` y
> el navegador cae al multipart clásico (solo válido en local).

4. Deploy. Copiar el dominio final y **añadirlo a `Cors__Origins`** del proyecto
   `api`, luego redeploy de la API.

## Paso 4 — Migraciones

Vercel no permite sobreescribir el comando de arranque, así que las migraciones
**no** se aplican al arrancar el servicio.

- **Automático:** el job `migrate` de `.github/workflows/api-ci.yml` corre en
  cada push a `main`. Requiere el secret `NEON_CONNECTION_STRING`.
- **Manual (primera vez):**

```bash
cd api
# ⚠️ ASPNETCORE_ENVIRONMENT=Production es OBLIGATORIO.
# Sin él, `dotnet run` carga appsettings.Development.json, cuyo
# ConnectionStrings:Default apunta a localhost:5432 y tiene MÁS prioridad que
# DATABASE_URL_UNPOOLED: las migraciones irían a tu Postgres local y Neon
# quedaría vacío (síntoma: 42P01 "relation tutors does not exist").
ASPNETCORE_ENVIRONMENT=Production \
DATABASE_URL_UNPOOLED="postgresql://...?sslmode=require" \
Jwt__Key="migration-only-placeholder-not-used-for-auth" \
ApplyMigrations=true \
dotnet run --project AuraLearn.Api
# imprime "[auralearn] Conexión a PostgreSQL desde DATABASE_URL_UNPOOLED (ep-xxx.neon.tech/neondb)"
# y "[auralearn] Migraciones aplicadas.", luego termina
```

Verifica que el log diga `desde DATABASE_URL_UNPOOLED` y el host de Neon. Si
dice `desde ConnectionStrings:Default` y `localhost`, apuntaste a la base
equivocada.

Comprobación de que el esquema está aplicado (deben aparecer `tutors`, `users`,
`slots`, `reservations`, `wallet_entries`, `tutor_availability`,
`tutor_availability_exceptions`, `tutor_documents`, `booking_rules` y
`__EFMigrationsHistory`):

```bash
psql "postgresql://...neon.tech/neondb?sslmode=require" -c '\dt'
```

> **Recordatorio:** `/health` responde `Healthy` aunque la base esté vacía
> (solo ejecuta `SELECT 1`). El arranque del API registra el estado del esquema:
> con migraciones pendientes imprime
> `[auralearn] AVISO: N migraciones pendientes …`.

## Paso 5 — GitHub Actions

Secret requerido (Settings → Secrets and variables → Actions):

| Secret | Valor |
|---|---|
| `NEON_CONNECTION_STRING` | URL **directa** (unpooled) de Neon, p. ej. `DATABASE_URL_UNPOOLED` |

`api-ci` valida en PRs y migra en `main`; el deploy lo hace Vercel con su
integración Git. `web-ci` solo valida (lint + tsc + build).

## Verificación final

- [ ] `GET /health` en el proyecto `api` → 200 `Healthy` (readiness: incluye PostgreSQL)
- [ ] `GET /health/live` → 200 `Healthy` (liveness, sin dependencias)
- [ ] `GET /api/tutors` → 200 con datos de Neon
- [ ] Login desde el dominio de `web` → JWT emitido (CORS correcto)
- [ ] `GET /api/blob/upload` en `web` → `{"enabled":true}`
- [ ] Subir un PDF de **4.8 MB** en `/tutor/documentos` → se crea (rango entre el límite de 4.5 MB de Vercel y el tope de negocio de 5 MB: antes fallaba con 413)
- [ ] Descargar ese documento como Admin → 200; como tutor → 403
- [ ] Foto de perfil visible en `/tutores` tras subirla
- [ ] Reserva completa desde `web` → fila en Neon
- [ ] Workflows `api-ci` y `web-ci` en verde

## Deudas y advertencias

- **Container Runtime está en Beta** en todos los planes.
- **Scale-to-zero:** la función se apaga a los 5 min sin tráfico (producción) →
  cold start .NET de ~1-3 s. Neon también auto-suspende.
- **Sin Secure Compute ni Static IPs** con imágenes propias.
- **Región única** por defecto (`iad1`); elegir la misma región para `api`, `web`
  y el store de Blob para minimizar latencia.
- **`BLOB_READ_WRITE_TOKEN` es un secreto estático de larga vida.** Rotarlo
  invalida las subidas en curso.
- **Documentos huérfanos:** si el `attach` falla tras la subida, el blob queda en
  el store sin fila en BD; limpiar con `vercel blob list`/`del` o desde el
  dashboard.
- **JWT key rotation** invalida todas las sesiones (8 h de exp).
