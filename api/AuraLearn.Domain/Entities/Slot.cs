namespace AuraLearn.Domain.Entities;

/// <summary>
/// Espacio horario reservable en la agenda de un tutor.
/// Un slot es exclusivo: la unicidad (tutor, inicio) es el guardián real contra el doble-booking.
/// Fechas en UTC; la zona America/Costa_Rica se aplica solo en presentación/contrato.
/// </summary>
public class Slot
{
    public Guid Id { get; set; }

    /// <summary>Tutor dueño del slot.</summary>
    public Guid TutorId { get; set; }

    /// <summary>Inicio del slot en UTC.</summary>
    public DateTime StartAt { get; set; }

    /// <summary>Fin del slot en UTC (StartAt + duración).</summary>
    public DateTime EndAt { get; set; }

    public Tutor Tutor { get; set; } = null!;
}
