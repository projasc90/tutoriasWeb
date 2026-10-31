using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Dto;

/// <summary>Documento de verificación visible por el tutor o el admin (sin contenido).</summary>
public record TutorDocumentItemDto(
    Guid Id,
    TutorDocumentKind Kind,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt);

/// <summary>
/// Comando de subida de un documento (el controller adapta IFormFile a este
/// comando; la validación de negocio vive en el servicio).
/// </summary>
public record UploadDocumentCommand(
    TutorDocumentKind Kind,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

/// <summary>
/// Confirmación de una subida directa desde el navegador (ADR-009): el archivo
/// ya está en el store y solo se envía la referencia. <c>SizeBytes</c> y
/// <c>ContentType</c> son declarados por el cliente, pero el servicio los
/// contrasta con los metadatos reales del storage antes de persistir.
/// </summary>
public record AttachDocumentCommand(
    TutorDocumentKind Kind,
    string StoragePath,
    string FileName,
    string ContentType,
    long SizeBytes);

/// <summary>Contenido de un documento para descarga (el stream lo cierra el FileResult).</summary>
public record TutorDocumentFile(string FileName, string ContentType, Stream Content);

/// <summary>Códigos de error de los documentos de verificación.</summary>
public static class TutorDocumentErrors
{
    /// <summary>Un tutor verificado no puede modificar sus documentos (historial inmutable).</summary>
    public const string DocumentsLocked = "DOCUMENTS_LOCKED";

    /// <summary>La identificación ya fue verificada: no se reemplaza tras la aprobación.</summary>
    public const string IdentificationLocked = "IDENTIFICATION_LOCKED";
}

/// <summary>Resultado tipado de las operaciones de documentos (patrón TutorProfileResult).</summary>
public record TutorDocumentResult<T>(
    T? Value,
    bool IsCreated = false,
    bool IsFound = false,
    bool IsInvalid = false,
    bool IsNotFound = false,
    bool IsConflict = false,
    string? ErrorCode = null)
{
    public static TutorDocumentResult<T> Created(T value) => new(value, IsCreated: true);
    public static TutorDocumentResult<T> Found(T value) => new(value, IsFound: true);
    public static TutorDocumentResult<T> Invalid() => new(default, IsInvalid: true);
    public static TutorDocumentResult<T> NotFound() => new(default, IsNotFound: true);
    public static TutorDocumentResult<T> Conflict(string errorCode) =>
        new(default, IsConflict: true, ErrorCode: errorCode);
}
