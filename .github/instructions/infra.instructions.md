---
applyTo: ".github/workflows/**, **/Dockerfile, **/*.bicep, **/*.yaml, **/*.yml, **/docker-compose*"
description: Infraestructura, CI/CD y despliegue de AuraLearn (VPS/Azure/AWS)
---

# Infra / CI-CD — AuraLearn

## Topología de despliegue

| Componente | Destino |
|------------|---------|
| Frontend `web/` | Vercel o Netlify |
| Backend `api/` (futuro) | VPS Linux (systemd + nginx) **o** Azure App Service **o** AWS Elastic Beanstalk |
| PostgreSQL | Mismo host (VPS) o Azure Database for PostgreSQL Flexible Server / Amazon RDS |
| Portal `portal/` (futuro) | Vercel/Netlify, independiente de `web/` |

## Reglas generales

1. **Secretos nunca en el repo.** CI usa secrets del proveedor (GitHub Actions Secrets / Vercel env); prod usa variables de entorno, Key Vault o equivalentes.
2. Configuración por entorno: `NODE_ENV`, `ASPNETCORE_ENVIRONMENT`; connection strings solo por variable de entorno.
3. Los workflows de GitHub Actions viven en `.github/workflows/`; pipelines declarativos, sin scripts imperfativos largos inline.
4. IaC (si aplica) en Bicep/Terraform versionado; los recursos efímeros se destruyen.

## Backend (cuando exista)

- Servicio Linux con `systemd` (`auralearn-api.service`): `Restart=on-failure`, `Environment=` para secretos o `EnvironmentFile` protegido.
- `nginx` como reverse proxy: TLS terminado (Let's Encrypt), gzip, headers de seguridad (HSTS, X-Content-Type-Options), proxy a Kestrel en `localhost:<puerto>`.
- Migraciones: `dotnet ef database update` al desplegar (o SQL versionado); **nunca** perder de vista el rollback manual de cada migración.
- Backups de PostgreSQL programados y probados de restauración.

## CI/CD mínimo esperado

- `web`: lint + type-check + build en cada PR; deploy automático a preview en PR y a producción en merge a main.
- `api` (futuro): build + tests en cada PR; despliegue manual o por tag con migración explícita.

## Verificación

- Tras desplegar: healthcheck del backend, smoke del frontend (200 en rutas clave), y registro de la incidencia si algo falla.
- Cualquier cambio de infraestructura significativo → ADR + entrada en `.ai/STACK.md`.
