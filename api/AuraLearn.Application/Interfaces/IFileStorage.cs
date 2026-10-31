using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Interfaces;

/// <summary>
/// Puerto de almacenamiento de archivos de verificación de tutores.
/// Implementaciones: LocalFileStorage (disco, dev/local) y
/// VercelBlobFileStorage (object storage, producción en Vercel) — ADR-009.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Guarda el contenido del stream y devuelve la ruta de storage relativa
    /// al root configurado. El nombre físico lo genera la implementación
    /// (nunca se usa el nombre original del usuario en disco).
    /// </summary>
    Task<string> SaveAsync(Guid tutorId, TutorDocumentKind kind, string fileName, Stream content, CancellationToken ct);

    /// <summary>Abre el archivo referenciado por la ruta de storage (o null si no existe).</summary>
    Task<Stream?> OpenAsync(string storagePath, CancellationToken ct);

    /// <summary>Elimina el archivo referenciado por la ruta de storage (idempotente).</summary>
    Task DeleteAsync(string storagePath, CancellationToken ct);

    /// <summary>
    /// Guarda la foto de perfil del tutor (pública, distinta de los documentos
    /// de verificación) y devuelve la ruta de storage relativa. El nombre
    /// físico lo genera la implementación.
    /// </summary>
    Task<string> SavePhotoAsync(Guid tutorId, string fileName, Stream content, CancellationToken ct);

    /// <summary>
    /// Metadatos de un archivo ya presente en el storage, o null si no existe.
    /// Se usa para confirmar subidas directas desde el cliente (el navegador
    /// sube a Blob y el API solo valida tamaño y tipo reales antes de persistir).
    /// </summary>
    Task<StoredFileInfo?> GetInfoAsync(string storagePath, CancellationToken ct);
}

/// <summary>Tamaño y tipo MIME reales de un archivo en el storage.</summary>
public record StoredFileInfo(long SizeBytes, string ContentType);

