namespace AuraLearn.Application.Dto;

/// <summary>
/// Perfil del tutor autenticado (GET /api/tutors/me/profile).
/// Excluye campos no editables por el tutor: Rating, Reviews, Featured y Name
/// (el nombre viene de la cuenta de usuario).
/// </summary>
public record TutorProfileResponse(
    Guid Id,
    string Name,
    string Credentials,
    string? Specialization,
    string University,
    string Bio,
    IReadOnlyList<string> Subjects,
    int PriceCrc,
    int PriceUsd,
    string VerificationStatus,
    string? RejectionReason,
    string? PhotoUrl = null);

/// <summary>
/// Actualización del perfil por parte del tutor autenticado (PUT /api/tutors/me/profile).
/// Mismas reglas de negocio que la postulación (SubmitTutorApplicationValidator).
/// </summary>
public record UpdateTutorProfileRequest(
    string Credentials,
    string? Specialization,
    string University,
    string Bio,
    IReadOnlyList<string> Subjects,
    int PriceCrc,
    int PriceUsd);

/// <summary>Códigos de error del perfil del tutor.</summary>
public static class TutorProfileErrors
{
    /// <summary>Un tutor rechazado no edita el perfil: la re-postulación actualiza todos los campos.</summary>
    public const string ProfileRejected = "PROFILE_REJECTED_USE_REAPPLY";
}

/// <summary>Resultado tipado de las operaciones de perfil (patrón TutorApplicationResult).</summary>
public record TutorProfileResult<T>(
    T? Value,
    bool IsFound = false,
    bool IsUpdated = false,
    bool IsInvalid = false,
    bool IsNotFound = false,
    bool IsConflict = false,
    string? ErrorCode = null)
{
    public static TutorProfileResult<T> Found(T value) => new(value, IsFound: true);
    public static TutorProfileResult<T> Updated(T value) => new(value, IsUpdated: true);
    public static TutorProfileResult<T> Invalid() => new(default, IsInvalid: true);
    public static TutorProfileResult<T> NotFound() => new(default, IsNotFound: true);
    public static TutorProfileResult<T> Conflict(string errorCode) =>
        new(default, IsConflict: true, ErrorCode: errorCode);
}
