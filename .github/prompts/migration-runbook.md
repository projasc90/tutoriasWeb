---
description: Flujo de migración de base de datos para AuraLearn con rollback manual obligatorio y verificación doble
---

# Migration Runbook — AuraLearn

## Contexto a cargar antes de comenzar
1. `.github/copilot-instructions.md`
2. `.ai/PROTOCOLS.md` (§11 ORM) + skill `.github/skills/database-migrations/SKILL.md`
3. Plan de prueba del módulo afectado.

## Paso 1 — Diseño
- ¿Qué cambia: esquema, datos, índices, constraints? ¿Es destructivo?
- Cambio destructivo o de datos masivos → activar `docs/plans/` (riesgo medio/alto) + backup plan.

## Paso 2 — Creación
```bash
cd api
dotnet ef migrations add <NombreDescriptivo>
```
- Completar `Down()` correcto.
- **Añadir sección `// ROLLBACK MANUAL`** al final del archivo con los pasos inversos (SQL/comandos) — obligatorio, ver template `.github/templates/migration-with-rollback.md`.

## Paso 3 — Verificación local
- `dotnet ef database update` desde una BD vacía (valida desde cero).
- Probar el flujo de negocio afectado (slots/pagos/verificación según el cambio).
- Gate: `cd api && dotnet build && dotnet test`.

## Paso 4 — Despliegue
1. Backup de BD (siempre).
2. Desplegar código retrocompatible.
3. `dotnet ef database update`.
4. Healthcheck + smoke del flujo afectado.

## Paso 5 — Registro
- Entrada en `.ai/version-changes.md` con clasificación `db`.
- Si el esquema cambia la semántica del dominio: actualizar `GLOSSARY.md` y `PROJECT_MAP.md`.
- Cierre: protocolo corto de `.ai/` + confirmación "He actualizado los archivos de contexto en `.ai/`".
