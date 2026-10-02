using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Services;

/// <summary>
/// Caso de uso de disponibilidad (ADR-005, ensure-ahead): antes de consultar los
/// slots de un tutor —o del catálogo—, genera y persiste de forma idempotente los
/// slots faltantes del horario de negocio CR en el rango pedido.
/// La generación pura vive en <see cref="SlotGenerationService"/>; la persistencia
/// (pre-consulta + inserción por lotes tolerante a carreras) en SlotRepository.
/// </summary>
public class SlotAvailabilityService(ISlotRepository slots)
{
    /// <summary>
    /// Ensure-ahead en [from, to) UTC para los tutores dados.
    /// Recorta el inicio al presente: nunca genera slots en el pasado.
    /// Devuelve todos los slots existentes del rango, ordenados por inicio.
    /// </summary>
    public async Task<IReadOnlyList<Slot>> EnsureAheadAsync(
        IReadOnlyList<Guid> tutorIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        if (tutorIds.Count == 0)
            return [];

        var from = fromUtc < DateTime.UtcNow ? DateTime.UtcNow : fromUtc;
        if (from >= toUtc)
            return [];

        return await slots.EnsureAheadInRangeAsync(tutorIds, from, toUtc, ct);
    }
}
