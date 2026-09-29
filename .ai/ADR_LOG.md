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
| [ADR-003](#adr-003) | Solución .NET Clean Architecture 4 proyectos + EF Core PostgreSQL | Vigente | 2026-09-28 |

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

---

## ADR-003

**Título:** Solución .NET Clean Architecture 4 proyectos + EF Core PostgreSQL
**Estado:** Vigente
**Fecha:** 2026-09-28

**Contexto:** `api/` estaba reservada. Se requiere un backend para reglas de negocio (SINPE, slots, verificación de tutores), persistencia PostgreSQL y emisión de JWT. El SDK disponible es .NET 10.0.401.

**Decisión:**
- Solución `api/AuraLearn.sln` con capas: `AuraLearn.Api` (webapi) → `AuraLearn.Application` → `AuraLearn.Domain`; `AuraLearn.Infrastructure` implementa puertos de Application.
- EF Core 10 (Npgsql 10.0.3) con migraciones en `AuraLearn.Infrastructure/Migrations/`; columnas snake_case; índices por filtros reales del catálogo.
- Seed del catálogo vía `HasData` estático (12 tutores); warning `PendingModelChangesWarning` suprimido explícitamente (falso-positivo de EF 10 con seed, ver `.github/errors/backend.md`).
- JWT bearer configurado desde `Jwt:Key` (appsettings, sin secretos commiteados); endpoints de auth llegan en tarea posterior.
- Tests xUnit en `api/tests/AuraLearn.Tests/`.

**Consecuencias:** todo nuevo código backend respeta la dirección de dependencias hacia Domain; los repositorios van en Infrastructure, los casos de uso en Application; el rollback manual es obligatorio en cada migración nueva.
