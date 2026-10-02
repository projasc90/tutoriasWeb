using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del mapeo de NextSlotAt en TutorService (reemplazo del demo).
/// TDD: escritos antes de la implementación.
/// </summary>
public class NextSlotMappingTests
{
    private static Tutor CreateTutor(Guid id) => new()
    {
        Id = id,
        Name = "Test Tutor",
        Credentials = "M.Sc.",
        University = "UCR",
        Rating = 4.9m,
        Reviews = 10,
        Subjects = ["Cálculo I"],
        PriceCrc = 14500,
        PriceUsd = 28,
        Bio = "Test",
        Featured = false,
        VerificationStatus = VerificationStatus.Verified,
        CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public void NextSlotAt_sin_slots_disponibles_devuelve_null()
    {
        // Arrange: tutor sin slots futuros
        var tutor = CreateTutor(Guid.NewGuid());
        var slots = new List<Slot>(); // vacío

        // Act
        var nextSlot = SlotQueryService.GetNextSlotOrNull(slots);

        // Assert
        Assert.Null(nextSlot);
    }

    [Fact]
    public void NextSlotAt_con_slots_disponibles_devuelve_el_mas_proximo()
    {
        var tutor = CreateTutor(Guid.NewGuid());
        var now = DateTime.UtcNow;
        var slots = new List<Slot>
        {
            new() { Id = Guid.NewGuid(), TutorId = tutor.Id, StartAt = now.AddDays(3), EndAt = now.AddDays(3).AddHours(1) },
            new() { Id = Guid.NewGuid(), TutorId = tutor.Id, StartAt = now.AddDays(1), EndAt = now.AddDays(1).AddHours(1) }, // el más próximo
            new() { Id = Guid.NewGuid(), TutorId = tutor.Id, StartAt = now.AddDays(5), EndAt = now.AddDays(5).AddHours(1) },
        };

        var nextSlot = SlotQueryService.GetNextSlotOrNull(slots);

        Assert.NotNull(nextSlot);
        Assert.Equal(now.AddDays(1), nextSlot!.Value);
    }

    [Fact]
    public void NextSlotAt_ordena_por_fecha_no_por_id()
    {
        var tutor = CreateTutor(Guid.NewGuid());
        var now = DateTime.UtcNow;
        var slots = new List<Slot>
        {
            new() { Id = Guid.NewGuid(), TutorId = tutor.Id, StartAt = now.AddDays(2), EndAt = now.AddDays(2).AddHours(1) },
            new() { Id = Guid.NewGuid(), TutorId = tutor.Id, StartAt = now.AddDays(1), EndAt = now.AddDays(1).AddHours(1) },
        };

        var nextSlot = SlotQueryService.GetNextSlotOrNull(slots);

        Assert.Equal(now.AddDays(1), nextSlot!.Value);
    }
}
