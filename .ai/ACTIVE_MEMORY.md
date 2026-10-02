---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Full-stack funcional (catálogo + reserva + pago SINPE)

**Periodo:** 2026-09-27 → abierto
**Versión:** backend .NET 10 (ADR-003) · frontend 0.6.0

### Hecho
- **Cierre de deuda de reservas** (plan `2026-10-01-reservations-closeout/` cerrado): 6 fases. F0 desbloqueo (migración regenerada con tooling, ensure-ahead cableado, SinpeConfig real, fix monedero PendingPayment, GET /tutors/{id}); F1 smoke E2E API 15 escenarios + bug arreglado (expiración perezosa inexistente → `TryExpireIfDueAsync` con TDD); F2 pago con monedero ADR-006 (`TryDebitAsync` atómico FOR UPDATE, `WalletPayment`, fallback SINPE); F3 Completed con liquidación (`PATCH complete` tutor dueño, `TutorPayout`, `tutor-sessions`, fix TutorId/StudentName del DTO, `/tutor/sesiones`); F4 disponibilidad semanal (`TutorAvailability` + migración rollback manual, honoring + purga, `GET/PUT me/availability`, `/tutor/disponibilidad`); F5 reprogramación cancelar+crear (botón en `/mis-tutorias`, skill + GLOSSARY). 79/79 tests; gates verdes (13 rutas). ADR-006. Bump 0.6.0.
- **Motor de slots y reservas + pago SINPE** (plan `2026-09-29-slots-reservations/` cerrado): reemplaza el `nextSlot` demo. Backend: entidades `Slot`/`Reservation`/`WalletEntry` + enums, `ReservationStateMachine` (Domain), `SinpeComprobanteValidator` (monto + nº confirmación único + receptor + ventana), `ReservationService` (crear/comprobante/cancelar/decidir), **índice único parcial `(slot_id) WHERE status IN (1,2)`** como guardián de exclusividad, migración `AddSlotsAndReservations` con ROLLBACK MANUAL. ADR-004 (confirmación manual SINPE por Admin) + ADR-005 (UTC en BD, zona CR en presentación, ensure-ahead). 34 tests nuevos (60/60 verde). Frontend: `nextSlotAt`, 10 funciones nuevas en `lib/api.ts`, 4 hooks (`use-tutor-slots`/`use-reservation`+poll/`use-my-reservations`/`use-admin-payments`), helpers `lib/time.ts` (Intl + `America/Costa_Rica`), 4 pantallas nuevas (`/tutores/[id]`, `/checkout/[id]`, `/mis-tutorias`, `/admin/pagos`). Gates verdes. Bump 0.5.0.
- **Panel admin de aprobación** (plan `2026-09-29-admin-panel/` cerrado): `/admin/tutores` con guard por rol, cola con `useAdminApplications`, rechazo con motivo obligatorio (modal ≥10 chars) y aprobación contra `PATCH {id}/verify`; recarga automática. Smoke E2E con el usuario admin real. Bump 0.4.2.
- **Onboarding del Profesor full-stack** (plan `2026-09-28-tutor-onboarding/` cerrado): wizard `/postular` 4 pasos + confirmación; endpoints de postulación y decisión admin; migración `AddTutorUserId` (rollback manual); 18 tests TDD (25/25). Bump 0.4.0.
- **Auth implementada con TDD** (plan `2026-09-28-auth-endpoints/` cerrado) y **Auth UI** (`2026-09-28-auth-ui/`): register/login/me con JWT + PBKDF2 + páginas de sesión.
- **Backend .NET 10 implementado** (plan `2026-09-28-api-scaffold/` cerrado, ADR-003) y **frontend conectado** (`2026-09-28-connect-frontend-api/`).
- Sistema de memoria externa y gobernanza (ADR-001).

### En curso
- Nada abierto a la fecha de este documento.

### Siguientes pasos
1. Smoke E2E en navegador (UI real con polling) del flujo completo con usuarios reales.
2. Firma digital SINPE real (cuando exista la API del banco; ADR-004 manual vigente).
3. Testcontainers: test de carrera del débito y doble-booking contra Postgres real.
4. Refresh tokens + rate limiting (deuda seguridad); ADR cookies httpOnly vs localStorage; Playwright E2E.
5. Pizarra digital realtime; notificaciones email; upload de atestados con URLs firmadas; portal público `portal/`.

---

## Entradas anteriores (resumen)

| Fecha | Ámbito | Resumen |
|-------|--------|---------|
| 2026-09-29 | Full-stack | Slots/reservas + SINPE end-to-end (0.5.0) y panel admin de aprobación (0.4.2) |
| 2026-09-28 | Full-stack | Onboarding tutor full-stack (0.4.0), auth backend TDD (0.3.0), auth UI, backend .NET (ADR-003) y conexión frontend (0.2.1) |
