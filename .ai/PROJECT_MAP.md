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
├── api/                    # ⚙️ Backend .NET 10 — ACTIVO (ADR-003, 2026-09-28)
│   ├── AuraLearn.sln       #    Solución: Api → Application → Domain
│   ├── AuraLearn.Api/      #    Program.cs (DI/JWT/CORS/health), Controllers/, appsettings.json
│   ├── AuraLearn.Application/  # Dto/, Interfaces/ (puertos), Services/ + FluentValidation
│   ├── AuraLearn.Domain/   #    Entities/Tutor.cs, Enums/{Role,VerificationStatus}.cs
│   ├── AuraLearn.Infrastructure/  # Persistence/ (DbContext Npgsql, TutorRepository), Migrations/
    │   └── tests/AuraLearn.Tests/     # xUnit + NSubstitute (AuthService, TutorApplicationService, validadores)
│
├── portal/                 # 🌐 Portal público — RESERVADA (.gitkeep)
│
├── design/                 # 🎨 Assets de diseño
│   └── stitch/             #    Exports HTML de Stitch (AuraLearn.html), referencia visual
│
└── web/                    # 🖥️ Frontend Next.js — ACTIVO
    ├── AGENTS.md           # Advertencia: Next.js 16 difiere de datos de entrenamiento
    ├── CLAUDE.md           # Alias de AGENTS.md
    ├── package.json        # v0.2.1 SEMVER — fuente de versiones del stack frontend
    ├── tsconfig.json       # strict, alias @/* → ./*
    ├── next.config.ts      # Config Next (mínima hoy)
    ├── .env.example        # Documentación de NEXT_PUBLIC_API_URL (.env.local ignorado)
    ├── app/
    │   ├── layout.tsx      # Root layout: fuente Plus Jakarta Sans, MaterialSymbols, metadata es_CR
    │   ├── globals.css     # Tokens Material 3 light (@theme inline) + escala tipográfica MD3
    │   ├── page.tsx        # Landing: compone secciones de components/landing/
    │   ├── tutores/page.tsx# Directorio (client) — consume useTutors → GET /api/tutors (datos reales)
    │   └── postular/page.tsx# Onboarding del profesor (server) — propuesta de valor + wizard client
    ├── components/
    │   ├── material-icon.tsx # MaterialIcon + MaterialSymbols (carga única de fuentes)
    │   ├── landing/        # Secciones landing: hero, trust-strip, rigor, disciplines,
    │   │                   # mentors (mock MENTORS), steps, testimonials, tutor-cta,
    │   │                   # faq, final-cta, footer, currency-toggle (SiteHeader),
    │   │                   # section-eyebrow (eyebrow/título/subtítulo reutilizables)
    │   └── onboarding/      # Wizard /postular: onboarding-wizard (orquestador), step-account,
    │                       # step-profile, step-subjects, step-review, confirmation, shared (Stepper/Field)
    │   └── ui/             # Primitivas shadcn base-nova sobre @base-ui/react:
    │                       # accordion, avatar, badge, button, card, input,
    │                       # navigation-menu, separator, sheet
    ├── lib/
    │   ├── utils.ts        # Reexporta cn del paquete "cn"
    │   ├── api.ts          # Cliente HTTP tipado (fetchTutors, registerUser, loginUser,
    │   │                   # submitTutorApplication, fetchTutorApplicationStatus) — espejo de los DTOs
    │   ├── auth.ts         # Sesión mínima del wizard (token JWT en localStorage) — no usar fuera de /postular
    │   ├── filters.ts      # applySort — orden client-side (el filtrado es server-side)
    │   ├── types/          # Tipos de dominio (Tutor, FilterState, SortOption, Mentor)
    │   │   ├── tutor.ts    #   espejo del DTO del backend .NET
    │   │   ├── tutor-application.ts # espejo de TutorApplicationDto.cs (postulación)
    │   │   └── mentor.ts   #   tipos de los mentores destacados de la landing
    │   ├── data/           # Constantes de filtro de la UI (los datos vienen de la API)
    │   │   ├── tutors.ts   #   UNIVERSITIES, LEVELS, AVAILABILITIES, RATINGS, DEFAULT_FILTERS
    │   │   └── mentors.ts  #   MENTORS de la landing (marketing estático)
    ├── hooks/
    │   ├── use-tutors.ts   # Hook de datos del directorio (estados, AbortController, debounce)
    │   └── use-tutor-application.ts # Hook de mutación de la postulación (idle/submitting/error/success)
    └── public/             # Assets estáticos
```

## Responsabilidades clave

| Módulo | Responsabilidad | No hace |
|--------|-----------------|---------|
| `web/app/` | Rutas, metadata, composición de páginas | Lógica de negocio |
| `web/components/landing/` | Secciones de marketing y catálogo visual | Validaciones de negocio |
| `web/components/ui/` | Primitivas de UI reutilizables | Conocimiento del dominio |
| `web/components/material-icon.tsx` | Iconografía Material Symbols + carga de fuentes | — |
| `api/AuraLearn.Api` | HTTP surface: TutorsController, AuthController, TutorApplicationsController (postulación), AdminTutorApplicationsController (decisión, rol Admin), JWT bearer, CORS, healthcheck | Reglas de negocio, acceso a datos |
| `api/AuraLearn.Application` | Casos de uso (TutorService, AuthService, TutorApplicationService), DTOs, validación (FluentValidation), puertos | Detalles de persistencia |
| `api/AuraLearn.Domain` | Entidades, enums, invariantes | Dependencias externas (EF, HTTP) |
| `api/AuraLearn.Infrastructure` | EF Core (DbContext, repositorios), migraciones | Lógica de caso de uso |
| `api/tests/AuraLearn.Tests` | Unit tests de Application/Domain | — |
| `design/stitch/` | Referencia visual exportada | Código productivo (no copiar tal cual) |

## Regla de actualización

Si cambia el ownership o la responsabilidad de un módulo (nueva carpeta, nueva app, migración de lógica), actualizar este archivo en el cierre de la tarea.
