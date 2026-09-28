---
applyTo: "**"
description: Fuente canónica del protocolo de contexto, mantenimiento automático y creación dinámica de skills para AuraLearn
---

# AI Context — Protocolo canónico

## 1. Lectura inicial obligatoria

Al abrir una nueva sesión o chat, **siempre primero**: `.github/copilot-instructions.md`. Después, según la tarea (ver §2).

## 2. Mapa de carga condicional

| Necesito… | Cargo |
|-----------|-------|
| Iniciar cualquier tarea compleja | `.ai/CONTEXT_INDEX.md` + `.ai/CURRENT_STATE.md` |
| Escribir código/arquitectura | + `.ai/AI_CONTEXT.md` (router tier-1) |
| Tocar versiones/dependencias | + `.ai/STACK.md` |
| Crear archivos/utilidades | + `.ai/PROJECT_MAP.md`, luego `.ai/CONVENTIONS.md` |
| Ejecutar tests/migraciones/desplegar | + `.ai/PROTOCOLS.md` |
| Solicitud ambigua | + `.ai/ACTIVE_MEMORY.md` |
| Debug de algo ya visto | + `.ai/TASK_PATTERNS.md` + `.github/errors/` |
| Término desconocido | + `.ai/GLOSSARY.md` |
| Decisión de arquitectura | + `.ai/ADR_LOG.md` |

Presupuestos y compresión: `.ai/CONTEXT_BUDGET.md`. Cambios grandes: además el `plan` activo de `docs/plans/`.

## 3. Protocolo de cierre de sesión (orden exacto)

1. Si el cambio activó `docs/plans/`: completar `summary` primero.
2. Actualizar `.ai/` en este orden: `CURRENT_STATE.md` → `ACTIVE_MEMORY.md` → `version-changes.md` → `ADR_LOG.md` (si aplica) → `PROJECT_MAP.md` (si aplica) → `TASK_PATTERNS.md` (si aplica).
3. Confirmar: **"He actualizado los archivos de contexto en `.ai/`"**.
4. Ejecutar el testing gate aplicable (ver `PROTOCOLS.md` §1).

## 4. Mantenimiento automático (sin que el usuario lo pida)

| Evento | Acción |
|--------|--------|
| Error nuevo resuelto | Agregar a `.github/errors/<dominio>.md` |
| Error común o patrón repetido | Consolidar en catálogo de errores o `TASK_PATTERNS.md` |
| Término de negocio nuevo | Agregar a `GLOSSARY.md` |
| Patrón recurrente descubierto | Agregar a `TASK_PATTERNS.md` |
| Necesidad recurrente sin skill | Crear skill nueva (ver §5) y registrarla en `copilot-instructions.md` §4 |
| Skill desactualizada | Actualizar su `SKILL.md` |
| Cambio grande terminado | Cerrar `docs/plans/` y luego actualizar `.ai/` |
| Cambio de ownership de módulo | Actualizar `PROJECT_MAP.md` |

## 5. Criterio para crear skills nuevas de forma dinámica

Crear una skill **proactivamente** cuando:
- El dominio es recurrente (≥2 tareas futuras previsibles: p. ej. SINPE, slots, pizarra realtime).
- Es complejo o con reglas no obvias que un agente nuevo fallaría sin la skill.
- No está bien cubierto por las skills existentes ni por `.ai/`.

No crear skills para tareas one-shot o triviales.

## 6. Reglas al crear skills, prompts, instrucciones o catálogos

- Skill: `.github/skills/<nombre>/SKILL.md` con: cuándo usarla, patrones recomendados, anti-patrones, checklist final, ejemplos reales del stack (no pseudocódigo).
- Registrar toda skill nueva en la tabla §4 de `copilot-instructions.md`; si debe cargarse por contexto, añadir su disparador en las instructions correspondientes.
- Si la skill cambia la orientación general del sistema: solo referencia breve en `CONTEXT_INDEX.md`, sin duplicar tablas.
- Prompt nuevo: frontmatter + sección "Contexto a cargar" + pasos accionables.
- Error nuevo en catálogo: nombre, mensaje observable, causa, solución, contexto/fecha.
- **No dejar placeholders `{{...}}` sin reemplazar.**

## 7. Límites de tamaño y compresión

- `CURRENT_STATE.md`: máx **60 líneas** (se reemplaza completo en cada cierre).
- `ACTIVE_MEMORY.md`: máx **5 entradas** (actual arriba detallada, anteriores en tabla).
- Skills: preferir ≤150 líneas; ejemplos mínimos y reales.
- Archivos que crezcan de más → mover lo viejo a `.ai/archive/` o `.ai/version-changes.md` (histórico sin carga automática).
- Presupuesto por sesión: ver `CONTEXT_BUDGET.md`.
