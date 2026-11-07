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

    /// <summary>
    /// Cambia el rol de un usuario (Estudiante | Tutor | Admin).
    /// El rol viaja en el JWT: el afectado debe volver a iniciar sesión.
    /// </summary>
    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeRole(
        Guid id, [FromBody] ChangeUserRoleRequest request, CancellationToken ct)
    {
        if (!TryParseRole(request.Role, out var newRole))
            return BadRequest(new ProblemDetails
            {
                Title = "Rol inválido. Valores: Estudiante, Tutor, Admin.",
                Status = StatusCodes.Status400BadRequest,
            });

        var result = await service.ChangeRoleAsync(id, newRole, ct);

        if (result.IsNotFound)
            return NotFound(new ProblemDetails
            {
                Title = "Usuario no encontrado",
                Status = StatusCodes.Status404NotFound,
            });

        if (result.IsInvalid)
            return UnprocessableEntity(new { error = result.ErrorCode });

        return Ok(result.Value);
    }

    /// <summary>
    /// Acepta el nombre del rol (case-insensitive) y rechaza números fuera del
    /// enum: <c>Enum.TryParse</c> por sí solo aceptaría <c>"99"</c>.
    /// </summary>
    private static bool TryParseRole(string? value, out Role role)
    {
        role = default;
        return !string.IsNullOrWhiteSpace(value)
            && Enum.TryParse(value, ignoreCase: true, out role)
            && Enum.IsDefined(role);
    }
}
