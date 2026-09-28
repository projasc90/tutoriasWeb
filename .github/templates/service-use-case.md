# Template — Service / Use-case (Application)

> Patrón de `AuraLearn.Application`. Resultados tipados para errores de negocio; FluentValidation antes de la lógica.

```csharp
namespace AuraLearn.Application.Services;

public interface IReservationService
{
    Task<Result<ReservationResponse>> CreateAsync(CreateReservationRequest request, CancellationToken ct);
    Task<Result<ReservationResponse>> GetAsync(Guid id, CancellationToken ct);
}

public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly ISinpeComprobanteValidator _sinpeValidator;

    public ReservationService(
        IReservationRepository reservations,
        ISinpeComprobanteValidator sinpeValidator)
    {
        _reservations = reservations;
        _sinpeValidator = sinpeValidator;
    }

    public async Task<Result<ReservationResponse>> CreateAsync(
        CreateReservationRequest request, CancellationToken ct)
    {
        // 1. Validación de firma del comprobante (dominio crítico — TDD obligatorio)
        var sinpeResult = _sinpeValidator.Validate(request.Comprobante);
        if (!sinpeResult.IsValid)
            return Result<ReservationResponse>.Conflict(sinpeResult.Error);

        // 2. Reserva con exclusividad garantizada por BD (constraint unique de slot)
        var reservation = Reservation.Pending(request.SlotId, request.StudentId, request.Comprobante);
        await _reservations.AddAsync(reservation, ct); // captura unique violation → 409

        return Result<ReservationResponse>.Ok(ReservationResponse.From(reservation));
    }
}
```

```csharp
// Result tipado compartido
public record Result<T>(T? Value, string? Error, bool IsConflict = false, bool NotFound = false)
{
    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Conflict(string error) => new(default, error, IsConflict: true);
    public static Result<T> NotFound() => new(default, null, NotFound: true);
}
```

## Puntos clave
- Invariantes de negocio como métodos de la entidad o servicio dedicado, nunca en el controller.
- `CancellationToken` en toda la cadena.
- Transacción de pagos/estados en el repositorio (unidad de trabajo), no aquí.
- TDD obligatorio: auth, SINPE, slots, reglas sensibles (ver `PROTOCOLS.md`).
