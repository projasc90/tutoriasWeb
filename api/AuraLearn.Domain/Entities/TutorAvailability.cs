namespace AuraLearn.Domain.Entities;

/// <summary>
/// Regla semanal de disponibilidad de un tutor: una franja horaria local (en la
/// zona horaria del tutor) para un día de la semana. Los slots solo se generan
/// dentro de las franjas definidas; sin reglas, aplica el horario de negocio
/// por defecto (08:00–20:00 CR).
/// </summary>
public class TutorAvailability
{
    public Guid Id { get; set; }

    /// <summary>Tutor dueño de la regla.</summary>
    public Guid TutorId { get; set; }

    /// <summary>Día de la semana (0=domingo … 6=sábado, igual que DayOfWeek).</summary>
    public int Weekday { get; set; }

    /// <summary>Hora local (zona del tutor) de inicio de la franja (incluida).</summary>
    public TimeOnly StartLocal { get; set; }

    /// <summary>Hora local (zona del tutor) de fin de la franja (excluida).</summary>
    public TimeOnly EndLocal { get; set; }

    /// <summary>
    /// Interruptor maestro del horario semanal: false = las franjas semanales
    /// no generan cupos (el tutor pausa su agenda recurrente).
    /// </summary>
    public bool WeeklyEnabled { get; set; } = true;

    /// <summary>
    /// true (default) = la franja se repite todas las semanas. false = aplica
    /// una sola vez, en la fecha de <see cref="AnchorDate"/>.
    /// </summary>
    public bool IsRecurring { get; set; } = true;

    /// <summary>
    /// Fecha local CR en la que aplica una franja no recurrente (null si es
    /// recurrente). La calcula AvailabilityService al guardar (próxima
    /// ocurrencia del weekday) para que la generación sea determinista.
    /// </summary>
    public DateOnly? AnchorDate { get; set; }

    public Tutor Tutor { get; set; } = null!;
}

