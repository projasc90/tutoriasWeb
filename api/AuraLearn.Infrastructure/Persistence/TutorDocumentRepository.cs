using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>Implementación del puerto ITutorDocumentRepository con EF Core.</summary>
public class TutorDocumentRepository(AuraLearnDbContext db) : ITutorDocumentRepository
{
    public Task<TutorDocument?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.TutorDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<TutorDocument?> GetByKindAsync(Guid tutorId, TutorDocumentKind kind, CancellationToken ct) =>
        db.TutorDocuments.FirstOrDefaultAsync(d => d.TutorId == tutorId && d.Kind == kind, ct);

    public async Task<IReadOnlyList<TutorDocument>> ListByTutorAsync(Guid tutorId, CancellationToken ct) =>
        await db.TutorDocuments.AsNoTracking()
            .Where(d => d.TutorId == tutorId)
            .OrderBy(d => d.Kind)
            .ThenBy(d => d.UploadedAt)
            .ToListAsync(ct);

    public async Task AddAsync(TutorDocument document, CancellationToken ct)
    {
        db.TutorDocuments.Add(document);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(TutorDocument document, CancellationToken ct)
    {
        db.TutorDocuments.Remove(document);
        await db.SaveChangesAsync(ct);
    }
}
