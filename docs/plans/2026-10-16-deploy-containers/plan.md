# Plan — Contenedores para desplegar la API + frontend en Vercel (2026-10-16)

## Solicitud y decisiones

- Usuario pidió "contenedores para desplegar api en vercel". **Vercel no soporta contenedores ni .NET** (solo runtimes serverless Node/Python/Go/Ruby). Aclarado y aceptado el ajuste:
  - **API .NET en contenedor → Railway** (elegido por el usuario).
  - **PostgreSQL → Neon** (elegido por el usuario; connection string por variable de entorno).
  - **Frontend `web/` → Vercel** (Next.js 16, despliegue nativo).
  - **CI/CD → GitHub Actions** (build + tests + docker build/push).
- Nota: Railway también ofrece Postgres, pero el usuario eligió Neon explícitamente.

## Cambios

### 1. API lista para contenedor (`api/`)
- `Program.cs`:
  - **PORT dinámico**: Railway/contenedores inyectan `PORT`; Kestrel debe escuchar en `$PORT` (o 8080 default). Vía `ASPNETCORE_URLS` en el Dockerfile o código.
  - **CORS por entorno**: `Cors:Origins` (separado por `;`) con fallback a `http://localhost:3000`. En prod: dominio `.vercel.app` + dominio custom.
  - Connection string: `ConnectionStrings__Default` por variable de entorno (Neon).
  - Migraciones: comando `apply-migrations` al desplegar (entrypoint del contenedor o paso de CI) — nunca automático silencioso en cada arranque sin control.
- `AuraLearn.Api.csproj`: verificar que no haya nada que impida el build en Linux (no lo hay: Npgsql, sin código nativo).

### 2. Contenedores
- `api/Dockerfile` — multi-stage:
  - Stage build: `mcr.microsoft.com/dotnet/sdk:10.0`, `dotnet publish -c Release`.
  - Stage runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`, usuario no-root, `EXPOSE $PORT`, entrypoint con migraciones opcionales (`APPLY_MIGRATIONS=true`).
- `api/.dockerignore` (bin/obj/tests).
- `docker-compose.yml` (raíz) para **local**: api + postgres 16 (volumen, healthcheck) — espejo del entorno dev actual. Neon no se puede correr local; compose usa Postgres local con la misma variable `ConnectionStrings__Default`.

### 3. CI/CD (`.github/workflows/`)
- `api-deploy.yml`: en push a main (paths `api/**`) → `dotnet build && dotnet test` → `docker build` → push a GHCR → deploy a Railway (token `RAILWAY_TOKEN`, service id por secret). Migraciones: paso explícito con `APPLY_MIGRATIONS` o job separado.
- `web-deploy.yml`: Vercel se puede dejar a su integración Git nativa (recomendado); el workflow solo hace lint + tsc + build como gate de PR. Alternativa: deploy con `vercel-action` + `VERCEL_TOKEN`. Decisión: gate de PR + deploy nativo de Vercel (menos secretos).

### 4. Frontend en Vercel (`web/`)
- `NEXT_PUBLIC_API_URL` apuntando a la URL de Railway (variable en Vercel).
- `vercel.json` mínimo (framework auto-detectado; solo si hace falta).
- CORS del backend debe incluir el dominio de Vercel.

### 5. Neon
- Solo configuración: `ConnectionStrings__Default` = connection string de Neon (pooled) en Railway. Sin código nuevo. Nota: Neon usa SNI/SSL — Npgsql lo soporta (`SSL Mode=Require`).

## Variables de entorno (prod)

| Variable | Dónde | Valor |
|---|---|---|
| `ConnectionStrings__Default` | Railway | connection string Neon (pooled) |
| `Jwt__Key` | Railway | secreto ≥32 chars |
| `Jwt__Issuer` | Railway | URL de la API o `AuraLearn.Api` |
| `Cors__Origins` | Railway | `https://<app>.vercel.app` |
| `Storage:DocumentsRoot` | Railway | `/data/documents` (volumen Railway) |
| `ASPNETCORE_ENVIRONMENT` | Railway | `Production` |
| `NEXT_PUBLIC_API_URL` | Vercel | `https://<api>.up.railway.app` |

⚠️ Deuda conocida: `LocalFileStorage` en disco — Railway tiene filesystem efímero salvo volumen montado; documentos de verificación requieren volumen o migrar a object storage (fuera de alcance; registrar como deuda).

## Verificación

1. `docker build` de la API en verde + contenedor local responde `/health` y `GET /api/tutors` contra Postgres del compose.
2. Gates del repo: `dotnet build && dotnet test` (149) y `npm run lint && npx tsc --noEmit`.
3. Workflow YAML validado (sintaxis + actionlint si está disponible).
4. Despliegue real (Railway/Vercel/Neon) queda como paso manual del usuario con sus cuentas; dejar guía paso a paso en `docs/plans/2026-10-16-deploy-containers/deploy-guide.md`.

## Criterios de aceptación

- `docker compose up` levanta api+postgres y `/health` responde 200.
- Dockerfile no contiene secretos; usuario no-root; imagen < ~250 MB.
- Workflows declarativos, secretos solo por GitHub Secrets.
- Guía de despliegue completa para Railway + Neon + Vercel.
