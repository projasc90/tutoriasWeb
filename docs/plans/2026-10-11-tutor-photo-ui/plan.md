# Plan — Foto de perfil del tutor + X en franjas

**Fecha:** 2026-10-11 · **Base:** 0.9.0 → 0.10.0

## Fase A — Backend foto (TDD)

| # | Tarea | Archivos |
|---|-------|----------|
| A1 | `Tutor.PhotoPath` (string?, ≤500) + mapping + migración `AddTutorPhoto` (rollback manual) | `Tutor.cs`, `AuraLearnDbContext.cs`, `Migrations/*` |
| A2 | `IFileStorage.SavePhotoAsync` + impl | `IFileStorage.cs`, `LocalFileStorage.cs` |
| A3 | DTOs: `TutorPhotoResponse`; `PhotoUrl` en `TutorDto` y `TutorProfileResponse` | `TutorDto.cs`, `TutorProfileDto.cs`, `TutorPhotoDto.cs` |
| A4 | `TutorPhotoService` (validación imagen ≤5MB, upsert borra previo, `GetPhotoAsync`) | `TutorPhotoService.cs` |
| A5 | **TDD:** tests del servicio | `TutorPhotoServiceTests.cs` |
| A6 | `TutorPhotosController`: `POST /api/tutors/me/photo` `[Authorize]` + `GET /api/tutors/{id}/photo` público | `TutorPhotosController.cs` |
| A7 | `TutorService` (PhotoUrl en Search/GetById) + `TutorProfileService.ToResponse` + DI | `TutorService.cs`, `TutorProfileService.cs`, `Program.cs` |

## Fase B — Frontend

| # | Tarea | Archivos |
|---|-------|----------|
| B1 | `uploadTutorPhoto`, `tutorPhotoUrl`; tipos `photoUrl` | `lib/api.ts`, `lib/types/tutor-profile.ts` |
| B2 | `ProfilePhotoCard` en `/tutor/perfil` (preview + subir/reemplazar) | `components/tutor/profile/profile-photo-card.tsx`, `app/tutor/perfil/page.tsx` |
| B3 | Foto en `TutorProfileHeader` (público) y tarjeta del directorio | `tutor-profile-header.tsx`, `app/tutores/page.tsx` |
| B4 | X en esquina superior derecha de cada franja (quitar delete inline) | `components/tutor/availability/day-column.tsx` |

## Fase C — Verificación y cierre

- `dotnet build && dotnet test` · `npm run lint && npx tsc --noEmit && npm run build`.
- Smoke: upload foto → GET público 200 → DTO con photoUrl; navegador: preview en perfil, foto en perfil público, X en franjas.
- Cierre: verification + summary; `.ai/`; bump 0.10.0.

## Criterios de aceptación

1. El tutor sube/reemplaza su foto desde `/tutor/perfil`; la foto aparece en su perfil público y en el directorio; sin foto → iniciales.
2. `GET /api/tutors/{id}/photo` es público y sirve la imagen con su content-type; solo JPEG/PNG/WebP ≤5 MB.
3. Cada franja de disponibilidad muestra una X en su esquina superior derecha que la elimina.
4. Gates verdes; migración con rollback manual.
