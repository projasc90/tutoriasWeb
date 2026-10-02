using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Máquina de estados centralizada de la reserva. Única fuente de verdad para
/// las transiciones válidas entre estados (skill slots-reservations: no duplicar
/// la lógica de "¿puedo...?" en varias capas).
/// </summary>
public static class ReservationStateMachine
{
    /// <summary>Estados desde los que ya no se puede transicionar.</summary>
    public static readonly IReadOnlySet<ReservationStatus> TerminalStates =
        new HashSet<ReservationStatus>
        {
            ReservationStatus.Rejected,
            ReservationStatus.Expired,
            ReservationStatus.Cancelled,
            ReservationStatus.Completed,
        };

    /// <summary>Estados que bloquean el slot (índice único parcial de BD).</summary>
    public static readonly IReadOnlySet<ReservationStatus> SlotBlockingStates =
        new HashSet<ReservationStatus>
        {
            ReservationStatus.PendingPayment,
            ReservationStatus.Confirmed,
        };

    /// <summary>Indica si la reserva puede recibir un comprobante SINPE.</summary>
    public static bool CanSubmitComprobante(ReservationStatus current) =>
        current == ReservationStatus.PendingPayment;

    /// <summary>Indica si la reserva puede ser cancelada por el estudiante.</summary>
    public static bool CanCancel(ReservationStatus current) =>
        current is ReservationStatus.PendingPayment or ReservationStatus.Confirmed;

    /// <summary>Indica si la reserva puede ser decidida (aprobada/rechazada) por un Admin.</summary>
    public static bool CanDecide(ReservationStatus current) =>
        current == ReservationStatus.PendingPayment;

    /// <summary>Transición al confirmar pago (decisión Admin: approve).</summary>
    public static bool CanConfirm(ReservationStatus current) => CanDecide(current);

    /// <summary>Transición al rechazar comprobante (decisión Admin: reject).</summary>
    public static bool CanReject(ReservationStatus current) => CanDecide(current);

    /// <summary>Transición al expirar la ventana de pago (60 min).</summary>
    public static bool CanExpire(ReservationStatus current) =>
        current == ReservationStatus.PendingPayment;

    /// <summary>Transición al concluir la sesión (tarea posterior: liquidación al tutor).</summary>
    public static bool CanComplete(ReservationStatus current) =>
        current == ReservationStatus.Confirmed;
}
