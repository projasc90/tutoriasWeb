# Análisis de gaps — Fase de tutores (qué falta para completarla)

**Fecha:** 2026-10-11 · **Base:** 0.9.0 (fase 2 del portal incluida)

## Estado actual del ciclo del tutor (implementado)

| Pieza | Estado |
|-------|--------|
| Onboarding `/postular` (5 pasos: cuenta, perfil, cursos, documentos, revisión) | ✅ |
| Documentos de verificación (títulos/certs acumulables máx 5, ID única, storage local, PII solo Admin) | ✅ |
| Cola admin `/admin/tutores` (documentos visibles + descarga protegida, gate 409 si faltan, rechazo con motivo) | ✅ |
| Promoción de rol al aprobar (re-login para el claim) | ✅ |
| Perfil editable `/tutor/perfil` (alta + estado de verificación + re-postular) | ✅ |
| Disponibilidad `/tutor/disponibilidad` (franjas semanales, purga de slots) | ✅ |
| Sesiones `/tutor/sesiones` (completar tras `EndAt` + liquidación `TutorPayout`) | ✅ |
| Documentos `/tutor/documentos` (agregar títulos/certs post-aprobación; ID bloqueada) | ✅ |
| Materias normalizadas (mayúscula + sugerencia del catálogo) | ✅ |
| Menú del header condicionado por rol | ✅ |

## Gaps pendientes, priorizados

### P1 — Bloquean la promesa de producto del tutor

1. **Canal de sesión (el más crítico):** no existe cómo "entrar" a una sesión reservada. Falta enlace de videollamada/sala por reserva (o integración Meet/Zoom) visible para estudiante y tutor en `/mis-tutorias` y `/tutor/sesiones`. Sin esto, la reserva confirmada no se puede ejecutar.
2. **Pizarra digital realtime:** promesa central del producto ("sesiones 1-a-1 con pizarra digital"); sin implementación. Requiere SignalR/WebSocket + canvas colaborativo (skill `realtime-whiteboard` reservada).
3. **Notificaciones al tutor:** el copy promete "te notificaremos en ≤48 h" pero no hay canal. Mínimo viable: email transaccional en aprobación/rechazo de postulación, nueva reserva, sesión completada con liquidación. (PROTOCOLS §7: si hay workers, idempotencia + reintentos.)

### P1 — Cierran el ciclo económico del tutor

4. **Vista de ganancias:** `TutorPayout` se registra al completar una sesión, pero el tutor no ve sus ingresos (histórico de liquidaciones, total del mes, próxima liquidación). Falta `GET /api/tutors/me/payouts` + sección en el panel.

### P2 — Completan la experiencia

5. **Reseñas reales:** `Rating`/`Reviews` son seed estático; no hay flujo post-sesión (estudiante califica 1-5 + comentario) ni recálculo del rating. Requiere entidad `Review`, endpoint y UI en `/mis-tutorias`.
6. **Dashboard "Resumen" del panel:** próximas sesiones, ingresos del mes, rating, estado de documentos — hoy el panel entra directo a una pestaña sin vista agregada.
7. **Catálogo maestro de materias:** la sugerencia usa 10 materias estáticas en el cliente; falta catálogo en BD (admin CRUD) para sugerencias reales y consistencia de datos.
8. **Badges de verificación en el perfil público:** mostrar "Título verificado · UCR" en `/tutores/[id]` (los datos ya existen; es presentación + confianza).

### P3 — Robustez y pulido

9. **Bloqueo de agenda (vacaciones):** el tutor no puede pausar fechas puntuales; la purga de slots al editar franjas no distingue vacaciones.
10. **Borrado/reemplazo de documentos subidos por error** (hoy solo acumulan).
11. **Re-verificación de documentos agregados post-aprobación:** decisión de producto pendiente — ¿quién revisa los títulos/certs nuevos y con qué estado intermedio? (Hoy quedan "disponibles para el equipo" sin flujo de revisión.)
12. **"Completar documentos" desde la fase "en revisión"** (deuda 0.8.0): postulaciones previas sin docs → 409 al aprobar; recuperación manual (rechazar → re-postular).
13. **Verificación visual del botón "Ver"** del admin (deuda 0.8.0; la API está validada por curl).
14. **Rate limiting en login + refresh tokens** (deuda histórica de seguridad).
15. **Playwright E2E permanente** del flujo del tutor (hoy smokes manuales).
16. **Testcontainers** para concurrencia de slots/reservas contra Postgres real.

## Recomendación de orden para "cerrar" la fase

1. Canal de sesión + pizarra (P1.1–P1.2) — es lo que hace usable el producto.
2. Notificaciones email mínimas (P1.3) + vista de ganancias (P1.4) — cierran confianza y dinero.
3. Reseñas reales + dashboard resumen (P2.5–P2.6).
4. El resto según demanda.

## Fuera de la fase de tutores (no contar como gap)

- Firma digital SINPE real (fase de pagos, ADR-004 manual vigente).
- Portal público `portal/`, Docker/compose, migración storage a blob/S3 (infra).
