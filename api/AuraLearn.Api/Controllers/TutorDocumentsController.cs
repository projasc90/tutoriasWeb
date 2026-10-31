using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Documentos de verificación del tutor autenticado (título, certificaciones,
/// identificación). Subida multipart y listado de metadatos; el contenido solo
/// lo descarga el admin (PII, ver AdminTutorDocumentsController).
/// </summary>
[ApiController]
[Route("api/tutors/me/documents")]
[Authorize]
public class TutorDocumentsController(TutorDocumentService service) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")!);

    /// <summary>Documentos del tutor autenticado (metadatos, sin contenido).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TutorDocumentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TutorDocumentItemDto>>> List(CancellationToken ct)
    {
        var result = await service.ListAsync(GetUserId(), ct);
        if (result.IsNotFound)
            return NotFound();

        return Ok(result.Value);
    }

    /// <summary>
    /// Sube un documento de verificación (multipart: kind + file).
    /// Titulo/Identificacion reemplazan el archivo previo; Certificacion acumula (máx 5).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TutorDocumentItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [RequestSizeLimit(TutorDocumentService.MaxSizeBytes + 1024)]
    public async Task<ActionResult<TutorDocumentItemDto>> Upload(
        [FromForm] TutorDocumentKind kind, IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo requerido (multipart: kind + file)",
                Status = StatusCodes.Status400BadRequest,
            });

        await using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(kind, file.FileName, file.ContentType, file.Length, stream);
        var result = await service.UploadAsync(GetUserId(), command, ct);

        if (result.IsNotFound)
            return NotFound(new ProblemDetails
            {
                Title = "No tienes una postulación de tutor",
                Status = StatusCodes.Status404NotFound,
            });

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo inválido: PDF/JPEG/PNG, ≤ 5 MB; certificaciones máx. 5",
                Status = StatusCodes.Status400BadRequest,
            });

        if (result.IsConflict)
            return Conflict(new { error = result.ErrorCode ?? TutorDocumentErrors.DocumentsLocked });

        return CreatedAtAction(nameof(List), result.Value);
    }
}
