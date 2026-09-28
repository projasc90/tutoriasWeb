---
id: project-map
priority: 3
loadWhen: crear-utilidades-o-buscar-ubicacion
maxLines: 90
---

# Project Map — AuraLearn

Mapa semántico del monorepo: dónde vive cada cosa y quién es responsable de qué.

```
tutoriasWeb/  (monorepo raíz)
│
├── Instrucciones.md        # Prompt origen del sistema de gobernanza (solo lectura)
├── README.md               # Arquitectura general del monorepo (humano)
├── .ai/                    # Memoria externa del agente (ver CONTEXT_INDEX.md)
├── docs/plans/             # Artefactos context/plan/verification/summary
├── .github/                # Gobernanza del agente
│
├── api/                    # ⚙️ Backend .NET Core — RESERVADA (.gitkeep)
│                           #    Owner futuro: negocio, validaciones, pagos SINPE,
│                           #    slots, JWT, EF Migrations/ en api/Migrations/
│
├── portal/                 # 🌐 Portal público — RESERVADA (.gitkeep)
│
├── design/                 # 🎨 Assets de diseño
│   └── stitch/             #    Exports HTML de Stitch (AuraLearn.html), referencia visual
│
└── web/                    # 🖥️ Frontend Next.js — ACTIVO
    ├── AGENTS.md           # Advertencia: Next.js 16 difiere de datos de entrenamiento
    ├── CLAUDE.md           # Alias de AGENTS.md
    ├── package.json        # v0.1.0 SEMVER — fuente de versiones del stack
    ├── tsconfig.json       # strict, alias @/* → ./*
    ├── next.config.ts      # Config Next (mínima hoy)
    ├── app/
    │   ├── layout.tsx      # Root layout: fuente Plus Jakarta Sans, MaterialSymbols, metadata es_CR
    │   ├── globals.css     # Tokens Material 3 light (@theme inline) + escala tipográfica MD3
    │   ├── page.tsx        # Landing: compone secciones de components/landing/
    │   └── tutores/page.tsx# Directorio con filtros/orden (client component, datos mock TUTORS)
    ├── components/
    │   ├── material-icon.tsx # MaterialIcon + MaterialSymbols (carga única de fuentes)
    │   ├── landing/        # Secciones landing: hero, trust-strip, rigor, disciplines,
    │   │                   # mentors (mock MENTORS), steps, testimonials, tutor-cta,
    │   │                   # faq, final-cta, footer, currency-toggle (SiteHeader),
    │   │                   # section-eyebrow (eyebrow/título/subtítulo reutilizables)
    │   └── ui/             # Primitivas shadcn base-nova sobre @base-ui/react:
    │                       # accordion, avatar, badge, button, card, input,
    │                       # navigation-menu, separator, sheet
    ├── lib/
    │   └── utils.ts        # Reexporta cn del paquete "cn"
    └── public/             # Assets estáticos
```

## Responsabilidades clave

| Módulo | Responsabilidad | No hace |
|--------|-----------------|---------|
| `web/app/` | Rutas, metadata, composición de páginas | Lógica de negocio |
| `web/components/landing/` | Secciones de marketing y catálogo visual | Validaciones de negocio |
| `web/components/ui/` | Primitivas de UI reutilizables | Conocimiento del dominio |
| `web/components/material-icon.tsx` | Iconografía Material Symbols + carga de fuentes | — |
| `api/` (futuro) | Reglas de negocio, firma digital SINPE, slots, credenciales de tutores, JWT, persistencia | Render de UI |
| `design/stitch/` | Referencia visual exportada | Código productivo (no copiar tal cual) |

## Regla de actualización

Si cambia el ownership o la responsabilidad de un módulo (nueva carpeta, nueva app, migración de lógica), actualizar este archivo en el cierre de la tarea.
