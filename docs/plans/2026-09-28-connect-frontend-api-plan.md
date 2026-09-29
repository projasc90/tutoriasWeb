# Plan — Conectar frontend a GET /api/tutors

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-connect-frontend-api-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Cliente HTTP tipado | `web/lib/api.ts` | pendiente |
| 2 | Hook de datos con debounce/abort | `web/hooks/use-tutors.ts` | pendiente |
| 3 | Directorio consume el hook | `web/app/tutores/page.tsx` | pendiente |
| 4 | Limpiar mock (solo constantes) | `web/lib/data/tutors.ts` | pendiente |
| 5 | Variables de entorno | `web/.env.local`, `web/.env.example` | pendiente |
| 6 | Gate: lint + tsc + build | — | pendiente |
| 7 | Smoke con API corriendo | — | pendiente |
| 8 | Cierre: verification, summary, `.ai/` | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `web/lib/api.ts`, `web/hooks/use-tutors.ts`, `web/.env.example`
- **Modificados:** `web/app/tutores/page.tsx` (importa hook), `web/lib/data/tutors.ts` (sin `TUTORS`)
- **Nuevos (ignorados):** `web/.env.local`
- **Actualizados (cierre):** `.ai/CURRENT_STATE.md`, `.ai/ACTIVE_MEMORY.md`, `.ai/version-changes.md` (0.2.1)

## Criterios de aceptación

1. `TUTORS` ya no existe ni se importa; el directorio consume `GET /api/tutors`.
2. Estados loading/error/success visibles; error con botón "Reintentar".
3. Filtros (query, universidad, rating, precio) disparan requests al backend con debounce en el query.
4. Gate frontend verde (lint + tsc + build).
5. Smoke: 12 tutores de la BD visibles; filtros funcionan; API caída → error accionable.
