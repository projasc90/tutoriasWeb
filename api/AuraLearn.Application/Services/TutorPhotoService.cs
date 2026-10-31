using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;

namespace AuraLearn.Application.Services;

/// <summary>
/// Casos de uso de la foto de perfil del tutor (subida desde /tutor/perfil).
/// La foto es pública (no es documento de verificación ni PII restringida):
/// se sirve sin autenticación vía GET /api/tutors/{id}/photo. Reglas:
/// - Solo JPEG/PNG/WebP ≤ 5 MB con extensión coherente.
/// - Upsert: subir una foto nueva reemplaza la anterior (borra el archivo previo).
/// El nombre físico en disco lo genera IFileStorage (nunca el del usuario).
/// </summary>
public class TutorPhotoService(ITutorRepository tutors, IFileStorage storage)
{
    /// <summary>Tamaño máximo de la foto: 5 MB.</summary>
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    /// <summary>Tipos MIME permitidos y las extensiones coherentes con cada uno.</summary>
    private static readonly Dictionary<string, string[]> AllowedContentTypes = new()
    {
        ["image/jpeg"] = [".jpg", ".jpeg"],
        ["image/png"] = [".png"],
        ["image/webp"] = [".webp"],
    };

    public async Task<TutorPhotoResult<TutorPhotoResponse>> UploadAsync(
        Guid userId, string fileName, string contentType, long sizeBytes, Stream content, CancellationToken ct)
    {
        if (!IsValid(fileName, contentType, sizeBytes))
            return TutorPhotoResult<TutorPhotoResponse>.Invalid();

        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorPhotoResult<TutorPhotoResponse>.NotFound();

        var oldPath = tutor.PhotoPath;
        var storagePath = await storage.SavePhotoAsync(tutor.Id, fileName, content, ct);

        tutor.PhotoPath = storagePath;
        await tutors.UpdateAsync(tutor, ct);

        if (!string.IsNullOrEmpty(oldPath))
            await storage.DeleteAsync(oldPath, ct);

        return TutorPhotoResult<TutorPhotoResponse>.Updated(
            new TutorPhotoResponse(PhotoUrlFor(tutor.Id)));
    }

    public async Task<TutorPhotoResult<TutorPhotoFile>> GetPhotoAsync(Guid tutorId, CancellationToken ct)
    {
        var tutor = await tutors.GetByIdAsync(tutorId, ct);
        if (tutor is null || string.IsNullOrEmpty(tutor.PhotoPath))
            return TutorPhotoResult<TutorPhotoFile>.NotFound();

        var content = await storage.OpenAsync(tutor.PhotoPath, ct);
        if (content is null)
            return TutorPhotoResult<TutorPhotoFile>.NotFound();

        return TutorPhotoResult<TutorPhotoFile>.Found(
            new TutorPhotoFile(ContentTypeFor(tutor.PhotoPath), content));
    }

    /// <summary>URL pública relativa de la foto (null si el tutor no tiene).</summary>
    public static string? PhotoUrlOrNull(Tutor tutor) =>
        string.IsNullOrEmpty(tutor.PhotoPath) ? null : PhotoUrlFor(tutor.Id);

    private static string PhotoUrlFor(Guid tutorId) => $"/api/tutors/{tutorId}/photo";

    private static bool IsValid(string fileName, string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
            return false;

        if (sizeBytes <= 0 || sizeBytes > MaxSizeBytes)
            return false;

        if (!AllowedContentTypes.TryGetValue(contentType, out var extensions))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extensions.Contains(extension);
    }

    private static string ContentTypeFor(string storagePath) =>
        Path.GetExtension(storagePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
}
