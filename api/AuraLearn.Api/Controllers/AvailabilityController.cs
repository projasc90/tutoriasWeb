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

    /// <summary>Reglas actuales del tutor autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AvailabilityRuleDto>>> Get(CancellationToken ct)
    {
        var rules = await service.GetForCurrentUserAsync(GetUserId(), ct);
        if (rules is null)
            return Forbid();

        return Ok(rules.Select(r => new AvailabilityRuleDto(r.Weekday, r.StartLocal, r.EndLocal)).ToList());
    }

    /// <summary>Reemplazo completo de las reglas del tutor autenticado.</summary>
    [HttpPut]
    public async Task<ActionResult> Replace(
        [FromBody] ReplaceAvailabilityRequest request, CancellationToken ct)
    {
        var result = await service.ReplaceForCurrentUserAsync(GetUserId(), request.Rules, ct);

        if (result.IsForbidden) return Forbid();
        if (result.IsInvalid)
            return UnprocessableEntity(new { error = result.ErrorCode ?? AvailabilityErrors.InvalidRules });

        return NoContent();
    }
}
