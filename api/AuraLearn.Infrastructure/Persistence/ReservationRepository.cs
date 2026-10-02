using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto IReservationRepository usando EF Core.
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly AuraLearnDbContext _db;

    public ReservationRepository(AuraLearnDbContext db) => _db = db;

    public async Task<Reservation?> GetActiveBySlotAsync(Guid slotId, CancellationToken ct)
    {
        // Solo estados que bloquean el slot (índice único parcial: status IN (1,2))
        return await _db.Reservations
            .Where(r => r.SlotId == slotId &&
                        (r.Status == Domain.Enums.ReservationStatus.PendingPayment ||
                         r.Status == Domain.Enums.ReservationStatus.Confirmed))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Reservation?> GetByIdempotencyKeyAsync(string key, CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Slot).ThenInclude(s => s.Tutor)
            .FirstOrDefaultAsync(r => r.IdempotencyKey == key, ct);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Slot).ThenInclude(s => s.Tutor)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Reservation?> GetByConfirmationNumberAsync(string number, CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == number, ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetByStudentAsync(
        Guid studentId, CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Slot).ThenInclude(s => s.Tutor)
            .Include(r => r.Student)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetPendingComprobantesAsync(CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Slot).ThenInclude(s => s.Tutor)
            .Include(r => r.Student)
            .Where(r => r.Status == Domain.Enums.ReservationStatus.PendingPayment &&
                        r.ConfirmationNumber != null)
            .OrderBy(r => r.ComprobanteSubmittedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetByTutorUserAsync(
        Guid tutorUserId, CancellationToken ct)
    {
        return await _db.Reservations
            .Include(r => r.Slot).ThenInclude(s => s.Tutor)
            .Include(r => r.Student)
            .Where(r => r.Slot.Tutor.UserId == tutorUserId)
            .OrderByDescending(r => r.Slot.StartAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken ct)
    {
        await _db.Reservations.AddAsync(reservation, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken ct)
    {
        _db.Reservations.Update(reservation);
        await _db.SaveChangesAsync(ct);
    }
}
