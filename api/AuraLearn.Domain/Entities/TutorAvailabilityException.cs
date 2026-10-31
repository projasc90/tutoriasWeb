namespace AuraLearn.Domain.Entities;

/// <summary>
/// Día de excepción de la agenda de un tutor (feriado, vacaciones, bloqueo
/// puntual): en esa fecha el horario semanal NO genera cupos. La fecha es
/// local a la zona horaria del tutor.
/// </summary>
public class TutorAvailabilityException
{
    public Guid Id { get; set; }

    /// <summary>Tutor dueño de la excepción.</summary>
    public Guid TutorId { get; set; }

    /// <summary>Fecha local (zona del tutor) en la que no se generan cupos.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Motivo opcional (feriado, vacaciones, evento…).</summary>
    public string? Reason { get; set; }

    public Tutor Tutor { get; set; } = null!;
}
