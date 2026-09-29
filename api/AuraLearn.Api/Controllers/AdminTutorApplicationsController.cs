using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

[ApiController]
[Route("api/admin/tutor-applications")]
[Authorize(Roles = "Admin")]
public class AdminTutorApplicationsController(TutorApplicationService service) : ControllerBase
{
    /// <summary>Cola de postulaciones pendientes de revisión (solo Admin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminTutorApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListPending(CancellationToken ct)
    {
        var pending = await service.ListPendingAsync(ct);
        return Ok(pending);
    }

    /// <summary>Aprueba o rechaza una postulación (solo Admin). El rechazo exige motivo.</summary>
    [HttpPatch("{id:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Verify(Guid id, VerifyTutorApplicationRequest request, CancellationToken ct)
    {
        var result = await service.DecideAsync(id, request.Decision, request.Reason, ct);

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails { Title = "Decisión inválida (approve/reject; el rechazo exige motivo)", Status = StatusCodes.Status400BadRequest });

        if (result.IsNotFound)
            return NotFound(new ProblemDetails { Title = "Postulación no encontrada", Status = StatusCodes.Status404NotFound });

        return NoContent();
    }
}
