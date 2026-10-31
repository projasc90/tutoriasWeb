using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>Validador de criterios de búsqueda de usuarios (panel Admin).</summary>
public class AdminUserSearchCriteriaValidator : AbstractValidator<AdminUserSearchCriteria>
{
    public const int MaxPageSize = 100;

    public AdminUserSearchCriteriaValidator()
    {
        RuleFor(c => c.Page).GreaterThanOrEqualTo(1);
        RuleFor(c => c.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(MaxPageSize);
        RuleFor(c => c.Query).MaximumLength(200);
    }
}

/// <summary>Resultado tipado de la búsqueda de usuarios para el panel Admin.</summary>
public record AdminUserSearchResult(PagedResult<AdminUserDto>? Value, bool IsInvalid = false)
{
    public static AdminUserSearchResult Ok(PagedResult<AdminUserDto> value) => new(value);
    public static AdminUserSearchResult Invalid() => new(default, IsInvalid: true);
}

/// <summary>
/// Caso de uso: listado de usuarios para el panel de administración de
/// estudiantes (solo Admin). Delega el filtrado/paginado en el repositorio.
/// </summary>
public class AdminUserService(
    IUserRepository users,
    AdminUserSearchCriteriaValidator validator)
{
    public async Task<AdminUserSearchResult> SearchAsync(
        AdminUserSearchCriteria criteria, CancellationToken ct)
    {
        if (!validator.Validate(criteria).IsValid)
            return AdminUserSearchResult.Invalid();

        var page = await users.SearchAsync(criteria, ct);
        return AdminUserSearchResult.Ok(page);
    }
}
