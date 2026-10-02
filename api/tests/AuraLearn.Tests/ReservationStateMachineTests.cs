using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests de la máquina de estados de reservas (TDD: escritos antes de la implementación).
/// Cada transición válida/inválida es un invariante del skill slots-reservations.
/// </summary>
public class ReservationStateMachineTests
{
    // ── CanSubmitComprobante ──

    [Fact]
    public void CanSubmitComprobante_solo_en_PendingPayment()
    {
        Assert.True(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.PendingPayment));

        Assert.False(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.Confirmed));
        Assert.False(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.Rejected));
        Assert.False(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.Expired));
        Assert.False(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.Cancelled));
        Assert.False(ReservationStateMachine.CanSubmitComprobante(ReservationStatus.Completed));
    }

    // ── CanCancel ──

    [Fact]
    public void CanCancel_en_PendingPayment_y_Confirmed()
    {
        Assert.True(ReservationStateMachine.CanCancel(ReservationStatus.PendingPayment));
        Assert.True(ReservationStateMachine.CanCancel(ReservationStatus.Confirmed));

        Assert.False(ReservationStateMachine.CanCancel(ReservationStatus.Rejected));
        Assert.False(ReservationStateMachine.CanCancel(ReservationStatus.Expired));
        Assert.False(ReservationStateMachine.CanCancel(ReservationStatus.Cancelled));
        Assert.False(ReservationStateMachine.CanCancel(ReservationStatus.Completed));
    }

    // ── CanDecide (admin) ──

    [Fact]
    public void CanDecide_solo_en_PendingPayment()
    {
        Assert.True(ReservationStateMachine.CanDecide(ReservationStatus.PendingPayment));

        Assert.False(ReservationStateMachine.CanDecide(ReservationStatus.Confirmed));
        Assert.False(ReservationStateMachine.CanDecide(ReservationStatus.Rejected));
        Assert.False(ReservationStateMachine.CanDecide(ReservationStatus.Expired));
        Assert.False(ReservationStateMachine.CanDecide(ReservationStatus.Cancelled));
        Assert.False(ReservationStateMachine.CanDecide(ReservationStatus.Completed));
    }

    // ── CanExpire ──

    [Fact]
    public void CanExpire_solo_en_PendingPayment()
    {
        Assert.True(ReservationStateMachine.CanExpire(ReservationStatus.PendingPayment));

        Assert.False(ReservationStateMachine.CanExpire(ReservationStatus.Confirmed));
        Assert.False(ReservationStateMachine.CanExpire(ReservationStatus.Cancelled));
        Assert.False(ReservationStateMachine.CanExpire(ReservationStatus.Completed));
    }

    // ── CanComplete ──

    [Fact]
    public void CanComplete_solo_desde_Confirmed()
    {
        Assert.True(ReservationStateMachine.CanComplete(ReservationStatus.Confirmed));

        Assert.False(ReservationStateMachine.CanComplete(ReservationStatus.PendingPayment));
        Assert.False(ReservationStateMachine.CanComplete(ReservationStatus.Cancelled));
    }

    // ── Estados terminales y que bloquean slot ──

    [Fact]
    public void TerminalStates_son_Rejected_Expired_Cancelled_Completed()
    {
        Assert.Contains(ReservationStatus.Rejected, ReservationStateMachine.TerminalStates);
        Assert.Contains(ReservationStatus.Expired, ReservationStateMachine.TerminalStates);
        Assert.Contains(ReservationStatus.Cancelled, ReservationStateMachine.TerminalStates);
        Assert.Contains(ReservationStatus.Completed, ReservationStateMachine.TerminalStates);

        Assert.DoesNotContain(ReservationStatus.PendingPayment, ReservationStateMachine.TerminalStates);
        Assert.DoesNotContain(ReservationStatus.Confirmed, ReservationStateMachine.TerminalStates);
    }

    [Fact]
    public void SlotBlockingStates_son_PendingPayment_y_Confirmed()
    {
        Assert.Contains(ReservationStatus.PendingPayment, ReservationStateMachine.SlotBlockingStates);
        Assert.Contains(ReservationStatus.Confirmed, ReservationStateMachine.SlotBlockingStates);

        Assert.DoesNotContain(ReservationStatus.Rejected, ReservationStateMachine.SlotBlockingStates);
        Assert.DoesNotContain(ReservationStatus.Expired, ReservationStateMachine.SlotBlockingStates);
        Assert.DoesNotContain(ReservationStatus.Cancelled, ReservationStateMachine.SlotBlockingStates);
        Assert.DoesNotContain(ReservationStatus.Completed, ReservationStateMachine.SlotBlockingStates);
    }
}
