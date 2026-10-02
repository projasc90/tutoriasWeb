using AuraLearn.Application.Dto;
using FluentValidation;

namespace AuraLearn.Application.Services;

/// <summary>
/// Valida un comprobante SINPE contra la reserva antes de agregarlo a la cola del Admin.
/// Reglas: monto exacto, receptor coincide, ventana temporal válida, formato de nº confirmación.
/// La firma digital real se sustituye por confirmación manual del Admin (ADR-004).
/// </summary>
public class SinpeComprobanteValidator : AbstractValidator<SinpeComprobanteDto>
{
    public SinpeComprobanteValidator()
    {
        RuleFor(x => x.AmountCrc)
            .Equal(x => x.PriceCrc)
            .WithMessage("El monto del comprobante no coincide con el precio de la reserva.");

        RuleFor(x => x.ConfirmationNumber)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("El número de confirmación debe tener al menos 3 caracteres.");

        RuleFor(x => x.ReceptorPhone)
            .Equal(x => x.ReservationPhone)
            .WithMessage("El número de teléfono receptor no coincide con el de la reserva.");

        RuleFor(x => x.ComprobanteSubmittedAt)
            .Must((dto, v) =>
                v == null ||
                (v >= dto.WindowStart && v <= dto.WindowEnd))
            .WithMessage("El comprobante fue generado fuera de la ventana de vigencia de la reserva.");
    }
}
