using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>Validador de la edición de reglas de reserva por el Admin.</summary>
public class UpdateBookingRulesValidator : AbstractValidator<UpdateBookingRulesRequest>
{
    public UpdateBookingRulesValidator()
    {
        // Anticipación: 0 h (siempre acredita) a 7 días
        RuleFor(r => r.CancellationNoticeHours)
            .InclusiveBetween(0, 168).When(r => r.CancellationNoticeHours.HasValue);
        // Ventana de pago SINPE: 15 min a 4 h
        RuleFor(r => r.PaymentWindowMinutes)
            .InclusiveBetween(15, 240).When(r => r.PaymentWindowMinutes.HasValue);
        // Duración de slots: 15 min a 3 h
        RuleFor(r => r.SlotDurationMinutes)
            .InclusiveBetween(15, 180).When(r => r.SlotDurationMinutes.HasValue);
        // Horizonte de reserva: 1 día a 8 semanas
        RuleFor(r => r.BookingHorizonDays)
            .InclusiveBetween(1, 56).When(r => r.BookingHorizonDays.HasValue);
    }
}

/// <summary>
/// Caso de uso: lectura y edición de las reglas de reserva de la plataforma
/// (fila única). El Admin las define; los servicios del dominio usan el valor
/// definido o el default (<see cref="BookingRulesDefaults"/>) si quedó sin definir.
/// </summary>
public class BookingRulesService(
    IBookingRulesRepository repository,
    UpdateBookingRulesValidator validator)
{
    /// <summary>Reglas vigentes (todo null si el Admin no definió ninguna aún).</summary>
    public async Task<BookingRulesDto> GetAsync(CancellationToken ct)
    {
        var rules = await repository.GetAsync(ct);
        return ToDto(rules);
    }

    /// <summary>
    /// Define las reglas (upsert de la fila única) con auditoría del Admin.
    /// Reglas vacías (null) quedan sin definir → default en los consumidores.
    /// </summary>
    public async Task<BookingRulesResult> UpdateAsync(
        UpdateBookingRulesRequest request, Guid adminUserId, CancellationToken ct)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            return BookingRulesResult.Invalid(ErrorCodeFor(validation.Errors[0].PropertyName));

        var entity = await repository.GetAsync(ct) ?? new BookingRules();
        entity.CancellationNoticeHours = request.CancellationNoticeHours;
        entity.PaymentWindowMinutes = request.PaymentWindowMinutes;
        entity.SlotDurationMinutes = request.SlotDurationMinutes;
        entity.BookingHorizonDays = request.BookingHorizonDays;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = adminUserId;

        await repository.UpsertAsync(entity, ct);
        return BookingRulesResult.Ok(ToDto(entity));
    }

    private static BookingRulesDto ToDto(BookingRules? rules) => rules is null
        ? new BookingRulesDto(null, null, null, null, DateTime.MinValue)
        : new BookingRulesDto(
            rules.CancellationNoticeHours,
            rules.PaymentWindowMinutes,
            rules.SlotDurationMinutes,
            rules.BookingHorizonDays,
            rules.UpdatedAt);

    private static string ErrorCodeFor(string propertyName) => propertyName switch
    {
        nameof(UpdateBookingRulesRequest.CancellationNoticeHours) => BookingRulesErrors.InvalidCancellationNotice,
        nameof(UpdateBookingRulesRequest.PaymentWindowMinutes) => BookingRulesErrors.InvalidPaymentWindow,
        nameof(UpdateBookingRulesRequest.SlotDurationMinutes) => BookingRulesErrors.InvalidSlotDuration,
        nameof(UpdateBookingRulesRequest.BookingHorizonDays) => BookingRulesErrors.InvalidHorizon,
        _ => BookingRulesErrors.Invalid,
    };
}

/// <summary>Resultado tipado de la edición de reglas de reserva.</summary>
public record BookingRulesResult(bool IsInvalid, string? ErrorCode = null, BookingRulesDto? Value = null)
{
    public static BookingRulesResult Ok(BookingRulesDto value) => new(false, null, value);
    public static BookingRulesResult Invalid(string? code = null) => new(true, code ?? BookingRulesErrors.Invalid);
}
