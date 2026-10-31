---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Panel estudiante + login en reserva + admin de estudiantes (0.12.0)

**Periodo:** 2026-10-16
**Versión:** backend .NET 10 (ADR-003) · frontend 0.12.0

### Hecho
- **Plan `2026-10-16-student-panel/` cerrado**:
  - **Backend:** `[Authorize(Roles = "Estudiante")]` en `Create`/`SubmitComprobante`/`Cancel` de `ReservationsController` (Tutor/Admin ya no pueden reservar); `AdminUserDto`, `AdminUserSearchCriteria` + validador (MaxPageSize 100), `AdminUserService`, `AdminUsersController` (`GET /api/admin/users?query&role&page&pageSize`, 400 rol inválido), `IUserRepository.SearchAsync` + EF (filtro rol, query Email/FullName lowercase, orden CreatedAt desc). **Sin migración.** DI: `AdminUserService` + `AdminUserSearchCriteriaValidator` registrados en `Program.cs` (⚠️ se olvidó en el primer deploy → 500; corregido).
  - **Frontend:** `lib/redirect.ts` (`readInternalRedirect` anti open-redirect + `DEFAULT_LOGIN_REDIRECT`); `/login` y `/registro` con `?redirect=` (aviso contextual, redirect propagado al link de registro); CTA "Inicia sesión para reservar" en `BookingEngine` cuando `requiresLogin` (banner + Link a `/login?redirect=/tutores/{id}&slot={slotId}`); retorno preselecciona el slot del query cuando el catálogo carga; **panel estudiante** `/estudiante/{reservas,monedero}` con layout + `StudentSubnav` (guard por rol, modal de cancelación con `noticeHoras`); `/mis-tutorias` → redirect; `/panel` Estudiante → `/estudiante/reservas`; `PanelMenu` Estudiante (2 items) y Admin (+ "Estudiantes"); **`/admin/estudiantes`** con `fetchAdminUsers` + `useAdminUsers` (debounce 300 ms, filtro rol, paginación); landing `Mentors` con `fetchTutors` real (client component, fetch en runtime, skeleton; "Ver Perfil" → Link al perfil; mock `MENTORS` archivado en `.ai/archive/`); nav: "Para Universidades" eliminado, Logo → `/`.
  - **Tests:** 149/149 (5 nuevos `AdminUserServiceTests`). **Gates:** verdes (21 rutas). **Smoke E2E navegador verde:** catálogo sin login → perfil → slot → CTA con slot en redirect → login → retorno con slot preseleccionado → panel estudiante (reservas/monedero) → admin lista/filtra/busca estudiantes; curl: 401 sin token, 403 Admin al reservar, 201 Estudiante, cancelación PendingPayment sin crédito (correcto por diseño).
  - **Nota:** admins de prueba del smoke eliminados de la BD; `admin.test@auralearn.dev` y `admin-smoke-2@test.dev` siguen pendientes de eliminar.

### En curso
- Nada abierto a la fecha de este documento.

### Siguientes pasos
1. Historial de movimientos del monedero (`GET /api/wallet/movements`) — fuera de alcance por decisión del usuario.
2. Efecto real de `slotDurationMinutes` (ADR: granularidad de franjas + índice único `(tutor_id, start_at)` + UI de disponibilidad).
3. P1 del análisis de gaps (`2026-10-11-tutor-portal-phase2/gaps-analysis.md`): canal de sesión, pizarra realtime, notificaciones email, vista de ganancias.

---

## Entrada anterior — Reglas de reserva del Admin (0.11.0)

**Resumen:** tabla `booking_rules` (fila única, 4 reglas nullable + auditoría; seed 12/60/60/14), `GET /api/booking-rules` público + `PUT /api/admin/booking-rules` `[Admin]` (422 `BOOKING_RULES_INVALID_*`), página `/admin/configuracion` + `BookingRulesForm`; la lógica (cancelación, ventana de pago, horizonte ensure-ahead) se gobierna desde BD con fallback `BookingRulesDefaults`; `slotDurationMinutes` persistida sin efecto (deuda). Tests 144/144 (23 nuevos TDD). Detalle en `docs/plans/2026-10-12-booking-rules/` y `version-changes.md` v0.11.0.

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
| 2026-10-10 → 10-11 | Full-stack | Sección del tutor completa (0.7.0): fix rol (approve promueve a Tutor), perfil editable GET/PUT me/profile, especialización, /tutor/perfil + subnav |
| 2026-09-27 → 2026-10-01 | Full-stack | Cierre de reservas (0.6.0), slots/reservas + SINPE (0.5.0), panel admin (0.4.2), onboarding tutor (0.4.0), auth TDD (0.3.0), auth UI, backend .NET (ADR-003), conexión frontend, gobernanza (ADR-001/002) |
