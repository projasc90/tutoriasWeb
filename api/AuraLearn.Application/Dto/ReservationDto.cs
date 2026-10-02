using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Dto;

/// <summary>Slot disponible de un tutor (contrato con el frontend).</summary>
public record SlotDto(Guid Id, Guid TutorId, DateTime StartAt, DateTime EndAt);

/// <summary>Información de pago SINPE para completar la reserva.</summary>
public record PaymentInfoDto(string ReceptorPhone, int AmountCrc, string Currency, DateTime ExpiresAt);

/// <summary>
/// Reserva desde la perspectiva del estudiante.
/// PaymentInfo solo está presente cuando se acaba de crear la reserva (respuesta 201 del POST).
/// </summary>
public record ReservationDto(
    Guid Id,
    Guid SlotId,
    Guid TutorId,
    string TutorName,
    string StudentName,
    DateTime StartAt,
    DateTime EndAt,
    ReservationStatus Status,
    int PriceCrc,
    DateTime ExpiresAt,
    string? ConfirmationNumber,
    string? DecisionReason,
    DateTime CreatedAt)
{
    /// <summary>Presente solo en la respuesta de creación de reserva (201).</summary>
    public PaymentInfoDto? PaymentInfo { get; init; }
}

/// <summary>
/// Solicitud de creación de reserva (idempotente por clave).
/// UseWalletBalance: pagar directo con el saldo del monedero si alcanza (ADR-006);
/// si no alcanza, cae automáticamente al flujo SINPE normal.
/// </summary>
public record CreateReservationRequest(Guid SlotId, string IdempotencyKey, bool UseWalletBalance = false);

/// <summary>Solicitud de comprobante SINPE.</summary>
public record SubmitComprobanteRequest(string ConfirmationNumber, int AmountCrc, string? Phone);

/// <summary>Decisión del admin sobre un comprobante.</summary>
public record DecidePaymentRequest(string Decision, string? Reason);

/// <summary>Comprobante pendiente de revisión (cola del admin).</summary>
public record AdminPaymentDto(
    Guid ReservationId,
    Guid StudentId,
    string StudentName,
    Guid TutorId,
    string TutorName,
    DateTime StartAt,
    int PriceCrc,
    string ConfirmationNumber,
    int? ComprobanteAmountCrc,
    string? ComprobantePhone,
    DateTime SubmittedAt);

/// <summary>Saldo del monedero del estudiante.</summary>
public record WalletBalanceDto(int BalanceCrc);

/// <summary>
/// Configuración de SINPE (inyectada desde appsettings).
/// </summary>
public sealed record SinpeConfig(string ReceptorPhone);

/// <summary>
/// DTO interno del validador de comprobante SINPE (construido por ReservationService
/// antes de llamar al validador).
/// </summary>
public record SinpeComprobanteDto(
    string ConfirmationNumber,
    int AmountCrc,
    string ReceptorPhone,
    string ReservationPhone,
    DateTime WindowStart,
    DateTime WindowEnd,
    int PriceCrc,
    DateTime? ComprobanteSubmittedAt = null,
    string? Phone = null);

/// <summary>
/// Resultado tipado de las operaciones de reserva (patrón TutorApplicationResult).
/// Flags adicionales: IsGone (slot ya no existe), IsForbidden (no es dueño).
/// </summary>
public record ReservationResult<T>(
    T? Value,
    bool IsCreated = false,
    bool IsConflict = false,
    bool IsInvalid = false,
    bool IsUnauthorized = false,
    bool IsNotFound = false,
    bool IsFound = false,
    bool IsDecided = false,
    bool IsGone = false,
    bool IsForbidden = false)
{
    public static ReservationResult<T> Created(T value) => new(value, IsCreated: true);
    public static ReservationResult<T> Conflict() => new(default, IsConflict: true);
    public static ReservationResult<T> Invalid() => new(default, IsInvalid: true);
    public static ReservationResult<T> Unauthorized() => new(default, IsUnauthorized: true);
    public static ReservationResult<T> NotFound() => new(default, IsNotFound: true);
    public static ReservationResult<T> Found(T value) => new(value, IsFound: true);
    public static ReservationResult<T> Decided(T value) => new(value, IsDecided: true);
    public static ReservationResult<T> Gone() => new(default, IsGone: true);
    public static ReservationResult<T> Forbidden() => new(default, IsForbidden: true);
}
