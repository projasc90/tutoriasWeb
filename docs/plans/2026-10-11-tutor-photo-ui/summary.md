# Summary — Foto de perfil del tutor + X en franjas

**Fecha:** 2026-10-11 · **Versión:** 0.10.0 (frontend) · Plan `2026-10-11-tutor-photo-ui/` cerrado.

## Qué cambió

### Backend (`api/`)
- **`tutors.photo_path`** (migración `AddTutorPhoto`, nullable, rollback manual): ruta de storage relativa de la foto.
- **`IFileStorage.SavePhotoAsync`** + `LocalFileStorage` (carpeta `{tutorId}/photo/`, nombre generado por el servidor).
- **`TutorPhotoService`**: JPEG/PNG/WebP ≤5 MB con extensión coherente; upsert (borra el archivo previo); `GetPhotoAsync` para el streaming público; helper `PhotoUrlOrNull`.
- **`TutorPhotosController`**: `POST /api/tutors/me/photo` (`[Authorize]`, multipart) y `GET /api/tutors/{id}/photo` **público** (la foto no es PII de verificación).
- **DTOs**: `photoUrl` en `TutorDto` (catálogo/detalle) y `TutorProfileResponse` (perfil editable).
- **TDD**: 9 tests nuevos (`TutorPhotoServiceTests`). Suite **121/121**.

### Frontend (`web/`)
- **`ProfilePhotoCard`** en `/tutor/perfil`: preview circular con fallback a iniciales, subir/reemplazar, invalidación de caché con `?v=timestamp`, avisos de éxito/error.
- **Foto en el perfil público** (`TutorProfileHeader`) y en las **tarjetas del directorio** (`/tutores`): `<img>` con `object-cover` cuando hay foto; iniciales si no.
- **`lib/api.ts`**: `uploadTutorPhoto` (FormData) + `tutorPhotoUrl`; `photoUrl` en los tipos `Tutor` (api y types/tutor) y `TutorProfile`.
- **Franjas de disponibilidad**: el botón `delete` inline se reemplaza por una **X (`close`) absoluta en la esquina superior derecha** de cada tarjeta de franja (hover → error, aria-label conservado).
- Versión **0.10.0**.

## Qué NO cambió
- Documentos de verificación (siguen siendo PII de Admin, descarga protegida).
- El wizard `/postular` no pide foto (solo desde el portal).
- Sin recorte de imagen, sin borrado de foto (solo reemplazo), sin moderación.

## Deuda y próximos pasos
1. Moderación de fotos de perfil (revisión admin) — opcional.
2. Recorte/edición de imagen en cliente — opcional.
3. Deuda histórica vigente: canal de sesión, pizarra realtime, notificaciones email, vista de ganancias (ver `gaps-analysis.md` de la fase 2).
