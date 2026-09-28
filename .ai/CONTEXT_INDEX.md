---
id: context-index
priority: 1
loadWhen: always-first
maxLines: 120
---

# Índice de Contexto — AuraLearn

> **LEER SIEMPRÉ PRIMERO.** Este archivo es el punto de entrada de la memoria externa.
> No duplica reglas profundas; solo orienta la carga contextual.

## Proyecto

- **Nombre:** AuraLearn
- **Descripción:** Red de tutorías universitarias verificadas en Centroamérica (UCR, TEC, UNA, LEAD). Sesiones 1-a-1 online con pizarra digital, pago en colones (SINPE Móvil) o dólares.
- **Monorepo:** `api/` (backend .NET, reservado), `web/` (Next.js 16, activo), `portal/` (reservado), `design/` (assets Stitch)

## Carga base (siempre)

Al iniciar cualquier sesión o tarea:

1. `CONTEXT_INDEX.md` (este archivo)
2. `CURRENT_STATE.md` — estado vivo del proyecto
3. `AI_CONTEXT.md` — router de reglas tier-1

## Catálogo de archivos `.ai/` y cuándo cargarlos

| Archivo | Prioridad | Cargar cuando |
|---------|-----------|---------------|
| `CURRENT_STATE.md` | 1 | Siempre (carga base) |
| `AI_CONTEXT.md` | 1 | Siempre (carga base) |
| `STACK.md` | 2 | Tocar dependencias, versiones, setup o infraestructura |
| `CONVENTIONS.md` | 2 | Escribir código, crear archivos, nombrar módulos |
| `PROTOCOLS.md` | 2 | Ejecutar testing gate, migraciones, despliegue, seguridad |
| `PROJECT_MAP.md` | 3 | Antes de crear utilidades o buscar dónde vive una pieza |
| `ACTIVE_MEMORY.md` | 2 | Solicitudes ambiguas o continuidad de sprint |
| `TASK_PATTERNS.md` | 3 | Bugs recurrentes, gotchas, debug de errores ya vistos |
| `GLOSSARY.md` | 3 | Dudas de dominio (SINPE, slots, atestados, etc.) |
| `ADR_LOG.md` | 3 | Decisiones de arquitectura, contradicciones con ADR vigente |
| `version-changes.md` | 4 | Bump de versión, changelog al cerrar tarea |
| `CONTEXT_BUDGET.md` | 4 | Sesiones largas, presión de contexto |

**NO cargar automáticamente:** changelogs históricos en `version-changes.md` (más allá de la última versión), y todo lo que esté en `archive/`.

## Artefactos operativos (fuera de `.ai/`)

- `docs/plans/` — metodología ligera por fases para cambios grandes/transversales/riesgo medio-alto (`context`, `plan`, `verification`, `summary`).
- Ver `PROTOCOLS.md` → sección "Metodología por fases" para la regla de activación.

## Gobernanza complementaria

- `.github/copilot-instructions.md` — archivo central del agente
- `.github/instructions/` — reglas por carpeta (`applyTo`)
- `.github/skills/` — conocimiento profundo por dominio
- `.github/agents/` — agentes de revisión especializados
- `.github/prompts/` — flujos de trabajo reutilizables
- `.github/errors/` — catálogo de errores resueltos
- `.github/templates/` — plantillas del stack real

## Protocolo corto de cierre de sesión

Al terminar una tarea significativa, actualizar en este orden:

1. `CURRENT_STATE.md`
2. `ACTIVE_MEMORY.md`
3. `version-changes.md`
4. `ADR_LOG.md` si aplica
5. `PROJECT_MAP.md` si aplica
6. `TASK_PATTERNS.md` si aplica
7. Confirmación explícita: **"He actualizado los archivos de contexto en `.ai/`"**

Si el cambio activó `docs/plans/`, cerrar primero `summary` y luego hacer el cierre de `.ai/`.
