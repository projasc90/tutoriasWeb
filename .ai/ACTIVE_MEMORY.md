---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Cambio de rol desde el panel Admin (0.14.0)

**Periodo:** 2026-11-01
**Versión:** backend .NET 10 (ADR-003) · frontend 0.14.0

### Hecho
- **`PATCH /api/admin/users/{id}/role`** (`[Authorize(Roles = "Admin")]`): gestiona permisos sin `psql`.
  - `ChangeUserRoleRequest` + `AdminUserErrors` en `Dto/AdminUserDto.cs`; `ChangeUserRoleResult` y `AdminUserService.ChangeRoleAsync` en Application; `IUserRepository.CountByRoleAsync` + EF en Infrastructure.
  - **Invariante:** no quitar Admin al **último** administrador (`USER_ROLE_LAST_ADMIN_REQUIRED`, 422), validada antes de escribir. `400` rol inválido (con `Enum.IsDefined`, porque `Enum.TryParse("99")` devuelve `true`), `404` no encontrado, `422 USER_ROLE_UNCHANGED`.
  - **Efecto JWT:** el rol va en el token (8 h) → tras un cambio hay que **re-login**. Sin migración (`users.role` era int).
  - **Tests:** 7 nuevos (`AdminUserRoleTests`), **189/189**. Smoke E2E: 200 / 422 UNCHANGED / 400 x2 / 404 / 422 LAST_ADMIN / 401. BD local restaurada y usuarios de smoke eliminados.
- **Bootstrap del primer Admin** documentado en `deploy-guide.md`: registrar (`register` siempre crea Estudiante) → `UPDATE users SET role = 3` → re-login. Único paso manual que queda.

### En curso
- Nada abierto. Pendiente manual: crear el secret `NEON_CONNECTION_STRING` (URL directa) y correr `migrate`, porque Neon sigue con **12 migraciones pendientes** (`/api/tutors` → 42P01; `/health` da Healthy porque solo hace `SELECT 1`).

### Siguientes pasos
1. Migrar Neon (job `migrate` con `workflow_dispatch` o local con `ASPNETCORE_ENVIRONMENT=Production`).
2. Crear el Admin de producción con el procedimiento de bootstrap.
3. UI para el cambio de rol en `/admin/estudiantes` (el endpoint ya existe).
4. Revocación inmediata de JWT (`security_stamp`): un Admin degradado conserva permisos hasta 8 h.
5. P1 del análisis de gaps (canal de sesión, pizarra realtime, notificaciones email, ganancias).

---

## Entrada anterior — Configuración por variables del proveedor (0.13.1)

**Resumen:** reservas solo rol Estudiante + `?redirect=` con anti open-redirect, panel `/estudiante/{reservas,monedero}`, `/admin/estudiantes` (búsqueda, filtro rol, paginación), landing con `fetchTutors` real. Backend: `[Authorize(Roles = "Estudiante")]`, `AdminUserService`/`AdminUsersController`, `IUserRepository.SearchAsync` (sin migración). Tests 149/149. Detalle en `docs/plans/2026-10-16-student-panel/` y `version-changes.md` v0.12.0. Nota: `admin.test@auralearn.dev` y `admin-smoke-2@test.dev` siguen pendientes de eliminar.

---

## Entrada anterior — Foto de perfil + X en franjas (0.10.0)

**Resumen:** `tutors.photo_path` (migración `AddTutorPhoto`), `IFileStorage.SavePhotoAsync` + `TutorPhotoService` (JPEG/PNG/WebP ≤5 MB), `POST me/photo` + `GET {id}/photo` público, `photoUrl` en DTOs, `ProfilePhotoCard` en `/tutor/perfil`, foto en perfil público y directorio, X en franjas de disponibilidad. Tests 121/121. Detalle en `docs/plans/2026-10-11-tutor-photo-ui/` y `version-changes.md` v0.10.0.

---

## Entrada anterior — Documentos del tutor + menú condicionado (0.8.0)

**Resumen:** subida de documentos de verificación como paso del wizard (tabla `tutor_documents`, `IFileStorage`/`LocalFileStorage` disco local, gate 409 `MISSING_DOCUMENTS` al aprobar, cola admin con descarga protegida, "Conviértete en Tutor" oculto para Tutor/Admin). Detalle completo en `docs/plans/2026-10-11-tutor-documents/` y `version-changes.md` v0.8.0.

---

## Entradas anteriores (resumen)

| Fecha | Ámbito | Resumen |
|-------|--------|---------|
| 2026-10-12 | Backend + UI | Reglas de reserva del Admin (0.11.0): tabla `booking_rules` (fila única, 4 reglas nullable + auditoría), `GET` público + `PUT` `[Admin]`, `/admin/configuracion`; la lógica se gobierna desde BD con fallback `BookingRulesDefaults` (`slotDurationMinutes` persistida sin efecto). Tests 144/144 |
| 2026-10-10 → 10-11 | Full-stack | Sección del tutor completa (0.7.0): fix rol (approve promueve a Tutor), perfil editable GET/PUT me/profile, especialización, /tutor/perfil + subnav |
| 2026-09-27 → 2026-10-01 | Full-stack | Cierre de reservas (0.6.0), slots/reservas + SINPE (0.5.0), panel admin (0.4.2), onboarding tutor (0.4.0), auth TDD (0.3.0), auth UI, backend .NET (ADR-003), conexión frontend, gobernanza (ADR-001/002) |
