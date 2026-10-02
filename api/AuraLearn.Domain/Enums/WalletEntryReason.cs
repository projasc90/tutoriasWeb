namespace AuraLearn.Domain.Enums;

/// <summary>Motivo de un movimiento del monedero del estudiante.</summary>
public enum WalletEntryReason
{
    /// <summary>Crédito por cancelación con ≥12 h de anticipación.</summary>
    CancellationCredit = 1,

    /// <summary>Débito por pagar una reserva con saldo del monedero (monto negativo; ADR-006).</summary>
    WalletPayment = 2,

    /// <summary>Liquidación al tutor al concluir la sesión (monto positivo; retención hasta cerrar).</summary>
    TutorPayout = 3,
}
