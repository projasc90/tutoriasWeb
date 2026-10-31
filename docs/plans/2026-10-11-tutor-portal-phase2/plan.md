# Plan — Portal del tutor fase 2

**Fecha:** 2026-10-11 · **Base:** 0.8.0 → 0.9.0

## Fase A — Backend (TDD)

| # | Tarea | Archivos |
|---|-------|----------|
| A1 | Migración: índice único parcial solo para `kind = 3` (Identificacion); Titulo pasa a acumulativo | `AuraLearnDbContext.cs`, `Migrations/*TutorDocumentsAccumulateTitles*` |
| A2 | Reglas del servicio: Titulo acumulativo máx 5; `Verified` permite Titulo/Certificacion y bloquea Identificacion (409 `IDENTIFICATION_LOCKED`) | `TutorDocumentService.cs`, `TutorDocumentDto.cs` |
| A3 | Tests: título acumula, máx 5 títulos, Verified permite/agrega, Verified bloquea ID | `TutorDocumentServiceTests.cs` |
| A4 | `SubjectNormalizer` (Application): primera letra mayúscula por palabra, conectores en minúscula; aplicar en `SubmitAsync` y `UpdateProfileForCurrentUserAsync` | `SubjectNormalizer.cs`, `TutorApplicationService.cs`, `TutorProfileService.cs` |
| A5 | Tests del normalizador (submit + update) | `TutorApplicationServiceTests.cs`, `TutorProfileServiceTests.cs` |

## Fase B — Frontend

| # | Tarea | Archivos |
|---|-------|----------|
| B1 | Helper `normalizeSubject` + `suggestSubjectCorrection` (catálogo, ignora acentos/mayúsculas) | `web/lib/subjects.ts` |
| B2 | `StepSubjects` y `ProfileForm`: normalizar al agregar + sugerencia inline "¿Quisiste decir X?" con botón Usar | `step-subjects.tsx`, `profile-form.tsx` |
| B3 | Wizard: Título múltiple (1..5 acumulativo con existentes); `DocumentsData.titulos: File[]`; StepReview resumen | `step-documents.tsx`, `onboarding-wizard.tsx`, `step-review.tsx` |
| B4 | Página `/tutor/documentos` + `DocumentsManager` (agregar títulos/certs; ID bloqueada si Verified; badge de estado) | `web/app/tutor/documentos/page.tsx`, `web/components/tutor/documents/documents-manager.tsx` |
| B5 | Subnav + PanelMenu: pestaña/ítem "Documentos" | `tutor-subnav.tsx`, `currency-toggle.tsx` |

## Fase C — Verificación y cierre

- `cd api && dotnet build && dotnet test` · `cd web && npm run lint && npx tsc --noEmit && npm run build`.
- Smoke: upload de 2º título como Verified (201), 3er tipo ID (409), materias "cálculo i" → "Cálculo I" en BD.
- Análisis de gaps de la fase tutores (documento en el plan).
- Cierre: `verification.md` + `summary.md`; `.ai/` en orden; bump 0.9.0.

## Criterios de aceptación

1. Un tutor Verified puede agregar títulos y certificaciones desde `/tutor/documentos`; la identificación muestra candado y el backend responde 409 `IDENTIFICATION_LOCKED`.
2. Máx 5 títulos y 5 certificaciones (backend y frontend); el índice único solo aplica a Identificacion.
3. Toda materia guardada (wizard y perfil) inicia con mayúscula por palabra significativa; el input sugiere la forma canónica del catálogo.
4. Gates verdes; migración con rollback manual.
