using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Foto de perfil del tutor. Subida autenticada (POST me/photo, multipart);
/// la foto es pública: GET /api/tutors/{id}/photo no requiere autenticación
/// (a diferencia de los documentos de verificación, que son PII de Admin).
/// </summary>
[ApiController]
[Route("api/tutors")]
public class TutorPhotosController(TutorPhotoService service) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")!);

    /// <summary>
    /// Sube o reemplaza la foto de perfil del tutor autenticado (multipart: file).
    /// Devuelve la URL pública relativa de la foto.
    /// </summary>
    [HttpPost("me/photo")]
    [Authorize]
    [ProducesResponseType(typeof(TutorPhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(TutorPhotoService.MaxSizeBytes + 1024)]
    public async Task<ActionResult<TutorPhotoResponse>> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo requerido (multipart: file)",
                Status = StatusCodes.Status400BadRequest,
            });

        await using var stream = file.OpenReadStream();
        var result = await service.UploadAsync(
            GetUserId(), file.FileName, file.ContentType, file.Length, stream, ct);

        if (result.IsNotFound)
            return NotFound(new ProblemDetails
            {
                Title = "No tienes un perfil de tutor",
                Status = StatusCodes.Status404NotFound,
            });

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails
            {
                Title = "Imagen inválida: JPEG/PNG/WebP, ≤ 5 MB",
                Status = StatusCodes.Status400BadRequest,
            });

        return Ok(result.Value);
    }

    /// <summary>Contenido público de la foto de perfil del tutor (stream con su content-type).</summary>
    [HttpGet("{id:guid}/photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(Guid id, CancellationToken ct)
    {
        var result = await service.GetPhotoAsync(id, ct);
        if (result.IsNotFound)
            return NotFound();

        var photo = result.Value!;
        return File(photo.Content, photo.ContentType);
    }

    /// <summary>
    /// Confirma una subida directa de la foto (ADR-009): el navegador subió el
    /// archivo al store y aquí solo se envía la referencia. Evita el límite de
    /// 4.5 MB por request de Vercel Functions.
    /// </summary>
    [HttpPost("me/photo/attach")]
    [Authorize]
    [ProducesResponseType(typeof(TutorPhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorPhotoResponse>> Attach(
        [FromBody] AttachPhotoCommand command, CancellationToken ct)
    {
        var result = await service.AttachAsync(GetUserId(), command, ct);

        if (result.IsNotFound)
            return NotFound(new ProblemDetails
            {
                Title = "No tienes un perfil de tutor",
                Status = StatusCodes.Status404NotFound,
            });

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails
            {
                Title = "Adjunto inválido: JPEG/PNG/WebP, ≤ 5 MB, o el archivo no pertenece a tu cuenta",
                Status = StatusCodes.Status400BadRequest,
            });

        return Ok(result.Value);
    }
}
