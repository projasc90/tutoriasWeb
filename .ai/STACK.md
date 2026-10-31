---
id: stack
priority: 2
loadWhen: tocar-dependencias-o-versiones
maxLines: 80
---

# Stack — AuraLearn

> ⚠️ **No cambiar versiones sin justificación explícita** (ADR + entrada en `version-changes.md`). Las versiones provienen de archivos reales del repo.

## Frontend (`web/` — activo)

Fuente: `web/package.json` (v0.1.0) y `web/components.json`.

| Paquete | Versión | Nota |
|---------|---------|------|
| next | 16.3.6 | App Router; ver `web/AGENTS.md` (docs en `node_modules/next/dist/docs/`) |
| react / react-dom | 19.2.8 | |
| typescript | ^5 | `strict: true` (ver `web/tsconfig.json`) |
| tailwindcss | ^4 | vía `@tailwindcss/postcss`; sin `tailwind.config` (CSS-first) |
| @base-ui/react | ^1.8.0 | primitivas sobre las que se construyen los componentes `ui/` |
| shadcn | ^4.21.0 | CLI; estilo `base-nova`, baseColor `neutral`, RSC `true` |
| class-variance-authority | ^0.7.1 | variantes de botones/badges |
| lucide-react | ^1.47.0 | iconos en `components/ui` |
| @vercel/blob | 2.x | **ADR-009** — subida directa a Vercel Blob desde `web/app/api/blob/upload` (firma de tokens de cliente). Solo en el proyecto `web`; el API .NET usa la REST con `HttpClient`. |
| cn | ^0.4.0 | `cn` helper (`web/lib/utils.ts` reexporta) |
| tw-animate-css | ^1.4.0 | animaciones Tailwind |
| eslint | ^9 | flat config (`web/eslint.config.mjs`) + `eslint-config-next` 16.3.6 |
| @types/node | ^20 | |

## Backend (`api/` — activo desde 2026-09-28, ADR-003)

Fuente: los `.csproj` reales de `api/`. .NET SDK **10.0.401** (target `net10.0`).

| Componente | Versión | Ubicación |
|------------|---------|-----------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | `AuraLearn.Infrastructure` |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 | `AuraLearn.Infrastructure` + `AuraLearn.Api` |
| FluentValidation | 12.x | `AuraLearn.Application` |
| FluentValidation.DependencyInjectionExtensions | 12.x | `AuraLearn.Api` |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.x | `AuraLearn.Api` |
| Swashbuckle.AspNetCore | 10.x | `AuraLearn.Api` |
| xUnit | (template net10.0) | `tests/AuraLearn.Tests` |

- Migraciones EF en `AuraLearn.Infrastructure/Migrations/` con `dotnet ef` (tool global dotnet-ef 10.0.12).
- BD local dev: PostgreSQL en `localhost:5432`, BD `auralearn_dev`, usuario `auralearn` (credenciales solo dev en `appsettings.Development.json`, ignorado por git).
- Tooling: `dotnet ef migrations add ... --project AuraLearn.Infrastructure --startup-project AuraLearn.Api`.

## Infraestructura

- **Frontend:** Vercel (Next.js nativo; `web/vercel.json`; env `NEXT_PUBLIC_API_URL`).
- **Backend:** contenedor Docker (`api/Dockerfile` multi-stage, usuario no-root, entrypoint con migraciones opt-in) desplegado en **Railway** (ADR-008). Vercel no soporta .NET/contenedores.
- **BD en la nube:** **Neon** (Postgres gestionado, connection string pooled por variable de entorno). Local: Postgres 16 en `docker-compose.yml` (puerto 5433) o Postgres dev existente (5432).
- **CI/CD:** GitHub Actions — `.github/workflows/api-ci.yml` (build+tests en PR; build+tests+job `migrate` one-shot contra Neon en `main`; el deploy del contenedor lo hace Vercel desde Git — ADR-009) y `.github/workflows/web-ci.yml` (gate lint/tsc/build; deploy nativo de Vercel).
- **Dev OS:** macOS · **Prod OS:** Linux (contenedores).

## Scripts reales (`web/package.json`)

```
npm run dev      # next dev
npm run build    # next build
npm run start    # next start
npm run lint     # eslint
```

No existe script dedicado `type-check`: usar `npx tsc --noEmit` desde `web/`.

## Iconografía y tipografía

- Material Symbols Outlined (Google Fonts) cargado en `web/components/material-icon.tsx` desde el root layout.
- Fuente: Plus Jakarta Sans (`next/font/google`), pesos 400/600/700/800.
