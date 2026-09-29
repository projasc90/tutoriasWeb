# Summary — Extraer mocks de datos a web/lib/data

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-extract-mock-data-plan.md)

## Qué cambió

- **Nuevos módulos de dominio/datos en `web/lib/`:**
  - `web/lib/types/tutor.ts` — `Tutor`, `FilterState`, `SortOption`
  - `web/lib/types/mentor.ts` — `Mentor`
  - `web/lib/data/tutors.ts` — `TUTORS` (12 tutores), constantes de filtro (`UNIVERSITIES`, `LEVELS`, `AVAILABILITIES`, `RATINGS`) y `DEFAULT_FILTERS`
  - `web/lib/data/mentors.ts` — `MENTORS` (4 mentores)
  - `web/lib/filters.ts` — funciones puras `applyFilters`/`applySort`
- **Consumidores adelgazados:** `web/app/tutores/page.tsx` (−~70 líneas) y `web/components/landing/mentors.tsx` (−~69 líneas) ahora solo renderizan e importan.
- **Versión:** `web/package.json` `0.1.0` → `0.1.1` (clasificación `refactor`).

## Qué NO cambió

- Comportamiento: mismo filtrado, mismo orden, mismos estados vacíos, misma UI.
- Contenido de marketing estático no extraído (decisión del `context`): `DISCIPLINES`, `POPULAR_SUBJECTS`, `QUESTIONS`, `STEPS`, `INSTITUTIONS`, `TESTIMONIALS`.
- No se añadió Vitest ni tests (pospuesto a tarea dedicada con ADR).
- `api/` sigue reservada; los datos siguen siendo mock transitorio.

## Deuda / próximos pasos

1. Confirmar smoke manual visual (filtros, orden, limpiar, landing).
2. Cuando exista el backend .NET: reemplazar `web/lib/data/tutors.ts` por un hook de datos (template `.github/templates/data-hook.md`) y alinear `web/lib/types/` con los DTOs reales.
3. Unificar el literal `25000` (techo de precio) en una constante compartida cuando se integre la API.
4. Primer test unitario candidado: `web/lib/filters.ts` (ya es puro y testable).

## Verificación

Ver [verification](./2026-09-28-extract-mock-data-verification.md): lint ✅, tsc ✅, build ✅.
