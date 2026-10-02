using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto ITutorAvailabilityRepository usando EF Core.
/// </summary>
public class TutorAvailabilityRepository(AuraLearnDbContext db) : ITutorAvailabilityRepository
{
    public async Task<IReadOnlyList<TutorAvailability>> GetByTutorAsync(
        Guid tutorId, CancellationToken ct)
    {
        return await db.TutorAvailability
            .Where(a => a.TutorId == tutorId)
            .OrderBy(a => a.Weekday).ThenBy(a => a.StartLocal)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task ReplaceAllAsync(
        Guid tutorId, IReadOnlyList<TutorAvailability> rules, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var old = await db.TutorAvailability
            .Where(a => a.TutorId == tutorId)
            .ToListAsync(ct);
        db.TutorAvailability.RemoveRange(old);

        foreach (var rule in rules)
        {
            rule.Id = Guid.NewGuid();
            rule.TutorId = tutorId;
        }
        await db.TutorAvailability.AddRangeAsync(rules, ct);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
