# Plan — Documentos del tutor + menú condicionado por rol

**Fecha:** 2026-10-11 · **Base:** 0.7.0 → 0.8.0

## Fase A — Backend: datos + storage (independiente)

| # | Tarea | Archivos |
|---|-------|----------|
| A1 | Enum `TutorDocumentKind` (Titulo=1, Certificacion=2, Identificacion=3) | `api/AuraLearn.Domain/Enums/TutorDocumentKind.cs` |
| A2 | Entidad `TutorDocument` (Id, TutorId FK cascade, Kind, FileName ≤255, ContentType ≤100, SizeBytes, StoragePath ≤500, UploadedAt UTC) | `api/AuraLearn.Domain/Entities/TutorDocument.cs` |
| A3 | Mapping snake_case + DbSet + índice `(tutor_id)` | `api/AuraLearn.Infrastructure/Persistence/AuraLearnDbContext.cs` |
| A4 | Migración `AddTutorDocuments` con rollback manual comentado | `api/AuraLearn.Infrastructure/Migrations/*AddTutorDocuments*` |
| A5 | Puerto `IFileStorage` (SaveAsync/OpenAsync/DeleteAsync) | `api/AuraLearn.Application/Interfaces/IFileStorage.cs` |
| A6 | Impl `LocalFileStorage` (root de `Storage:DocumentsRoot`, nombre generado `{tutorId}/{kind}/{Guid}.{ext}`) | `api/AuraLearn.Infrastructure/Persistence/LocalFileStorage.cs` |
| A7 | Puerto + impl `ITutorDocumentRepository` | `Application/Interfaces/ITutorDocumentRepository.cs`, `Infrastructure/Persistence/TutorDocumentRepository.cs` |
| A8 | DTOs + `TutorDocumentResult<T>` + `TutorDocumentService` (validación tipo/tamaño/estado, upsert por tipo, borra archivo al reemplazar) | `Application/Dto/TutorDocumentDto.cs`, `Application/Services/TutorDocumentService.cs` |
| A9 | **TDD primero:** tests del servicio | `api/tests/AuraLearn.Tests/TutorDocumentServiceTests.cs` |

## Fase B — Backend: API + gate (depende de A)

| # | Tarea | Archivos |
|---|-------|----------|
| B1 | `TutorDocumentsController`: `POST/GET /api/tutors/me/documents` (multipart, `[Authorize]`) | `api/AuraLearn.Api/Controllers/TutorDocumentsController.cs` |
| B2 | `AdminTutorDocumentsController`: `GET /api/admin/tutor-documents/{id}/file` (`[Authorize(Roles="Admin")]`) | `api/AuraLearn.Api/Controllers/AdminTutorDocumentsController.cs` |
| B3 | `AdminTutorApplicationDto.Documents` (joineado en `ListPendingAsync`) | `Application/Dto/TutorApplicationDto.cs`, `Application/Services/TutorApplicationService.cs` |
| B4 | Gate en `DecideAsync(approve)`: faltan kinds → `IsConflict` con detalle (409) | `Application/Services/TutorApplicationService.cs` |
| B5 | DI + config `Storage:DocumentsRoot` | `api/AuraLearn.Api/Program.cs`, `appsettings*.json` |
| B6 | **TDD primero:** tests del gate | `api/tests/AuraLearn.Tests/TutorApplicationServiceTests.cs` |
| B7 | Smoke curl del ciclo completo | terminal |

## Fase C — Frontend wizard (depende de B)

| # | Tarea | Archivos |
|---|-------|----------|
| C1 | `uploadTutorDocument` (FormData), `fetchTutorDocuments`, `fetchTutorDocumentBlobUrl`; tipos | `web/lib/api.ts`, `web/lib/types/tutor-application.ts` |
| C2 | Hook `use-tutor-documents` | `web/hooks/use-tutor-documents.ts` |
| C3 | Stepper a 5 pasos | `web/components/onboarding/shared.tsx` |
| C4 | `StepDocuments` (3 tarjetas, validación espejo, precarga de docs ya subidos) | `web/components/onboarding/step-documents.tsx` |
| C5 | Wizard: estado documents, submit JSON → uploads → confirmation con reintento; StepReview resume archivos | `web/components/onboarding/onboarding-wizard.tsx`, `step-review.tsx`, `confirmation.tsx` |

## Fase D — Menú + admin (menú independiente; admin depende de B)

| # | Tarea | Archivos |
|---|-------|----------|
| D1 | Filtrar `NAV_ITEMS` para role Tutor/Admin en `SiteHeader` | `web/components/landing/currency-toggle.tsx` |
| D2 | `ApplicationCard` sección Documentos + "Ver" (blob URL); 409 del gate mapeado | `web/components/admin/application-card.tsx`, `web/app/admin/tutores/page.tsx`, `web/lib/api.ts` |

## Fase E — Verificación y cierre

- `cd api && dotnet build && dotnet test` (todos verdes) · `cd web && npm run lint && npx tsc --noEmit` · `cd web && npm run build`.
- Smoke navegador: wizard con archivos reales (incl. reintento), menú condicionado, cola admin con descarga.
- Cierre: `verification.md` + `summary.md`; `.ai/` en orden del protocolo; bump 0.8.0.

## Criterios de aceptación

1. El wizard pide los 3 tipos de documento y no permite avanzar sin ellos (client-side) ni confirmar sin subirlos (server-side).
2. `POST /api/tutors/me/documents` valida tipo MIME/extension, tamaño ≤5 MB y estado de verificación; reemplaza Titulo/Identificacion y acumula hasta 5 Certificaciones.
3. `DecideAsync(approve)` devuelve 409 con lista de kinds faltantes cuando el tutor tiene usuario real; los seed quedan exentos.
4. El admin ve los documentos en la cola y los descarga con JWT de Admin; un Tutor recibe 403.
5. "Conviértete en Tutor" no aparece en el header para Tutor ni Admin; sí para anónimo y Estudiante.
6. Todos los gates verdes; migración con rollback manual.
