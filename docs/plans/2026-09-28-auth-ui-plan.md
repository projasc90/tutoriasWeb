# Plan — Auth UI en el frontend (login/registro + sesión)

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-auth-ui-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Storage + helpers de sesión | `web/lib/auth.ts` | pendiente |
| 2 | Cliente HTTP: auth endpoints | `web/lib/api.ts` | pendiente |
| 3 | AuthProvider + useAuth | `web/hooks/use-auth.tsx` | pendiente |
| 4 | Página login | `web/app/login/page.tsx` | pendiente |
| 5 | Página registro | `web/app/registro/page.tsx` | pendiente |
| 6 | Header con sesión + layout | `web/components/landing/currency-toggle.tsx`, `web/app/layout.tsx` | pendiente |
| 7 | Gate: lint + tsc + build | — | pendiente |
| 8 | Smoke con backend | — | pendiente |
| 9 | Cierre: verification, summary, `.ai/` | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `web/lib/auth.ts`, `web/hooks/use-auth.tsx`, `web/app/login/page.tsx`, `web/app/registro/page.tsx`
- **Modificados:** `web/lib/api.ts`, `web/components/landing/currency-toggle.tsx`, `web/app/layout.tsx`
- **Actualizados (cierre):** `.ai/CURRENT_STATE.md`, `.ai/ACTIVE_MEMORY.md`, `.ai/version-changes.md` (0.3.1), `.ai/PROJECT_MAP.md`

## Criterios de aceptación

1. `/login` y `/registro` funcionales contra el backend real (estados loading/error/éxito).
2. Header muestra sesión activa (nombre + cerrar sesión) o links a login/registro.
3. La sesión persiste al recargar (localStorage) y se descarta si el token expiró.
4. Gate frontend verde (lint + tsc + build).
5. Smoke: registro → sesión activa; login; logout limpia; recarga mantiene sesión.
