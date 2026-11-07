using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Enums;
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

/// <summary>Resultado tipado del cambio de rol de un usuario.</summary>
public record ChangeUserRoleResult(
    AdminUserDto? Value = null,
    bool IsNotFound = false,
    bool IsInvalid = false,
    string? ErrorCode = null)
{
    public static ChangeUserRoleResult Ok(AdminUserDto value) => new(Value: value);
    public static ChangeUserRoleResult NotFound() => new(IsNotFound: true);
    public static ChangeUserRoleResult Invalid(string errorCode) =>
        new(IsInvalid: true, ErrorCode: errorCode);
}

/// <summary>
/// Caso de uso: listado de usuarios para el panel de administración de
/// estudiantes y cambio de rol (bootstrap y escalado de permisos). Ambos son
/// exclusivos de Admin a nivel de controller.
///
/// El rol viaja dentro del JWT con 8 h de vida: tras un cambio, el afectado
/// debe volver a iniciar sesión para que el token refleje el rol nuevo.
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

    /// <summary>
    /// Cambia el rol de un usuario. Invariante de negocio: nunca se puede quitar
    /// el rol Admin al último administrador, porque nadie podría aprobar pagos,
    /// verificar tutores ni editar las reglas de reserva.
    /// </summary>
    /// <param name="targetUserId">Usuario cuyo rol cambia.</param>
    /// <param name="newRole">Rol destino.</param>
    /// <param name="ct">Cancelación.</param>
    public async Task<ChangeUserRoleResult> ChangeRoleAsync(
        Guid targetUserId, Role newRole, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(targetUserId, ct);
        if (user is null)
            return ChangeUserRoleResult.NotFound();

        if (user.Role == newRole)
            return ChangeUserRoleResult.Invalid(AdminUserErrors.RoleUnchanged);

        // Degradar al último Admin dejaría la plataforma sin administración:
        // se valida antes de escribir para no depender de un rollback.
        if (user.Role == Role.Admin && newRole != Role.Admin)
        {
            var adminCount = await users.CountByRoleAsync(Role.Admin, ct);
            if (adminCount <= 1)
                return ChangeUserRoleResult.Invalid(AdminUserErrors.LastAdmin);
        }

        user.Role = newRole;
        await users.UpdateAsync(user, ct);

        return ChangeUserRoleResult.Ok(
            new AdminUserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt));
    }
}
