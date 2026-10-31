# Contexto — Foto de perfil del tutor + X en franjas de horario

**Fecha:** 2026-10-11 · **Base:** 0.9.0 · **Objetivo:** 0.10.0

## Alcance

1. **Foto de perfil:** el tutor sube su foto desde `/tutor/perfil` (portal); aparece en el perfil público (`/tutores/[id]`), en las tarjetas del directorio y como preview en el propio formulario. La foto es pública (no PII de verificación): `GET /api/tutors/{id}/photo` sin autenticación.
2. **Franjas de horario:** el botón de borrar inline de cada franja se reemplaza por una **X en la esquina superior derecha** de la tarjeta de franja.

## Decisiones

| Decisión | Valor |
|----------|-------|
| Storage de la foto | `IFileStorage.SavePhotoAsync` (misma raíz, carpeta `{tutorId}/photo/`), nombre generado por el servidor |
| Formatos | JPEG/PNG/WebP, ≤ 5 MB, extensión coherente |
| Upsert | Subir una foto nueva reemplaza la anterior (borra el archivo previo) |
| Visibilidad | Pública vía `GET /api/tutors/{id}/photo` (streaming con content-type); DTOs exponen `photoUrl` relativo |
| Sin foto | Fallback a iniciales (comportamiento actual) |
| Franjas | Solo UI: X (`close`) absoluta top-right; sin cambios de backend |

## Fuera de alcance

- Recorte/edición de imagen; foto en la postulación inicial (wizard); borrado de foto (solo reemplazo); moderación de fotos.
