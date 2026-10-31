# Guía de despliegue — Railway (API) + Neon (Postgres) + Vercel (web)

> ⚠️ **SUPERADA (2026-10-30).** Esta guía describe la topología de ADR-008, que
> partía de una premisa hoy falsa: Vercel no soporta contenedores ni .NET.
> Vercel lanzó **Container Runtime** (imágenes OCI desde un `Dockerfile`, Beta en
> todos los planes) y ya existe guía oficial para ASP.NET Core.
> **Guía vigente:** `docs/plans/2026-10-30-vercel-container/deploy-guide.md`
> (ADR-009). Se conserva este documento como referencia del camino alternativo
> con Railway.

> Contexto: Vercel no soporta contenedores ni .NET. La API .NET vive en un
> contenedor Docker en Railway; la BD es Neon (Postgres gestionado); el
> frontend Next.js se despliega en Vercel. CI/CD con GitHub Actions.

## Arquitectura

```
┌─────────────┐   HTTPS    ┌──────────────────┐   Npgsql/SSL   ┌─────────┐
│ Vercel      │ ─────────► │ Railway          │ ─────────────► │ Neon    │
│ (web/,      │            │ (contenedor API, │                │ (Postgres│
│  Next.js)   │            │  Dockerfile)     │                │  gestionado)│
└─────────────┘            └──────────────────┘                └─────────┘
```

## Paso 1 — Neon (base de datos)

1. Crear cuenta en neon.tech → **New Project** (región cercana a CR, p. ej. `aws-us-east-2`).
2. Copiar el **connection string pooled** (botón "Pooled connection"):
   `postgresql://user:pass@ep-xxx-pooler.region.aws.neon.tech/neondb?sslmode=require`
3. Guardarlo para el paso 2 (nunca en el repo).

## Paso 2 — Railway (API en contenedor)

1. Crear cuenta en railway.app → **New Project → Deploy from GitHub repo** → seleccionar el repo.
2. Railway detecta el monorepo: configurar el servicio con **Root Directory = `api`** (usa `api/Dockerfile`).
3. Variables del servicio (Variables → New Variable):

| Variable | Valor |
|---|---|
| `ConnectionStrings__Default` | connection string pooled de Neon (paso 1) |
| `Jwt__Key` | secreto aleatorio ≥ 32 caracteres (generar: `openssl rand -base64 48`) |
| `Jwt__Issuer` | `AuraLearn.Api` |
| `Cors__Origins` | `https://<tu-app>.vercel.app` (paso 3; añadir dominio custom si existe, separado por `;`) |
| `Storage__DocumentsRoot` | `/data/documents` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

4. **Volumen** para documentos (filesystem efímero sin él): Settings → Volumes → montar en `/data`.
5. **Migraciones** (primera vez y tras cambios de esquema): en Railway → Service → Settings → Command, o ejecutar un deploy one-shot:
   - Opción simple: temporalmente definir variable `ApplyMigrations=true`, redeploy, ver logs ("Migraciones aplicadas."), quitar la variable y redeploy de nuevo.
   - Opción CI: job `migrations` del workflow con `APPLY_MIGRATIONS=true` (contenedor efímero).
6. Verificar: `curl https://<tu-api>.up.railway.app/health` → `Healthy`, y `GET /api/tutors` → 200.
7. Generar **token** (Account → Tokens) y anotar el **service id** para los secrets de GitHub.

## Paso 3 — Vercel (frontend)

1. vercel.com → **Add New Project** → importar el repo.
2. **Root Directory = `web`** (el `vercel.json` ya está en `web/`).
3. Variables (Project → Settings → Environment Variables, para Production y Preview):

| Variable | Valor |
|---|---|
| `NEXT_PUBLIC_API_URL` | `https://<tu-api>.up.railway.app` (paso 2.6) |

4. Deploy. Verificar: la landing carga tutores reales (consume la API de Railway).
5. Copiar el dominio final (`https://<app>.vercel.app`) y **añadirlo a `Cors__Origins`** en Railway (paso 2.3) → redeploy de la API.

## Paso 4 — GitHub Actions (CI/CD)

Secrets del repo (Settings → Secrets and variables → Actions → New repository secret):

| Secret | Valor |
|---|---|
| `RAILWAY_TOKEN` | token de Railway (paso 2.7) |
| `RAILWAY_SERVICE` | id del servicio de la API en Railway |

Con esto, cada push a `main` que toque `api/**` ejecuta: build + tests → imagen Docker a GHCR → migraciones → redeploy en Railway. Los PRs solo corren build + tests. `web/**` corre lint + tsc + build (deploy lo hace Vercel nativamente).

## Verificación final (checklist)

- [ ] `GET /health` en Railway → 200 `Healthy`
- [ ] `GET /api/tutors` en Railway → 200 con datos de Neon
- [ ] Login desde el frontend en Vercel → JWT emitido (CORS correcto)
- [ ] Reserva completa desde Vercel → reserva en Neon
- [ ] `/admin/estudiantes` con Admin real → listado desde Neon
- [ ] Workflow `api-deploy` en verde en GitHub Actions

## Deudas y advertencias

- **Documentos en disco**: Railway tiene filesystem efímero; el volumen `/data` persiste pero es local al servicio. Para multi-instancia o durabilidad fuerte, migrar `IFileStorage` a object storage (S3/R2/Azure Blob) — ADR futuro.
- **Neon free tier**: auto-suspend tras inactividad (~5 min); el primer request tras pausa tarda ~1 s. Considerar "scale to zero" aceptable o plan pago.
- **Railway free**: crédito limitado mensual; el servicio duerme si no hay tráfico (cold start ~ segundos).
- **JWT key rotation**: cambiar `Jwt__Key` invalida todas las sesiones (8 h de exp); rotar en ventana de mantenimiento.
- **Secretos**: nunca en el repo ni en la imagen; solo en Railway/Vercel/GitHub Secrets.
