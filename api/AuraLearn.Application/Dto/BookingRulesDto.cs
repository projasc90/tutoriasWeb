namespace AuraLearn.Application.Dto;

/// <summary>
/// Reglas de reserva de la plataforma (contrato con el frontend). Cada regla
/// es opcional: null = el Admin no la definió (no se muestra en el portal y
/// la lógica usa el default de <see cref="BookingRulesDefaults"/>).
/// </summary>
public record BookingRulesDto(
    int? CancellationNoticeHours,
    int? PaymentWindowMinutes,
    int? SlotDurationMinutes,
    int? BookingHorizonDays,
    DateTime UpdatedAt);

/// <summary>Solicitud del Admin para definir las reglas (null = dejar sin definir).</summary>
public record UpdateBookingRulesRequest(
    int? CancellationNoticeHours,
    int? PaymentWindowMinutes,
    int? SlotDurationMinutes,
    int? BookingHorizonDays);

/// <summary>
/// Defaults de plataforma cuando una regla quedó sin definir (los valores
/// históricos del código: 12 h de anticipación, 60 min de ventana de pago,
/// slots de 60 min y horizonte de 14 días).
/// </summary>
public static class BookingRulesDefaults
{
    public const int CancellationNoticeHours = 12;
    public const int PaymentWindowMinutes = 60;
    public const int SlotDurationMinutes = 60;
    public const int BookingHorizonDays = 14;
}

/// <summary>Códigos de error de las reglas de reserva.</summary>
public static class BookingRulesErrors
{
    public const string Invalid = "BOOKING_RULES_INVALID";
    public const string InvalidCancellationNotice = "BOOKING_RULES_INVALID_CANCELLATION_NOTICE";
    public const string InvalidPaymentWindow = "BOOKING_RULES_INVALID_PAYMENT_WINDOW";
    public const string InvalidSlotDuration = "BOOKING_RULES_INVALID_SLOT_DURATION";
    public const string InvalidHorizon = "BOOKING_RULES_INVALID_HORIZON";
}
