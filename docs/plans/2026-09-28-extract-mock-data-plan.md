# Plan — Extraer mocks de datos a web/lib/data

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-extract-mock-data-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Crear tipos de dominio | `web/lib/types/tutor.ts`, `web/lib/types/mentor.ts` | pendiente |
| 2 | Crear módulos de datos | `web/lib/data/tutors.ts`, `web/lib/data/mentors.ts` | pendiente |
| 3 | Extraer lógica pura de filtros/orden | `web/lib/filters.ts` | pendiente |
| 4 | Actualizar directorio `/tutores` | `web/app/tutores/page.tsx` | pendiente |
| 5 | Actualizar sección de mentores | `web/components/landing/mentors.tsx` | pendiente |
| 6 | Testing gate + build | — | pendiente |
| 7 | Cierre (verification, summary, `.ai/`, bump 0.1.1) | — | pendiente |

## Archivos esperados (resultado final)

- **Nuevos:** `web/lib/types/tutor.ts`, `web/lib/types/mentor.ts`, `web/lib/data/tutors.ts`, `web/lib/data/mentors.ts`, `web/lib/filters.ts`
- **Modificados:** `web/app/tutores/page.tsx` (−~70 líneas), `web/components/landing/mentors.tsx` (−~69 líneas)
- **Modificados (cierre):** `web/package.json` (0.1.1), `.ai/CURRENT_STATE.md`, `.ai/ACTIVE_MEMORY.md`, `.ai/version-changes.md`, `.ai/PROJECT_MAP.md`

## Criterios de aceptación

1. `TUTORS`/`MENTORS` ya no están definidos dentro de componentes (solo importados).
2. Los tipos de dominio viven solo en `web/lib/types/` (sin duplicados).
3. El filtrado/orden de `/tutores` pasa por `applyFilters`/`applySort` con comportamiento idéntico (incluido el techo `priceMax < 25000` y que `level`/`availability` no filtren aún).
4. `npm run lint` y `npx tsc --noEmit` sin errores; `npm run build` exitoso.
5. Smoke manual: búsqueda, filtro por universidad, rango de precio, orden por precio/rating y "Limpiar filtros" funcionan igual; la landing renderiza "Docentes Destacados" sin cambios.
