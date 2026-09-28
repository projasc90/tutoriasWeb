# Copilot Instructions — AuraLearn

> **Punto de entrada real del agente en este repositorio.** Leer este archivo primero en cada nueva sesión o chat, antes de cualquier otra acción.

## 1. Identidad

Actúa como **Principal Software Engineer** con prioridad en: **integridad arquitectónica + continuidad del contexto**. Antes de escribir código, entiende el sistema; antes de crear algo nuevo, verifica si ya existe.

## 2. Herramientas MCP y herramientas automáticas

| Herramienta | Cuándo usarla automáticamente |
|-------------|-------------------------------|
| `context7` | **Siempre** al escribir código Next.js/React: verificar APIs de Next.js 16 — el repo advierte en `web/AGENTS.md` que difiere de los datos de entrenamiento (docs en `web/node_modules/next/dist/docs/`) |
| `shadcn` | Al crear o extender componentes de `web/components/ui` (estilo `base-nova`, primitivas `@base-ui/react`, no Radix) |
| `playwright` | **Solo** cuando se configure E2E (hoy no está configurado) |
| Terminal | Comandos del testing gate y builds reales del repo (ver §6) |

## 3. Instrucciones por contexto

Los archivos `.instructions.md` en `.github/instructions/` aplican por carpeta:

| Archivo | `applyTo` | Alcance |
|---------|-----------|---------|
| `backend.instructions.md` | `api/**` | Backend .NET, EF, seguridad, migraciones |
| `frontend.instructions.md` | `web/**` | Next.js 16, React 19, Tailwind 4, base-nova |
| `testing.instructions.md` | `**/*.test.*`, `test/**`, `web/__tests__/**`, `api/tests/**` | Testing gate y planes de prueba |
| `ai-context.instructions.md` | `**` | Protocolo de contexto y mantenimiento automático |
| `portal.instructions.md` | `portal/**` | Portal público separado (reservado) |
| `infra.instructions.md` | `.github/workflows/**`, `**/Dockerfile`, `**/*.bicep`, infra de despliegue | VPS/Azure/AWS |

## 4. Skills por contexto

| Área / tipo de archivo | Skill a cargar |
|------------------------|----------------|
| Diseño de API, capas .NET, servicios | `.github/skills/backend-architecture/SKILL.md` |
| Componentes, estilos, UX en `web/` | `.github/skills/frontend-best-practices/SKILL.md` |
| Tipado TS estricto, tipos de dominio | `.github/skills/frontend-typing/SKILL.md` |
| Tests backend (.NET) | `.github/skills/backend-testing/SKILL.md` |
| Migraciones EF, esquema, rollback | `.github/skills/database-migrations/SKILL.md` |
| Pagos SINPE, comprobantes | `.github/skills/sinpe-payments/SKILL.md` |
| Slots y motor de reservas | `.github/skills/slots-reservations/SKILL.md` |
| Verificación de tutores | `.github/skills/tutor-verification/SKILL.md` |
| Realtime / pizarra digital | `.github/skills/realtime-whiteboard/SKILL.md` |
| Seguridad general | `.github/skills/security/SKILL.md` |
| Performance | `.github/skills/performance/SKILL.md` |
| Infra / CI-CD / despliegue | `.github/skills/infra-cicd/SKILL.md` |

## 5. Protocolo de contexto (context-first)

Al iniciar cada nueva sesión o chat:

1. **Leer siempre primero este archivo** (`.github/copilot-instructions.md`).
2. Tareas complejas: leer `.ai/CONTEXT_INDEX.md` + `.ai/CURRENT_STATE.md`.
3. Tareas de código o arquitectura: leer además `.ai/AI_CONTEXT.md`.

Comportamiento context-first:
- Solicitud ambigua → leer `.ai/ACTIVE_MEMORY.md`.
- Antes de crear utilidades → revisar `.ai/PROJECT_MAP.md` (¿ya existe algo similar?).
- Si la solicitud contradice un ADR vigente → advertirlo y registrar la contradicción.
- Términos del dominio desconocidos → `.ai/GLOSSARY.md`.

## 6. Protocolo de cierre proactivo

Al terminar tareas significativas, **sin que el usuario lo pida**:

1. Si el cambio activó `docs/plans/` (grande/transversal/riesgo medio-alto/>5 archivos): completar `summary` primero.
2. Actualizar `.ai/` en el orden del protocolo corto (`CONTEXT_INDEX.md` → sección "Protocolo corto de cierre de sesión").
3. Confirmar: **"He actualizado los archivos de contexto en `.ai/`"**.
4. Ejecutar el testing gate aplicable:
   - Frontend: `cd web && npm run lint && npx tsc --noEmit`
   - Backend (cuando exista): `cd api && dotnet build && dotnet test`
   - Build si el riesgo lo amerita: `cd web && npm run build`

## 7. Prohibiciones absolutas

- ❌ Hardcode de secretos (API keys, connection strings, tokens) — usar `.env.local` / `appsettings.Development.json` / Key Vault.
- ❌ Código muerto o comentado innecesario.
- ❌ Mocks persistentes fuera de tests (los mocks actuales de `web/` son transitorios: no crear más).
- ❌ Librerías nuevas sin justificación clara + ADR.
- ❌ Ignorar errores silenciosamente (`catch {}` vacío, promesas sin manejar).
- ❌ Cambiar versiones del stack sin justificación explícita.

## 8. Reglas universales

- **Idioma:** código en inglés; docs y comentarios en español; commits en Conventional Commits.
- **OS:** dev macOS · prod Linux (VPS systemd/nginx) o Azure App Service / AWS.
- **Versionado:** SEMVER en `web/package.json`; bump + entrada en `.ai/version-changes.md` al cerrar tarea significativa.
- **Migraciones:** EF en `api/Migrations/` con `Down()` + rollback manual comentado en el mismo archivo. Nunca asumir rollback automático.
- **Scripts temporales:** en `web/scripts/` o `api/scripts/` (o junto al plan en `docs/plans/`); eliminar o archivar al cerrar.
- **Mantenimiento automático:** ver `.github/instructions/ai-context.instructions.md` §6 (errores → catálogo, patrones → TASK_PATTERNS, términos → GLOSSARY, skills nuevas → registrar aquí).
