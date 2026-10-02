namespace AuraLearn.Domain.Enums;

/// <summary>
/// Estados del ciclo de vida de una reserva de tutoría.
/// Transiciones válidas centralizadas en <see cref="AuraLearn.Domain.Entities.ReservationStateMachine"/>.
/// </summary>
public enum ReservationStatus
{
    /// <summary>Reserva creada, esperando comprobante SINPE (expira en 60 min).</summary>
    PendingPayment = 1,

    /// <summary>Pago acreditado (comprobante aprobado por Admin).</summary>
    Confirmed = 2,

    /// <summary>Comprobante rechazado por el Admin (slot liberado).</summary>
    Rejected = 3,

    /// <summary>Sin comprobante dentro de la ventana de 60 min (slot liberado).</summary>
    Expired = 4,

    /// <summary>Cancelada por el estudiante (≥12 h acredita monedero; <12 h sin reembolso).</summary>
    Cancelled = 5,

    /// <summary>Sesión concluida con éxito (liquidación al tutor, tarea posterior).</summary>
    Completed = 6,
}
