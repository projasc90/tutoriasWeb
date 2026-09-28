---
id: current-state
priority: 1
loadWhen: always
maxLines: 60
---

# Estado Actual — AuraLearn

**Versión:** `0.1.0` (SEMVER en `web/package.json`)

## Completado recientemente

- Sistema de memoria externa y gobernanza (`.ai/`, `.github/`, `docs/plans/`) — configuración inicial (ADR-001).
- Landing pública en `web/`: Hero, TrustStrip, Rigor, Disciplines, Mentors, Steps, Testimonials, TutorCta, FAQ, FinalCta, Footer.
- Directorio `/tutores` con filtros (universidad, nivel, rating, precio, disponibilidad), orden y datos mock en cliente.

## Pendientes inmediatos

- Implementar proyecto .NET en `api/` (la carpeta está reservada con `.gitkeep`).
- Extraer tipos y datos mock (`TUTORS`, `MENTORS`) a módulos de datos compartidos antes de integrar API real.
- Configurar Playwright para E2E (hoy no aplica el gate E2E).
- Conectar `CurrencyToggle` (CRC/USD) a estado global si se añaden más páginas con precios.
- Portal público en `portal/` (reservado).

## Contexto rápido de arquitectura

- `web/` = Next.js 16.3.6 App Router + React 19.2.8 + TS 5 + Tailwind 4 + shadcn/ui (estilo `base-nova`, primitivas `@base-ui/react`).
- Tokens de diseño: Material 3 light en `web/app/globals.css` (`@theme inline`); tipografías MD3 (`text-display-lg`, `text-title-md`, etc.); fuente Plus Jakarta Sans.
- Iconos: Material Symbols vía `web/components/material-icon.tsx` (cargado una vez en `layout.tsx`); `lucide-react` en `components/ui`.
- Path alias `@/*` → `web/*`; util `cn` reexportada desde el paquete `cn`.
- Backend previsto: .NET Core + EF + FluentValidation + JWT + PostgreSQL; migraciones en `api/Migrations/`.
- Roles: ESTUDIANTE, TUTOR, ADMIN.
- Frontend despliega en Vercel/Netlify; backend en VPS Linux (systemd/nginx), Azure App Service o AWS.
