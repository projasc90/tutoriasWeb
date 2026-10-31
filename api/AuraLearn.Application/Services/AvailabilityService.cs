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
    ITutorRepository tutors,
    ITutorAvailabilityExceptionRepository exceptions)
{
    /// <summary>Zonas horarias IANA ofrecidas en el selector del portal.</summary>
    public static readonly IReadOnlyList<string> SupportedTimeZones =
    [
        "America/Costa_Rica",
        "America/Mexico_City",
        "America/Bogota",
        "America/Lima",
        "America/Guatemala",
        "America/New_York",
        "America/Chicago",
        "America/Los_Angeles",
        "Europe/Madrid",
        "UTC",
    ];

    public async Task<AvailabilitySettingsDto?> GetForCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return null;

        var rules = await availability.GetByTutorAsync(tutor.Id, ct);
        var exceptionDays = await exceptions.GetByTutorAsync(tutor.Id, ct);

        return new AvailabilitySettingsDto(
            tutor.TimeZoneId,
            rules.Count == 0 || rules.All(r => r.WeeklyEnabled),
            [.. rules.Select(r => new AvailabilityRuleDto(r.Weekday, r.StartLocal, r.EndLocal, r.IsRecurring, r.AnchorDate, r.WeeklyEnabled))],
            [.. exceptionDays.Select(e => new AvailabilityExceptionDto(e.Date, e.Reason))]);
    }

    /// <summary>
    /// Reemplazo completo de la configuración de agenda del tutor (zona horaria,
    /// toggle maestro, franjas y excepciones). Devuelve null si el usuario no es tutor.
    /// </summary>
    public async Task<AvailabilityResult> ReplaceForCurrentUserAsync(
        Guid userId, ReplaceAvailabilitySettingsRequest request, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return AvailabilityResult.Forbidden();

        // Zona horaria: debe ser IANA válida (idealmente del catálogo soportado)
        if (string.IsNullOrWhiteSpace(request.TimeZoneId) || !IsValidTimeZone(request.TimeZoneId))
            return AvailabilityResult.Invalid(AvailabilityErrors.InvalidTimeZone);

        var rules = new List<TutorAvailability>();
        var byDay = new Dictionary<int, List<(TimeOnly Start, TimeOnly End)>>();

        foreach (var dto in request.Rules)
        {
            if (dto.Weekday is < 0 or > 6)
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidRules);
            if (dto.StartLocal >= dto.EndLocal)
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidRules);
            // Granularidad 1 h alineada con la duración de los slots
            if (dto.StartLocal.Minute != 0 || dto.StartLocal.Second != 0 ||
                dto.EndLocal.Minute != 0 || dto.EndLocal.Second != 0)
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidRules);
            if (dto.StartLocal < new TimeOnly(0, 0) || dto.EndLocal > new TimeOnly(23, 59))
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidRules);

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

        // Excepciones: fechas válidas, sin duplicados, hoy o futuro (zona del tutor)
        var zone = SlotGenerationService.ResolveZone(request.TimeZoneId);
        var hoyLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
        var seenDates = new HashSet<DateOnly>();
        foreach (var dto in request.Exceptions)
        {
            if (dto.Date < hoyLocal)
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidExceptions);
            if (!seenDates.Add(dto.Date))
                return AvailabilityResult.Invalid(AvailabilityErrors.InvalidExceptions);
        }

        foreach (var (weekday, _) in byDay)
        {
            foreach (var dto in request.Rules.Where(r => r.Weekday == weekday))
            {
                rules.Add(new TutorAvailability
                {
                    Id = Guid.NewGuid(),
                    TutorId = tutor.Id,
                    Weekday = dto.Weekday,
                    StartLocal = dto.StartLocal,
                    EndLocal = dto.EndLocal,
                    WeeklyEnabled = request.WeeklyEnabled,
                    IsRecurring = dto.IsRecurring,
                    // No recurrente: ancla = próxima ocurrencia del weekday en la zona del tutor
                    AnchorDate = dto.IsRecurring ? null : NextOccurrence(dto.Weekday, zone),
                    Tutor = null!,
                });
            }
        }

        var exceptionEntities = request.Exceptions
            .Select(dto => new TutorAvailabilityException
            {
                Id = Guid.NewGuid(),
                TutorId = tutor.Id,
                Date = dto.Date,
                Reason = dto.Reason,
                Tutor = null!,
            })
            .ToList();

        // Zona horaria del tutor (afecta la generación y lo que ven los alumnos)
        tutor.TimeZoneId = request.TimeZoneId;
        await tutors.UpdateAsync(tutor, ct);

        await availability.ReplaceAllAsync(tutor.Id, rules, ct);
        await exceptions.ReplaceAllAsync(tutor.Id, exceptionEntities, ct);
        return AvailabilityResult.Ok();
    }

    private static bool IsValidTimeZone(string timeZoneId) =>
        SlotGenerationService.ResolveZone(timeZoneId) != null;

    /// <summary>Próxima ocurrencia (hoy incluido) del weekday en la zona dada.</summary>
    private static DateOnly NextOccurrence(int weekday, TimeZoneInfo zone)
    {
        var hoyLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
        var dias = ((DayOfWeek)weekday - hoyLocal.DayOfWeek + 7) % 7;
        return hoyLocal.AddDays(dias);
    }
}

/// <summary>Resultado tipado de la operación de disponibilidad.</summary>
public record AvailabilityResult(bool IsForbidden, bool IsInvalid, string? ErrorCode = null)
{
    public static AvailabilityResult Ok() => new(false, false);
    public static AvailabilityResult Forbidden() => new(true, false);
    public static AvailabilityResult Invalid(string? code = null) => new(false, true, code);
}