---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Full-stack funcional (catálogo + auth + onboarding + admin)

**Periodo:** 2026-09-27 → abierto
**Versión:** backend .NET 10 (ADR-003) · frontend 0.4.2

### Hecho
- **Panel admin de aprobación** (plan `2026-09-29-admin-panel/` cerrado): `/admin/tutores` con guard por rol, cola con `useAdminApplications`, tarjeta del postulante, rechazo con motivo obligatorio (modal ≥10 chars) y aprobación contra `PATCH {id}/verify`; recarga automática. Smoke E2E con el usuario admin real: cola visible → rechazo validado → `Rejected` + `rejection_reason` persistidos en BD. Bump 0.4.2.
- **Auth UI en el frontend** (plan `2026-09-28-auth-ui/` cerrado): `/login` + `/registro` contra el backend real, `AuthProvider`/`useAuth` con expiración JWT, header con sesión ("Hola, {nombre}" + cerrar sesión), `fetchMe`. Smoke E2E completo (login/registro/logout/persistencia).
- **Onboarding del Profesor full-stack** (plan `2026-09-28-tutor-onboarding/` cerrado): wizard `/postular` 4 pasos + confirmación; endpoints de postulación y decisión admin; migración `AddTutorUserId` (rollback manual); 18 tests TDD (25/25); smoke E2E 19 verificaciones.
- **Auth implementada con TDD** (plan `2026-09-28-auth-endpoints/` cerrado): register/login/me con JWT + roles, hash PBKDF2, migración con rollback manual + unique email. 7 tests escritos ANTES del código.
- **Frontend conectado a la API real** (plan `2026-09-28-connect-frontend-api/` cerrado): `/tutores` consume `GET /api/tutors`; mock `TUTORS` eliminado.
- **Backend .NET 10 implementado** (plan `2026-09-28-api-scaffold/` cerrado, ADR-003): Clean Architecture, EF Core + PostgreSQL, seed 12 tutores.
- Sistema de memoria externa y gobernanza (ADR-001).

### En curso
- Nada abierto a la fecha de este documento.

### Siguientes pasos
1. Aprobar un tutor real en producción; upload de atestados (URLs firmadas); notificaciones por email.
2. Motor de slots/reservas (reemplazar `nextSlot` demo) — flujo reserva + pago SINPE.
3. Cookies httpOnly (ADR); refresh tokens + rate limiting (deuda seguridad); Playwright E2E.

---

## Entradas anteriores (resumen)

| Fecha | Ámbito | Resumen |
|-------|--------|---------|
| 2026-09-28 | Full-stack | Auth backend con TDD (register/login/me, JWT, PBKDF2) + `/tutores` conectado a la API real (mock eliminado, bump 0.2.1) |
| 2026-09-28 | Full-stack | Backend .NET 10 con Clean Architecture + EF PostgreSQL (ADR-003); mocks extraídos a `web/lib/` (bump 0.1.1) |
