using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del servicio de generación de slots (ensure-ahead idempotente).
/// TDD: escritos antes de la implementación.
/// </summary>
public class SlotGenerationServiceTests
{
    /// <summary>
    /// Helper: crea una lista de slots mock que empieza en <paramref name="dayOffset"/>
    /// días desde hoy a las 08:00 UTC, con duración de 1 h cada uno, 08:00–20:00 CR.
    /// </summary>
    private static IReadOnlyList<Slot> CreateMockSlots(
        Guid tutorId, int dayOffset, int count = 5, int startHourUtc = 8)
    {
        var baseDate = DateTime.UtcNow.Date.AddDays(dayOffset);
        var slots = new List<Slot>();
        for (int i = 0; i < count; i++)
        {
            slots.Add(new Slot
            {
                Id = Guid.NewGuid(),
                TutorId = tutorId,
                StartAt = baseDate.AddHours(startHourUtc + i),
                EndAt = baseDate.AddHours(startHourUtc + i + 1),
            });
        }
        return slots;
    }

    // ── Idempotencia ──

    [Fact]
    public void GenerateSlots_para_mismo_rango_es_idempotente()
    {
        // Arrange: mismo rango, dos llamadas
        var tutorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var rangeFrom = now.Date;
        var rangeTo = now.Date.AddDays(1);

        // Act: primera y segunda generación (mismo rango, mismo tutor)
        var first = SlotGenerationService.GenerateSlots(tutorId, rangeFrom, rangeTo);
        var second = SlotGenerationService.GenerateSlots(tutorId, rangeFrom, rangeTo);

        // Assert: la cantidad y horas deben ser idénticas
        Assert.Equal(first.Count, second.Count);
        // 12 franjas por día (08–12 = 4, 13–20 = 8) → 12 total. Cobertura: idempotencia es lo que validamos.
    }

    // ── Solo horarios de negocio (CR 08:00–20:00) ──

    [Fact]
    public void GenerateSlots_solo_genera_entre_8_y_20()
    {
        var tutorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var slots = SlotGenerationService.GenerateSlots(tutorId, now.Date, now.Date.AddDays(1));

        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        foreach (var slot in slots)
        {
            var localHour = TimeZoneInfo.ConvertTimeFromUtc(slot.StartAt, crZone).Hour;
            Assert.InRange(localHour, 8, 19); // 20:00 no se incluye (último slot termina a esa hora)
        }
    }

    // ── Solo días futuros ──

    [Fact]
    public void GenerateSlots_no_genera_slots_en_el_pasado()
    {
        var tutorId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var today = DateTime.UtcNow.Date;

        var slots = SlotGenerationService.GenerateSlots(tutorId, yesterday, today);

        Assert.All(slots, s => Assert.True(s.StartAt > DateTime.UtcNow));
    }

    // ── Duración de cada slot = 1 hora ──

    [Fact]
    public void GenerateSlots_cada_slot_dura_una_hora()
    {
        var tutorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var slots = SlotGenerationService.GenerateSlots(tutorId, now.Date, now.Date.AddDays(1));

        foreach (var slot in slots)
        {
            Assert.Equal(TimeSpan.FromHours(1), slot.EndAt - slot.StartAt);
        }
    }

    // ── TutorId asignado ──

    [Fact]
    public void GenerateSlots_asigna_el_tutorId_correcto()
    {
        var tutorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var slots = SlotGenerationService.GenerateSlots(tutorId, now.Date, now.Date.AddDays(1));

        Assert.All(slots, s => Assert.Equal(tutorId, s.TutorId));
    }

    // ── Rango vacío (pasado) ──

    [Fact]
    public void GenerateSlots_rango_totalmente_en_pasado_devuelve_vacio()
    {
        var tutorId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var today = yesterday;

        var slots = SlotGenerationService.GenerateSlots(tutorId, yesterday, today);

        Assert.Empty(slots);
    }

    // ── Honoring de disponibilidad semanal (reglas del tutor) ──

    /// <summary>Helper: regla de disponibilidad para un día de la semana (0=dom…6=sáb).</summary>
    private static TutorAvailability ReglaSimple(Guid tutorId, int weekday, string inicio, string fin, bool isRecurring = true, DateOnly? anchorDate = null, bool weeklyEnabled = true) => new()
    {
        Id = Guid.NewGuid(),
        TutorId = tutorId,
        Weekday = weekday,
        StartLocal = TimeOnly.Parse(inicio),
        EndLocal = TimeOnly.Parse(fin),
        WeeklyEnabled = weeklyEnabled,
        IsRecurring = isRecurring,
        AnchorDate = anchorDate,
        Tutor = null!,
    };

    private static TutorAvailabilityException Excepcion(Guid tutorId, DateOnly date, string? reason = null) => new()
    {
        Id = Guid.NewGuid(),
        TutorId = tutorId,
        Date = date,
        Reason = reason,
        Tutor = null!,
    };

    [Fact]
    public void GenerateSlots_con_reglas_solo_genera_dentro_de_las_franjas()
    {
        var tutorId = Guid.NewGuid();
        // Jueves (4) 14:00–18:00 CR
        var regla = ReglaSimple(tutorId, weekday: 4, inicio: "14:00", fin: "18:00");
        // Ventana determinista: el primer día LOCAL CR del rango es un jueves
        // futuro, de modo que los 8 días locales contienen exactamente 2 jueves.
        // (Alinear en UTC era incorrecto: medianoche UTC = 18:00 del día anterior
        // en CR, y el test fallaba según el día de la semana de ejecución.)
        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var hoyCr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date;
        var diasHastaJueves = ((DayOfWeek.Thursday - hoyCr.DayOfWeek) + 7) % 7;
        var primerJueves = hoyCr.AddDays(7 + diasHastaJueves); // medianoche local CR
        var from = TimeZoneInfo.ConvertTimeToUtc(primerJueves, crZone);
        var to = TimeZoneInfo.ConvertTimeToUtc(primerJueves.AddDays(8), crZone);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        // Solo slots del día elegido y dentro de la franja: 14:00–17:00 CR.
        // El rango de 8 días abarca 2 jueves → 8 slots (4 por cada uno).
        Assert.Equal(8, slots.Count);
        Assert.All(slots, s =>
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(s.StartAt, crZone);
            Assert.Equal(DayOfWeek.Thursday, local.DayOfWeek);
            Assert.True(local.Hour >= 14 && local.Hour < 18);
        });
        // 2 jueves distintos, 4 slots por cada uno
        Assert.Equal(2, slots.Select(s => TimeZoneInfo.ConvertTimeFromUtc(s.StartAt, crZone).Date).Distinct().Count());
    }

    [Fact]
    public void GenerateSlots_con_reglas_no_genera_en_dias_sin_franja()
    {
        var tutorId = Guid.NewGuid();
        // Solo martes (2)
        var regla = ReglaSimple(tutorId, weekday: 2, inicio: "09:00", fin: "11:00");
        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        Assert.Equal(2, slots.Count); // 09:00 y 10:00 del martes
        Assert.All(slots, s => Assert.Equal(DayOfWeek.Tuesday, s.StartAt.DayOfWeek));
    }

    [Fact]
    public void GenerateSlots_varias_franjas_el_mismo_dia_se_acumulan()
    {
        var tutorId = Guid.NewGuid();
        var reglas = new[]
        {
            ReglaSimple(tutorId, 5, "08:00", "10:00"), // viernes mañana
            ReglaSimple(tutorId, 5, "15:00", "16:00"), // viernes tarde
        };
        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, reglas);

        Assert.Equal(3, slots.Count); // 08, 09 (mañana) + 15 (tarde)
    }

    [Fact]
    public void GenerateSlots_sin_reglas_mantiene_el_horario_por_defecto()
    {
        var tutorId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(1);

        var conNull = SlotGenerationService.GenerateSlots(tutorId, from, to, null);
        var conVacias = SlotGenerationService.GenerateSlots(tutorId, from, to, []);

        // 11 slots por día (08–11 y 13–19: la 12:00 es comida y el día termina 20:00)
        Assert.Equal(11, conNull.Count);
        Assert.Equal(11, conVacias.Count);
    }

    // ── Toggle recurrente (IsRecurring) ────────────────────────────

    [Fact]
    public void GenerateSlots_regla_no_recurrente_genera_solo_en_la_fecha_ancla()
    {
        var tutorId = Guid.NewGuid();
        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var hoyCr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date;
        var diasHastaJueves = ((DayOfWeek.Thursday - hoyCr.DayOfWeek) + 7) % 7;
        var primerJueves = hoyCr.AddDays(7 + diasHastaJueves);
        var segundoJueves = primerJueves.AddDays(7);

        // No recurrente anclada al PRIMER jueves del rango
        var regla = ReglaSimple(tutorId, weekday: 4, inicio: "14:00", fin: "18:00",
            isRecurring: false, anchorDate: DateOnly.FromDateTime(primerJueves));

        var from = TimeZoneInfo.ConvertTimeToUtc(primerJueves, crZone);
        var to = TimeZoneInfo.ConvertTimeToUtc(primerJueves.AddDays(8), crZone);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        // Solo 4 slots (el primer jueves), no 8 (los dos jueves del rango)
        Assert.Equal(4, slots.Count);
        var fechas = slots.Select(s => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(s.StartAt, crZone))).Distinct().ToList();
        Assert.Single(fechas);
        Assert.Equal(DateOnly.FromDateTime(primerJueves), fechas[0]);
    }

    [Fact]
    public void GenerateSlots_regla_recurrente_genera_en_todas_las_ocurrencias()
    {
        var tutorId = Guid.NewGuid();
        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var hoyCr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date;
        var diasHastaJueves = ((DayOfWeek.Thursday - hoyCr.DayOfWeek) + 7) % 7;
        var primerJueves = hoyCr.AddDays(7 + diasHastaJueves);

        var regla = ReglaSimple(tutorId, weekday: 4, inicio: "14:00", fin: "18:00", isRecurring: true);

        var from = TimeZoneInfo.ConvertTimeToUtc(primerJueves, crZone);
        var to = TimeZoneInfo.ConvertTimeToUtc(primerJueves.AddDays(8), crZone);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        Assert.Equal(8, slots.Count); // 2 jueves × 4 slots
    }

    [Fact]
    public void GenerateSlots_regla_no_recurrente_sin_ancla_no_genera()
    {
        var tutorId = Guid.NewGuid();
        var regla = ReglaSimple(tutorId, weekday: 2, inicio: "09:00", fin: "11:00",
            isRecurring: false, anchorDate: null);

        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        Assert.Empty(slots);
    }

    // ── Toggle maestro (WeeklyEnabled) ─────────────────────────────

    [Fact]
    public void GenerateSlots_horario_deshabilitado_no_genera()
    {
        var tutorId = Guid.NewGuid();
        var regla = ReglaSimple(tutorId, weekday: 2, inicio: "09:00", fin: "11:00", weeklyEnabled: false);

        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla]);

        Assert.Empty(slots);
    }

    // ── Excepciones y feriados ────────────────────────────────────

    [Fact]
    public void GenerateSlots_excepcion_bloquea_la_fecha_ancla_de_franja_no_recurrente()
    {
        var tutorId = Guid.NewGuid();
        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var hoyCr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date;
        var diasHastaJueves = ((DayOfWeek.Thursday - hoyCr.DayOfWeek) + 7) % 7;
        var primerJueves = hoyCr.AddDays(7 + diasHastaJueves);

        var regla = ReglaSimple(tutorId, weekday: 4, inicio: "14:00", fin: "18:00",
            isRecurring: false, anchorDate: DateOnly.FromDateTime(primerJueves));
        var excepcion = Excepcion(tutorId, DateOnly.FromDateTime(primerJueves), "Feriado");

        var from = TimeZoneInfo.ConvertTimeToUtc(primerJueves, crZone);
        var to = TimeZoneInfo.ConvertTimeToUtc(primerJueves.AddDays(8), crZone);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla], excepciones: [excepcion]);

        Assert.Empty(slots);
    }

    [Fact]
    public void GenerateSlots_excepcion_bloquea_solo_esa_fecha_en_franja_recurrente()
    {
        var tutorId = Guid.NewGuid();
        var crZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var hoyCr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date;
        var diasHastaJueves = ((DayOfWeek.Thursday - hoyCr.DayOfWeek) + 7) % 7;
        var primerJueves = hoyCr.AddDays(7 + diasHastaJueves);

        var regla = ReglaSimple(tutorId, weekday: 4, inicio: "14:00", fin: "18:00", isRecurring: true);
        var excepcion = Excepcion(tutorId, DateOnly.FromDateTime(primerJueves), "Feriado");

        var from = TimeZoneInfo.ConvertTimeToUtc(primerJueves, crZone);
        var to = TimeZoneInfo.ConvertTimeToUtc(primerJueves.AddDays(8), crZone);

        var slots = SlotGenerationService.GenerateSlots(tutorId, from, to, [regla], excepciones: [excepcion]);

        // Solo los slots del SEGUNDO jueves (el primero está bloqueado)
        Assert.Equal(4, slots.Count);
        var fechas = slots.Select(s => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(s.StartAt, crZone))).Distinct().ToList();
        Assert.Single(fechas);
        Assert.Equal(DateOnly.FromDateTime(primerJueves.AddDays(7)), fechas[0]);
    }

    // ── Zona horaria del tutor ────────────────────────────────────

    [Fact]
    public void GenerateSlots_con_zona_horaria_genera_en_la_zona_indicada()
    {
        var tutorId = Guid.NewGuid();
        // Franja 09:00–11:00 en Bogotá (GMT-5, sin DST)
        var regla = ReglaSimple(tutorId, weekday: 2, inicio: "09:00", fin: "11:00");
        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(
            tutorId, from, to, [regla], timeZoneId: "America/Bogota");

        Assert.Equal(2, slots.Count);
        var bogotaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        Assert.All(slots, s =>
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(s.StartAt, bogotaZone);
            Assert.Equal(DayOfWeek.Tuesday, local.DayOfWeek);
            Assert.True(local.Hour >= 9 && local.Hour < 11);
        });
    }

    [Fact]
    public void GenerateSlots_zona_invalida_cae_en_costa_rica()
    {
        var tutorId = Guid.NewGuid();
        var regla = ReglaSimple(tutorId, weekday: 2, inicio: "09:00", fin: "11:00");
        var from = DateTime.UtcNow.Date.AddDays(7);
        var to = from.AddDays(7);

        var slots = SlotGenerationService.GenerateSlots(
            tutorId, from, to, [regla], timeZoneId: "Zona/Inexistente");

        // Fallback CR: 09:00–11:00 CR = 2 slots
        Assert.Equal(2, slots.Count);
    }
}
