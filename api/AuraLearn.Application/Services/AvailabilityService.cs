using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Services;

/// <summary>
/// Caso de uso: gestión de las reglas semanales de disponibilidad del tutor.
/// Validación de negocio aquí (franjas válidas, sin solapes, granularidad 1 h);
/// el honoring lo aplica SlotGenerationService al generar slots.
/// </summary>
public class AvailabilityService(
    ITutorAvailabilityRepository availability,
    ITutorRepository tutors)
{
    public async Task<IReadOnlyList<TutorAvailability>?> GetForCurrentUserAsync(
        Guid userId, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return null;
        return await availability.GetByTutorAsync(tutor.Id, ct);
    }

    /// <summary>Reemplazo completo de las reglas del tutor. Devuelve null si el usuario no es tutor.</summary>
    public async Task<AvailabilityResult> ReplaceForCurrentUserAsync(
        Guid userId, IReadOnlyList<AvailabilityRuleDto> rulesDto, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return AvailabilityResult.Forbidden();

        var rules = new List<TutorAvailability>();
        var byDay = new Dictionary<int, List<(TimeOnly Start, TimeOnly End)>>();

        foreach (var dto in rulesDto)
        {
            if (dto.Weekday is < 0 or > 6)
                return AvailabilityResult.Invalid();
            if (dto.StartLocal >= dto.EndLocal)
                return AvailabilityResult.Invalid();
            // Granularidad 1 h alineada con la duración de los slots
            if (dto.StartLocal.Minute != 0 || dto.StartLocal.Second != 0 ||
                dto.EndLocal.Minute != 0 || dto.EndLocal.Second != 0)
                return AvailabilityResult.Invalid();
            if (dto.StartLocal < new TimeOnly(0, 0) || dto.EndLocal > new TimeOnly(23, 59))
                return AvailabilityResult.Invalid();

            if (!byDay.TryGetValue(dto.Weekday, out var franjas))
                byDay[dto.Weekday] = franjas = [];
            franjas.Add((dto.StartLocal, dto.EndLocal));
        }

        // Sin solapes dentro del mismo día (incluye duplicados exactos)
        foreach (var (_, franjas) in byDay)
        {
            var ordered = franjas.OrderBy(f => f.Start).ToList();
            for (int i = 1; i < ordered.Count; i++)
                if (ordered[i].Start < ordered[i - 1].End)
                    return AvailabilityResult.Invalid(AvailabilityErrors.Overlap);
        }

        foreach (var (weekday, _) in byDay)
        {
            foreach (var dto in rulesDto.Where(r => r.Weekday == weekday))
            {
                rules.Add(new TutorAvailability
                {
                    Id = Guid.NewGuid(),
                    TutorId = tutor.Id,
                    Weekday = dto.Weekday,
                    StartLocal = dto.StartLocal,
                    EndLocal = dto.EndLocal,
                    Tutor = null!,
                });
            }
        }

        await availability.ReplaceAllAsync(tutor.Id, rules, ct);
        return AvailabilityResult.Ok();
    }
}

/// <summary>Resultado tipado de la operación de disponibilidad.</summary>
public record AvailabilityResult(bool IsForbidden, bool IsInvalid, string? ErrorCode = null)
{
    public static AvailabilityResult Ok() => new(false, false);
    public static AvailabilityResult Forbidden() => new(true, false);
    public static AvailabilityResult Invalid(string? code = null) => new(false, true, code);
}