using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Perfil del tutor autenticado: consulta y edición de sus datos de alta
/// (título, especialización, universidad, bio, materias y tarifas).
/// Cualquier usuario autenticado puede consultarlo; sin fila de tutor → 404
/// (un estudiante que nunca postuló no tiene perfil).
/// </summary>
[ApiController]
[Route("api/tutors/me/profile")]
[Authorize]
public class TutorProfileController(TutorProfileService service) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")!);

    /// <summary>Perfil del tutor autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<TutorProfileResponse>> Get(CancellationToken ct)
    {
        var result = await service.GetProfileForCurrentUserAsync(GetUserId(), ct);
        if (result.IsNotFound)
            return NotFound();

        return Ok(result.Value);
    }

    /// <summary>
    /// Actualiza los datos de alta del tutor autenticado. Devuelve el estado
    /// canónico del perfil tras la escritura (evita un segundo GET).
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<TutorProfileResponse>> Update(
        [FromBody] UpdateTutorProfileRequest request, CancellationToken ct)
    {
        var result = await service.UpdateProfileForCurrentUserAsync(GetUserId(), request, ct);

        if (result.IsNotFound) return NotFound();
        if (result.IsInvalid) return BadRequest();
        if (result.IsConflict)
            return Conflict(new { error = result.ErrorCode ?? TutorProfileErrors.ProfileRejected });

        return Ok(result.Value);
    }
}
