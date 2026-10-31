using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Almacenamiento de documentos en disco local, fuera del repo (root
/// configurable con Storage:DocumentsRoot; por defecto ~/.auralearn/uploads/documents).
/// El nombre físico lo genera el servidor ({tutorId}/{kind}/{Guid}.{ext}) para
/// eliminar cualquier riesgo de path traversal; el nombre original del usuario
/// solo se conserva como metadato en la BD.
/// Migrar a blob/S3 en el futuro solo requiere otra implementación de IFileStorage.
/// </summary>
public class LocalFileStorage(string root) : IFileStorage
{
    public async Task<string> SaveAsync(
        Guid tutorId, TutorDocumentKind kind, string fileName, Stream content, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var relativePath = $"{tutorId:N}/{kind.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}{extension}";

        var absolutePath = ToAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    public Task<Stream?> OpenAsync(string storagePath, CancellationToken ct)
    {
        var absolutePath = ToAbsolutePath(storagePath);
        if (!File.Exists(absolutePath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(new FileStream(absolutePath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        var absolutePath = ToAbsolutePath(storagePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    public async Task<string> SavePhotoAsync(
        Guid tutorId, string fileName, Stream content, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var relativePath = $"{tutorId:N}/photo/{Guid.NewGuid():N}{extension}";

        var absolutePath = ToAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    /// <summary>
    /// Resuelve la ruta relativa contra el root y verifica que no escape de él
    /// (defensa en profundidad: las rutas en BD las genera el servidor).
    /// </summary>
    private string ToAbsolutePath(string storagePath)
    {
        var rootFullPath = Path.GetFullPath(root);
        var absolutePath = Path.GetFullPath(Path.Combine(rootFullPath, storagePath));

        if (!absolutePath.StartsWith(rootFullPath, StringComparison.Ordinal))
            throw new InvalidOperationException("Ruta de storage inválida");

        return absolutePath;
    }
}
