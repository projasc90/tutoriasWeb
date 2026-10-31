using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto IBookingRulesRepository usando EF Core.
/// Upsert sobre la fila única (id fijo <see cref="BookingRules.SingletonId"/>).
/// </summary>
public class BookingRulesRepository(AuraLearnDbContext db) : IBookingRulesRepository
{
    public async Task<BookingRules?> GetAsync(CancellationToken ct)
    {
        return await db.BookingRules
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpsertAsync(BookingRules rules, CancellationToken ct)
    {
        rules.Id = BookingRules.SingletonId;

        var existing = await db.BookingRules.FirstOrDefaultAsync(ct);
        if (existing is null)
        {
            db.BookingRules.Add(rules);
        }
        else
        {
            // Copia sobre la entidad tracked para evitar conflicto de tracking
            existing.CancellationNoticeHours = rules.CancellationNoticeHours;
            existing.PaymentWindowMinutes = rules.PaymentWindowMinutes;
            existing.SlotDurationMinutes = rules.SlotDurationMinutes;
            existing.BookingHorizonDays = rules.BookingHorizonDays;
            existing.UpdatedAt = rules.UpdatedAt;
            existing.UpdatedByUserId = rules.UpdatedByUserId;
        }

        await db.SaveChangesAsync(ct);
    }
}
