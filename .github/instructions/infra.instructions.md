---
applyTo: ".github/workflows/**, **/Dockerfile, **/*.bicep, **/*.yaml, **/*.yml, **/docker-compose*"
description: Infraestructura, CI/CD y despliegue de AuraLearn (VPS/Azure/AWS)
---

# Infra / CI-CD — AuraLearn

## Topología de despliegue

| Componente | Destino |
|------------|---------|
| Frontend `web/` | **Vercel** (Next.js nativo, `web/vercel.json`) |
| Backend `api/` | **Contenedor Docker en Railway** (ADR-008; alternativas: Azure Container Apps, VPS con compose) |
| PostgreSQL | **Neon** (connection string por variable de entorno) o Postgres en compose/VPS |
| Portal `portal/` (futuro) | Vercel/Netlify, independiente de `web/` |

> ⚠️ Vercel **no** soporta contenedores ni .NET: solo aloja el frontend. La API .NET vive en su contenedor en Railway (ver `docs/plans/2026-10-16-deploy-containers/deploy-guide.md`).

## Reglas generales

1. **Secretos nunca en el repo.** CI usa secrets del proveedor (GitHub Actions Secrets / Vercel env); prod usa variables de entorno, Key Vault o equivalentes.
2. Configuración por entorno: `NODE_ENV`, `ASPNETCORE_ENVIRONMENT`; connection strings solo por variable de entorno.
3. Los workflows de GitHub Actions viven en `.github/workflows/`; pipelines declarativos, sin scripts imperfativos largos inline.
4. IaC (si aplica) en Bicep/Terraform versionado; los recursos efímeros se destruyen.

## Backend (contenedor en Railway — ADR-008)

- Imagen multi-stage `api/Dockerfile` (SDK 10 build → aspnet 10 runtime), usuario no-root, `entrypoint.sh` con `PORT` dinámico.
- Migraciones: opt-in explícito (`APPLY_MIGRATIONS=true` → one-shot y termina); nunca automáticas silenciosas en cada arranque. `Down()` + rollback manual comentado en cada migración.
- Variables del servicio: `ConnectionStrings__Default` (Neon), `Jwt__Key`, `Cors__Origins` (dominios Vercel separados por `;`), `Storage__DocumentsRoot` (`/data/documents` con volumen), `ASPNETCORE_ENVIRONMENT=Production`.
- Volumen Railway en `/data` para documentos (filesystem efímero sin él); migrar a object storage si se necesita durabilidad multi-instancia.
- Backups: Neon incluye PITR según plan; probar restauración al menos una vez.

## CI/CD mínimo esperado

- `web`: `.github/workflows/web-ci.yml` (lint + tsc + build en cada PR); deploy automático a preview en PR y a producción en merge a main vía integración Git de Vercel.
- `api`: `.github/workflows/api-deploy.yml` — build + tests en cada PR; en push a main: imagen Docker a GHCR → migraciones one-shot → redeploy en Railway. Secretos: `RAILWAY_TOKEN`, `RAILWAY_SERVICE`.

## Verificación

- Tras desplegar: healthcheck del backend, smoke del frontend (200 en rutas clave), y registro de la incidencia si algo falla.
- Cualquier cambio de infraestructura significativo → ADR + entrada en `.ai/STACK.md`.
