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
| cn | ^0.4.0 | `cn` helper (`web/lib/utils.ts` reexporta) |
| tw-animate-css | ^1.4.0 | animaciones Tailwind |
| eslint | ^9 | flat config (`web/eslint.config.mjs`) + `eslint-config-next` 16.3.6 |
| @types/node | ^20 | |

## Backend (`api/` — reservado, por definir)

- .NET Core (C#) + Entity Framework Core + FluentValidation + JWT auth.
- PostgreSQL como base de datos; migraciones EF en `api/Migrations/` con `dotnet ef`.
- ⚠️ Al crear el proyecto, **actualizar este archivo** con versiones reales del `.csproj` y registrar ADR.

## Infraestructura

- **Frontend:** Vercel o Netlify.
- **Backend:** VPS Linux (systemd + nginx) o Azure App Service o AWS Elastic Beanstalk.
- **BD en la nube:** Azure Database for PostgreSQL Flexible Server / Amazon RDS for PostgreSQL.
- **Dev OS:** macOS · **Prod OS:** Linux.

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
