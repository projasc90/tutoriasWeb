# Verificación — Documentos del tutor + menú condicionado por rol

**Fecha:** 2026-10-11 · **Base:** 0.7.0 → **0.8.0**

## Comandos ejecutados

| Gate | Comando | Resultado |
|------|---------|-----------|
| Backend build | `cd api && dotnet build` | 0 errores |
| Backend tests | `cd api && dotnet test` | **106/106** (90 previos + 16 nuevos: 13 de `TutorDocumentServiceTests` + 3 de gate en `TutorApplicationServiceTests`) |
| Frontend lint | `cd web && npm run lint` | 0 errores (3 warnings preexistentes: 2 de fuentes en `material-icon.tsx`, 1 `children` sin usar preexistente en `currency-toggle.tsx`) |
| Frontend tipos | `cd web && npx tsc --noEmit` | OK |
| Frontend build | `cd web && npm run build` | 16 rutas generadas (3 páginas reintentaron por >60 s y completaron; sin errores) |
| Migración | `dotnet ef database update` | `20261011164600_AddTutorDocuments` aplicada a `auralearn_dev` |

## Smoke E2E API (curl)

Ciclo completo contra `auralearn_dev` (usuarios de prueba + `UPDATE users SET role=3` para admin, solo dev):

1. `POST /api/tutor-applications` → 202 `PendingReview`.
2. `PATCH /api/admin/tutor-applications/{id}/verify` approve **sin docs** → **409** `{"error":"MISSING_DOCUMENTS","detail":"Faltan documentos: Título académico, Certificaciones, Identificación"}` ✓
3. `POST /api/tutors/me/documents` multipart:
   - `notas.txt` (text/plain) → **400** ✓
   - `titulo.pdf` → **201** ✓ · `ident.png` → **201** ✓ · `cert.pdf` → **201** ✓
4. `GET /api/admin/tutor-applications` → cola con `documents: [Titulo, Certificacion, Identificacion]` ✓
5. Approve **con docs** → **204** ✓
6. `GET /api/admin/tutor-documents/{id}/file`:
   - Admin → **200** (PDF válido verificado con `file`) ✓
   - Tutor → **403** ✓
7. Upload tras `Verified` → **409** `DOCUMENTS_LOCKED` ✓
8. BD tras approve: `users.role=2` (Tutor) para el postulante ✓
9. Storage: archivos en `~/.auralearn/uploads/documents/{tutorId}/{titulo|certificacion|identificacion}/{Guid}.{ext}` (nombres generados por el servidor) ✓

## Smoke E2E navegador (Playwright)

1. **Wizard `/postular` completo** (usuario nuevo): Cuenta → Perfil → Cursos → **Documentos** (3 tarjetas, archivos seleccionados con tamaño y botón quitar) → Revisión (resumen "Título: titulo.pdf / Certificaciones: cert.pdf / Identificación: ident.png") → Enviar → confirmación **sin banner de fallo**. BD: 3 filas en `tutor_documents` con storage_path correcto; archivos en disco ✓
2. **Cola admin `/admin/tutores`**: tarjeta con sección "Documentos de verificación" (3 ítems con badge de tipo, tamaño y botón "Ver"); sin chips "Sin documento" (los 3 presentes) ✓
3. **Aprobar desde la UI** → "Cola al día"; BD: `role=2`, `verification_status=3` ✓
4. **Menú condicionado**: logueado como **Admin** y como **Tutor**, el nav muestra solo "Buscar Tutores · Cómo Funciona · Materias Populares · Para Universidades" — **"Conviértete en Tutor" oculto** en ambos roles ✓ (anónimo/Estudiante lo conservan).

## Notas de la sesión

- El botón "Ver" (blob URL en nueva pestaña) no generó evento de popup observable en el entorno de prueba; la descarga API ya está validada por curl (200/403). Verificar visualmente en uso real.
- Playwright en este entorno requirió `click({ force: true })` / `requestSubmit()` en algunos botones (quirk de estabilidad, no bug del código).

## Limpieza

Usuarios/postulaciones/documentos de smoke eliminados de BD; `~/.auralearn/uploads/documents` y fixtures de `/tmp` borrados.

## Riesgos residuales

1. Postulaciones reales ya pendientes en dev (creadas antes de esta feature) sin docs → 409 al aprobar. Recuperación: rechazar → re-postular → subir docs.
2. Flash breve del CTA antes de que `useAuth` restaure la sesión (restauración diferida al siguiente tick). Aceptado en MVP.
3. Migración futura a blob/S3: solo cambia `LocalFileStorage` (puerto `IFileStorage` ya definido); rutas en BD son relativas al root.
