# Context — Extraer mocks de datos a web/lib/data

**Fecha:** 2026-09-28
**Plan asociado:** [plan](./2026-09-28-extract-mock-data-plan.md)

## Alcance

Mover datos mock y tipos de dominio fuera de los componentes UI hacia módulos dedicados:

- `TUTORS` + tipos `Tutor`/`FilterState`/`SortOption` + constantes de filtro (hoy inline en `web/app/tutores/page.tsx`)
- `MENTORS` + tipo `Mentor` (hoy inline en `web/components/landing/mentors.tsx`)
- Lógica de filtrado/orden de `/tutores` → funciones puras en `web/lib/filters.ts`

Destinos: `web/lib/types/`, `web/lib/data/`, `web/lib/filters.ts`.

## Decisiones

1. Refactor estrictamente **behavior-preserving**: sin cambios visuales ni de lógica.
2. Se extraen funciones puras (`applyFilters`/`applySort`) para dejar la lógica testable; **no** se añade Vitest en esta tarea (dependencia nueva → ADR aparte).
3. Excluido del alcance (contenido de marketing estático, no ligado a API futura): `DISCIPLINES`, `POPULAR_SUBJECTS`, `QUESTIONS`, `STEPS`, `INSTITUTIONS`, `TESTIMONIALS`.
4. Los mocks siguen transitorios (regla del repo): este movimiento es el paso previo a reemplazarlos por la API real.
5. Sin conexión real de `CurrencyToggle` a estado global (fuera de alcance; cambia comportamiento).

## Restricciones

- No romper el testing gate: `cd web && npm run lint && npx tsc --noEmit` + `npm run build`.
- Conservar el comportamiento exacto del filtro: techo `priceMax < 25000` tal cual, y que `level`/`availability` se muestren en la UI pero aún no filtran datos.
- Comentarios en español, identificadores en inglés (`CONVENTIONS.md`).

## Dudas cerradas

- **Nombres de archivos:** `lib/types/tutor.ts`, `lib/types/mentor.ts`, `lib/data/tutors.ts`, `lib/data/mentors.ts` (kebab-case como `material-icon.tsx`).
- **Bump de versión:** sí, `0.1.0` → `0.1.1` con clasificación `refactor`.
- **Project Map:** actualizar con las nuevas carpetas `web/lib/data/` y `web/lib/types/`.
