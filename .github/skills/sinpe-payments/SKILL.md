# Skill — Pagos SINPE y validación de comprobantes

## Cuándo usarla
Al implementar o revisar cualquier flujo de pago: alta de reserva, acreditación de comprobante SINPE, reembolsos, monedero.

## Flujo de negocio

1. Estudiante selecciona slot → reserva en estado `PENDING_PAYMENT`.
2. La plataforma emite número de confirmación + teléfono de recepción de fondos.
3. Estudiante transfiere por SINPE Móvil desde su app bancaria.
4. Estudiante carga/comparte el **comprobante**.
5. Backend valida la **firma digital** del comprobante → estado `CONFIRMED` o `REJECTED`.
6. Retención del pago hasta concluir la sesión con éxito; luego liquidación al tutor.

## Patrones recomendados

### Validación siempre en backend (test-first obligatorio)
```csharp
public class SinpeComprobanteValidator
{
    public SinpeValidationResult Validate(SinpeComprobante comprobante)
    {
        // 1. Firma digital verificable (API de validación / certificado del emisor)
        // 2. Monto exacto contra la reserva (sin descuentos no autorizados)
        // 3. Moneda correcta (CRC) o tasa documentada si USD
        // 4. Teléfono receptor coincide con el número de recepción de fondos de la reserva
        // 5. Número de confirmación único (idempotencia: no reutilizable entre reservas)
        // 6. Ventana temporal válida (comprobante no fuera de la vigencia de la reserva)
        throw new NotImplementedException("Implementar con tests primero");
    }
}
```

### Idempotencia e integridad
- El número de confirmación del comprobante es **clave única** en BD (constraint unique).
- Reintentos del webhook/worker nunca deben duplicar una acreditación.
- Estado de reserva transaccional: la acreditación y el cambio de estado en la misma transacción BD.

### Contrato de API
```
POST /api/reservations/{id}/comprobante
  → 202 Accepted  { status: "VALIDATING" }
  → 409 Conflict  { error: "COMPROBANTE_REUTILIZADO" }
  → 422 Unprocessable { error: "MONTO_NO_COINCIDE" | "FIRMA_INVALIDA" | "RECEPTOR_INCORRECTO" }
```

## Anti-patrones
- Validar el comprobante solo en el frontend o confiar en la pantalla de la app bancaria del usuario.
- Aceptar capturas de pantalla sin validación de firma como prueba de pago.
- Duplicar acreditaciones por reintentos (sin idempotencia).
- Mover el dinero al tutor antes de concluir la sesión (rompe la garantía y la retención).
- Loguear datos financieros completos del comprobante (PII/PCI): ofuscar en logs.

## Reglas del repo
- TDD **obligatorio** (test-first) en este dominio: cada regla de validación = al menos un test.
- Cambios en este flujo → skill `backend-testing` + plan de regresión.
- Auditoría funcional: registrar decisión (aprobado/rechazado y motivo) además del log técnico.

## Checklist final
- [ ] Validación íntegra en backend con FluentValidation + servicio dedicado
- [ ] Idempotencia por número de confirmación (constraint unique)
- [ ] Monto, moneda, receptor, firma y vigencia verificados
- [ ] Estados de reserva transaccionales y trazables
- [ ] Tests first; regresión para cada bug de pago
- [ ] `dotnet build && dotnet test` verde
