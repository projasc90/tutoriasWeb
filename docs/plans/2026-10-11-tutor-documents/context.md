# Contexto — Documentos del tutor + menú condicionado por rol

**Fecha:** 2026-10-11 · **Base:** 0.7.0 · **Objetivo:** 0.8.0

## Alcance

1. **Subida de documentos de verificación** (título académico, certificaciones, identificación) como paso nuevo del wizard `/postular`, con storage en disco local y descarga protegida por JWT para el admin.
2. **Menú:** ocultar "Conviértete en Tutor" del header cuando el usuario logueado tiene rol Tutor o Admin.

## Decisiones cerradas (con el usuario)

| Decisión | Valor |
|----------|-------|
| Storage | Disco local configurable (`Storage:DocumentsRoot`, fuera del repo) tras `IFileStorage` |
| Ubicación del upload | Paso nuevo del wizard `/postular` (no página `/tutor/documentos`) |
| Obligatoriedad | Los 3 tipos exigidos al aprobar (409 si faltan); tutores seed (`UserId == null`) exentos |
| CTA del menú | Oculto para Tutor **y** Admin |
| Permisos de subida | `PendingReview`/`Rejected`; `Verified` → 409 `DOCUMENTS_LOCKED`; sin fila Tutor → 404 |
| Reglas por tipo | `Titulo`/`Identificacion` upsert (reemplaza y borra archivo previo); `Certificacion` acumulativa (máx 5) |
| Archivos | PDF/JPEG/PNG, ≤ 5 MB, nombre ≤ 255 |
| TDD | Obligatorio (PII/seguridad): tests primero en servicio y gate |

## Restricciones

- Documentos son PII: solo rol Admin los descarga (skill `tutor-verification`).
- Archivos nunca en BD ni repo; nombre de archivo en disco generado por el servidor (sin path traversal).
- Migración con rollback manual comentado (patrón `AddTutorSpecialization`).
- Controller fino; validación en servicio; resultado tipado estilo `TutorProfileResult<T>`.

## Fuera de alcance

- Página `/tutor/documentos`; descarga del propio archivo por el tutor; previews inline en admin; notificaciones email; edición de docs tras `Verified`; migración a blob/S3 (solo se define el puerto).

## Riesgos conocidos

1. Postulaciones reales ya pendientes en dev sin docs → 409 al aprobar. Recuperación: rechazar → re-postular → subir docs.
2. Flash breve del CTA antes de que `useAuth` restaure la sesión (restauración diferida al siguiente tick). Aceptado en MVP.
