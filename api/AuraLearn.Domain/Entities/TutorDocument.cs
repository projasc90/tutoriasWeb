using AuraLearn.Domain.Enums;

namespace AuraLearn.Domain.Entities;

/// <summary>
/// Documento presentado por un tutor para la verificación de credenciales
/// (título académico, certificaciones, identificación). PII: el contenido
/// vive en storage (nunca en la BD ni en el repo); aquí solo metadatos.
/// El archivo en disco se referencia con una ruta relativa al root configurado
/// (Storage:DocumentsRoot), generada por el servidor.
/// </summary>
public class TutorDocument
{
    public Guid Id { get; set; }

    /// <summary>Tutor propietario del documento (FK a tutors).</summary>
    public Guid TutorId { get; set; }

    public TutorDocumentKind Kind { get; set; }

    /// <summary>Nombre original del archivo (solo para mostrar; nunca se usa en disco).</summary>
    public required string FileName { get; set; }

    /// <summary>Tipo MIME validado contra la whitelist (pdf/jpeg/png).</summary>
    public required string ContentType { get; set; }

    /// <summary>Tamaño del archivo en bytes (≤ 5 MB validado en el servicio).</summary>
    public long SizeBytes { get; set; }

    /// <summary>Ruta relativa al root de storage, generada por el servidor.</summary>
    public required string StoragePath { get; set; }

    public DateTime UploadedAt { get; set; }
}
