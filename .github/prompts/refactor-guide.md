---
description: Guía de refactor para AuraLearn — división del cambio, validación sin perder comportamiento y cierre de contexto
---

# Refactor Guide — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md` + `.ai/AI_CONTEXT.md`
2. `.ai/PROJECT_MAP.md` (responsabilidades actuales) + `.ai/CONVENTIONS.md` (estilo objetivo)
3. `.ai/TASK_PATTERNS.md` (gotchas al mover código entre capas)
4. Si el refactor es grande (>5 archivos, transversal): activar `docs/plans/` con plan.

## Principios
- **Refactor ≠ cambio de comportamiento.** Los tests (o el gate) deben seguir pasando sin editar expectativas, salvo que el refactor las corrija explícitamente.
- Un objetivo por refactor: extraer datos, extraer componente, extraer servicio, renombrar — no todo a la vez.

## Cómo dividir el cambio

### Refactor de módulo frontend (ej.: extraer TUTORS de `tutores/page.tsx`)
1. Crear `web/lib/data/tutors.ts` con los datos y tipos (`Tutor`, `FilterState`).
2. Importar desde la página; **no** cambiar la lógica de filtrado en el mismo commit.
3. Gate ligero del módulo: `npm run lint` + `npx tsc --noEmit` (+ prueba manual del directorio).

### Refactor estructural (ej.: mover lógica a hooks)
1. Extraer hook con la misma firma de comportamiento.
2. Actualizar un consumidor a la vez; no reescribir todo el árbol.
3. Verificar cada hito antes de continuar.

### Refactor backend (futuro)
1. Extraer servicio manteniendo el contrato del controller.
2. Tests primero que anclen el comportamiento actual.
3. Migrar consumers; eliminar el código viejo en el mismo cambio (sin código muerto).

## Validar sin perder comportamiento
- Evidencia previa: capturar salida actual (screenshots de secciones, outputs de API, snapshots de consultas).
- Gate obligatorio en cada hito: `cd web && npm run lint && npx tsc --noEmit`; `npm run build` si tocó estructura/bundling.
- Comparar evidencia después: nada debe cambiar visible salvo el objetivo del refactor.
- Cambios a más capas: gate de cada capa tocada.

## Riesgos frecuentes
- "Refactor" que introduce un fix de comportamiento camuflado → separar en dos cambios.
- Mover archivos sin actualizar alias/import (`@/*`).
- Eliminar constantes "sin uso" que otra sección usa (verificar con búsqueda global).

## Cierre
- Si activó `docs/plans/`: `verification` con comandos/evidencia + `summary` (deuda y próximos pasos).
- Protocolo corto de `.ai/` (CURRENT_STATE → ACTIVE_MEMORY → version-changes con clasificación `refactor`; PROJECT_MAP si cambió ownership).
- Confirmación: "He actualizado los archivos de contexto en `.ai/`".
