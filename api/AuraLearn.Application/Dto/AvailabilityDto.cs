namespace AuraLearn.Application.Dto;

/// <summary>Regla semanal de disponibilidad (contrato con el frontend).</summary>
public record AvailabilityRuleDto(int Weekday, TimeOnly StartLocal, TimeOnly EndLocal);

/// <summary>Solicitud de reemplazo completo de las reglas del tutor.</summary>
public record ReplaceAvailabilityRequest(IReadOnlyList<AvailabilityRuleDto> Rules);

/// <summary>Error específico de validación de disponibilidad (contrato 422).</summary>
public static class AvailabilityErrors
{
    public const string InvalidRules = "INVALID_AVAILABILITY_RULES";
    public const string Overlap = "AVAILABILITY_OVERLAP";
}
