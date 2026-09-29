# Verification — Extraer mocks de datos a web/lib/data

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-extract-mock-data-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `cd web && npm run lint` | ✅ 0 errores (3 warnings pre-existentes en `currency-toggle.tsx` y `material-icon.tsx`, archivos no tocados) |
| `cd web && npx tsc --noEmit` | ✅ exit 0, sin errores de tipos |
| `cd web && npm run build` | ✅ exitoso; rutas `/`, `/tutores`, `/_not-found` prerenderizadas como estáticas |

## Evidencia estructural

- `grep "const TUTORS: Tutor[]"` → solo `web/lib/data/tutors.ts` (definición única).
- `grep "const MENTORS: Mentor[]"` → solo `web/lib/data/mentors.ts` (definición única).
- `web/app/tutores/page.tsx`: −~70 líneas; ahora importa desde `@/lib/data/tutors`, `@/lib/filters` y `@/lib/types/tutor`.
- `web/components/landing/mentors.tsx`: −~69 líneas; importa desde `@/lib/data/mentors` y `@/lib/types/mentor`.

## Criterios del plan

1. ✅ `TUTORS`/`MENTORS` ya no definidos dentro de componentes.
2. ✅ Tipos de dominio únicamente en `web/lib/types/`.
3. ✅ Filtrado/orden vía `applyFilters`/`applySort` con lógica idéntica (techo `priceMax < 25000` preservado; `level`/`availability` siguen sin filtrar, como antes).
4. ✅ Gate completo verde (lint + tsc + build).
5. ⚠️ Smoke manual pendiente del usuario: filtros/orden/limpiar en `/tutores` y sección "Docentes Destacados" en la landing.

## Riesgos residuales

- El smoke visual manual queda por confirmar (la app prerenderiza estático; el build no puede validar interacciones de filtro).
- El literal `25000` está duplicado (`DEFAULT_FILTERS.priceMax` en data y el techo en `filters.ts`) — intencional para preservar comportamiento tal cual; se unificará al integrar la API real.

> Nota: esta verificación no sustituye al testing gate (que ya se ejecutó por separado con comandos reales).
