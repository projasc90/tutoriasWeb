---
id: context-budget
priority: 4
loadWhen: sesiones-largas-o-presion-de-contexto
maxLines: 60
---

# Context Budget — AuraLearn

Presupuesto orientativo de contexto por tipo de sesión. Priorizar `docs/plans/` como artefacto operativo cuando aplique.

## Presupuestos por tipo de sesión

| Tipo de sesión | Carga base | + Condicional (presupuesto máx.) |
|----------------|-----------|----------------------------------|
| Pregunta simple / chat corto | `CONTEXT_INDEX.md` | — |
| Tarea de código frontend | base + `AI_CONTEXT.md` | `CONVENTIONS.md` + `PROJECT_MAP.md` + sección relevante de `STACK.md` (~4k tokens) |
| Tarea backend / migración | base + `AI_CONTEXT.md` | `PROTOCOLS.md` + `STACK.md` (~5k tokens) |
| Debug | base | `TASK_PATTERNS.md` + errores de `.github/errors/` relevantes (~3k tokens) |
| Cambio grande (activa `docs/plans/`) | base | `plan` activo + `PROTOCOLS.md` + `PROJECT_MAP.md` (~6k tokens) |
| Cierre de sesión | base | `ACTIVE_MEMORY.md` + última entrada de `version-changes.md` (~2k tokens) |

## Reglas de compresión

1. **Nunca cargar automáticamente:** changelogs históricos de `version-changes.md` (solo la versión actual) y todo el contenido de `.ai/archive/`.
2. Al leer `SKILL.md`, si excede el presupuesto, priorizar: "cuándo usarla" + "checklist final" + patrones; omitir ejemplos largos si el patrón ya es claro.
3. En sesiones largas, resumir lecturas previas en una línea antes de leer el siguiente archivo.
4. `GLOSSARY.md` y `PROJECT_MAP.md`: leer solo la sección pertinente (están organizadas por encabezados para búsqueda).
5. Si el contexto se satura: delegar lectura masiva a un subagente y pedir solo el resumen accionable.
