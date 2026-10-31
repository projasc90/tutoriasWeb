using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Reglas de reserva de la plataforma: lectura pública (el gestor de
/// disponibilidad del tutor las muestra) y edición exclusiva del Admin.
/// </summary>
[ApiController]
public class BookingRulesController(BookingRulesService service) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")!);

    /// <summary>Reglas vigentes (null = sin definir por el Admin).</summary>
    [HttpGet("api/booking-rules")]
    [AllowAnonymous] // sin PII: los valores se muestran en el portal del tutor
    public async Task<ActionResult<BookingRulesDto>> Get(CancellationToken ct)
        => Ok(await service.GetAsync(ct));

    /// <summary>Define las reglas de reserva (fila única; null = dejar sin definir).</summary>
    [HttpPut("api/admin/booking-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(
        [FromBody] UpdateBookingRulesRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(request, GetUserId(), ct);

        if (result.IsInvalid)
            return UnprocessableEntity(new { error = result.ErrorCode ?? BookingRulesErrors.Invalid });

        return Ok(result.Value!);
    }
}
