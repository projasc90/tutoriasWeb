---
description: Flujo de corrección de bug productivo para AuraLearn con caso de regresión obligatorio y cierre documentado
---

# Bug Fix — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md`
2. `.ai/CURRENT_STATE.md` + `.ai/TASK_PATTERNS.md` + `.github/errors/` relevantes
3. Plan de prueba del módulo afectado si existe en `test/`.

## Paso 1 — Caracterizar
- Síntoma observable exacto (qué se ve vs qué se espera).
- Alcance: ¿afecta a producción o solo dev? ¿desde cuándo? ¿qué cambió justo antes?

## Paso 2 — Reproducir y aislar
- Reproducción mínima determinista.
- Localizar capa culpable con evidencia (ver `debug-session.md` paso 2).

## Paso 3 — Corregir (con regresión)
- Fix mínimo en la capa correcta.
- **Caso de regresión obligatorio** en el plan correspondiente (unit/integration/frontend/regression) + test automatizado cuando aplique.
- Validación ligera primero, luego gate completo del área:
  - `cd web && npm run lint && npx tsc --noEmit`
  - `cd api && dotnet build && dotnet test` (cuando exista)

## Paso 4 — Documentar y cerrar
- Entrada en `.github/errors/<dominio>.md` (nombre, síntoma, causa, solución, contexto/fecha).
- Si el bug reveló patrón: `.ai/TASK_PATTERNS.md`.
- Bump SEMVER con clasificación `fix` en `.ai/version-changes.md` (si sale a producción).
- Protocolo corto de `.ai/` y confirmación "He actualizado los archivos de contexto en `.ai/`".
