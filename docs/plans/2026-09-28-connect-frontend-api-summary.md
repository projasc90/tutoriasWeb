# Summary — Conectar frontend a GET /api/tutors

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-connect-frontend-api-plan.md)

## Qué cambió

- **Nuevos:**
  - `web/lib/api.ts` — cliente HTTP tipado (contrato espejo de los DTOs del backend), `fetchTutors(params, signal)`, base URL desde `NEXT_PUBLIC_API_URL`.
  - `web/hooks/use-tutors.ts` — hook con estados `idle|loading|error|success`, `AbortController`, debounce 300ms del query, `reload()`.
  - `web/.env.example` — documentación de variables (commiteable).
  - `web/.env.local` — `NEXT_PUBLIC_API_URL=http://localhost:5037` (ignorado por git).
- **Modificados:**
  - `web/app/tutores/page.tsx` — consume `useTutors`; nuevos estados `LoadingGrid` (skeletons) y `ErrorState` (con "Reintentar"); `TutorCard` adapta el DTO real (iniciales derivadas del nombre, `priceCrc`).
  - `web/lib/data/tutors.ts` — solo constantes de filtro; el array `TUTORS` fue **eliminado** (fin del mock del directorio).
  - `web/lib/types/tutor.ts` — `Tutor` espejo del DTO del backend.
  - `web/lib/filters.ts` — solo `applySort` (filtrado server-side).
- **Versión:** `web/package.json` `0.1.1` → `0.2.1` (clasificación `feat`: primera integración real frontend↔backend).

## Qué NO cambió

- Landing y `MENTORS` siguen estáticos (marketing, decisión previa).
- `level`/`availability` siguen sin filtrar (no existen en el contrato API).
- Sin paginación UI (una página de 12); el sort es client-side sobre la página actual.
- Sin librerías nuevas (fetch nativo + React hooks).

## Logro de la tarea

**El directorio `/tutores` ahora sirve datos reales de PostgreSQL** a través del backend .NET: búsqueda con acentos, filtro por universidad, valoración mínima y rango de precio son queries SQL server-side; el frontend muestra loading/empty/error con retry.

## Deuda / próximos pasos

1. Migrar el sort al backend cuando exista paginación UI real.
2. Conectar `level`/`availability` (requiere campos nuevos en el backend → tarea propia).
3. Endpoints de auth (`POST /api/auth/*`).
4. Motor de slots para el `nextSlot` real.
5. Docker/compose para reproducibilidad dev.

## Verificación

Ver [verification](./2026-09-28-connect-frontend-api-verification.md): gate verde + smoke completo con navegador real (12 tutores, filtros, búsqueda con acentos, empty state, limpiar filtros).
