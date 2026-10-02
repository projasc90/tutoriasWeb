using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Services;

/// <summary>
/// Servicio de reservas: crear, acreditar comprobante, cancelar, consultar.
/// Reglas de negocio centralizadas aquí (incluida la máquina de estados).
/// TDD: cada invariante = test primero (ver ReservationServiceTests).
/// </summary>
public class ReservationService(
    IReservationRepository reservations,
    ISlotRepository slots,
    IWalletRepository wallet,
    SinpeConfig sinpe,
    SinpeComprobanteValidator comprobanteValidator)
{
    /// <summary>Ventana de pago en minutos (60 min, decisión alineada con el usuario).</summary>
    private static readonly TimeSpan PaymentWindow = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Crea una reserva idempotente. Reintentos del cliente con la misma idempotencyKey
    /// devuelven la reserva existente (200, no 201).
    /// El guardián real contra doble-booking es el índice único parcial en BD.
    /// </summary>
    public async Task<ReservationResult<ReservationDto>> CreateAsync(
        Guid studentId, CreateReservationRequest request, CancellationToken ct)
    {
        // 1. Idempotencia por clave del request
        var existing = await reservations.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
            return ReservationResult<ReservationDto>.Found(ToDto(existing, "", DateTime.MinValue, DateTime.MinValue));

        // 2. El slot debe existir (incluye el Tutor para el precio)
        var slot = await slots.GetByIdAsync(request.SlotId, ct);
        if (slot is null)
            return ReservationResult<ReservationDto>.NotFound();

        // 3. ¿Ya tiene una reserva activa? (índice único parcial — el guardián real de BD)
        // Expiración perezosa: una PendingPayment sin comprobante ya vencida pasa a
        // Expired (persistido, libera el índice parcial) y el slot vuelve a ser reservable.
        var active = await reservations.GetActiveBySlotAsync(request.SlotId, ct);
        if (active is not null && await TryExpireIfDueAsync(active, ct))
            active = null;
        if (active is not null)
            return ReservationResult<ReservationDto>.Conflict();

        // 4. ¿El slot es en el futuro?
        if (slot.StartAt <= DateTime.UtcNow)
            return ReservationResult<ReservationDto>.Invalid();

        // 5. ¿El estudiante no es el tutor?
        if (slot.TutorId == studentId)
            return ReservationResult<ReservationDto>.Invalid();

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SlotId = request.SlotId,
            StudentId = studentId,
            Status = ReservationStatus.PendingPayment,
            PriceCrc = slot.Tutor.PriceCrc,
            IdempotencyKey = request.IdempotencyKey,
            ExpiresAt = DateTime.UtcNow.Add(PaymentWindow),
            CreatedAt = DateTime.UtcNow,
        };

        // La reserva se persiste primero: el débito registra un WalletEntry con FK
        // a reservations. Si el saldo no alcanza, la reserva queda en flujo SINPE.
        await reservations.AddAsync(reservation, ct);

        if (request.UseWalletBalance &&
            await wallet.TryDebitAsync(studentId, reservation.PriceCrc, reservation.Id, ct))
        {
            reservation.Status = ReservationStatus.Confirmed;
            reservation.ConfirmedAt = DateTime.UtcNow;
            await reservations.UpdateAsync(reservation, ct);
        }

        if (reservation.Status == ReservationStatus.Confirmed)
        {
            // Pagado con saldo: sin comprobante ni cola del Admin
            var dtoConfirmado = ToDto(reservation, slot.Tutor.Name, slot.StartAt, slot.EndAt);
            return ReservationResult<ReservationDto>.Created(dtoConfirmado);
        }

        var paymentInfo = new PaymentInfoDto(
            ReceptorPhone: sinpe.ReceptorPhone,
            AmountCrc: reservation.PriceCrc,
            Currency: "CRC",
            ExpiresAt: reservation.ExpiresAt);

        var dto = ToDto(reservation, slot.Tutor.Name, slot.StartAt, slot.EndAt, paymentInfo);
        return ReservationResult<ReservationDto>.Created(dto);
    }

    /// <summary>
    /// El estudiante sube un comprobante SINPE. Se valida monto/receptor/ventana
    /// automáticamente; la aprobación/rechazo lo decide el Admin después (cola de pagos).
    /// </summary>
    public async Task<ReservationResult<ReservationDto>> SubmitComprobanteAsync(
        Guid reservationId, SubmitComprobanteRequest request, CancellationToken ct)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);
        if (reservation is null)
            return ReservationResult<ReservationDto>.NotFound();

        if (await TryExpireIfDueAsync(reservation, ct))
            return ReservationResult<ReservationDto>.Found(ToDto(reservation, "", DateTime.MinValue, DateTime.MinValue));

        if (!ReservationStateMachine.CanSubmitComprobante(reservation.Status))
            return ReservationResult<ReservationDto>.Conflict();

        // ¿Comprobante ya usado en otra reserva? (idempotencia del nº de confirmación)
        var existingByConf = await reservations.GetByConfirmationNumberAsync(request.ConfirmationNumber, ct);
        if (existingByConf is not null && existingByConf.Id != reservationId)
            return ReservationResult<ReservationDto>.Conflict();

        // Validación automática del comprobante
        var slot = await slots.GetByIdAsync(reservation.SlotId, ct);
        var dto = new SinpeComprobanteDto(
            ConfirmationNumber: request.ConfirmationNumber,
            AmountCrc: request.AmountCrc,
            ReceptorPhone: sinpe.ReceptorPhone,
            ReservationPhone: sinpe.ReceptorPhone,
            WindowStart: reservation.CreatedAt,
            WindowEnd: reservation.ExpiresAt,
            PriceCrc: reservation.PriceCrc,
            ComprobanteSubmittedAt: DateTime.UtcNow,
            Phone: request.Phone);

        var validation = comprobanteValidator.Validate(dto);
        if (!validation.IsValid)
            return ReservationResult<ReservationDto>.Invalid();

        reservation.ConfirmationNumber = request.ConfirmationNumber;
        reservation.ComprobanteAmountCrc = request.AmountCrc;
        reservation.ComprobantePhone = request.Phone;
        reservation.ComprobanteSubmittedAt = DateTime.UtcNow;

        await reservations.UpdateAsync(reservation, ct);

        return ReservationResult<ReservationDto>.Found(
            ToDto(reservation, slot?.Tutor.Name ?? "", slot?.StartAt ?? DateTime.MinValue, slot?.EndAt ?? DateTime.MinValue));
    }

    /// <summary>
    /// El Admin aprueba o rechaza un comprobante. Al aprobar, pasa a Confirmed.
    /// Al rechazar, slot se libera y se guarda el motivo.
    /// </summary>
    public async Task<ReservationResult<ReservationDto>> DecideAsync(
        Guid reservationId, string decision, string? reason, CancellationToken ct)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);
        if (reservation is null)
            return ReservationResult<ReservationDto>.NotFound();

        if (!ReservationStateMachine.CanDecide(reservation.Status))
            return ReservationResult<ReservationDto>.Conflict();

        if (decision is not ("approve" or "reject"))
            return ReservationResult<ReservationDto>.Invalid();

        if (decision == "reject" && string.IsNullOrWhiteSpace(reason))
            return ReservationResult<ReservationDto>.Invalid();

        if (decision == "approve")
        {
            reservation.Status = ReservationStatus.Confirmed;
            reservation.ConfirmedAt = DateTime.UtcNow;
        }
        else
        {
            reservation.Status = ReservationStatus.Rejected;
            reservation.DecisionReason = reason;
        }

        await reservations.UpdateAsync(reservation, ct);

        var decidedSlot = await slots.GetByIdAsync(reservation.SlotId, ct);
        return ReservationResult<ReservationDto>.Decided(
            ToDto(reservation, decidedSlot?.Tutor.Name ?? "", decidedSlot?.StartAt ?? DateTime.MinValue, decidedSlot?.EndAt ?? DateTime.MinValue));
    }

    /// <summary>
    /// El estudiante cancela su reserva. Si faltan ≥12 h para la sesión,
    /// se acredita el monto al monedero. Si faltan menos, se pierde el pago.
    /// </summary>
    public async Task<ReservationResult<ReservationDto>> CancelAsync(
        Guid reservationId, Guid studentId, CancellationToken ct)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);
        if (reservation is null)
            return ReservationResult<ReservationDto>.NotFound();

        if (reservation.StudentId != studentId)
            return ReservationResult<ReservationDto>.Forbidden();

        if (!ReservationStateMachine.CanCancel(reservation.Status))
            return ReservationResult<ReservationDto>.Conflict();

        var slot = await slots.GetByIdAsync(reservation.SlotId, ct);

        // Determinar si hay reembolso: solo si el pago fue confirmado y faltan ≥12 h.
        // Una reserva en PendingPayment nunca pagó: cancelarla no genera crédito.
        var hoursUntilSlot = (slot?.StartAt ?? DateTime.UtcNow) - DateTime.UtcNow;
        var hasReimbursement =
            reservation.Status == ReservationStatus.Confirmed &&
            hoursUntilSlot >= TimeSpan.FromHours(12);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;

        if (hasReimbursement)
        {
            await wallet.AddAsync(new WalletEntry
            {
                Id = Guid.NewGuid(),
                UserId = studentId,
                AmountCrc = reservation.PriceCrc,
                Reason = WalletEntryReason.CancellationCredit,
                ReservationId = reservationId,
                CreatedAt = DateTime.UtcNow,
            }, ct);
        }

        await reservations.UpdateAsync(reservation, ct);

        return ReservationResult<ReservationDto>.Decided(
            ToDto(reservation, slot?.Tutor.Name ?? "", slot?.StartAt ?? DateTime.MinValue, slot?.EndAt ?? DateTime.MinValue));
    }

    /// <summary>Reservas del estudiante (para /mis-tutorias).</summary>
    public async Task<IReadOnlyList<ReservationDto>> GetByStudentAsync(
        Guid studentId, CancellationToken ct)
    {
        var all = await reservations.GetByStudentAsync(studentId, ct);
        var result = new List<ReservationDto>();
        foreach (var r in all)
        {
            // Expiración perezosa: barre las vencidas sin comprobante al listar
            await TryExpireIfDueAsync(r, ct);
            var slot = await slots.GetByIdAsync(r.SlotId, ct);
            result.Add(ToDto(r, slot?.Tutor.Name ?? "", slot?.StartAt ?? DateTime.MinValue, slot?.EndAt ?? DateTime.MinValue));
        }
        return result;
    }

    /// <summary>Detalle de una reserva para el checkout (polling de estado).</summary>
    public async Task<ReservationResult<ReservationDto>> GetByIdAsync(
        Guid reservationId, Guid requesterId, bool isAdmin, CancellationToken ct)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);
        if (reservation is null)
            return ReservationResult<ReservationDto>.NotFound();

        if (!isAdmin && reservation.StudentId != requesterId)
            return ReservationResult<ReservationDto>.Forbidden();

        // Expiración perezosa: puede que la ventana venciera desde la última lectura
        await TryExpireIfDueAsync(reservation, ct);

        var slot = await slots.GetByIdAsync(reservation.SlotId, ct);
        return ReservationResult<ReservationDto>.Found(
            ToDto(reservation, slot?.Tutor.Name ?? "", slot?.StartAt ?? DateTime.MinValue, slot?.EndAt ?? DateTime.MinValue));
    }

    /// <summary>Saldo del monedero del estudiante.</summary>
    public async Task<int> GetWalletBalanceAsync(Guid userId, CancellationToken ct)
        => await wallet.GetBalanceAsync(userId, ct);

    /// <summary>Reservas de las sesiones que imparte un usuario con rol Tutor.</summary>
    public async Task<IReadOnlyList<ReservationDto>> GetByTutorUserAsync(
        Guid tutorUserId, CancellationToken ct)
    {
        // Las navs (Slot.Tutor, Student) vienen cargadas y con AsNoTracking desde
        // el repositorio: evitar el barrido con slots.GetByIdAsync (tracking
        // duplicado del Tutor en el mismo DbContext).
        var all = await reservations.GetByTutorUserAsync(tutorUserId, ct);
        var result = new List<ReservationDto>();
        foreach (var r in all)
        {
            await TryExpireIfDueAsync(r, ct);
            var slot = r.Slot;
            result.Add(ToDto(r, slot?.Tutor.Name ?? "", slot?.StartAt ?? DateTime.MinValue, slot?.EndAt ?? DateTime.MinValue));
        }
        return result;
    }

    /// <summary>
    /// El tutor dueño de la sesión la marca como concluida. Solo reservas Confirmed
    /// con el slot ya terminado; al cerrar se liquida el pago retenido al ledger del
    /// tutor (+precio con reason TutorPayout). Escritura idempotente ante lectores concurrentes.
    /// </summary>
    public async Task<ReservationResult<ReservationDto>> CompleteAsync(
        Guid reservationId, Guid tutorUserId, CancellationToken ct)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, ct);
        if (reservation is null)
            return ReservationResult<ReservationDto>.NotFound();

        if (!ReservationStateMachine.CanComplete(reservation.Status))
            return ReservationResult<ReservationDto>.Conflict();

        var slot = await slots.GetByIdAsync(reservation.SlotId, ct);
        if (slot is null || slot.Tutor.UserId != tutorUserId)
            return ReservationResult<ReservationDto>.Forbidden();

        if (slot.EndAt > DateTime.UtcNow)
            return ReservationResult<ReservationDto>.Invalid();

        reservation.Status = ReservationStatus.Completed;
        await reservations.UpdateAsync(reservation, ct);

        await wallet.AddAsync(new WalletEntry
        {
            Id = Guid.NewGuid(),
            UserId = tutorUserId,
            AmountCrc = reservation.PriceCrc,
            Reason = WalletEntryReason.TutorPayout,
            ReservationId = reservation.Id,
            CreatedAt = DateTime.UtcNow,
        }, ct);

        return ReservationResult<ReservationDto>.Found(
            ToDto(reservation, slot.Tutor.Name, slot.StartAt, slot.EndAt));
    }

    /// <summary>
    /// Expiración perezosa (ADR-005): una reserva en PendingPayment sin comprobante
    /// cuya ventana venció pasa a Expired (persistido) y devuelve true.
    /// Con comprobante en cola NO expira: el dinero ya se transfirió; la decisión
    /// corresponde al Admin (cola de pagos).
    /// Escritura idempotente: lectores concurrentes pueden repetir la transición sin daño.
    /// </summary>
    private async Task<bool> TryExpireIfDueAsync(Reservation reservation, CancellationToken ct)
    {
        if (reservation.Status != ReservationStatus.PendingPayment ||
            reservation.ConfirmationNumber is not null ||
            reservation.ExpiresAt > DateTime.UtcNow)
            return false;

        reservation.Status = ReservationStatus.Expired;
        await reservations.UpdateAsync(reservation, ct);
        return true;
    }

    private static ReservationDto ToDto(
        Reservation r,
        string tutorName,
        DateTime startAt,
        DateTime endAt,
        PaymentInfoDto? paymentInfo = null) =>
        new(
            r.Id, r.SlotId, r.Slot?.TutorId ?? default, tutorName,
            r.Student?.FullName ?? "", startAt, endAt,
            r.Status, r.PriceCrc, r.ExpiresAt, r.ConfirmationNumber,
            r.DecisionReason, r.CreatedAt)
        { PaymentInfo = paymentInfo };
}
