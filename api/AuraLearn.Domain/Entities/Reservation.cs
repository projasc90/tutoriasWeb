using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Reserva de un slot por un estudiante, con su ciclo de pago SINPE.
/// La exclusividad del slot la garantiza el índice único parcial de BD sobre
/// (slot_id) WHERE status IN (PendingPayment, Confirmed) — no la validación en servicio.
/// </summary>
public class Reservation
{
    public Guid Id { get; set; }

    public Guid SlotId { get; set; }

    /// <summary>Estudiante que reserva (rol Estudiante).</summary>
    public Guid StudentId { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.PendingPayment;

    /// <summary>Precio CRC congelado al momento de reservar (snapshot del tutor).</summary>
    public int PriceCrc { get; set; }

    /// <summary>Clave de idempotencia del request de creación (reintentos del cliente).</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Límite para subir comprobante; al vencer, la reserva expira y el slot se libera.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Número de confirmación SINPE (clave única: idempotencia de acreditación).</summary>
    public string? ConfirmationNumber { get; set; }

    /// <summary>Monto declarado en el comprobante (CRC); validado contra PriceCrc.</summary>
    public int? ComprobanteAmountCrc { get; set; }

    /// <summary>Teléfono emisor declarado en el comprobante (opcional, para auditoría).</summary>
    public string? ComprobantePhone { get; set; }

    public DateTime? ComprobanteSubmittedAt { get; set; }

    /// <summary>Motivo del rechazo del comprobante (decisión del Admin).</summary>
    public string? DecisionReason { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Slot Slot { get; set; } = null!;
    public User Student { get; set; } = null!;
}
