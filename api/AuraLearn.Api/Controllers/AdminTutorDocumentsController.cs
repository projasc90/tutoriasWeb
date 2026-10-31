using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Descarga del contenido de un documento de verificación (solo Admin).
/// Los documentos son PII: el tutor no descarga su propio archivo desde la API
/// y el resto de roles reciben 403.
/// </summary>
[ApiController]
[Route("api/admin/tutor-documents")]
[Authorize(Roles = "Admin")]
public class AdminTutorDocumentsController(TutorDocumentService service) : ControllerBase
{
    /// <summary>Contenido del documento (stream con su tipo MIME y nombre original).</summary>
    [HttpGet("{id:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await service.GetFileAsync(id, ct);
        if (result.IsNotFound)
            return NotFound(new ProblemDetails
            {
                Title = "Documento no encontrado",
                Status = StatusCodes.Status404NotFound,
            });

        var file = result.Value!;
        return File(file.Content, file.ContentType, file.FileName);
    }
}
