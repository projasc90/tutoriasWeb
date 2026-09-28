---
id: conventions
priority: 2
loadWhen: escribir-codigo-o-archivos
maxLines: 110
---

# Convenciones — AuraLearn

## Idiomas

| Contexto | Idioma |
|----------|--------|
| Código (identificadores, nombres de archivo, constantes) | Inglés |
| Documentación, comentarios, commits (descripción), `.ai/`, `.github/` | Español |
| UI / copy del producto | Español (es_CR) |

## Nomenclatura por capa

### Frontend `web/`
- Componentes y sus archivos: `PascalCase` (`MentorCard.tsx`, `material-icon.tsx` admite kebab para utilidades).
- Hooks: `useXxx.ts`.
- Tipos/constantes de módulo: `PascalCase` para tipos (`Tutor`, `FilterState`), `UPPER_SNAKE` para constantes (`DEFAULT_FILTERS`, `NAV_ITEMS`).
- Alias de import: `@/*` (más prioridad) o paquete `cn` para `cn`.
- Rutas App Router: `web/app/<segmento>/page.tsx` en minúsculas (`/tutores`).

### Backend `api/` (cuando exista)
- Proyectos .NET: `AuraLearn.Api`, `AuraLearn.Application`, `AuraLearn.Domain`, `AuraLearn.Infrastructure` (arquitectura por capas, ver `PROTOCOLS.md`).
- C#: `PascalCase` en públicos, `_camelCase` en campos privados.
- Migraciones: `<NombreDescriptivo>.cs` en `api/Migrations/`.

## Estructura de carpetas

```
api/               # Backend .NET (reservada) → Migrations/ al crear EF
web/               # Frontend Next.js activo
  app/             # Rutas App Router (layout.tsx, page.tsx, tutores/)
  components/
    landing/       # Secciones de landing (server components salvo estado)
    ui/            # Primitivas shadcn/base-nova sobre @base-ui/react
  lib/             # Utilidades (utils.ts reexporta cn)
portal/            # Portal público (reservada)
design/            # Assets de diseño (Stitch exports)
.ai/               # Memoria externa del agente
docs/plans/        # Artefactos de planificación por fases
.github/           # Gobernanza del agente (instructions, skills, agents, prompts, errors, templates)
```

## Ubicación de artefactos

- **Migraciones:** `api/Migrations/` (EF) — toda migración incluye rollback manual comentado.
- **Scripts temporales:** `scripts/` en la raíz del subproyecto (`web/scripts/`, `api/scripts/`) o `docs/plans/YYYY-MM-DD-<tema>-scripts/` si están atados a un plan. Nunca dejarlos en la raíz ni dentro de `app/`. Eliminar o archivar al cerrar la tarea.
- **Tests:** cuando se añadan, en `web/` junto al módulo (`__tests__/` o `*.test.tsx`) y en `api/tests/` para .NET.
- **Planes de prueba `.md`:** `test/` en la raíz del subproyecto si se usa carpeta de planes.

## Políticas de archivo

- Máximo ~300 líneas por componente/módulo de UI; si crece, dividir por sección en `components/landing/` o extraer datos.
- **No hardcode:** URLs de API, claves, montos de negocio ni textos legales repetidos; usar constantes de módulo, `.env.local` (frontend) o `appsettings.*.json` (backend). Los secretos nunca van al repo.
- **No mocks persistentes fuera de tests:** los datos mock actuales (`TUTORS` en `app/tutores/page.tsx`, `MENTORS` en `components/landing/mentors.tsx`) son transitorios de la landing; migrar a `lib/data/` o API real en cuanto exista backend. No crear nuevos mocks para flujos productivos.
- **No código muerto ni comentado innecesario.**
- **No ignorar errores silenciosamente** (sin `catch {}` sin propósito documentado).

## Regla de migraciones y rollback manual

Toda migración o script que altere esquema, datos, índices o constraints:

1. En EF, además del método `Down()`, incluir al final del archivo una sección de comentarios `// ROLLBACK MANUAL` con los pasos inversos (SQL o comandos) para revertir sin tooling.
2. En scripts SQL sueltos, la sección `-- ROLLBACK MANUAL` es obligatoria dentro del mismo archivo.
3. No asumir rollback automático. Ver template en `.github/templates/migration-with-rollback.md`.

## Componentes UI

- Preferir server components; `"use client"` solo cuando hay estado/eventos (como `app/tutores/page.tsx`).
- Componentes de terceros (`components/ui/`) provienen de `shadcn` (estilo `base-nova`) sobre primitivas `@base-ui/react`; no editar su lógica, solo extender por composición.
- Iconos de producto: `MaterialIcon` (Material Symbols); en primitivas `ui/` se usa `lucide-react`.
- Tokens: usar las clases de escala MD3 (`text-title-md`, `bg-surface-container-low`, etc.) definidas en `app/globals.css`; no introducir colores crudos fuera del theme.
