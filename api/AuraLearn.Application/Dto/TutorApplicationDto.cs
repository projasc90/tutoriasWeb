using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Dto;

/// <summary>
/// Solicitud de postulación de un profesor (onboarding del tutor).
/// Espejo del formulario del wizard /postular del frontend.
/// </summary>
public record SubmitTutorApplicationRequest(
    string FullName,
    string Credentials,
    string University,
    string Bio,
    IReadOnlyList<string> Subjects,
    int PriceCrc,
    int PriceUsd);

/// <summary>Respuesta al enviar una postulación (202).</summary>
public record TutorApplicationResponse(Guid Id, string Status);

/// <summary>Estado de la postulación del usuario autenticado.</summary>
public record TutorApplicationStatusResponse(
    Guid Id,
    string Status,
    string? RejectionReason,
    DateTime SubmittedAt);

/// <summary>Postulación pendiente visible para el administrador.</summary>
public record AdminTutorApplicationDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Credentials,
    string University,
    string Bio,
    IReadOnlyList<string> Subjects,
    int PriceCrc,
    int PriceUsd,
    string Status,
    DateTime SubmittedAt);

/// <summary>Decisión del administrador sobre una postulación.</summary>
public record VerifyTutorApplicationRequest(string Decision, string? Reason);

/// <summary>Resultado tipado de las operaciones de postulación (patrón AuthResult).</summary>
public record TutorApplicationResult<T>(
    T? Value,
    bool IsCreated = false,
    bool IsConflict = false,
    bool IsInvalid = false,
    bool IsUnauthorized = false,
    bool IsNotFound = false,
    bool IsFound = false,
    bool IsDecided = false)
{
    public static TutorApplicationResult<T> Created(T value) => new(value, IsCreated: true);
    public static TutorApplicationResult<T> Conflict() => new(default, IsConflict: true);
    public static TutorApplicationResult<T> Invalid() => new(default, IsInvalid: true);
    public static TutorApplicationResult<T> Unauthorized() => new(default, IsUnauthorized: true);
    public static TutorApplicationResult<T> NotFound() => new(default, IsNotFound: true);
    public static TutorApplicationResult<T> Found(T value) => new(value, IsFound: true);
    public static TutorApplicationResult<T> Decided(T value) => new(value, IsDecided: true);
}
