namespace AuraLearn.Application.Dto;

/// <summary>Regla semanal de disponibilidad (contrato con el frontend).</summary>
public record AvailabilityRuleDto(
    int Weekday,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    bool IsRecurring = true,
    DateOnly? AnchorDate = null,
    bool WeeklyEnabled = true);

/// <summary>Día de excepción (feriado/vacaciones): no se generan cupos esa fecha.</summary>
public record AvailabilityExceptionDto(DateOnly Date, string? Reason = null);

/// <summary>
/// Configuración completa de la agenda del tutor: zona horaria, interruptor
/// maestro del horario semanal, franjas y días de excepción.
/// </summary>
public record AvailabilitySettingsDto(
    string TimeZoneId,
    bool WeeklyEnabled,
    IReadOnlyList<AvailabilityRuleDto> Rules,
    IReadOnlyList<AvailabilityExceptionDto> Exceptions);

/// <summary>Solicitud de reemplazo completo de la configuración de agenda.</summary>
public record ReplaceAvailabilitySettingsRequest(
    string TimeZoneId,
    bool WeeklyEnabled,
    IReadOnlyList<AvailabilityRuleDto> Rules,
    IReadOnlyList<AvailabilityExceptionDto> Exceptions);

/// <summary>Códigos de error de la disponibilidad.</summary>
public static class AvailabilityErrors
{
    public const string InvalidRules = "AVAILABILITY_INVALID_RULES";
    public const string Overlap = "AVAILABILITY_OVERLAP";
    public const string InvalidTimeZone = "AVAILABILITY_INVALID_TIMEZONE";
    public const string InvalidExceptions = "AVAILABILITY_INVALID_EXCEPTIONS";
}
