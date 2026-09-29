# Verification — Conectar frontend a GET /api/tutors

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-connect-frontend-api-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `cd web && npm run lint` | ✅ 0 errores (3 warnings pre-existentes en archivos no tocados) |
| `cd web && npx tsc --noEmit` | ✅ exit 0 |
| `cd web && npx next build` | ✅ rutas `/`, `/tutores`, `/_not-found` prerenderizadas |

## Smoke de integración (frontend dev + backend `dotnet run` simultáneos)

Verificado con navegador real sobre `http://localhost:3000/tutores`:

| Escenario | Resultado |
|-----------|-----------|
| Carga inicial | ✅ "12 tutores verificados disponibles" — datos de la BD vía API |
| Filtro universidad UCR | ✅ "5 resultados" — los 5 tutores UCR de la BD, ordenados rating desc (4.98→4.88) |
| Búsqueda "Cálculo" + UCR | ✅ 1 resultado (Dr. Carlos Solano) — acentos OK end-to-end |
| Búsqueda "zzz" | ✅ EmptyState "Sin resultados" con botón limpiar |
| Botón "Limpiar todos los filtros" | ✅ restaura los 12 tutores |
| Loading inicial | ✅ "Buscando tutores verificados..." + skeleton grid |
| Logs del backend | ✅ queries SQL correctos: filtro `verification_status = 3`, paginación `LIMIT/OFFSET`, búsqueda con `unnest(subjects)` y `LIKE` |

## Evidencia estructural

- `web/app/tutores/page.tsx` ya no importa `TUTORS`; consume `useTutors(...)` (estados loading/error/empty/success con `LoadingGrid`, `ErrorState` nuevos).
- `web/lib/data/tutors.ts` — solo constantes de filtro; el array mock fue eliminado.
- `web/lib/types/tutor.ts` — `Tutor` alineado al DTO real (`id: string`, `priceCrc`, sin `initials`/`avatarTone`; iniciales derivadas en el cliente).
- `web/lib/filters.ts` — ahora solo `applySort` client-side (el filtrado es server-side); `price` → `priceCrc`.
- `web/lib/api.ts` + `web/hooks/use-tutors.ts` — cliente tipado con `AbortController`, debounce 300ms y refetch.
- `web/.env.local` (IGNORADO por git, verificado con `git check-ignore`) + `web/.env.example` commiteable.

## Problemas resueltos durante la integración

1. **TS2345/TS2339** — el tipo `Tutor` de `lib/types` era el del mock (con `initials`/`price`/`avatarTone`); se alineó al DTO real del backend.
2. **`lib/filters.ts` con `applyFilters`** — el filtrado pasó a server-side; se conservó solo `applySort` con `priceCrc`.
3. **`debounceRef` sin uso** — eliminado del hook (el debounce se hace con `setTimeout` en el efecto).
4. **Terminal reseteaba el cwd** entre comandos — dev server arrancado con rutas absolutas al binario local de Next.

## Riesgos residuales

- Si la API cae con el frontend abierto, se muestra `ErrorState` con "Reintentar" (no probado en vivo; el manejo está cubierto en `api.ts`/hook).
- El sort client-side solo reordena la página actual (12 items); al agregar paginación UI real, el sort debería migrar al backend.
- `NEXT_PUBLIC_API_URL` default `localhost:5037`; en producción configurar por entorno.

> Nota: esta verificación no sustituye al testing gate (ejecutado por separado con evidencia arriba).
