using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso a datos de slots (implementado por Infrastructure).</summary>
public interface ISlotRepository
{
    /// <summary>Slots de un tutor en un rango [from, to) UTC, ordenados por inicio.</summary>
    Task<IReadOnlyList<Slot>> GetByTutorInRangeAsync(Guid tutorId, DateTime from, DateTime to, CancellationToken ct);

    /// <summary>Slots de varios tutores en un rango (para el próximo cupo del catálogo).</summary>
    Task<IReadOnlyList<Slot>> GetByTutorsInRangeAsync(IReadOnlyList<Guid> tutorIds, DateTime from, DateTime to, CancellationToken ct);

    /// <summary>Obtiene un slot por id (con su tutor, para validar precio/estado).</summary>
    Task<Slot?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Ensure-ahead del rango: genera los slots faltantes del horario de negocio
    /// para los tutores dados (idempotencia garantizada por el índice único
    /// (tutor_id, start_at)), persiste los nuevos y devuelve todos los slots
    /// del rango, ordenados por inicio.
    /// </summary>
    Task<IReadOnlyList<Slot>> EnsureAheadInRangeAsync(
        IReadOnlyList<Guid> tutorIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct);

    /// <summary>Primer slot futuro disponible de un tutor (para NextSlotAt del catálogo).</summary>
    Task<DateTime?> GetNextAvailableStartAsync(Guid tutorId, DateTime nowUtc, CancellationToken ct);
}
