using AuraLearn.Application.Dto;
using AuraLearn.Application.Services;
using FluentValidation.TestHelper;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del validador de comprobantes SINPE.
/// TDD: escritos antes de la implementación del validador.
/// </summary>
public class SinpeComprobanteValidatorTests
{
    private readonly SinpeComprobanteValidator _validator = new();

    // ── Happy path ──

    [Fact]
    public void Validate_monto_exacto_y_receptor_correcto_pasa()
    {
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: "ABC123456",
            AmountCrc: 14500,
            ReceptorPhone: "88888888",
            ReservationPhone: "88888888",
            WindowStart: DateTime.UtcNow.AddMinutes(-30),
            WindowEnd: DateTime.UtcNow.AddMinutes(30),
            PriceCrc: 14500
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Monto ──

    [Theory]
    [InlineData(14499)]
    [InlineData(1)]
    [InlineData(0)]
    public void Validate_monto_diferente_falla(int monto)
    {
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: "ABC123456",
            AmountCrc: monto,
            ReceptorPhone: "88888888",
            ReservationPhone: "88888888",
            WindowStart: DateTime.UtcNow.AddMinutes(-30),
            WindowEnd: DateTime.UtcNow.AddMinutes(30),
            PriceCrc: 14500
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AmountCrc);
    }

    // ── Formato de número de confirmación ──

    [Theory]
    [InlineData("")]       // vacío
    [InlineData("AB")]     // demasiado corto
    public void Validate_confirmation_number_formato_invalido_falla(string conf)
    {
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: conf,
            AmountCrc: 14500,
            ReceptorPhone: "88888888",
            ReservationPhone: "88888888",
            WindowStart: DateTime.UtcNow.AddMinutes(-30),
            WindowEnd: DateTime.UtcNow.AddMinutes(30),
            PriceCrc: 14500
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmationNumber);
    }

    // ── Ventana temporal ──

    [Fact]
    public void Validate_comprobante_fuera_de_ventana_falla()
    {
        // Comprobante generado 3 h ANTES de la ventana (fuera de vigencia)
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: "ABC123456",
            AmountCrc: 14500,
            ReceptorPhone: "88888888",
            ReservationPhone: "88888888",
            WindowStart: DateTime.UtcNow.AddMinutes(-30),  // ventana empieza hace 30 min
            WindowEnd: DateTime.UtcNow.AddMinutes(60),    // ventana termina en 60 min
            PriceCrc: 14500,
            ComprobanteSubmittedAt: DateTime.UtcNow.AddMinutes(-180) // 3 h antes de ahora: FUERA
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteSubmittedAt);
    }

    // ── Receptor ──

    [Fact]
    public void Validate_receptor_distinto_falla()
    {
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: "ABC123456",
            AmountCrc: 14500,
            ReceptorPhone: "88888888",
            ReservationPhone: "99999999", // receptor errado
            WindowStart: DateTime.UtcNow.AddMinutes(-30),
            WindowEnd: DateTime.UtcNow.AddMinutes(30),
            PriceCrc: 14500
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ReceptorPhone);
    }

    // ── Teléfono opcional (no falla si es null) ──

    [Fact]
    public void Validate_phone_null_pasa()
    {
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: "ABC123456",
            AmountCrc: 14500,
            ReceptorPhone: "88888888",
            ReservationPhone: "88888888",
            WindowStart: DateTime.UtcNow.AddMinutes(-30),
            WindowEnd: DateTime.UtcNow.AddMinutes(30),
            PriceCrc: 14500,
            Phone: null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
