using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso a los días de excepción de la agenda del tutor.</summary>
public interface ITutorAvailabilityExceptionRepository
{
    Task<IReadOnlyList<TutorAvailabilityException>> GetByTutorAsync(Guid tutorId, CancellationToken ct);

    /// <summary>Reemplazo completo de las excepciones del tutor.</summary>
    Task ReplaceAllAsync(Guid tutorId, IReadOnlyList<TutorAvailabilityException> exceptions, CancellationToken ct);
}
