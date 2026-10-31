using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Interfaces;

/// <summary>Puerto de las reglas de reserva de la plataforma (fila única).</summary>
public interface IBookingRulesRepository
{
    /// <summary>Fila única de reglas (null si aún no existe en BD).</summary>
    Task<BookingRules?> GetAsync(CancellationToken ct);

    /// <summary>Inserta o actualiza la fila única (upsert idempotente).</summary>
    Task UpsertAsync(BookingRules rules, CancellationToken ct);
}
