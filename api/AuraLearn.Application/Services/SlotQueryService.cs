using AuraLearn.Application.Dto;
using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Services;

/// <summary>
/// Lógica pura de consulta de slots disponibles. DateTime en UTC;
/// la conversión a zona CR se hace en el contrato/presentación.
/// </summary>
public static class SlotQueryService
{
    /// <summary>Convierte una lista de slots a DTOs ordenados por StartAt.</summary>
    public static IReadOnlyList<SlotDto> ToDtos(IReadOnlyList<Slot> slots) =>
        slots
            .OrderBy(s => s.StartAt)
            .Select(s => new SlotDto(s.Id, s.TutorId, s.StartAt, s.EndAt))
            .ToList();

    /// <summary>Primer StartAt UTC futuro, o null si no hay slots.</summary>
    public static DateTime? GetNextSlotOrNull(IReadOnlyList<Slot> slots)
    {
        var now = DateTime.UtcNow;
        return slots
            .Where(s => s.StartAt > now)
            .OrderBy(s => s.StartAt)
            .FirstOrDefault()
            ?.StartAt;
    }
}
