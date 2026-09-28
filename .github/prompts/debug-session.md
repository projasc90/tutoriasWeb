---
description: Flujo de depuración estructurada para AuraLearn con reproducción, aislamiento, corrección y prevención
---

# Debug Session — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md`
2. `.ai/CURRENT_STATE.md` (estado vivo) + `.ai/TASK_PATTERNS.md` (¿bug ya visto?)
3. Catálogo relevante: `.github/errors/` (¿error documentado?)
4. Si hay plan activo: el `plan` de `docs/plans/` en curso.

## Paso 1 — Reproducción
- Definir la secuencia exacta para reproducir (URL, rol, datos, entorno).
- Frontend: reproducir en `npm run dev` y anotar consola/network. Backend (futuro): correlacionar con logs y correlation-id (auditoría funcional vs logs técnicos).

## Paso 2 — Aislamiento
- Delimitar la capa culpable: UI (web) → contrato (fetch/API) → backend (validación/negocio) → datos (EF/BD).
- Verificar supuestos con evidencia, no suposiciones: breakpoints, logs, queries directas.
- Revisar `.ai/TASK_PATTERNS.md` y `.github/errors/` antes de hipótesis nuevas (ej.: Next 16 difiere de lo conocido — GOTCHA-001).

## Paso 3 — Corrección
- Fix mínimo en la capa correcta (no parches de máscara en otra capa).
- Validación ligera primero: test/suite del área tocada antes de la suite completa.
- Gate: `cd web && npm run lint && npx tsc --noEmit` (+ `dotnet build && dotnet test` si aplica).

## Paso 4 — Prevención
- **Caso de regresión obligatorio:** añadir al plan de prueba correspondiente (unit/integration/frontend) y, cuando aplique, test automatizado de regresión.
- Documentar en `.github/errors/<dominio>.md` (nombre, síntoma, causa, solución, contexto) y, si es un patrón transversal, en `.ai/TASK_PATTERNS.md`.
- Si el bug reveló un hueco de observabilidad, señalarlo (logs faltantes, correlación).
- Cierre: protocolo corto de `.ai/` (CURRENT_STATE → ACTIVE_MEMORY → version-changes → TASK_PATTERNS) y confirmación "He actualizado los archivos de contexto en `.ai/`".
