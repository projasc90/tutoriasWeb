namespace AuraLearn.Domain.Entities;

/// <summary>
/// Regla semanal de disponibilidad de un tutor: una franja horaria local (CR)
/// para un día de la semana. Los slots solo se generan dentro de las franjas
/// definidas; sin reglas, aplica el horario de negocio por defecto (08:00–20:00 CR).
/// </summary>
public class TutorAvailability
{
    public Guid Id { get; set; }

    /// <summary>Tutor dueño de la regla.</summary>
    public Guid TutorId { get; set; }

    /// <summary>Día de la semana (0=domingo … 6=sábado, igual que DayOfWeek).</summary>
    public int Weekday { get; set; }

    /// <summary>Hora local CR de inicio de la franja (incluida).</summary>
    public TimeOnly StartLocal { get; set; }

    /// <summary>Hora local CR de fin de la franja (excluida).</summary>
    public TimeOnly EndLocal { get; set; }

    public Tutor Tutor { get; set; } = null!;
}
