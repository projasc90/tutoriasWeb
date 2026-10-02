using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del ensure-ahead (ADR-005): antes de consultar disponibilidad, los slots
/// faltantes se generan y persisten de forma idempotente. TDD: escritos antes del servicio.
/// </summary>
public class SlotAvailabilityServiceTests
{
    private readonly ISlotRepository _slots = Substitute.For<ISlotRepository>();

    [Fact]
    public async Task EnsureAhead_sin_tutores_no_consulta_repositorio()
    {
        var service = new SlotAvailabilityService(_slots);

        var result = await service.EnsureAheadAsync(
            [], DateTime.UtcNow, DateTime.UtcNow.AddDays(1), default);

        Assert.Empty(result);
        await _slots.DidNotReceive().EnsureAheadInRangeAsync(
            Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureAhead_rango_terminado_en_el_pasado_no_consulta_repositorio()
    {
        var service = new SlotAvailabilityService(_slots);
        var past = DateTime.UtcNow.AddHours(-2);

        var result = await service.EnsureAheadAsync(
            [Guid.NewGuid()], past, past.AddHours(1), default);

        // from queda recortado al presente → rango vacío → no hay nada que hacer
        Assert.Empty(result);
        await _slots.DidNotReceive().EnsureAheadInRangeAsync(
            Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureAhead_recorta_el_inicio_al_presente()
    {
        var service = new SlotAvailabilityService(_slots);
        var from = DateTime.UtcNow.AddHours(-5);
        var to = DateTime.UtcNow.AddDays(1);
        _slots.EnsureAheadInRangeAsync(
                Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await service.EnsureAheadAsync([Guid.NewGuid()], from, to, default);

        await _slots.Received(1).EnsureAheadInRangeAsync(
            Arg.Any<IReadOnlyList<Guid>>(),
            Arg.Is<DateTime>(d => d > DateTime.UtcNow.AddSeconds(-5) && d <= DateTime.UtcNow.AddSeconds(5)),
            Arg.Is<DateTime>(d => d == to),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureAhead_devuelve_los_slots_del_repositorio()
    {
        var service = new SlotAvailabilityService(_slots);
        var to = DateTime.UtcNow.AddDays(1);
        var esperados = new List<AuraLearn.Domain.Entities.Slot>
        {
            new() { Id = Guid.NewGuid(), TutorId = Guid.NewGuid(), StartAt = DateTime.UtcNow.AddHours(2), EndAt = DateTime.UtcNow.AddHours(3) },
        };
        _slots.EnsureAheadInRangeAsync(
                Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(esperados);

        var result = await service.EnsureAheadAsync([Guid.NewGuid()], DateTime.UtcNow, to, default);

        Assert.Single(result);
        Assert.Equal(esperados[0].Id, result[0].Id);
    }
}
