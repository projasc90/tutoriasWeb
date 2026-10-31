using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Reglas semanales de disponibilidad del tutor autenticado.
/// </summary>
[ApiController]
[Route("api/tutors/me/availability")]
[Authorize(Roles = "Tutor")]
public class AvailabilityController(AvailabilityService service) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")!);

    /// <summary>Configuración completa de la agenda del tutor autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<AvailabilitySettingsDto>> Get(CancellationToken ct)
    {
        var settings = await service.GetForCurrentUserAsync(GetUserId(), ct);
        if (settings is null)
            return Forbid();

        return Ok(settings);
    }

    /// <summary>Reemplazo completo de la configuración de agenda (zona, toggle, franjas, excepciones).</summary>
    [HttpPut]
    public async Task<ActionResult> Replace(
        [FromBody] ReplaceAvailabilitySettingsRequest request, CancellationToken ct)
    {
        var result = await service.ReplaceForCurrentUserAsync(GetUserId(), request, ct);

        if (result.IsForbidden) return Forbid();
        if (result.IsInvalid)
            return UnprocessableEntity(new { error = result.ErrorCode ?? AvailabilityErrors.InvalidRules });

        return NoContent();
    }
}
