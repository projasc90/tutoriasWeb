using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

[ApiController]
[Route("api/tutor-applications")]
[Authorize]
public class TutorApplicationsController(TutorApplicationService service) : ControllerBase
{
    /// <summary>Envía la postulación del profesor autenticado (queda PendingReview).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TutorApplicationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Submit(SubmitTutorApplicationRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await service.SubmitAsync(userId.Value, request, ct);

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Status = StatusCodes.Status400BadRequest });

        if (result.IsUnauthorized)
            return Unauthorized();

        if (result.IsConflict)
            return Conflict(new ProblemDetails { Title = "Ya tienes una postulación activa", Status = StatusCodes.Status409Conflict });

        return Accepted(result.Value);
    }

    /// <summary>Estado de la postulación del profesor autenticado.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(TutorApplicationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await service.GetStatusAsync(userId.Value, ct);

        if (result.IsNotFound)
            return NotFound(new ProblemDetails { Title = "No tienes una postulación", Status = StatusCodes.Status404NotFound });

        return Ok(result.Value);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
