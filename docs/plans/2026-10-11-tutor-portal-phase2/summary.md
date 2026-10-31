# Summary — Portal del tutor fase 2

**Fecha:** 2026-10-11 · **Versión:** 0.9.0 (frontend) · Plan `2026-10-11-tutor-portal-phase2/` cerrado.

## Qué cambió

### Backend (`api/`)
- **Títulos acumulables:** migración `TutorDocumentsAccumulateTitles` — el índice único parcial `(tutor_id, kind)` ahora solo aplica a `kind = 3` (Identificacion); Título y Certificacion acumulan máx 5 c/u (rollback manual comentado).
- **Post-aprobación:** con `Verified` el tutor puede **agregar** títulos y certificaciones (quedan disponibles para el equipo sin re-abrir el estado); la identificación responde **409 `IDENTIFICATION_LOCKED`** (nuevo código de error). `DOCUMENTS_LOCKED` queda para estados no contemplados.
- **`SubjectNormalizer`** (Application): primera letra mayúscula por palabra significativa, conectores en minúscula ("cálculo i" → "Cálculo I"); preserva mayúsculas internas (TOEFL). Aplicado en `SubmitAsync` (creación y re-postulación) y `UpdateProfileForCurrentUserAsync` — regla de producto: las materias se guardan SIEMPRE normalizadas.
- **TDD:** tests reescritos/ampliados (título acumula, máx 5 títulos, Verified permite/agrega, Verified bloquea ID, normalización en submit y update). Suite **112/112**.

### Frontend (`web/`)
- **`lib/subjects.ts`:** `normalizeSubject` (espejo del backend) + `suggestSubjectCorrection` (compara contra el catálogo ignorando acentos/mayúsculas, por igualdad o prefijo). Sugerencia inline "¿Quisiste decir X? [Usar]" en `StepSubjects` (wizard) y `ProfileForm` (perfil).
- **Wizard:** `DocumentsData.titulos: File[]` — el paso Documentos pide 1..5 títulos acumulables (con cupos compartidos con lo ya subido); `StepReview` resume "Títulos: …".
- **`/tutor/documentos`** (nueva pestaña en subnav + ítem "Mis documentos" en PanelMenu): `DocumentsManager` agrupa por tipo con contadores n/5, permite agregar títulos/certificaciones en cualquier estado, muestra la identificación con candado cuando el perfil está Verified, avisos de éxito/error y recarga tras subir. Guard client-side + empty-state con CTA a `/postular` si no hay fila de tutor.
- **`lib/documents.ts`:** validación de archivos compartida (wizard + portal).
- Versión **0.9.0**.

## Qué NO cambió
- El gate de aprobación (3 kinds) y el flujo de reserva/pagos quedan intactos.
- Sin borrado de documentos, sin descarga del propio archivo por el tutor, sin re-verificación de docs nuevos (fuera de alcance decidido).

## Análisis de gaps (entregable nuevo)
`gaps-analysis.md` — 16 gaps priorizados (P1–P3) para completar la fase de tutores. Los 4 críticos: **canal de sesión** (no hay cómo entrar a una sesión reservada), **pizarra digital realtime**, **notificaciones email** y **vista de ganancias** (`TutorPayout` existe pero el tutor no la ve).

## Deuda y próximos pasos
1. P1 del análisis de gaps: canal de sesión + pizarra + notificaciones + ganancias.
2. Catálogo maestro de materias en BD (la sugerencia hoy usa 10 materias estáticas).
3. Decisión de producto: re-verificación de documentos agregados post-aprobación.
4. Deuda histórica: rate limiting/refresh tokens, Playwright E2E permanente, Testcontainers, firma digital SINPE.
