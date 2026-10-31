namespace AuraLearn.Domain.Entities;

/// <summary>
/// Reglas de reserva de la plataforma (fila única, definidas por el Admin).
/// Cada regla es opcional (null = sin definir): los servicios usan su valor por
/// defecto (<c>BookingRulesDefaults</c> en Application) y el portal del tutor no
/// muestra la tarjeta correspondiente.
/// </summary>
public class BookingRules
{
    /// <summary>Id fijo de la fila única (singleton de configuración).</summary>
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; } = SingletonId;

    /// <summary>
    /// Anticipación mínima (horas) para que el estudiante cancele con crédito al
    /// monedero. null = sin definir (fallback 12 h).
    /// </summary>
    public int? CancellationNoticeHours { get; set; }

    /// <summary>
    /// Ventana para subir el comprobante SINPE (minutos desde la creación de la
    /// reserva). null = sin definir (fallback 60 min).
    /// </summary>
    public int? PaymentWindowMinutes { get; set; }

    /// <summary>
    /// Duración de los slots (minutos). Configuración declarativa: la generación
    /// sigue fijada a 60 min (acoplada a la granularidad de franjas e índices).
    /// null = sin definir (fallback 60 min).
    /// </summary>
    public int? SlotDurationMinutes { get; set; }

    /// <summary>
    /// Horizonte máximo de reserva (días) para la generación ensure-ahead de
    /// slots. null = sin definir (fallback 14 días).
    /// </summary>
    public int? BookingHorizonDays { get; set; }

    /// <summary>Última actualización (UTC).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Admin que definió las reglas por última vez (auditoría informativa, sin FK).</summary>
    public Guid? UpdatedByUserId { get; set; }
}
