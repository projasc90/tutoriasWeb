using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de las reglas semanales de disponibilidad del tutor.</summary>
public interface ITutorAvailabilityRepository
{
    /// <summary>Todas las reglas del tutor.</summary>
    Task<IReadOnlyList<TutorAvailability>> GetByTutorAsync(Guid tutorId, CancellationToken ct);

    /// <summary>Reemplazo completo de las reglas del tutor (transaccional).</summary>
    Task ReplaceAllAsync(Guid tutorId, IReadOnlyList<TutorAvailability> rules, CancellationToken ct);
}
