# Contexto — Motor de slots y reservas + pago SINPE

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-29-slots-reservations-plan.md)

## Alcance

Reemplazar el `nextSlot` demo del backend por un motor de reservas real end-to-end:

1. Slots exclusivos garantizados en BD (índice único parcial, no solo validación en servicio).
2. Reserva con expiración de 60 min en `PENDING_PAYMENT` (slot liberado al expirar).
3. Comprobante SINPE con validación automática (monto, nº confirmación único, receptor, ventana) + **confirmación manual del Admin** (cola de pagos).
4. Cancelación con regla de 12 h: ≥12 h → crédito completo al monedero; <12 h → sin reembolso.
5. Rehacer las 4 pantallas de Stitch (directorio, perfil+reserva, checkout, mis-tutorías) conservando el wiring real de `useTutors`.

## Decisiones (alineadas con el usuario)

| # | Decisión | Elección |
|---|----------|----------|
| 1 | Validación SINPE | Confirmación manual por Admin (sin firma digital real; ADR-004). Backend valida monto/receptor/ventana/idempotencia automáticamente. |
| 2 | Origen de slots | Seed automático idempotente (franjas 1 h, 08:00–20:00 CR, próximos 14 días, tutores Verified) con ensure-ahead. CRUD de disponibilidad del tutor → tarea posterior. |
| 3 | Alcance | Reserva + pago + cancelación (12 h + monedero). Reprogramación excluida. |
| 4 | Pantallas | Rehacer las 4 de Stitch (directorio incluido, conservando `useTutors` real). |
| 5 | Expiración | 60 min → `EXPIRED`, slot liberado. Expiración perezosa (al leer/crear), sin workers (ADR-005). |
| 6 | Cancelación <12 h | Sin reembolso. |
| 7 | Moneda checkout | Solo CRC (PriceCrc); USD visible como referencia. |

## Restricciones

- TDD obligatorio (skills `slots-reservations` + `sinpe-payments`): cada invariante = test, incluido test de carrera de concurrencia contra Postgres real.
- UTC en BD (`timestamptz`); zona `America/Costa_Rica` solo en presentación/contrato.
- Migración con bloque ROLLBACK MANUAL (patrón de `AddTutorUserId`).
- Idempotencia: reserva por `idempotencyKey`; comprobante por nº confirmación (unique).
- El frontend nunca valida reglas de negocio; solo muestra estados.
- No loguear PII financiera completa del comprobante.

## Dudas cerradas

- ¿Aplicar saldo del monedero al pagar? → No en esta iteración (complica la validación del monto); el monedero acumula.
- ¿Worker de expiración? → No; expiración perezosa documentada en ADR-005.
- ¿`nextSlot` de la landing (`Mentors`)? → Se mantiene (marketing estático, no datos reales).

## Referencias

- Skills: `.github/skills/slots-reservations/SKILL.md`, `.github/skills/sinpe-payments/SKILL.md`
- Pantallas Stitch: proyecto `12227662788794145381` (4 screens, IDs en el plan)
- Estado previo: `.ai/CURRENT_STATE.md` (v0.4.2, panel admin cerrado)
