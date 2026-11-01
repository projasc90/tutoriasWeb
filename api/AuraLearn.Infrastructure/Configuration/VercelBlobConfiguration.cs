namespace AuraLearn.Infrastructure.Configuration;

/// <summary>
/// Credenciales resueltas del store de Vercel Blob.
/// <see cref="Access"/> es el modo del store (<c>private</c> por defecto).
/// </summary>
public sealed record VercelBlobCredentials(string Token, string StoreId, string Access);

/// <summary>
/// Resuelve las credenciales de Vercel Blob desde las variables que Vercel
/// inyecta al conectar el store a un proyecto, para no mantener nombres propios:
/// <list type="bullet">
///   <item><c>BLOB_READ_WRITE_TOKEN</c> — token de lectura-escritura (<c>vercel_blob_rw_&lt;storeId&gt;_&lt;secreto&gt;</c>).</item>
///   <item><c>BLOB_STORE_ID</c> — id del store, en formato <c>store_&lt;id&gt;</c> o <c>&lt;id&gt;</c>.</item>
/// </list>
/// El mismo par lo usa el frontend (<c>web/.env.local</c>), así que ambos lados
/// apuntan al mismo store sin configuración duplicada.
///
/// ⚠️ Solo se soporta el token estático: <c>handleUpload</c> del SDK JS y este
/// cliente REST de .NET necesitan credencial de escritura, y OIDC
/// (<c>VERCEL_OIDC_TOKEN</c>) no la sustituye en esos dos caminos.
/// </summary>
public static class VercelBlobConfiguration
{
    /// <summary>Token estático de lectura-escritura del store.</summary>
    public const string TokenVariable = "BLOB_READ_WRITE_TOKEN";

    /// <summary>Id del store inyectado al conectarlo al proyecto.</summary>
    public const string StoreIdVariable = "BLOB_STORE_ID";

    /// <summary>Modo del store: <c>private</c> (PII) o <c>public</c>.</summary>
    public const string AccessConfigurationKey = "Storage:VercelBlobAccess";

    private const string TokenPrefix = "vercel_blob_rw_";
    private const string StoreIdPrefix = "store_";

    /// <summary>
    /// Resuelve las credenciales. <paramref name="read"/> abstrae el lector de
    /// configuración (variables de entorno incluidas).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Falta el token, el formato es inválido o <c>BLOB_STORE_ID</c> apunta a un
    /// store distinto al del token.
    /// </exception>
    public static VercelBlobCredentials Resolve(Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);

        // Storage:VercelBlobToken se mantiene como alias de compatibilidad; no se documenta.
        var token = Clean(read(TokenVariable)) ?? Clean(read("Storage:VercelBlobToken"));
        if (token is null)
            throw MissingToken();

        var access = Clean(read(AccessConfigurationKey)) ?? "private";

        return new VercelBlobCredentials(token, ResolveStoreId(token, Clean(read(StoreIdVariable))), access);
    }

    /// <summary>
    /// Determina el id del store, normalizado a la forma que espera la URL
    /// (<c>&lt;id&gt;.private.blob.vercel-storage.com</c>, sin el prefijo <c>store_</c>).
    /// </summary>
    /// <param name="token">Token de lectura-escritura.</param>
    /// <param name="configuredStoreId">Valor de <c>BLOB_STORE_ID</c>, si existe.</param>
    public static string ResolveStoreId(string token, string? configuredStoreId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var fromToken = StoreIdFromToken(token);
        var fromConfiguration = string.IsNullOrWhiteSpace(configuredStoreId)
            ? null
            : NormalizeStoreId(configuredStoreId);

        if (string.IsNullOrEmpty(fromConfiguration))
            fromConfiguration = null;

        if (fromConfiguration is not null
            && fromToken is not null
            && !string.Equals(fromConfiguration, fromToken, StringComparison.Ordinal))
        {
            // Los store ids no son secretos (aparecen en las URLs), así que se
            // muestran: el error típico es haber copiado el id de otro store.
            throw new InvalidOperationException(
                $"{StoreIdVariable} ({fromConfiguration}) no coincide con el store del " +
                $"{TokenVariable} ({fromToken}). Corrige la variable o el token.");
        }

        var storeId = fromConfiguration ?? fromToken;
        if (storeId is null)
            throw new InvalidOperationException(
                $"{TokenVariable} con formato inválido: se espera {TokenPrefix}<storeId>_<secreto>. " +
                $"Define {StoreIdVariable} si el token no lo incluye.");

        return storeId;
    }

    /// <summary>
    /// Extrae el store id del token. El secreto final no tiene guiones bajos, así
    /// que se corta por el último <c>_</c> y se tolera que el id venga con el
    /// prefijo <c>store_</c>.
    /// </summary>
    public static string? StoreIdFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var trimmed = token.Trim();
        if (!trimmed.StartsWith(TokenPrefix, StringComparison.Ordinal))
            return null;

        var rest = trimmed[TokenPrefix.Length..];
        var lastSeparator = rest.LastIndexOf('_');
        if (lastSeparator <= 0 || lastSeparator == rest.Length - 1)
            return null;

        var storeId = NormalizeStoreId(rest[..lastSeparator]);
        return string.IsNullOrEmpty(storeId) ? null : storeId;
    }

    /// <summary>Quita el prefijo <c>store_</c> y espacios sobrantes.</summary>
    public static string NormalizeStoreId(string storeId)
    {
        ArgumentNullException.ThrowIfNull(storeId);

        var trimmed = storeId.Trim();
        return trimmed.StartsWith(StoreIdPrefix, StringComparison.OrdinalIgnoreCase)
            ? trimmed[StoreIdPrefix.Length..]
            : trimmed;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static InvalidOperationException MissingToken() => new(
        $"{TokenVariable} no configurado. {StoreIdVariable} por sí solo no basta: el API firma " +
        "y lee los blobs con el token de lectura-escritura (VERCEL_OIDC_TOKEN no está soportado).");
}
