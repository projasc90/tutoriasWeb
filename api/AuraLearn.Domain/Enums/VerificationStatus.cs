namespace AuraLearn.Domain.Enums;

/// <summary>
/// Estado de verificación de credenciales de un tutor.
/// Solo <see cref="Verified"/> es visible y reservable en el catálogo público.
/// </summary>
public enum VerificationStatus
{
    PendingReview = 1,
    UnderReview = 2,
    Verified = 3,
    Rejected = 4,
}
