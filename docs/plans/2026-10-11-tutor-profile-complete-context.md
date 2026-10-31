# Contexto — Completar sección del tutor (títulos, materias, alta y horarios)

**Fecha:** 2026-10-11 · **Solicitud:** "enfocarnos en la sección del tutor para completarla: títulos, materias que puede dar clases, datos para darse de alta y horarios disponibles".

## Estado previo (exploración)

- El wizard `/postular` recoge el alta (1 título string, materias tags, bio, tarifa) pero **congela** los datos tras aprobarse: no existía `GET/PUT /api/tutors/me` de perfil.
- **Bloqueo crítico descubierto:** `TutorApplicationService.DecideAsync(approve)` solo cambiaba `VerificationStatus`; nadie asignaba `Role.Tutor` en `users` → `/tutor/disponibilidad`, `/tutor/sesiones` y `complete` daban 403 al tutor real (el JWT lleva el rol del login).
- No existía pantalla "mi perfil de tutor" ni layout/subnav en `/tutor/*`; `PanelMenu` solo linkeaba Sesiones y Disponibilidad.
- Entidad `Tutor` sin campo de especialización; `Credentials` string único.
- Diseño Stitch `05-panel-disponibilidad.png` define la visión: panel con tarjeta "Estado del Perfil Docente" + resumen de perfil + horarios.

## Decisiones (alineadas con el usuario)

1. **Alcance:** ambos — wizard recoge el alta Y `/tutor/perfil` permite ver/editar después.
2. **Títulos:** Título académico + campo Especialización (requiere migración aditiva; corrección del estimado inicial "sin migración").
3. **Upload de atestados/documentos:** diferido (ADR-004 manual vigente; queda en deuda).
4. **Fix de promoción de rol:** incluido (bloqueo crítico).
5. Editables por el tutor: `Credentials`, `Specialization`, `University`, `Bio`, `Subjects`, precios. No editables: `Rating`, `Reviews`, `Featured`, `VerificationStatus`, `Name`.
6. Estados que editan perfil: `PendingReview` y `Verified`; `Rejected` → 409 `PROFILE_REJECTED_USE_REAPPLY` (la re-postulación actualiza todo).

## Alcance excluido

Upload de documentos (storage + URLs firmadas), catálogo maestro de materias (siguen tags libres), dashboard "Resumen" del panel, reseñas reales, filtro por nivel académico, refresh tokens.
