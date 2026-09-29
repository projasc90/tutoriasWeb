using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Criterios de búsqueda del catálogo de tutores.</summary>
public record TutorSearchCriteria(
    string? Query,
    string? University,
    decimal? MinRating,
    int? PriceMinCrc,
    int? PriceMaxCrc,
    int Page,
    int PageSize);

/// <summary>Puerto de acceso a datos de tutores (implementado por Infrastructure).</summary>
public interface ITutorRepository
{
    Task<(IReadOnlyList<Tutor> Items, int TotalCount)> SearchAsync(
        TutorSearchCriteria criteria, CancellationToken ct);

    /// <summary>Postulación del usuario autenticado (o null si nunca postuló).</summary>
    Task<Tutor?> GetByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>Postulación por id (para la decisión del admin).</summary>
    Task<Tutor?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Postulaciones pendientes de revisión (cola del admin).</summary>
    Task<IReadOnlyList<Tutor>> GetPendingAsync(CancellationToken ct);

    Task AddAsync(Tutor tutor, CancellationToken ct);

    Task UpdateAsync(Tutor tutor, CancellationToken ct);
}
