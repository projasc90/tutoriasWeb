using AuraLearn.Application.Dto;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Services;

/// <summary>
/// Casos de uso de los documentos de verificación del tutor (títulos,
/// certificaciones, identificación). Reglas de negocio:
/// - Whitelist de tipos (PDF/JPEG/PNG) con extensión coherente, ≤ 5 MB.
/// - Titulo y Certificacion acumulan (máx 5 c/u); Identificacion es única
///   (upsert: reemplaza y borra el archivo previo del storage).
/// - Con postulación PendingReview/Rejected se puede subir todo.
/// - Con Verified se pueden AGREGAR títulos y certificaciones (quedan
///   disponibles para el equipo de verificación sin re-abrir el estado),
///   pero la identificación queda bloqueada → 409 IDENTIFICATION_LOCKED
///   (ya fue verificada; el historial es inmutable).
/// El nombre físico en disco lo genera IFileStorage (nunca el del usuario).
/// </summary>
public class TutorDocumentService(
    ITutorRepository tutors,
    ITutorDocumentRepository documents,
    IFileStorage storage)
{
    /// <summary>Tamaño máximo por archivo: 5 MB.</summary>
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    /// <summary>Máximo de archivos acumulados por tipo (títulos y certificaciones).</summary>
    public const int MaxPerAccumulatedKind = 5;

    /// <summary>Tipos MIME permitidos y las extensiones coherentes con cada uno.</summary>
    private static readonly Dictionary<string, string[]> AllowedContentTypes = new()
    {
        ["application/pdf"] = [".pdf"],
        ["image/jpeg"] = [".jpg", ".jpeg"],
        ["image/png"] = [".png"],
    };

    public async Task<TutorDocumentResult<TutorDocumentItemDto>> UploadAsync(
        Guid userId, UploadDocumentCommand command, CancellationToken ct)
    {
        if (!IsValidCommand(command))
            return TutorDocumentResult<TutorDocumentItemDto>.Invalid();

        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorDocumentResult<TutorDocumentItemDto>.NotFound();

        // Post-aprobación: la identificación ya fue verificada y no se toca;
        // títulos y certificaciones sí se pueden agregar (sin cambiar el estado).
        if (tutor.VerificationStatus == VerificationStatus.Verified
            && command.Kind == TutorDocumentKind.Identificacion)
            return TutorDocumentResult<TutorDocumentItemDto>.Conflict(TutorDocumentErrors.IdentificationLocked);

        if (tutor.VerificationStatus is not (VerificationStatus.PendingReview or VerificationStatus.Rejected or VerificationStatus.Verified))
            return TutorDocumentResult<TutorDocumentItemDto>.Conflict(TutorDocumentErrors.DocumentsLocked);

        var all = await documents.ListByTutorAsync(tutor.Id, ct);
        if (command.Kind is TutorDocumentKind.Titulo or TutorDocumentKind.Certificacion)
        {
            // Acumulativo: máx 5 archivos por tipo.
            if (all.Count(d => d.Kind == command.Kind) >= MaxPerAccumulatedKind)
                return TutorDocumentResult<TutorDocumentItemDto>.Invalid();
        }
        else
        {
            // Upsert: la identificación es única; el archivo previo se elimina.
            var existing = await documents.GetByKindAsync(tutor.Id, command.Kind, ct);
            if (existing is not null)
            {
                await documents.RemoveAsync(existing, ct);
                await storage.DeleteAsync(existing.StoragePath, ct);
            }
        }

        var storagePath = await storage.SaveAsync(
            tutor.Id, command.Kind, command.FileName, command.Content, ct);

        var document = new TutorDocument
        {
            Id = Guid.NewGuid(),
            TutorId = tutor.Id,
            Kind = command.Kind,
            FileName = command.FileName,
            ContentType = command.ContentType,
            SizeBytes = command.SizeBytes,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow,
        };
        await documents.AddAsync(document, ct);

        return TutorDocumentResult<TutorDocumentItemDto>.Created(ToDto(document));
    }

    public async Task<TutorDocumentResult<IReadOnlyList<TutorDocumentItemDto>>> ListAsync(
        Guid userId, CancellationToken ct)
    {
        var tutor = await tutors.GetByUserIdAsync(userId, ct);
        if (tutor is null)
            return TutorDocumentResult<IReadOnlyList<TutorDocumentItemDto>>.NotFound();

        var docs = await documents.ListByTutorAsync(tutor.Id, ct);
        return TutorDocumentResult<IReadOnlyList<TutorDocumentItemDto>>.Found(
            [.. docs.Select(ToDto)]);
    }

    /// <summary>
    /// Abre el contenido de un documento por id (descarga del admin; PII).
    /// El stream devuelto lo cierra el FileResult del controller.
    /// </summary>
    public async Task<TutorDocumentResult<TutorDocumentFile>> GetFileAsync(Guid documentId, CancellationToken ct)
    {
        var document = await documents.GetByIdAsync(documentId, ct);
        if (document is null)
            return TutorDocumentResult<TutorDocumentFile>.NotFound();

        var content = await storage.OpenAsync(document.StoragePath, ct);
        if (content is null)
            return TutorDocumentResult<TutorDocumentFile>.NotFound();

        return TutorDocumentResult<TutorDocumentFile>.Found(
            new TutorDocumentFile(document.FileName, document.ContentType, content));
    }

    private static bool IsValidCommand(UploadDocumentCommand command)
    {
        if (!Enum.IsDefined(command.Kind))
            return false;

        if (string.IsNullOrWhiteSpace(command.FileName) || command.FileName.Length > 255)
            return false;

        if (command.SizeBytes <= 0 || command.SizeBytes > MaxSizeBytes)
            return false;

        return IsAllowedPair(command.ContentType, command.FileName);
    }

    /// <summary>El tipo MIME debe estar en la whitelist y la extensión ser coherente con él.</summary>
    private static bool IsAllowedPair(string contentType, string fileName)
    {
        if (!AllowedContentTypes.TryGetValue(contentType, out var extensions))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extensions.Contains(extension);
    }

    private static TutorDocumentItemDto ToDto(TutorDocument document) =>
        new(document.Id, document.Kind, document.FileName, document.ContentType, document.SizeBytes, document.UploadedAt);
}
