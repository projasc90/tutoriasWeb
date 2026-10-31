# Summary — Documentos del tutor + menú condicionado por rol

**Fecha:** 2026-10-11 · **Versión:** 0.8.0 (frontend) · Plan `2026-10-11-tutor-documents/` cerrado.

## Qué cambió

### Backend (`api/`)
- **Nueva tabla `tutor_documents`** (migración `AddTutorDocuments` con rollback manual comentado): metadatos de los documentos de verificación (kind, file_name, content_type, size_bytes, storage_path, uploaded_at) con FK cascade a `tutors`, índice por tutor e **índice único parcial `(tutor_id, kind) WHERE kind IN (1,3)`** (Titulo/Identificacion únicos; Certificacion acumulativa).
- **Enum `TutorDocumentKind`** (Titulo=1, Certificacion=2, Identificacion=3) y entidad `TutorDocument`.
- **Puerto `IFileStorage`** + `LocalFileStorage`: disco local fuera del repo (root `Storage:DocumentsRoot`, default `~/.auralearn/uploads/documents`), nombre físico generado por el servidor `{tutorId}/{kind}/{Guid}.{ext}` (sin path traversal; defensa en profundidad con verificación de escape del root). Migrable a blob/S3 cambiando solo la implementación.
- **`ITutorDocumentRepository`** + **`TutorDocumentService`**: whitelist PDF/JPEG/PNG con extensión coherente, ≤5 MB, nombre ≤255; solo sube con `PendingReview`/`Rejected` (`Verified` → 409 `DOCUMENTS_LOCKED`); upsert de Titulo/Identificacion (borra archivo previo del storage) y máx 5 certificaciones; `GetFileAsync` para la descarga del admin.
- **`TutorDocumentsController`** (`POST/GET /api/tutors/me/documents`, multipart, `[Authorize]`, `RequestSizeLimit`) y **`AdminTutorDocumentsController`** (`GET /api/admin/tutor-documents/{id}/file`, `[Authorize(Roles="Admin")]`, streaming `File()`).
- **Gate de aprobación**: `DecideAsync(approve)` exige los 3 kinds cuando `tutor.UserId != null` (tutores seed exentos) → 409 `MISSING_DOCUMENTS` con detalle "Faltan documentos: …" (mapeado a `ProblemDetails` en el controller). `AdminTutorApplicationDto` ahora incluye `Documents` (join en `ListPendingAsync`).
- **TDD**: 13 tests nuevos de `TutorDocumentService` + 3 de gate en `TutorApplicationServiceTests` (escritos antes de la implementación). Suite: **106/106**.

### Frontend (`web/`)
- **Wizard `/postular` a 5 pasos** (Cuenta · Perfil · Cursos · **Documentos** · Revisión): nuevo `StepDocuments` con 3 tarjetas de subida (validación client-side espejo, lista de archivos con tamaño y quitar, precarga de docs ya subidos en re-postulación con "ya subido"), `useTutorDocuments` (uploads secuenciales con progreso y fallos), submit JSON → subida de documentos → `Confirmation` con **banner de reintento** si algún upload falló (la postulación ya existe; el endpoint acepta re-subida). `StepReview` resume los archivos y muestra "Subiendo documentos (n/m)…".
- **`api.ts`**: `uploadTutorDocument` (FormData), `fetchTutorDocuments`, `fetchTutorDocumentBlobUrl` (blob URL con Authorization para el admin), mapeo del 409 del gate en `verifyApplication`; tipos `TutorDocumentItem`/`TutorDocumentKindName` en `types/tutor-application.ts`; `AdminTutorApplication.documents`.
- **Menú**: `SiteHeader` filtra "Conviértete en Tutor" cuando `role ∈ {Tutor, Admin}` (el rol viene de la sesión `useAuth`).
- **Cola admin**: `ApplicationCard` con sección "Documentos de verificación" (badge por tipo, tamaño, botón "Ver" que abre blob URL y lo revoca a los 60 s), chips "Sin documento" para kinds faltantes; la página pasa el token.
- Versión **0.8.0**.

## Qué NO cambió
- El flujo de reserva/pagos, slots, disponibilidad y perfil del tutor quedan intactos (solo se añadió el puerto de documentos al servicio de postulación).
- `MENTORS` de la landing sigue estático (marketing).
- No hay página `/tutor/documentos` ni descarga del propio archivo por el tutor (fuera de alcance decidido).
- Sin refresh tokens ni rate limiting (pendiente histórico).

## Deuda y próximos pasos
1. **"Completar documentos" desde la fase "en revisión"**: postulaciones previas a esta feature quedan en 409 al aprobar; hoy se recupera rechazando y re-postulando. Mejora: acceso a subida desde `AlreadySubmitted`.
2. **Migración a blob/S3** cuando haya cuenta cloud: implementar `IFileStorage` alternativo + ADR (el puerto ya está).
3. **Ver visualmente el botón "Ver"** del admin en uso real (la descarga API está validada por curl; el popup no fue observable en el entorno de prueba).
4. Pendientes históricos: firma digital SINPE real, Testcontainers, refresh tokens, Playwright E2E permanente, pizarra realtime, notificaciones email.
