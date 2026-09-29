# Context — Conectar frontend a GET /api/tutors

**Fecha:** 2026-09-28
**Plan asociado:** [plan](./2026-09-28-connect-frontend-api-plan.md)

## Alcance

El directorio `/tutores` de `web/` pasa de datos mock locales a datos reales del backend .NET (`GET /api/tutors`, corriendo en `localhost:5037`):

- Nuevo cliente HTTP tipado `web/lib/api.ts` (fetch nativo, sin librerías).
- Nuevo hook `web/hooks/use-tutors.ts` con estados explícitos, AbortController y debounce del query.
- `app/tutores/page.tsx` consume el hook; UI idéntica.
- `web/lib/data/tutors.ts` queda solo con constantes de filtro (el array `TUTORS` se elimina).
- `web/.env.local` (ignorado) + `web/.env.example` (commiteado) para `NEXT_PUBLIC_API_URL`.

## Decisiones

1. **Sort client-side sobre la página actual:** no se amplía el contrato API en esta tarea (el backend ya ordena por rating desc; el select del frontend reordena la página recibida).
2. **`level`/`availability` siguen sin filtrar:** no existen en el contrato API; se muestran en la UI como hoy (comportamiento ya documentado en `lib/filters.ts`).
3. **MENTORS de la landing NO se conecta:** contenido de marketing estático (decisión previa en extract-mock-data).
4. **Sin librerías nuevas:** fetch nativo + React hooks. React Query sería ADR aparte si se necesita caché.
5. **Debounce 300ms** del query de búsqueda para no disparar un request por tecla.
6. **Puerto configurable:** `NEXT_PUBLIC_API_URL` (default `http://localhost:5037`, el puerto real de `launchSettings.json`).

## Restricciones

- El backend debe estar corriendo para el smoke (`dotnet run` en `api/AuraLearn.Api`).
- CORS del backend ya permite `http://localhost:3000` (configurado en el scaffold).
- Sin cambios visuales: mismos componentes, mismos estados vacíos, misma paginación visual (una página de 12).

## Dudas cerradas

- **Manejo de error:** mensaje accionable en español + botón "Reintentar" (patrón del template `data-hook.md`).
- **AbortController:** sí, para cancelar requests obsoletos cuando los filtros cambian rápido.
- **Bump:** `0.2.1` con clasificación `feat` (primera integración real frontend↔backend).
