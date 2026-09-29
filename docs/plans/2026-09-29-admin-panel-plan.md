# Plan — Panel admin de aprobación de tutores

**Fecha:** 2026-09-29
**Contexto:** [context](./2026-09-29-admin-panel-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Endpoints admin en el cliente HTTP | `web/lib/api.ts` | pendiente |
| 2 | Hook de cola admin | `web/hooks/use-admin-applications.ts` | pendiente |
| 3 | Página protegida + componentes | `web/app/admin/tutores/page.tsx`, `web/components/admin/{application-card,reject-dialog}.tsx` | pendiente |
| 4 | Gate: lint + tsc + build | — | pendiente |
| 5 | Smoke: aprobar/rechazar con admin | — | pendiente |
| 6 | Cierre: verification, summary, `.ai/` | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `web/hooks/use-admin-applications.ts`, `web/app/admin/tutores/page.tsx`, `web/components/admin/{application-card,reject-dialog}.tsx`
- **Modificados:** `web/lib/api.ts`
- **Actualizados (cierre):** `.ai/CURRENT_STATE.md`, `.ai/ACTIVE_MEMORY.md`, `.ai/version-changes.md`, `.ai/PROJECT_MAP.md`

## Criterios de aceptación

1. No-Admin ve mensaje 403 con link a `/login`; Admin ve la cola.
2. Aprobar → tutor `Verified` en BD y visible en `/tutores`; el item sale de la cola.
3. Rechazar exige motivo (modal); → estado `Rejected` con motivo persistido.
4. Gate frontend verde (lint + tsc + build).
5. Smoke completo con el usuario admin real.
