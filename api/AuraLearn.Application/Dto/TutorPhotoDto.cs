namespace AuraLearn.Application.Dto;

/// <summary>Estado de la foto de perfil tras subir/reemplazar (URL pública relativa).</summary>
public record TutorPhotoResponse(string PhotoUrl);

/// <summary>
/// Confirmación de una subida directa de la foto desde el navegador (ADR-009).
/// El servicio contrasta tamaño y tipo con los metadatos reales del storage.
/// </summary>
public record AttachPhotoCommand(
    string StoragePath,
    string FileName,
    string ContentType,
    long SizeBytes);

/// <summary>Contenido de la foto para descarga (el stream lo cierra el FileResult).</summary>
public record TutorPhotoFile(string ContentType, Stream Content);

/// <summary>Resultado tipado de las operaciones de foto (patrón TutorProfileResult).</summary>
public record TutorPhotoResult<T>(
    T? Value,
    bool IsUpdated = false,
    bool IsFound = false,
    bool IsInvalid = false,
    bool IsNotFound = false)
{
    public static TutorPhotoResult<T> Updated(T value) => new(value, IsUpdated: true);
    public static TutorPhotoResult<T> Found(T value) => new(value, IsFound: true);
    public static TutorPhotoResult<T> Invalid() => new(default, IsInvalid: true);
    public static TutorPhotoResult<T> NotFound() => new(default, IsNotFound: true);
}
