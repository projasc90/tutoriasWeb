---
description: Flujo de planificación e implementación de una feature en AuraLearn con metodología por fases y cierre de contexto
---

# Feature Planning — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md` (punto de entrada)
2. `.ai/CONTEXT_INDEX.md` + `.ai/CURRENT_STATE.md` + `.ai/AI_CONTEXT.md`
3. Según el área: `.ai/PROJECT_MAP.md` (¿dónde vive?), `.ai/STACK.md` (versiones), `.ai/CONVENTIONS.md`
4. Si el cambio activa `docs/plans/` (múltiples archivos, transversal, riesgo medio/alto, >5 archivos): crear los artefactos del plan.

## Fase 1 — Entendimiento
- Reformular la feature como requisitos verificables (qué entra, qué sale, qué NO cambia).
- Mapear el dominio: términos nuevos → `.ai/GLOSSARY.md`; ¿contradice un ADR? → advertir.
- ¿Ya existe algo similar? → revisar `.ai/PROJECT_MAP.md` antes de crear utilidades.
- Cerrar dudas con el usuario antes de diseñar (listarlas en el `context` del plan).

## Fase 2 — Diseño
- Definir capas tocadas (web/ api/) y contratos.
- Si es grande/transversal: crear `docs/plans/YYYY-MM-DD-<tema>-context.md` (alcance, decisiones, restricciones, dudas cerradas) y `...-plan.md` (tareas atómicas, archivos esperados, criterios de aceptación).
- TDD por riesgo: auth/SINPE/slots/migraciones críticas → test-first obligatorio.

## Fase 3 — Implementación
- Seguir el `plan` tarea por tarea; código en inglés, comentarios en español.
- Respetar convenciones (`.ai/CONVENTIONS.md`) y design system en `web/`.
- No introducir librerías nuevas sin ADR; no dejar mocks persistentes.

## Fase 4 — Validación
- Ejecución ligera primero: la validación más acotada del área tocada.
- Gate: `cd web && npm run lint && npx tsc --noEmit` (y `dotnet build && dotnet test` si api existe).
- `npm run build` si el cambio es estructural.
- Registrar evidencia en `docs/plans/YYYY-MM-DD-<tema>-verification.md` (no sustituye el gate).

## Fase 5 — Cierre
- Completar `summary` del plan (qué cambió, qué no, deuda, próximos pasos).
- Protocolo corto de `.ai/` en orden: CURRENT_STATE → ACTIVE_MEMORY → version-changes → ADR_LOG/PROJECT_MAP/TASK_PATTERNS si aplican.
- Confirmar: "He actualizado los archivos de contexto en `.ai/`".
