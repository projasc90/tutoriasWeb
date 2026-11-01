---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Configuración por variables del proveedor (0.13.1)

**Periodo:** 2026-11-01
**Versión:** backend .NET 10 (ADR-003) · frontend 0.13.1

### Hecho
- **Plan `2026-11-01-env-vars-reuse/` cerrado (ADR-010)**: el API dejó de exigir variables propias.
  - `Infrastructure/Configuration/DatabaseConnectionString`: prioridad `ConnectionStrings__Default` → `DATABASE_URL` → `POSTGRES_URL` → `PGHOST`+`PGPORT`+`PGDATABASE`+`PGUSER`+`PGPASSWORD`; en modo migraciones `DATABASE_URL_UNPOOLED` → `POSTGRES_URL_NON_POOLING` → `PGHOST_UNPOOLED`+`PG*`. **Npgsql no acepta URIs**: se normaliza a `keyword=value` con `NpgsqlConnectionStringBuilder` (`sslmode`, `channel_binding`, `connect_timeout`, `application_name`, `options`, `search_path`; `pgbouncer`/`supa` ignorados) y `SslMode=Require` automático en `.neon.tech`.
  - `Infrastructure/Configuration/VercelBlobConfiguration`: reutiliza `BLOB_READ_WRITE_TOKEN` + `BLOB_STORE_ID` (los de `web/.env.local`), normaliza `store_` y falla al arrancar si el store no coincide con el del token. Elimina el `Split('_')[3]` de `VercelBlobFileStorage`.
  - **Fail-fast:** la validación se movió antes de `builder.Build()`; la factory de `AddSingleton<IFileStorage>` es perezosa y el error solo aparecía en la primera subida.
  - **Tests:** 181/181 (20 nuevos); `api/tests` referencia `AuraLearn.Infrastructure`; EF alineado a 10.0.12 y `Microsoft.Extensions.Identity.Core` 10.0.12 sustituye al paquete legacy con advisories. `dotnet build` 0 warnings.

### En curso
- Nada abierto. Pendiente manual: desplegar con la integración de Neon + Blob conectadas y borrar `ConnectionStrings__Default` antigua del panel de Vercel.

### Siguientes pasos
1. `NEON_CONNECTION_STRING` del CI → rotarlo a la URL **directa** de Neon.
2. Subida real de un PDF por la ruta directa y verificación de metadatos/descarga.
3. P1 del análisis de gaps (canal de sesión, pizarra realtime, notificaciones email, ganancias).

---

## Entrada anterior — Panel estudiante + admin de estudiantes (0.12.0)

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
