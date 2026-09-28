# Skill — Arquitectura Backend (.NET)

## Cuándo usarla
Al crear o revisar código en `api/`: diseño de endpoints, capas, servicios, contratos DTO, manejo de errores, inyección de dependencias.

## Patrones recomendados

### Estructura de solución (una sola vez, con ADR)
```
api/
  AuraLearn.Api/            → Program.cs, Controllers/, Middlewares/, appsettings.json (sin secretos)
  AuraLearn.Application/    → Services/, Validators/ (FluentValidation), Dto/, Interfaces/
  AuraLearn.Domain/         → Entities/, ValueObjects/, Enums/ (Role: ESTUDIANTE|TUTOR|ADMIN)
  AuraLearn.Infrastructure/ → Persistence/ (DbContext, config EF), ExternalServices/ (SINPE)
  AuraLearn.Tests/ o api/tests/
```

### Endpoint (controller fino)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
        => _reservationService = reservationService;

    [HttpPost]
    [Authorize(Roles = nameof(Role.Estudiante))]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateReservationRequest request, CancellationToken ct)
    {
        var result = await _reservationService.CreateAsync(request, ct);
        return result.IsConflict
            ? Conflict(new ProblemDetails { Title = result.Error, Status = StatusCodes.Status409Conflict })
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}
```

### Validación con FluentValidation
```csharp
public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(r => r.SlotId).NotEmpty();
        RuleFor(r => r.ComprobanteSINPE).NotEmpty()
            .When(r => r.MetodoPago == MetodoPago.Sinpe);
    }
}
```
Registrar: `services.AddValidatorsFromAssemblyContaining<CreateReservationRequestValidator>();`

### Manejo de errores
- Excepciones de dominio (`SlotAlreadyReservedException`) → middleware que las traduce a `ProblemDetails` con status correcto (409, 422, 404). Un solo middleware de excepciones global, no try/catch en cada controller.

## Anti-patrones
- Lógica de negocio en controllers o en entidades anémicas pasivas (reglas vivas van en `Domain`/`Application`).
- `DbContext` inyectado en controllers.
- Retornar entidades EF directamente (usar DTOs).
- Validar SINPE/slots con `if` dispersos: centralizar en servicios con tests.
- `_dbContext.Users.FirstOrDefault(u => u.Email == email)` sin índice ni `AsNoTracking` en lecturas masivas.

## Checklist final
- [ ] Controller sin lógica de negocio; validación con FluentValidation antes de la capa de servicio
- [ ] Resultados de error tipados o `ProblemDetails` consistentes
- [ ] `CancellationToken` propagado; async hasta el final
- [ ] DTOs en la frontera, nunca entidades EF expuestas
- [ ] Autorización por roles/claims en rutas sensibles
- [ ] Si tocó datos: migración con rollback manual comentado
- [ ] `cd api && dotnet build && dotnet test` verde
