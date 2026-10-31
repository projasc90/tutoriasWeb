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
    /// <summary>Zona por defecto (horario de negocio CR) si el tutor no definió otra o es inválida.</summary>
    public const string DefaultTimeZoneId = "America/Costa_Rica";

    private static readonly TimeZoneInfo CostaRicaZone =
        TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);

    /// <summary>Resuelve la zona horaria IANA del tutor (fallback CR si es inválida).</summary>
    public static TimeZoneInfo ResolveZone(string? timeZoneId) =>
        string.IsNullOrEmpty(timeZoneId) ? CostaRicaZone : SafeZone(timeZoneId);

    private static TimeZoneInfo SafeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return CostaRicaZone;
        }
        catch (InvalidTimeZoneException)
        {
            return CostaRicaZone;
        }
    }

    /// <summary>
    /// Genera slots con el horario de negocio por defecto (sin módulo de disponibilidad).
    /// No persiste; solo devuelve la lista de slots candidatos.
    /// La persistencia la hace SlotRepository.EnsureAheadInRangeAsync (vía SlotAvailabilityService).
    /// </summary>
    public static IReadOnlyList<Slot> GenerateSlots(Guid tutorId, DateTime fromUtc, DateTime toUtc) =>
        GenerateSlots(tutorId, fromUtc, toUtc, AvailabilityRules: null);

    /// <summary>
    /// Genera slots respetando las reglas semanales del tutor, su zona horaria
    /// y sus días de excepción. Cada franja es una ventana local [inicio, fin)
    /// en un día de la semana; los slots se generan hora a hora dentro de la
    /// franja. Sin reglas definidas, se usa el horario por defecto.
    /// Con WeeklyEnabled=false las franjas semanales no generan cupos.
    /// Las fechas de excepción (feriados/vacaciones) no generan cupos.
    /// </summary>
    public static IReadOnlyList<Slot> GenerateSlots(
        Guid tutorId, DateTime fromUtc, DateTime toUtc,
        IReadOnlyList<TutorAvailability>? AvailabilityRules,
        string? timeZoneId = null,
        IReadOnlyList<TutorAvailabilityException>? excepciones = null)
    {
        if (fromUtc >= toUtc)
            return [];

        var zone = ResolveZone(timeZoneId);
        var fechasBloqueadas = excepciones is { Count: > 0 }
            ? excepciones.Select(e => e.Date).ToHashSet()
            : new HashSet<DateOnly>();

        var slots = new List<Slot>();
        var nowUtc = DateTime.UtcNow;

        var dayLocal = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, zone).Date;
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(toUtc, zone).Date;

        while (dayLocal < endLocal)
        {
            var weekday = (int)dayLocal.DayOfWeek;
            var dayDateOnly = DateOnly.FromDateTime(dayLocal);

            if (AvailabilityRules is { Count: > 0 })
            {
                // Toggle maestro: con el horario semanal deshabilitado no se
                // generan cupos (las excepciones son irrelevantes aquí).
                if (AvailabilityRules.All(r => !r.WeeklyEnabled))
                    return [];

                // Solo las franjas habilitadas del día; no recurrentes solo en
                // su fecha ancla; las fechas de excepción se bloquean.
                foreach (var franja in AvailabilityRules.Where(r =>
                    r.WeeklyEnabled &&
                    r.Weekday == weekday &&
                    (r.IsRecurring || r.AnchorDate == dayDateOnly) &&
                    !fechasBloqueadas.Contains(dayDateOnly)))
                {
                    if (franja.EndLocal <= franja.StartLocal)
                        continue; // franja inválida: ignorar

                    var slotStart = franja.StartLocal;
                    while (slotStart.AddHours(1) <= franja.EndLocal)
                    {
                        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                            dayDateOnly.ToDateTime(slotStart), zone);
                        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                            dayDateOnly.ToDateTime(slotStart.AddHours(1)), zone);

                        if (startUtc > nowUtc)
                            slots.Add(new Slot { Id = Guid.NewGuid(), TutorId = tutorId, StartAt = startUtc, EndAt = endUtc });

                        slotStart = slotStart.AddHours(1);
                    }
                }
            }
            else if (!fechasBloqueadas.Contains(dayDateOnly))
            {
                // Horario de negocio por defecto: 08:00–12:00 y 13:00–20:00
                for (int hour = 0; hour < 12; hour++)
                {
                    var slotStartCr = dayLocal.AddHours(8 + hour);
                    if (slotStartCr.Hour == 12)
                        continue; // hora de comida CR

                    var startUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartCr, zone);
                    var endUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartCr.AddHours(1), zone);

                    if (startUtc > nowUtc)
                        slots.Add(new Slot { Id = Guid.NewGuid(), TutorId = tutorId, StartAt = startUtc, EndAt = endUtc });
                }
            }

            dayLocal = dayLocal.AddDays(1);
        }

        return slots;
    }
}
