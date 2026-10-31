using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>Implementación del puerto ITutorAvailabilityExceptionRepository con EF Core.</summary>
public class TutorAvailabilityExceptionRepository(AuraLearnDbContext db) : ITutorAvailabilityExceptionRepository
{
    public async Task<IReadOnlyList<TutorAvailabilityException>> GetByTutorAsync(Guid tutorId, CancellationToken ct) =>
        await db.TutorAvailabilityExceptions.AsNoTracking()
            .Where(e => e.TutorId == tutorId)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);

    public async Task ReplaceAllAsync(Guid tutorId, IReadOnlyList<TutorAvailabilityException> exceptions, CancellationToken ct)
    {
        var existing = await db.TutorAvailabilityExceptions
            .Where(e => e.TutorId == tutorId)
            .ToListAsync(ct);
        db.TutorAvailabilityExceptions.RemoveRange(existing);
        db.TutorAvailabilityExceptions.AddRange(exceptions);
        await db.SaveChangesAsync(ct);
    }
}
