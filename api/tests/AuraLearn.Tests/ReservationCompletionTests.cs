using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del cierre de sesión (PATCH complete, rol Tutor dueño).
/// TDD: escritos antes de la implementación.
/// </summary>
public class ReservationCompletionTests
{
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly ISlotRepository _slots = Substitute.For<ISlotRepository>();
    private readonly IWalletRepository _wallet = Substitute.For<IWalletRepository>();
    private readonly ReservationService _service;

    private static Tutor CreateTutor(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "M.Sc. Tutor Test",
        Credentials = "M.Sc.",
        University = "UCR",
        Bio = "Mock",
        Subjects = ["Cálculo I"],
        PriceCrc = 14500,
        PriceUsd = 28,
        VerificationStatus = VerificationStatus.Verified,
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
    };

    private static (Slot slot, Tutor tutor) CreateSlotConTutor(Guid? tutorUserId, DateTime? endAt = null)
    {
        var tutor = CreateTutor(tutorUserId);
        var end = endAt ?? DateTime.UtcNow.AddHours(-1); // sesión terminada hace 1h
        var slot = new Slot
        {
            Id = Guid.NewGuid(),
            TutorId = tutor.Id,
            StartAt = end.AddHours(-1),
            EndAt = end,
            Tutor = tutor,
        };
        return (slot, tutor);
    }

    public ReservationCompletionTests()
    {
        _service = new ReservationService(
            _reservations, _slots, _wallet, new SinpeConfig("88888888"), new SinpeComprobanteValidator());
    }

    private Reservation CrearReserva(Guid slotId, ReservationStatus status = ReservationStatus.Confirmed)
    {
        var r = new Reservation
        {
            Id = Guid.NewGuid(),
            SlotId = slotId,
            StudentId = Guid.NewGuid(),
            Status = status,
            PriceCrc = 14500,
            IdempotencyKey = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            CreatedAt = DateTime.UtcNow,
        };
        return r;
    }

    [Fact]
    public async Task Complete_reserva_Confirmed_y_terminada_marca_Completed_y_liquida()
    {
        var tutorUserId = Guid.NewGuid();
        var (slot, tutor) = CreateSlotConTutor(tutorUserId);
        var reservation = CrearReserva(slot.Id);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.CompleteAsync(reservation.Id, tutorUserId, default);

        // Liquidación al ledger del tutor: +precio con reason TutorPayout
        Assert.True(result.IsFound);
        Assert.Equal(ReservationStatus.Completed, result.Value!.Status);
        await _reservations.Received(1).UpdateAsync(
            Arg.Is<Reservation>(r => r.Status == ReservationStatus.Completed), Arg.Any<CancellationToken>());
        await _wallet.Received(1).AddAsync(
            Arg.Is<WalletEntry>(w =>
                w.UserId == tutorUserId &&
                w.AmountCrc == 14500 &&
                w.Reason == WalletEntryReason.TutorPayout &&
                w.ReservationId == reservation.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_por_tutor_ajeno_devuelve_Forbidden()
    {
        var (slot, _) = CreateSlotConTutor(tutorUserId: Guid.NewGuid()); // dueño real
        var reservation = CrearReserva(slot.Id);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.CompleteAsync(reservation.Id, Guid.NewGuid(), default); // ajeno

        Assert.True(result.IsForbidden);
        await _reservations.DidNotReceive().UpdateAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_reserva_no_Confirmed_devuelve_Conflict()
    {
        var (slot, _) = CreateSlotConTutor(tutorUserId: Guid.NewGuid());
        var reservation = CrearReserva(slot.Id, ReservationStatus.PendingPayment);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.CompleteAsync(reservation.Id, slot.Tutor.UserId!.Value, default);

        Assert.True(result.IsConflict);
    }

    [Fact]
    public async Task Complete_sesion_no_terminada_devuelve_Invalid()
    {
        var (slot, _) = CreateSlotConTutor(tutorUserId: Guid.NewGuid(), endAt: DateTime.UtcNow.AddHours(2)); // aún no termina
        var reservation = CrearReserva(slot.Id);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.CompleteAsync(reservation.Id, slot.Tutor.UserId!.Value, default);

        Assert.True(result.IsInvalid);
        await _wallet.DidNotReceive().AddAsync(Arg.Any<WalletEntry>(), Arg.Any<CancellationToken>());
    }
}
