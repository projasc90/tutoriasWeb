---
description: Revisión de código para AuraLearn con hallazgos primero, dos gates para cambios grandes y verificación de riesgos/regresiones/cobertura
---

# Code Review — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md` (reglas y prohibiciones)
2. `.ai/AI_CONTEXT.md` (reglas tier-1) + `.ai/PROJECT_MAP.md` (ownership)
3. `.ai/PROTOCOLS.md` (revisión en dos etapas si activa `docs/plans/`)
4. El diff a revisar y, si existe, el `plan` de `docs/plans/` asociado.

## Orden del review: hallazgos primero

### Gate 1 — Cumplimiento de especificación
- ¿Implementa lo pedido? ¿Criterios de aceptación del `plan` cubiertos?
- ¿Alcance excedido o recortado? (archivos no previstos / tareas pendientes)

### Gate 2 — Calidad técnica
- **Arquitectura:** responsabilidades correctas (web pinta; api decide), capas según `PROJECT_MAP.md`, sin lógica de negocio en el frontend.
- **Seguridad:** sin secretos, validación en backend, roles/claims correctos, sin PII en logs (usar checklist de `.github/skills/security/SKILL.md`).
- **Performance:** solo si hay evidencia de problema (regla: medir antes de optimizar).
- **Tests:** gate ejecutado (`cd web && npm run lint && npx tsc --noEmit`; backend cuando exista), tests nuevos donde correspondan, regresión para bugs.

## Riesgos y regresiones a buscar
- Cambios que rompan contratos entre capas (tipos duplicados/divergentes).
- Cambios estructurales sin `npm run build`.
- Migraciones sin rollback manual.
- Uso de APIs de Next 16 no verificadas (GOTCHA-001).
- Colores crudos/primitivas mal usadas en UI (GOTCHA-002/003).

## Faltantes de prueba
- Código nuevo sin test alguno (donde aplique).
- Bug corregido sin caso de regresión.
- Plan de prueba de `test/` no actualizado pese a cambios de comportamiento.

## Salida esperada
1. Resumen de hallazgos: bloqueantes → advertencias → sugerencias (con archivo:línea).
2. Veredicto: aprobar / aprobar con cambios / rechazar.
3. Si el cambio es grande: nota de si corresponde activar `docs/plans/` y completar `summary`.
