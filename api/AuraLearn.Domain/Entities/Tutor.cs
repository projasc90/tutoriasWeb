using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Tutor verificado de la plataforma. La visibilidad en el catálogo público
/// depende de <see cref="VerificationStatus.Verified"/> (regla de negocio:
/// solo tutores verificados son reservables).
/// </summary>
public class Tutor
{
    public Guid Id { get; set; }

    /// <summary>Usuario propietario de esta postulación/perfil (FK a users).</summary>
    public Guid? UserId { get; set; }

    public required string Name { get; set; }

    /// <summary>Título o credencial académica (ej. "PhD Matemáticas").</summary>
    public required string Credentials { get; set; }

    /// <summary>Institución de procedencia (UCR, TEC, UNA, LEAD, ...).</summary>
    public required string University { get; set; }

    /// <summary>Valoración promedio (0-5).</summary>
    public decimal Rating { get; set; }

    public int Reviews { get; set; }

    /// <summary>Materias que imparte (etiquetas simples).</summary>
    public List<string> Subjects { get; set; } = [];

    /// <summary>Tarifa por hora en colones (CRC).</summary>
    public int PriceCrc { get; set; }

    /// <summary>Tarifa por hora en dólares (USD).</summary>
    public int PriceUsd { get; set; }

    public required string Bio { get; set; }

    /// <summary>Destacado en la landing.</summary>
    public bool Featured { get; set; }

    public VerificationStatus VerificationStatus { get; set; }

    /// <summary>Motivo del rechazo (auditoría funcional; null si no aplica).</summary>
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
}
