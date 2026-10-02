# Skill — Slots y motor de reservas

## Cuándo usarla
Al implementar o revisar el calendario de tutores, la disponibilidad ("Próximo cupo"), la reserva de sesiones y su concurrencia.

## Invariantes de negocio

1. **Un slot es exclusivo:** una vez reservado (o confirmado), nadie más puede tomarlo.
2. La reserva exige pago acreditado (SINPE validado) o se queda en `PENDING_PAYMENT` con expiración.
3. Reprogramación sin penalización con ≥12 h de anticipación; después, según política documentada.
4. Cancelación a tiempo → saldo al monedero o reembolso al método original.
5. El listado muestra solo slots disponibles (estado correcto en el origen de datos, no en la UI).

## Patrones recomendados

### Concurrencia: exclusión en BD, no solo en código
```sql
-- La unicidad del slot es el guardián real contra la carrera:
ALTER TABLE reservations ADD CONSTRAINT uq_reservations_slot UNIQUE (slot_id);
```
```csharp
public async Task<Result> ReserveAsync(Guid slotId, Guid studentId, CancellationToken ct)
{
    //INSERT ... idempotente; capturar excepción de unique constraint → 409 SLOT_ALREADY_RESERVED
    throw new NotImplementedException("Implementar con tests first");
}
```

### Estados de reserva
```
PENDING_PAYMENT → CONFIRMED → IN_PROGRESS → COMPLETED
                          ↘ EXPIRED      ↘ CANCELLED (reglas de 12h)
```
- Transiciones válidas centralizadas en un único lugar (máquina de estados en `Domain` o `Application`), con tests por transición.
- Fechas en UTC en BD; conversión a zona (America/Costa_Rica) solo en la capa de presentación/contrato.

### API del repo (vigente)
```
GET   /api/tutors/{id}/slots?from=&to=     → slots disponibles (ensure-ahead ADR-005)
GET   /api/tutors/{id}                     → detalle público
POST  /api/reservations                    → reserva (409 si slot tomado; useWalletBalance opcional, ADR-006)
POST  /api/reservations/{id}/comprobante   → comprobante SINPE (202; valida monto/receptor/ventana)
PATCH /api/reservations/{id}/cancel        → cancelación (≥12 h Confirmed → monedero)
PATCH /api/reservations/{id}/complete      → cierre por el tutor (liquidación, EndAt <= now)
PATCH /api/admin/payments/{id}             → decisión Admin (approve/reject)
GET   /api/wallet                          → saldo (estudiante o tutor)
```

**Reprogramación = cancelar + crear nueva** (decisión 2026-10-01): NO existe PATCH reschedule. La UI de /mis-tutorias ofrece "Reprogramar" que cancela (acreditando monedero si aplica) y navega al perfil del tutor para elegir nuevo horario. Razón: un reschedule in-place desincroniza el comprobante pagado (el nº de confirmación apunta a la reserva vieja).

## Anti-patrones
- Chequear disponibilidad solo en el frontend o con `SELECT` + `INSERT` sin constraint único.
- Doble-booking por reintentos del cliente (idempotencia obligatoria por request ID).
- Lógica de "¿puedo reprogramar?" duplicada en varias capas.
- Guardar horas locales del usuario en BD sin zona explícita.
- Exponer slots cancelados/expirados en el listado por culpa de filtros en memoria.

## Reglas del repo
- TDD **obligatorio** en este dominio: cada invariante = test (incluye test de carrera de concurrencia).
- El mock de "Próximo cupo" en la landing (`nextSlot`) es transitorio: al integrar backend, alimentar desde `GET /slots`.
- Índices: `slot_id` único en reservas; índice `(tutor_id, start_at)` para consultas de disponibilidad.

## Checklist final
- [ ] Constraint UNIQUE de slot en BD (no solo validación en servicio)
- [ ] Máquina de estados centralizada y testeada
- [ ] Idempotencia por request
- [ ] UTC en BD; zona en presentación
- [ ] Reglas de 12h y cancelación con tests
- [ ] `dotnet build && dotnet test` verde
