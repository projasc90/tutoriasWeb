using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Implementación del puerto ISlotRepository usando EF Core.
/// </summary>
public class SlotRepository : ISlotRepository
{
    private static readonly TimeZoneInfo CostaRicaZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    private readonly AuraLearnDbContext _db;

    public SlotRepository(AuraLearnDbContext db) => _db = db;

    public async Task<IReadOnlyList<Slot>> GetByTutorInRangeAsync(
        Guid tutorId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _db.Slots
            .Where(s => s.TutorId == tutorId && s.StartAt >= from && s.StartAt < to)
            .OrderBy(s => s.StartAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Slot>> GetByTutorsInRangeAsync(
        IReadOnlyList<Guid> tutorIds, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _db.Slots
            .Where(s => tutorIds.Contains(s.TutorId) && s.StartAt >= from && s.StartAt < to)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Slot?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Slots
            .Include(s => s.Tutor)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Slot>> EnsureAheadInRangeAsync(
        IReadOnlyList<Guid> tutorIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        if (tutorIds.Count == 0)
            return [];

        // 0. Reglas semanales de disponibilidad (si el tutor definió alguna)
        var rules = await _db.Set<TutorAvailability>()
            .Where(a => tutorIds.Contains(a.TutorId))
            .AsNoTracking()
            .ToListAsync(ct);

        // 1. Slots ya existentes en el rango (evita excepciones por índice único)
        var existing = await GetByTutorsInRangeAsync(tutorIds, fromUtc, toUtc, ct);
        var existingKeys = existing
            .Select(s => (s.TutorId, s.StartAt))
            .ToHashSet();

        // 2. Generar candidatos (honoring de reglas; sin reglas → horario por defecto)
        var missing = tutorIds
            .SelectMany(tutorId => SlotGenerationService.GenerateSlots(
                tutorId, fromUtc, toUtc,
                rules.Count > 0
                    ? rules.Where(r => r.TutorId == tutorId).ToList()
                    : null))
            .Where(candidate => !existingKeys.Contains((candidate.TutorId, candidate.StartAt)))
            .ToList();

        // 2b. Purga de honorabilidad: si el tutor tiene reglas, los slots existentes
        // FUERA de sus franjas (y sin reserva activa) dejan de existir. Con reserva
        // activa se conservan: la sesión ya está comprometida con el estudiante.
        if (rules.Count > 0)
        {
            var rulesByTutor = rules
                .GroupBy(r => r.TutorId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<TutorAvailability>)g.ToList());

            var stale = new List<Slot>();
            foreach (var slot in existing)
            {
                if (!rulesByTutor.TryGetValue(slot.TutorId, out var franjas))
                    continue;

                var localCr = TimeZoneInfo.ConvertTimeFromUtc(slot.StartAt, CostaRicaZone);
                var weekday = (int)localCr.DayOfWeek;
                var localTime = TimeOnly.FromDateTime(localCr);
                var dentro = franjas.Any(f =>
                    f.Weekday == weekday &&
                    localTime >= f.StartLocal &&
                    localTime.AddHours(1) <= f.EndLocal);

                if (!dentro)
                    stale.Add(slot);
            }

            if (stale.Count > 0)
            {
                var staleIds = stale.Select(s => s.Id).ToList();

                // Solo se eliminan los slots sin NINGUNA reserva (ni activa ni terminal):
                // la FK reservations→slots es Restrict y el historial de reservas se
                // conserva (auditoría). Un slot con historial simplemente deja de
                // ser reservable (la reserva terminal no lo bloquea más).
                var slotsConReserva = await _db.Reservations
                    .Where(r => staleIds.Contains(r.SlotId))
                    .Select(r => r.SlotId)
                    .ToListAsync(ct);
                var purgables = staleIds.Except(slotsConReserva).ToList();

                if (purgables.Count > 0)
                    await _db.Slots
                        .Where(s => purgables.Contains(s.Id))
                        .ExecuteDeleteAsync(ct);
            }
        }

        // 3. Persistir los faltantes en un solo SaveChanges
        if (missing.Count > 0)
        {
            _db.Slots.AddRange(missing);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Carrera benigna: otro request insertó el mismo (tutor_id, start_at)
                // entre la pre-consulta y el insert (índice único ux_slots_tutor_start).
                // Reintento individual ignorando los duplicados ya presentes en BD.
                _db.ChangeTracker.Clear();
                foreach (var slot in missing)
                {
                    _db.Slots.Add(slot);
                    try
                    {
                        await _db.SaveChangesAsync(ct);
                    }
                    catch (DbUpdateException)
                    {
                        _db.ChangeTracker.Clear();
                    }
                }
            }
        }

        // 4. Estado final del rango (incluye lo insertado), ordenado por inicio
        var final = await GetByTutorsInRangeAsync(tutorIds, fromUtc, toUtc, ct);
        return final.OrderBy(s => s.StartAt).ToList();
    }

    public async Task<DateTime?> GetNextAvailableStartAsync(
        Guid tutorId, DateTime nowUtc, CancellationToken ct)
    {
        return await _db.Slots
            .Where(s => s.TutorId == tutorId && s.StartAt > nowUtc)
            .OrderBy(s => s.StartAt)
            .Select(s => (DateTime?)s.StartAt)
            .FirstOrDefaultAsync(ct);
    }
}
