using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Dto;

/// <summary>
/// DTO de usuario para el panel de administración de estudiantes.
/// Expone solo datos no sensibles (sin PasswordHash ni documentos).
/// </summary>
public record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    Role Role,
    DateTime CreatedAt);

/// <summary>
/// Solicitud de cambio de rol de un usuario (panel Admin).
/// <see cref="Role"/> llega como texto (<c>Estudiante</c> | <c>Tutor</c> | <c>Admin</c>)
/// para que el cliente no dependa de los valores numéricos del enum.
/// </summary>
public record ChangeUserRoleRequest(string? Role);

/// <summary>Códigos de error del cambio de rol (body <c>{ error }</c> con 422).</summary>
public static class AdminUserErrors
{
    /// <summary>El rol recibido no es un valor válido de <see cref="Role"/>.</summary>
    public const string InvalidRole = "USER_ROLE_INVALID";

    /// <summary>El usuario ya tiene ese rol: no hay nada que cambiar.</summary>
    public const string RoleUnchanged = "USER_ROLE_UNCHANGED";

    /// <summary>
    /// Quitar el rol Admin al último administrador dejaría la plataforma sin
    /// nadie capaz de aprobar pagos, verificar tutores ni editar reglas.
    /// </summary>
    public const string LastAdmin = "USER_ROLE_LAST_ADMIN_REQUIRED";
}
