using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;
using NSubstitute;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del servicio de reservas (TDD: escritos antes de la implementacion).
/// Cubren: crear, replay idempotente, 409 slot ocupado, comprobante, cancelacion 12h.
/// </summary>
public class ReservationServiceTests
{
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly ISlotRepository _slots = Substitute.For<ISlotRepository>();
    private readonly IWalletRepository _wallet = Substitute.For<IWalletRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ReservationService _service;

    // Tutor mock para el slot de prueba
    private static readonly Tutor _tutor = new()
    {
        Id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
        Name = "Dr. Carlos Solano",
        Credentials = "PhD",
        University = "UCR",
        Bio = "Mock tutor",
        PriceCrc = 14500,
        VerificationStatus = VerificationStatus.Verified,
    };

    private static Slot CreateSlot(DateTime? startAt = null) => new()
    {
        Id = Guid.NewGuid(),
        TutorId = _tutor.Id,
        StartAt = startAt ?? DateTime.UtcNow.Date.AddDays(1).AddHours(10),
        EndAt = (startAt ?? DateTime.UtcNow.Date.AddDays(1).AddHours(10)).AddHours(1),
        Tutor = _tutor,
    };

    private static Reservation CreateReservation(
        Guid slotId,
        Guid studentId,
        ReservationStatus status = ReservationStatus.PendingPayment,
        DateTime? expiresAt = null) => new()
    {
        Id = Guid.NewGuid(),
        SlotId = slotId,
        StudentId = studentId,
        Status = status,
        PriceCrc = 14500,
        IdempotencyKey = Guid.NewGuid().ToString(),
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(60),
        CreatedAt = DateTime.UtcNow,
    };

    public ReservationServiceTests()
    {
        _service = new ReservationService(_reservations, _slots, _wallet, new SinpeConfig("88888888"), new SinpeComprobanteValidator());
    }

    // --- Creacion: replay idempotente ---

    [Fact]
    public async Task CreateReservation_replay_con_misma_idempotency_key_devuelve_200_existente()
    {
        var slot = CreateSlot();
        var studentId = Guid.NewGuid();
        var existing = CreateReservation(slot.Id, studentId);
        var key = existing.IdempotencyKey;

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetByIdempotencyKeyAsync(key, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.CreateAsync(studentId, new CreateReservationRequest(slot.Id, key), default);

        Assert.True(result.IsFound);
        Assert.Equal(existing.Id, result.Value!.Id);
        await _reservations.DidNotReceive().AddAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }

    // --- Creacion: slot no existe ---

    [Fact]
    public async Task CreateReservation_slot_no_existe_devuelve_NotFound()
    {
        _slots.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Slot?)null);

        var result = await _service.CreateAsync(Guid.NewGuid(), new CreateReservationRequest(Guid.NewGuid(), "key"), default);

        Assert.True(result.IsNotFound);
    }

    // --- Creacion: slot ya reservado activamente (indice unico parcial) ---

    [Fact]
    public async Task CreateReservation_slot_ya_activo_devuelve_Conflict()
    {
        var slot = CreateSlot();
        var existing = CreateReservation(slot.Id, Guid.NewGuid());

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetActiveBySlotAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.CreateAsync(Guid.NewGuid(), new CreateReservationRequest(slot.Id, "new-key"), default);

        Assert.True(result.IsConflict);
    }

    // --- Cancelacion 12h o mas: acredita monedero ---

    [Fact]
    public async Task Cancel_con_12h_o_mas_credita_wallet()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot(DateTime.UtcNow.AddDays(1).AddHours(10)); // manana 10:00
        var reservation = CreateReservation(slot.Id, studentId, ReservationStatus.Confirmed,
            expiresAt: DateTime.UtcNow.AddMinutes(60));

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _wallet.GetBalanceAsync(studentId, Arg.Any<CancellationToken>()).Returns(0);

        var result = await _service.CancelAsync(reservation.Id, studentId, default);

        // Regla de negocio: 12h o mas -> credito al monedero
        Assert.True(result.IsDecided || result.IsFound);
        await _wallet.Received(1).AddAsync(
            Arg.Is<WalletEntry>(w => w.UserId == studentId && w.AmountCrc == 14500),
            Arg.Any<CancellationToken>());
    }

    // --- Cancelacion menos de 12h: sin reembolso ---

    [Fact]
    public async Task Cancel_menos_de_12h_no_acredita_wallet()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot(DateTime.UtcNow.AddHours(3)); // en 3 horas
        var reservation = CreateReservation(slot.Id, studentId, ReservationStatus.Confirmed);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _wallet.GetBalanceAsync(studentId, Arg.Any<CancellationToken>()).Returns(0);

        var result = await _service.CancelAsync(reservation.Id, studentId, default);

        // Menos de 12h -> sin reembolso -> no se llama wallet.AddAsync
        Assert.True(result.IsDecided || result.IsFound);
        await _wallet.DidNotReceive().AddAsync(Arg.Any<WalletEntry>(), Arg.Any<CancellationToken>());
    }

    // --- Cancelacion: estado terminal -> 409 ---

    [Fact]
    public async Task Cancel_estado_terminal_devuelve_Conflict()
    {
        var studentId = Guid.NewGuid();
        var reservation = CreateReservation(Guid.NewGuid(), studentId, ReservationStatus.Cancelled);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);

        var result = await _service.CancelAsync(reservation.Id, studentId, default);

        Assert.True(result.IsConflict);
    }

    // --- SubmitComprobante: monto incorrecto -> 422 ---

    [Fact]
    public async Task SubmitComprobante_monto_incorrecto_devuelve_Invalid()
    {
        var slot = CreateSlot();
        var reservation = CreateReservation(slot.Id, Guid.NewGuid());

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        // Monto diferente al de la reserva (14500)
        var result = await _service.SubmitComprobanteAsync(
            reservation.Id, new SubmitComprobanteRequest("CONF123", 9999, null), default);

        Assert.True(result.IsInvalid);
    }

    // --- SubmitComprobante: replay del mismo comprobante -> 409 ---

    [Fact]
    public async Task SubmitComprobante_numero_reutilizado_devuelve_Conflict()
    {
        var slot = CreateSlot();
        var reservation = CreateReservation(slot.Id, Guid.NewGuid());
        // Otra reserva (id distinto) YA usó este nº de confirmación
        var otraReserva = CreateReservation(slot.Id, Guid.NewGuid());
        otraReserva.ConfirmationNumber = "CONF123";

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _reservations.GetByConfirmationNumberAsync("CONF123", Arg.Any<CancellationToken>())
            .Returns(otraReserva);

        var result = await _service.SubmitComprobanteAsync(
            reservation.Id, new SubmitComprobanteRequest("CONF123", 14500, null), default);

        Assert.True(result.IsConflict);
    }

    // --- SubmitComprobante: reserva no PendingPayment -> 409 ---

    [Fact]
    public async Task SubmitComprobante_reserva_no_PendingPayment_devuelve_Conflict()
    {
        var slot = CreateSlot();
        var reservation = CreateReservation(slot.Id, Guid.NewGuid(), ReservationStatus.Confirmed);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);

        var result = await _service.SubmitComprobanteAsync(
            reservation.Id, new SubmitComprobanteRequest("CONF123", 14500, null), default);

        Assert.True(result.IsConflict);
    }

    // --- Cancelacion en PendingPayment: nunca credita monedero (no hubo pago) ---

    [Fact]
    public async Task Cancel_en_PendingPayment_no_credita_wallet()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot(DateTime.UtcNow.AddDays(2)); // ≥12h
        var reservation = CreateReservation(slot.Id, studentId, ReservationStatus.PendingPayment);

        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.CancelAsync(reservation.Id, studentId, default);

        // La reserva nunca pagó: cancelarla no puede generar crédito
        Assert.True(result.IsDecided);
        await _wallet.DidNotReceive().AddAsync(Arg.Any<WalletEntry>(), Arg.Any<CancellationToken>());
    }

    // --- PaymentInfo usa el receptor de configuracion (no hardcode) ---

    [Fact]
    public async Task CreateReservation_devuelve_el_receptor_configurado()
    {
        var service = new ReservationService(
            _reservations, _slots, _wallet, new SinpeConfig("89997777"), new SinpeComprobanteValidator());
        var slot = CreateSlot();
        var studentId = Guid.NewGuid();

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetByIdempotencyKeyAsync("key-conf", Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);
        _reservations.GetActiveBySlotAsync(slot.Id, Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var result = await service.CreateAsync(
            studentId, new CreateReservationRequest(slot.Id, "key-conf"), default);

        Assert.True(result.IsCreated);
        Assert.Equal("89997777", result.Value!.PaymentInfo!.ReceptorPhone);
    }

    // --- Expiracion perezosa (ADR-005): al leer/crear, sin worker ---

    [Fact]
    public async Task GetByStudent_reserva_vencida_sin_comprobante_pasa_a_Expired()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot();
        var vencida = CreateReservation(
            slot.Id, studentId, ReservationStatus.PendingPayment,
            expiresAt: DateTime.UtcNow.AddMinutes(-1)); // ya venció
        _reservations.GetByStudentAsync(studentId, Arg.Any<CancellationToken>()).Returns(new List<Reservation> { vencida });
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.GetByStudentAsync(studentId, default);

        Assert.All(result, r => Assert.Equal(ReservationStatus.Expired, r.Status));
        await _reservations.Received(1).UpdateAsync(
            Arg.Is<Reservation>(r => r.Status == ReservationStatus.Expired), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_reserva_vencida_con_comprobante_NO_expira()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot();
        var pagada = CreateReservation(
            slot.Id, studentId, ReservationStatus.PendingPayment,
            expiresAt: DateTime.UtcNow.AddMinutes(-1));
        pagada.ConfirmationNumber = "SMOKE-CONF-0001"; // comprobante en cola del admin
        _reservations.GetByIdAsync(pagada.Id, Arg.Any<CancellationToken>()).Returns(pagada);
        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);

        var result = await _service.GetByIdAsync(pagada.Id, studentId, false, default);

        // El dinero ya se transfirió: la decisión la tiene el Admin, no la ventana
        Assert.True(result.IsFound);
        Assert.Equal(ReservationStatus.PendingPayment, result.Value!.Status);
        await _reservations.DidNotReceive().UpdateAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReservation_slot_con_reserva_vencida_sin_comprobante_permite_nueva()
    {
        var slot = CreateSlot();
        var studentId = Guid.NewGuid();
        var vencida = CreateReservation(
            slot.Id, Guid.NewGuid(), ReservationStatus.PendingPayment,
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetActiveBySlotAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(vencida);
        _reservations.GetByIdempotencyKeyAsync("key-nueva", Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var result = await _service.CreateAsync(studentId, new CreateReservationRequest(slot.Id, "key-nueva"), default);

        // La vencida pasa a Expired (libera el índice parcial) y la nueva se crea
        Assert.True(result.IsCreated);
        await _reservations.Received(1).UpdateAsync(
            Arg.Is<Reservation>(r => r.Status == ReservationStatus.Expired), Arg.Any<CancellationToken>());
        await _reservations.Received(1).AddAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }

    // --- Pago con saldo del monedero (solo pago total; ADR-006) ---

    [Fact]
    public async Task CreateReservation_con_saldo_suficiente_y_useWallet_devuelve_Confirmed_y_debita()
    {
        var slot = CreateSlot();
        var studentId = Guid.NewGuid();

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetActiveBySlotAsync(slot.Id, Arg.Any<CancellationToken>()).Returns((Reservation?)null);
        _reservations.GetByIdempotencyKeyAsync("key-wallet", Arg.Any<CancellationToken>()).Returns((Reservation?)null);
        _wallet.TryDebitAsync(studentId, 14500, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.CreateAsync(
            studentId, new CreateReservationRequest(slot.Id, "key-wallet", UseWalletBalance: true), default);

        // Pago con saldo: la reserva se persiste, luego el débito atómico confirma
        Assert.True(result.IsCreated);
        Assert.Equal(ReservationStatus.Confirmed, result.Value!.Status);
        Assert.Null(result.Value!.PaymentInfo);
        await _wallet.Received(1).TryDebitAsync(
            studentId, 14500, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        // La reserva se persiste una única vez (antes del débito); la confirmación
        // es una actualización posterior. El estado histórico del objeto compartido
        // es observable solo por UpdateAsync (el objeto quedó en Confirmed).
        await _reservations.Received(1).AddAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
        await _reservations.Received(1).UpdateAsync(
            Arg.Is<Reservation>(r => r.Status == ReservationStatus.Confirmed), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReservation_con_saldo_insuficiente_hace_fallback_a_SINPE()
    {
        var slot = CreateSlot();
        var studentId = Guid.NewGuid();

        _slots.GetByIdAsync(slot.Id, Arg.Any<CancellationToken>()).Returns(slot);
        _reservations.GetActiveBySlotAsync(slot.Id, Arg.Any<CancellationToken>()).Returns((Reservation?)null);
        _reservations.GetByIdempotencyKeyAsync("key-wallet2", Arg.Any<CancellationToken>()).Returns((Reservation?)null);
        _wallet.TryDebitAsync(studentId, 14500, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _service.CreateAsync(
            studentId, new CreateReservationRequest(slot.Id, "key-wallet2", UseWalletBalance: true), default);

        // Fallback: flujo SINPE normal con PaymentInfo (sin confirmación)
        Assert.True(result.IsCreated);
        Assert.Equal(ReservationStatus.PendingPayment, result.Value!.Status);
        Assert.NotNull(result.Value!.PaymentInfo);
        await _reservations.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.Status == ReservationStatus.PendingPayment), Arg.Any<CancellationToken>());
        await _reservations.DidNotReceive().UpdateAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
    }

    // --- GetByStudent: devuelve reservas del estudiante ---

    [Fact]
    public async Task GetByStudent_devuelve_reservas_del_estudiante()
    {
        var studentId = Guid.NewGuid();
        var slot = CreateSlot();
        var reservaNueva = CreateReservation(slot.Id, studentId, ReservationStatus.PendingPayment);
        // Navs pobladas como lo hace EF en producción (Include Slot.Tutor / Student)
        reservaNueva.Slot = slot;
        reservaNueva.Student = new User
        {
            Id = studentId,
            Email = "smoke@test.dev",
            FullName = "Smoke Estudiante",
            Role = Role.Estudiante,
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
        };

        _reservations.GetByStudentAsync(studentId, Arg.Any<CancellationToken>()).Returns(new List<Reservation> { reservaNueva });

        var result = await _service.GetByStudentAsync(studentId, default);

        Assert.Single(result);
        Assert.All(result, r => Assert.Equal(_tutor.Id, r.TutorId)); // TutorId real, no el StudentId
        Assert.All(result, r => Assert.Equal("Smoke Estudiante", r.StudentName));
    }
}
