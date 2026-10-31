# Summary — Completar sección del tutor (0.7.0)

**Fecha de cierre:** 2026-10-11 · **Versión:** frontend 0.7.0 · **Tests:** 90/90 backend · Gates verdes (backend + frontend + build).

## Qué cambió

- **Promoción de rol (fix crítico):** al aprobar una postulación, `TutorApplicationService.DecideAsync` ahora promueve `users.role` a `Tutor` (vía nuevo `IUserRepository.UpdateAsync`). Los endpoints `[Authorize(Roles="Tutor")]` (disponibilidad, sesiones, complete) dejan de dar 403 al tutor real. El JWT conserva el rol del login → el tutor aprobado debe re-iniciar sesión (nota visible en la tarjeta de estado).
- **Perfil editable del tutor:** `GET/PUT /api/tutors/me/profile` (`TutorProfileController` + `TutorProfileService` + `UpdateTutorProfileValidator`). Editables: título, especialización, universidad, bio, materias (1–8), tarifas (₡1000–50000 / $1–100). No editables: Rating, Reviews, Featured, estado de verificación, nombre. `Rejected` → 409 `PROFILE_REJECTED_USE_REAPPLY`.
- **Especialización:** nueva columna nullable `tutors.specialization` (migración `AddTutorSpecialization` con rollback manual); fluye por postulación, cola admin, DTO público y perfil.
- **Frontend:** `/tutor/perfil` (formulario de alta + tarjeta de estado con progreso de verificación y CTA de re-postulación), layout `/tutor` con subnav (Perfil·Disponibilidad·Sesiones), `/tutor` → redirect, link "Mi perfil de tutor" en `PanelMenu`, especialización en perfil público y tarjeta admin.
- **Wizard `/postular`:** paso Perfil pide Especialización (opcional); resumen y confirmation actualizados (timeline apunta a configurar horarios tras la aprobación).

## Deuda explícita restante

- Upload de atestados/documentos del título (storage + URLs firmadas) — diferido por decisión.
- Catálogo maestro de materias (siguen siendo tags libres con búsqueda `Contains`).
- Dashboard "Resumen" del panel tutor (Stitch 05) — el índice redirige a Perfil.
- Reseñas reales en `/tutores/[id]` (siguen mock), KPIs fijos.
- Refresh tokens (el lag de rol tras aprobación se mitiga con re-login + copy).
