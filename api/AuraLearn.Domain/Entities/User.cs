using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Usuario de la plataforma (estudiante, tutor o admin).
/// El hash de contraseña nunca se expone fuera de la capa de persistencia.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Email único del usuario (índice unique en BD).</summary>
    public required string Email { get; set; }

    /// <summary>Hash PBKDF2 generado por PasswordHasher de ASP.NET Core.</summary>
    public required string PasswordHash { get; set; }

    public required string FullName { get; set; }

    public Role Role { get; set; } = Role.Estudiante;

    public DateTime CreatedAt { get; set; }
}
