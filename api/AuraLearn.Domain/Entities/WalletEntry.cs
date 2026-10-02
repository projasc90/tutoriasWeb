using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Movimiento del monedero del estudiante (créditos por cancelación a tiempo).
/// El saldo se calcula sumando AmountCrc; los débitos futuros usarán montos negativos.
/// </summary>
public class WalletEntry
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Monto en colones (positivo = crédito).</summary>
    public int AmountCrc { get; set; }

    public WalletEntryReason Reason { get; set; }

    /// <summary>Reserva que originó el movimiento (nullable: movimientos manuales futuros).</summary>
    public Guid? ReservationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
