using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TutorsController(TutorService tutorService) : ControllerBase
{
    /// <summary>
    /// Catálogo público de tutores verificados, con filtros y paginación.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? university,
        [FromQuery] decimal? minRating,
        [FromQuery] int? priceMin,
        [FromQuery] int? priceMax,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var criteria = new TutorSearchCriteria(
            query, university, minRating, priceMin, priceMax, page, pageSize);

        var result = await tutorService.SearchAsync(criteria, ct);
        return Ok(result);
    }

    /// <summary>Detalle público de un tutor verificado (perfil /tutores/[id]).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tutor = await tutorService.GetByIdAsync(id, ct);
        return tutor is null ? NotFound() : Ok(tutor);
    }
}
