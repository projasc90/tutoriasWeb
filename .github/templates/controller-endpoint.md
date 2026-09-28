# Template — Controller / Endpoint (ASP.NET Core)

> Patrón real de `AuraLearn.Api`. El controller es fino: mapea HTTP ↔ caso de uso; la lógica vive en `Application` y la validación en FluentValidation.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
        => _reservationService = reservationService;

    /// <summary>Crea una reserva para el estudiante autenticado.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(Role.Estudiante))]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateReservationRequest request, CancellationToken ct)
    {
        var result = await _reservationService.CreateAsync(request, ct);

        if (result.IsConflict)
            return Conflict(new ProblemDetails { Title = result.Error, Status = StatusCodes.Status409Conflict });

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _reservationService.GetAsync(id, ct);
        return result.NotFound ? NotFound() : Ok(result.Value);
    }
}
```

## Puntos clave
- `CancellationToken` propagado siempre.
- Errores de dominio traducidos a `ProblemDetails` (un middleware global también es válido).
- Autorización por política/rol en rutas sensibles.
- Sin `DbContext`, sin reglas de negocio, sin validación manual en el controller.
