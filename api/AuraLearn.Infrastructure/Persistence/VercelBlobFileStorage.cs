using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Enums;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Almacenamiento de documentos y fotos en Vercel Blob (store privado) — ADR-009.
///
/// No existe SDK oficial de .NET, así que se replica el contrato REST que usa
/// el SDK oficial JS (@vercel/blob, x-api-version 13) para no añadir
/// dependencias ni cambiar de proveedor si se migra a S3/R2 más adelante.
///
/// Las rutas de storage guardadas en BD son <em>pathnames</em> del store
/// (p. ej. <c>{userId:N}/documents/titulo/{guid}.pdf</c>), no rutas de disco.
/// Nunca se usa el nombre original del usuario en el pathname.
///
/// El store es privado: la lectura siempre pasa por el API con el token de
/// servicio, lo que permite mantener la autorización (PII solo Admin) en .NET.
/// </summary>
public sealed class VercelBlobFileStorage : IFileStorage, IDisposable
{
    /// <summary>Versión de la API de Blob usada por el SDK oficial JS.</summary>
    private const string ApiVersion = "13";

    private const string ApiBaseUrl = "https://vercel.com/api/blob";

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly string _token;
    private readonly string _storeId;
    private readonly string _access;

    /// <param name="token">BLOB_READ_WRITE_TOKEN (formato vercel_blob_rw_&lt;storeId&gt;_&lt;secreto&gt;).</param>
    /// <param name="access">Modo del store: "private" (PII) o "public".</param>
    public VercelBlobFileStorage(string token, string access = "private", HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("BLOB_READ_WRITE_TOKEN no configurado", nameof(token));

        _token = token;
        _storeId = ParseStoreId(token);
        _access = access;

        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        _ownsHttpClient = httpClient is null;
    }

    /// <summary>
    /// El store remoto es la fuente de verdad de los bytes: la subida directa
    /// desde el navegador es el único camino soportado en este entorno (evita
    /// el límite de 4.5 MB por request de Vercel Functions).
    /// </summary>
    public Task<string> SaveAsync(
        Guid tutorId, TutorDocumentKind kind, string fileName, Stream content, CancellationToken ct) =>
        PutAsync(BuildPathname(tutorId, kind.ToString().ToLowerInvariant()), fileName, content, ct);

    public Task<string> SavePhotoAsync(Guid tutorId, string fileName, Stream content, CancellationToken ct) =>
        PutAsync(BuildPathname(tutorId, "photo"), fileName, content, ct);

    /// <summary>
    /// Descarga el contenido a memoria (documentos ≤ 5 MB). Se bufferiza a
    /// propósito: el HttpResponseMessage no debe sobrevivir al método, y el
    /// stream devuelto lo cierra el FileResult del controller.
    /// </summary>
    public async Task<Stream?> OpenAsync(string storagePath, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BlobUrl(storagePath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();

        var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, ct);
        buffer.Position = 0;
        return buffer;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        // Endpoint de borrado: POST /delete con la lista de URLs (idempotente:
        // borrar un blob inexistente no es error).
        var payload = JsonSerializer.Serialize(new { urls = new[] { BlobUrl(storagePath) } });

        using var request = CreateApiRequest(HttpMethod.Post, "/delete");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, ct);
        if (response.StatusCode != HttpStatusCode.NotFound)
            response.EnsureSuccessStatusCode();
    }

    public async Task<StoredFileInfo?> GetInfoAsync(string storagePath, CancellationToken ct)
    {
        // GET /?url=<pathname> devuelve los metadatos (equivale a head() del SDK).
        using var request = CreateApiRequest(
            HttpMethod.Get, $"?url={Uri.EscapeDataString(storagePath)}");

        using var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = document.RootElement;

        var size = root.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : 0L;
        var contentType = root.TryGetProperty("contentType", out var typeElement)
            ? typeElement.GetString() ?? "application/octet-stream"
            : "application/octet-stream";

        return new StoredFileInfo(size, contentType);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
            _http.Dispose();
    }

    // ── Internos ────────────────────────────────────────────────────

    /// <summary>Pathname determinista por tutor + categoría + GUID (sin nombre del usuario).</summary>
    private static string BuildPathname(Guid tutorId, string category) =>
        $"{tutorId:N}/{category}/{Guid.NewGuid():N}";

    private async Task<string> PutAsync(
        string pathnameWithoutExtension, string fileName, Stream content, CancellationToken ct)
    {
        var pathname = pathnameWithoutExtension + Path.GetExtension(fileName).ToLowerInvariant();

        using var request = CreateApiRequest(
            HttpMethod.Put, $"/?pathname={Uri.EscapeDataString(pathname)}");

        // x-add-random-suffix=0: el pathname ya es único (GUID) y así el valor
        // guardado en BD coincide exactamente con el del store.
        request.Headers.Add("x-add-random-suffix", "0");
        request.Headers.Add("x-vercel-blob-access", _access);

        var contentType = ContentTypeForExtension(pathname);
        if (contentType is not null)
            request.Headers.Add("x-content-type", contentType);

        request.Content = new StreamContent(content);
        if (contentType is not null)
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return pathname;
    }

    private HttpRequestMessage CreateApiRequest(HttpMethod method, string relativeUrl)
    {
        var request = new HttpRequestMessage(method, ApiBaseUrl + relativeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Headers.Add("x-api-version", ApiVersion);
        request.Headers.Add("x-vercel-blob-store-id", _storeId);
        request.Headers.Add("x-api-blob-request-id", $"{_storeId}:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        request.Headers.Add("x-api-blob-request-attempt", "0");
        return request;
    }

    private string BlobUrl(string pathname) =>
        $"https://{_storeId}.{_access}.blob.vercel-storage.com/{pathname.TrimStart('/')}";

    /// <summary>El token tiene el formato vercel_blob_rw_&lt;storeId&gt;_&lt;secreto&gt;.</summary>
    private static string ParseStoreId(string token)
    {
        var parts = token.Split('_');
        if (parts.Length < 4 || string.IsNullOrEmpty(parts[3]))
            throw new ArgumentException("BLOB_READ_WRITE_TOKEN con formato inválido", nameof(token));

        return parts[3];
    }

    private static string? ContentTypeForExtension(string pathname) =>
        Path.GetExtension(pathname).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => null,
        };
}
