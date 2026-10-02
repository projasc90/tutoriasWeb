using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto IWalletRepository usando EF Core.
/// </summary>
public class WalletRepository(AuraLearnDbContext db) : IWalletRepository
{
    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken ct)
    {
        var balance = await db.WalletEntries
            .Where(w => w.UserId == userId)
            .SumAsync(w => w.AmountCrc, ct);
        return balance;
    }

    public async Task AddAsync(WalletEntry entry, CancellationToken ct)
    {
        await db.WalletEntries.AddAsync(entry, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> TryDebitAsync(
        Guid userId, int amountCrc, Guid reservationId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Serializa débitos concurrentes del mismo usuario: el lock dura hasta el
        // fin de la transacción; el SUM leído a continuación ya ve el estado final.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT id FROM users WHERE id = {userId} FOR UPDATE", ct);

        var balance = await db.WalletEntries
            .Where(w => w.UserId == userId)
            .SumAsync(w => w.AmountCrc, ct);

        if (balance < amountCrc)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        await db.WalletEntries.AddAsync(new WalletEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AmountCrc = -amountCrc,
            Reason = WalletEntryReason.WalletPayment,
            ReservationId = reservationId,
            CreatedAt = DateTime.UtcNow,
        }, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}
