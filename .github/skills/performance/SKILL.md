# Skill — Performance

## Cuándo usarla
Al optimizar carga de la landing, listados con filtros (`/tutores`), queries EF, payloads de API o tiempos de sala realtime.

## Patrones recomendados

### Frontend
- **Server components por defecto** (patrón actual del repo); el JS del cliente solo en `tutores/page.tsx` (estado de filtros) y toggles.
- Fuentes: solo Plus Jakarta Sans con `display: swap` (ya en el root layout); los iconos Material Symbols cargan por CSS de Google Fonts — no duplicar la carga.
- Imágenes: `next/image` con tamaños explícitos; el showcase del hero usa gradientes CSS (cero imágenes pesadas — mantener esa filosofía).
- Animaciones: `tw-animate-css`/CSS; evitar re-animación de secciones completas.
- Listados: filtrar/ordenar con `useMemo` si los datos crecen; virtualizar solo sobre 200+ filas.
- Medir antes de optimizar: Lighthouse/dev tools; no optimizar a ciegas.

### Backend (cuando exista)
- Lecturas con `AsNoTracking()` y proyecciones (no materializar entidades completas para listados).
- Paginación obligatoria en catálogos (`/tutores`): `limit/offset` o cursor.
- Índices para filtros reales: `tutors(verification_status, rating desc)`, `reservations(slot_id unique)`.
- N+1 prohibido: revisar SQL generado (`dotnet ef dbcontext` logging en dev).
- Caché: respuestas de catálogo con `Cache-Control` sensato; invalidar al cambiar verificación.

### Realtime (pizarra futura)
- Throttle de eventos de dibujo; batching de mensajes; comprimir strokes (delta encoding).

## Anti-patrones
- Duplicar el font de Material Symbols en varias páginas (carga única en layout).
- `useEffect` + fetch innecesarios donde el SSR ya provee los datos.
- Array completo de tutores en memoria cuando exista API paginada.
- `ToList()` sin filtro y filtrar en .NET en vez de en SQL.
- Animaciones con JS donde CSS basta.
- Optimizar sin medida de referencia.

## Checklist final
- [ ] Medición inicial capturada (Lighthouse/perf logs)
- [ ] Server components; `"use client"` mínimos
- [ ] Paginación/índices en listados con datos reales
- [ ] `AsNoTracking` + proyecciones en queries EF
- [ ] Sin regreso: Lighthouse/perf objetivo no empeoró
- [ ] Gate estándar verde + `npm run build` si tocó bundling
