using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso a datos de reservas (implementado por Infrastructure).</summary>
public interface IReservationRepository
{
    /// <summary>
    /// Busca una reserva activa (PendingPayment/Confirmed) del slot.
    /// Usada para detectar el conflicto ANTES del insert (el guardián real es el índice único parcial).
    /// </summary>
    Task<Reservation?> GetActiveBySlotAsync(Guid slotId, CancellationToken ct);

    /// <summary>Reserva por clave de idempotencia (replay del cliente).</summary>
    Task<Reservation?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct);

    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Reservas del estudiante, más recientes primero.</summary>
    Task<IReadOnlyList<Reservation>> GetByStudentAsync(Guid studentId, CancellationToken ct);

    /// <summary>Reservas de las sesiones que imparte el usuario con rol Tutor.</summary>
    Task<IReadOnlyList<Reservation>> GetByTutorUserAsync(Guid tutorUserId, CancellationToken ct);

    /// <summary>Comprobantes pendientes de decisión del admin (cola de pagos).</summary>
    Task<IReadOnlyList<Reservation>> GetPendingComprobantesAsync(CancellationToken ct);

    /// <summary>Busca una reserva por número de confirmación (idempotencia del comprobante).</summary>
    Task<Reservation?> GetByConfirmationNumberAsync(string confirmationNumber, CancellationToken ct);

    Task AddAsync(Reservation reservation, CancellationToken ct);

    Task UpdateAsync(Reservation reservation, CancellationToken ct);
}
