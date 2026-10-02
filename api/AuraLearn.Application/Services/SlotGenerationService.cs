using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Services;

/// <summary>
/// Genera franjas horarias (slots) para un tutor dentro de un rango UTC.
///
/// Reglas:
/// - Horario de negocio Costa Rica: 08:00–20:00 hora local (14:00–02:00 UTC,
///   según TimeZoneInfo "America/Costa_Rica" con DST).
/// - Duración de cada slot: 1 hora.
/// - Hora de comida CR: 12:00–13:00 (se excluye).
/// - Rango futuro: no se generan slots en el pasado.
/// - Idempotente: la misma llamada al mismo rango devuelve la misma lista
///   (la desduplicación en BD la hace el índice único).
///
/// La zona CR se aplica aquí solo para calcular las horas locales;
/// en BD se almacena en UTC. Ver ADR-005.
/// </summary>
public static class SlotGenerationService
{
    /// <summary>Hora de inicio del horario de negocio en Costa Rica (UTC, con DST).</summary>
    private static readonly TimeZoneInfo CostaRicaZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    /// <summary>
    /// Genera slots con el horario de negocio por defecto (sin módulo de disponibilidad).
    /// No persiste; solo devuelve la lista de slots candidatos.
    /// La persistencia la hace SlotRepository.EnsureAheadInRangeAsync (vía SlotAvailabilityService).
    /// </summary>
    public static IReadOnlyList<Slot> GenerateSlots(Guid tutorId, DateTime fromUtc, DateTime toUtc) =>
        GenerateSlots(tutorId, fromUtc, toUtc, AvailabilityRules: null);

    /// <summary>
    /// Genera slots respetando las reglas semanales del tutor. Cada franja es una
    /// ventana local CR [inicio, fin) en un día de la semana; los slots se generan
    /// hora a hora dentro de la franja. Sin reglas definidas, se usa el horario
    /// por defecto.
    /// </summary>
    public static IReadOnlyList<Slot> GenerateSlots(
        Guid tutorId, DateTime fromUtc, DateTime toUtc,
        IReadOnlyList<TutorAvailability>? AvailabilityRules)
    {
        if (fromUtc >= toUtc)
            return [];

        var slots = new List<Slot>();
        var nowUtc = DateTime.UtcNow;

        var dayLocal = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, CostaRicaZone).Date;
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(toUtc, CostaRicaZone).Date;

        while (dayLocal < endLocal)
        {
            var weekday = (int)dayLocal.DayOfWeek;

            if (AvailabilityRules is { Count: > 0 })
            {
                // Solo las franjas definidas por el tutor para este día de la semana
                foreach (var franja in AvailabilityRules.Where(r => r.Weekday == weekday))
                {
                    if (franja.EndLocal <= franja.StartLocal)
                        continue; // franja inválida: ignorar

                    var slotStart = franja.StartLocal;
                    var dayDate = DateOnly.FromDateTime(dayLocal);
                    while (slotStart.AddHours(1) <= franja.EndLocal)
                    {
                        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                            dayDate.ToDateTime(slotStart), CostaRicaZone);
                        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                            dayDate.ToDateTime(slotStart.AddHours(1)), CostaRicaZone);

                        if (startUtc > nowUtc)
                            slots.Add(new Slot { Id = Guid.NewGuid(), TutorId = tutorId, StartAt = startUtc, EndAt = endUtc });

                        slotStart = slotStart.AddHours(1);
                    }
                }
            }
            else
            {
                // Horario de negocio por defecto: 08:00–12:00 y 13:00–20:00
                for (int hour = 0; hour < 12; hour++)
                {
                    var slotStartCr = dayLocal.AddHours(8 + hour);
                    if (slotStartCr.Hour == 12)
                        continue; // hora de comida CR

                    var startUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartCr, CostaRicaZone);
                    var endUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartCr.AddHours(1), CostaRicaZone);

                    if (startUtc > nowUtc)
                        slots.Add(new Slot { Id = Guid.NewGuid(), TutorId = tutorId, StartAt = startUtc, EndAt = endUtc });
                }
            }

            dayLocal = dayLocal.AddDays(1);
        }

        return slots;
    }
}
