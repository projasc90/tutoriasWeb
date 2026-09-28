---
id: ai-context
priority: 1
loadWhen: always
maxLines: 40
---

# AI Context — Router Tier-1 (AuraLearn)

Router corto de carga. No duplica reglas profundas; apunta al archivo fuente.

## Reglas tier-1

1. **Separación de responsabilidades:** `web/` nunca accede a base de datos ni valida pagos ni ejecuta reglas de negocio; todo eso es del backend `.NET` en `api/`.
2. **Testing gate obligatorio tras cambios de código:** frontend `cd web && npm run lint && npx tsc --noEmit`; backend cuando exista `cd api && dotnet build && dotnet test`. Ejecución ligera: validar primero el subconjunto más acotado del área tocada (ver `PROTOCOLS.md`).
3. **Idiomas:** código en inglés, docs y comentarios en español. Commits en Conventional Commits.
4. **Next.js 16 no es el que conoces:** APIs y convenciones pueden diferir de los datos de entrenamiento; verificar en `web/node_modules/next/dist/docs/` antes de escribir código (ver `web/AGENTS.md`).
5. **Migraciones con rollback manual:** toda migración/script que altere esquema o datos incluye rollback manual comentado en el mismo archivo, además del `Down()` de EF.

## Mapa de carga condicional

| Necesito… | Cargo |
|-----------|-------|
| Versiones exactas / dependencias | `STACK.md` |
| Dónde crear un archivo / ownership | `PROJECT_MAP.md` |
| Ejecutar tests / migrar / desplegar | `PROTOCOLS.md` |
| Estilo de código, nomenclatura | `CONVENTIONS.md` |
| Continuidad de sprint / solicitud ambigua | `ACTIVE_MEMORY.md` |
| Bug ya visto / gotcha | `TASK_PATTERNS.md` |
| Término de dominio | `GLOSSARY.md` |
| Decisión de arquitectura previa | `ADR_LOG.md` |

Presupuesto por tipo de sesión: `CONTEXT_BUDGET.md`.
