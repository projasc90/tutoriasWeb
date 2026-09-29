using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto ITutorRepository con EF Core.
/// Lecturas con AsNoTracking y proyección mínima; paginación en SQL.
/// </summary>
public class TutorRepository(AuraLearnDbContext db) : ITutorRepository
{
    public async Task<(IReadOnlyList<Tutor> Items, int TotalCount)> SearchAsync(
        Application.Interfaces.TutorSearchCriteria criteria, CancellationToken ct)
    {
        var query = db.Tutors
            .AsNoTracking()
            .Where(t => t.VerificationStatus == VerificationStatus.Verified);

        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var q = criteria.Query.Trim().ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(q) ||
                t.Subjects.Any(s => s.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.University) && criteria.University != "Todas")
            query = query.Where(t => t.University == criteria.University);

        if (criteria.MinRating.HasValue)
            query = query.Where(t => t.Rating >= criteria.MinRating.Value);

        if (criteria.PriceMinCrc.HasValue)
            query = query.Where(t => t.PriceCrc >= criteria.PriceMinCrc.Value);

        if (criteria.PriceMaxCrc.HasValue)
            query = query.Where(t => t.PriceCrc <= criteria.PriceMaxCrc.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.Rating)
            .ThenByDescending(t => t.Reviews)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Tutor?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        await db.Tutors.FirstOrDefaultAsync(t => t.UserId == userId, ct);

    public async Task<Tutor?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Tutors.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Tutor>> GetPendingAsync(CancellationToken ct) =>
        await db.Tutors.AsNoTracking()
            .Where(t => t.VerificationStatus == VerificationStatus.PendingReview)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Tutor tutor, CancellationToken ct)
    {
        db.Tutors.Add(tutor);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tutor tutor, CancellationToken ct)
    {
        db.Tutors.Update(tutor);
        await db.SaveChangesAsync(ct);
    }
}
