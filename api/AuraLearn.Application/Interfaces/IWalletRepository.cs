using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de acceso al monedero del estudiante (implementado por Infrastructure).</summary>
public interface IWalletRepository
{
    /// <summary>Saldo total del usuario (suma de AmountCrc).</summary>
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct);

    Task AddAsync(WalletEntry entry, CancellationToken ct);

    /// <summary>
    /// Débito atómico y race-safe (ADR-006): bloquea la fila del usuario (FOR UPDATE),
    /// calcula el saldo dentro de la transacción y registra el movimiento negativo solo
    /// si el saldo alcanza. Devuelve false si el saldo es insuficiente (sin efectos).
    /// </summary>
    Task<bool> TryDebitAsync(Guid userId, int amountCrc, Guid reservationId, CancellationToken ct);
}
