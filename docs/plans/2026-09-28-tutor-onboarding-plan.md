# Plan — Onboarding del Profesor (postulación a tutor)

**Fecha:** 2026-09-28
**Contexto:** [context](./2026-09-28-tutor-onboarding-context.md)

## Tareas atómicas

| # | Tarea | Archivos | Estado |
|---|-------|----------|--------|
| 1 | Domain: `Tutor.UserId` (vínculo User↔Tutor) | `Domain/Entities/Tutor.cs` | ✅ hecha |
| 2 | **Tests FIRST** (TDD obligatorio) | `tests/AuraLearn.Tests/TutorApplicationServiceTests.cs` | ✅ hecha (18 tests) |
| 3 | Application: DTOs + validador + `TutorApplicationService` + puertos | `Application/{Dto/TutorApplicationDto.cs,Interfaces/ITutorRepository.cs,Services/TutorApplicationService.cs}` | ✅ hecha |
| 4 | Infrastructure: métodos de repos + migración `AddTutorUserId` (a mano, con Designer + Snapshot) | `Persistence/{TutorRepository.cs,Migrations/*}` | ✅ hecha |
| 5 | Api: `TutorApplicationsController` + `AdminTutorApplicationsController` + wiring | `Api/Controllers/*`, `Program.cs` | ✅ hecha |
| 6 | Gate backend: `dotnet build && dotnet test` | — | ✅ 25/25 |
| 7 | Frontend: tipos + `api.ts` (submit/status) + hook de postulación | `web/lib/{api.ts,types/tutor-application.ts}`, `web/hooks/use-tutor-application.ts` | ✅ hecha |
| 8 | Frontend: wizard `/postular` (4 pasos + confirmación) con tokens MD3 | `web/app/postular/page.tsx`, `web/components/onboarding/*` | ✅ hecha |
| 9 | Frontend: enlazar CTAs "Postular como Docente" → `/postular` | `web/components/landing/tutor-cta.tsx`, `currency-toggle.tsx` | ✅ hecha |
| 10 | Gate frontend: `npm run lint && npx tsc --noEmit && npm run build` | — | ✅ verde |
| 11 | Smoke E2E: postular → pending → admin approve/reject | — | ✅ 19 verificaciones |
| 12 | Cierre: verification, summary, `.ai/`, bump versión | — | ✅ hecha |

## Contrato de API (nuevo)

```
POST /api/tutor-applications          (auth) → 202 {id, status:"PendingReview"} | 400 | 409 (ya postuló y no está Rejected)
GET  /api/tutor-applications/status   (auth) → 200 {status, submittedAt} | 404 (sin postulación)
GET  /api/admin/tutor-applications    (Admin) → 200 [{id, name, ...}]
PATCH /api/admin/tutor-applications/{id}/verify (Admin) {decision:"approve"|"reject", reason?} → 204 | 400 | 404
```

## Wizard `/postular` (4 pasos)

1. **Cuenta** — nombre, email, password (usa `POST /api/auth/register`; si ya tiene sesión, se salta).
2. **Perfil académico** — credenciales (título), universidad (UCR/TEC/UNA/LEAD/ULACIT/U Latina), bio.
3. **Cursos y tarifa** — materias (chips), tarifa CRC/hora (USD derivado).
4. **Revisión y envío** — resumen + checkbox de compromiso → `POST /api/tutor-applications` → pantalla de confirmación (≤48 h).

## Criterios de aceptación

1. Tests de `TutorApplicationService` escritos **antes** de la implementación y verdes al final.
2. `POST /api/tutor-applications` → 202 con `PendingReview`; el postulante NO aparece en `GET /api/tutors`.
3. Aprobación admin → `Verified` (aparece en catálogo); rechazo → `Rejected` + motivo; re-postulación permitida.
4. Migración con `// ROLLBACK MANUAL`; FK `tutors.user_id` → `users.id` (delete cascade).
5. Wizard `/postular` con estados loading/error/success, validación por paso, a11y (labels, aria), tokens MD3.
6. CTAs "Postular como Docente" apuntan a `/postular`.
7. Gates verdes: `dotnet build && dotnet test` · `npm run lint && npx tsc --noEmit` · `npm run build`.
