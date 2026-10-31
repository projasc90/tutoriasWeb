using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del servicio de reglas de reserva (TDD: escritos antes de la
/// implementación). Cubren: lectura (con y sin fila), edición con auditoría
/// y validación de rangos por campo.
/// </summary>
public class BookingRulesServiceTests
{
    private readonly IBookingRulesRepository _repository = Substitute.For<IBookingRulesRepository>();
    private readonly BookingRulesService _service;

    public BookingRulesServiceTests()
    {
        _service = new BookingRulesService(_repository, new UpdateBookingRulesValidator());
    }

    // --- Lectura ---

    [Fact]
    public async Task Get_sin_fila_devuelve_todo_sin_definir()
    {
        _repository.GetAsync(Arg.Any<CancellationToken>()).Returns((BookingRules?)null);

        var dto = await _service.GetAsync(default);

        Assert.Null(dto.CancellationNoticeHours);
        Assert.Null(dto.PaymentWindowMinutes);
        Assert.Null(dto.SlotDurationMinutes);
        Assert.Null(dto.BookingHorizonDays);
    }

    [Fact]
    public async Task Get_con_fila_devuelve_los_valores_persistidos()
    {
        _repository.GetAsync(Arg.Any<CancellationToken>()).Returns(new BookingRules
        {
            CancellationNoticeHours = 24,
            PaymentWindowMinutes = 30,
            SlotDurationMinutes = null,
            BookingHorizonDays = 21,
            UpdatedAt = new DateTime(2026, 10, 12, 8, 0, 0, DateTimeKind.Utc),
        });

        var dto = await _service.GetAsync(default);

        Assert.Equal(24, dto.CancellationNoticeHours);
        Assert.Equal(30, dto.PaymentWindowMinutes);
        Assert.Null(dto.SlotDurationMinutes);
        Assert.Equal(21, dto.BookingHorizonDays);
        Assert.Equal(new DateTime(2026, 10, 12, 8, 0, 0, DateTimeKind.Utc), dto.UpdatedAt);
    }

    // --- Edición ---

    [Fact]
    public async Task Update_con_valores_validos_persiste_y_audita()
    {
        var adminId = Guid.NewGuid();

        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(24, 45, 90, 21), adminId, default);

        Assert.False(result.IsInvalid);
        Assert.Equal(24, result.Value!.CancellationNoticeHours);
        await _repository.Received(1).UpsertAsync(
            Arg.Is<BookingRules>(r =>
                r.Id == BookingRules.SingletonId &&
                r.CancellationNoticeHours == 24 &&
                r.PaymentWindowMinutes == 45 &&
                r.SlotDurationMinutes == 90 &&
                r.BookingHorizonDays == 21 &&
                r.UpdatedByUserId == adminId &&
                r.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_sin_definir_reglas_las_deja_en_null()
    {
        // Fila previa con valores: el request vacío (todo null) los limpia
        _repository.GetAsync(Arg.Any<CancellationToken>()).Returns(new BookingRules
        {
            CancellationNoticeHours = 6,
            PaymentWindowMinutes = 30,
        });

        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(null, null, null, null), Guid.NewGuid(), default);

        Assert.False(result.IsInvalid);
        Assert.Null(result.Value!.CancellationNoticeHours);
        Assert.Null(result.Value.PaymentWindowMinutes);
        await _repository.Received(1).UpsertAsync(
            Arg.Is<BookingRules>(r =>
                r.CancellationNoticeHours == null &&
                r.PaymentWindowMinutes == null &&
                r.SlotDurationMinutes == null &&
                r.BookingHorizonDays == null),
            Arg.Any<CancellationToken>());
    }

    // --- Validación de rangos por campo ---

    [Fact]
    public async Task Update_anticipacion_fuera_de_rango_devuelve_invalid()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(200, null, null, null), Guid.NewGuid(), default);

        Assert.True(result.IsInvalid);
        Assert.Equal(BookingRulesErrors.InvalidCancellationNotice, result.ErrorCode);
        await _repository.DidNotReceive().UpsertAsync(Arg.Any<BookingRules>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ventana_pago_fuera_de_rango_devuelve_invalid()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(null, 10, null, null), Guid.NewGuid(), default);

        Assert.True(result.IsInvalid);
        Assert.Equal(BookingRulesErrors.InvalidPaymentWindow, result.ErrorCode);
        await _repository.DidNotReceive().UpsertAsync(Arg.Any<BookingRules>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_duracion_fuera_de_rango_devuelve_invalid()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(null, null, 5, null), Guid.NewGuid(), default);

        Assert.True(result.IsInvalid);
        Assert.Equal(BookingRulesErrors.InvalidSlotDuration, result.ErrorCode);
        await _repository.DidNotReceive().UpsertAsync(Arg.Any<BookingRules>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_horizonte_fuera_de_rango_devuelve_invalid()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(null, null, null, 0), Guid.NewGuid(), default);

        Assert.True(result.IsInvalid);
        Assert.Equal(BookingRulesErrors.InvalidHorizon, result.ErrorCode);
        await _repository.DidNotReceive().UpsertAsync(Arg.Any<BookingRules>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_valores_en_el_limite_inferior_son_validos()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(0, 15, 15, 1), Guid.NewGuid(), default);

        Assert.False(result.IsInvalid);
    }

    [Fact]
    public async Task Update_valores_en_el_limite_superior_son_validos()
    {
        var result = await _service.UpdateAsync(
            new UpdateBookingRulesRequest(168, 240, 180, 56), Guid.NewGuid(), default);

        Assert.False(result.IsInvalid);
    }
}
