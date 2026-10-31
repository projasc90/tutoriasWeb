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
