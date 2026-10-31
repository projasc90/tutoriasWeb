using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraLearn.Api.Controllers;

/// <summary>
/// Panel de administración de estudiantes: listado paginado de usuarios
/// con filtro por rol y búsqueda por texto (solo Admin).
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(AdminUserService service) : ControllerBase
{
    /// <summary>Listado paginado de usuarios (filtro role: Estudiante|Tutor|Admin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        Role? roleFilter = null;
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!Enum.TryParse<Role>(role, ignoreCase: true, out var parsed))
                return BadRequest(new ProblemDetails
                {
                    Title = "Rol inválido. Valores: Estudiante, Tutor, Admin.",
                    Status = StatusCodes.Status400BadRequest,
                });
            roleFilter = parsed;
        }

        var criteria = new AdminUserSearchCriteria(query, roleFilter, page, pageSize);
        var result = await service.SearchAsync(criteria, ct);

        if (result.IsInvalid)
            return BadRequest(new ProblemDetails
            {
                Title = "Criterios de búsqueda inválidos",
                Status = StatusCodes.Status400BadRequest,
            });

        return Ok(result.Value);
    }
}
