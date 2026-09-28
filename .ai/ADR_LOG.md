---
id: adr-log
priority: 3
loadWhen: decisiones-arquitectura
maxLines: 80
---

# ADR Log — AuraLearn

## Índice rápido

| ADR | Título | Estado | Fecha |
|-----|--------|--------|-------|
| [ADR-001](#adr-001) | Configuración del sistema de memoria y gobernanza | Vigente | 2026-09-27 |
| [ADR-002](#adr-002) | shadcn/ui base-nova sobre @base-ui/react como sistema de componentes | Vigente | 2026-09-27 |

## ADRs supersedidas

Ninguna todavía.

---

## ADR-001

**Título:** Configuración del sistema de memoria y gobernanza
**Estado:** Vigente
**Fecha:** 2026-09-27

**Contexto:** El monorepo AuraLearn es multi-stack (web activo, api y portal reservados) y requiere continuidad de contexto entre sesiones del agente, reglas de ejecución verificables y conocimiento acumulado.

**Decisión:**
- Memoria externa versionada en `.ai/` (carga base: `CONTEXT_INDEX.md` + `CURRENT_STATE.md` + `AI_CONTEXT.md`).
- Punto de entrada del agente en `.github/copilot-instructions.md`, con instrucciones por `applyTo`, skills por dominio, agentes de revisión, prompts operativos, catálogo de errores y templates del stack.
- Metodología ligera por fases en `docs/plans/` para cambios grandes/transversales/riesgo medio-alto.
- Testing gate con comandos reales del repo y TDD escalado por riesgo.

**Consecuencias:** todo trabajo significativo termina con cierre de `.ai/`; los ADRs nuevos se agregan aquí y el índice se mantiene al día.

---

## ADR-002

**Título:** shadcn/ui base-nova sobre @base-ui/react como sistema de componentes
**Estado:** Vigente
**Fecha:** 2026-09-27

**Contexto:** `web/` usa componentes generados por `shadcn` (v4.21) con estilo `base-nova`, montados sobre primitivas de `@base-ui/react` (v1.8) y tokens Material 3 en `app/globals.css`.

**Decisión:** los componentes de `web/components/ui/` son la fuente única de primitivas. No se introduce otra librería UI (MUI, Chakra, etc.). El copy de producto usa Material Symbols; las primitivas usan lucide-react.

**Consecuencias:** para nuevos widgets usar `npx shadcn add <componente>`; extensiones por composición, nunca reescribiendo la primitiva.
