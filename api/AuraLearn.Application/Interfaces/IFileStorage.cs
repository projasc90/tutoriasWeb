using AuraLearn.Domain.Enums;

namespace AuraLearn.Application.Interfaces;

/// <summary>
/// Puerto de almacenamiento de archivos de verificación de tutores.
/// La implementación actual es disco local (LocalFileStorage); migrar a
/// blob/S3 solo requiere otra implementación de este puerto (ADR pendiente
/// de registrar al cerrar la tarea).
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
}
