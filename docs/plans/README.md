# docs/plans/ — Metodología por fases (AuraLearn)

Artefactos para cambios **grandes, transversales o de riesgo medio/alto**. Ver reglas completas en `.ai/PROTOCOLS.md` §5.

## Cuándo activar

Cualquiera de estas condiciones:
- Toca múltiples archivos o módulos relevantes
- Impacto transversal entre backend, frontend, workers, contratos o documentación operativa
- Riesgo medio o alto para producción
- Cambio de más de cinco archivos relevantes

## Artefactos (nombrado `YYYY-MM-DD-<tema>-<tipo>.md`)

| Artefacto | Contenido | Momento |
|-----------|-----------|---------|
| `context` | Alcance, decisiones, restricciones, dudas cerradas | Al iniciar |
| `plan` | Tareas atómicas, archivos esperados, criterios de aceptación | Tras el context |
| `verification` | Comandos ejecutados, evidencia, riesgos residuales, notas de validación | Durante/después de implementar |
| `summary` | Qué cambió, qué no cambió, deuda, próximos pasos | Al cerrar — **antes** del cierre de `.ai/` |

## Reglas de calidad

- `verification` **no reemplaza** el testing gate (`.ai/PROTOCOLS.md` §1): los comandos del gate se ejecutan igualmente.
- `summary` se completa **antes** del protocolo de cierre de `.ai/`.
- Los scripts temporales asociados a un plan viven junto a él o en `web/scripts/`/`api/scripts/` y se eliminan o archivan al cerrar.

## Estado actual

No hay planes activos. El cambio inicial de gobernanza se registró en `.ai/ADR_LOG.md` (ADR-001) y no requirió plan por fases.
