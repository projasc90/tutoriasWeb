using Npgsql;

namespace AuraLearn.Infrastructure.Configuration;

/// <summary>
/// Connection string resuelta junto con el nombre de la variable de la que salió.
/// <see cref="Source"/> es solo el nombre (nunca el valor): se puede registrar en
/// el log de arranque sin filtrar credenciales.
/// </summary>
public sealed record ResolvedConnectionString(string Value, string Source);

/// <summary>
/// Resuelve la conexión a PostgreSQL desde las variables estándar del proveedor
/// en lugar de exigir una copia propia de la cadena.
///
/// La integración Neon ↔ Vercel inyecta <c>DATABASE_URL</c>, <c>DATABASE_URL_UNPOOLED</c>,
/// <c>PGHOST</c>, <c>PGUSER</c>, <c>PGDATABASE</c>, <c>PGPASSWORD</c> y los alias
/// <c>POSTGRES_*</c> de las plantillas antiguas. Npgsql solo entiende
/// <c>keyword=value;</c> y de esas variables solo lee <c>PGUSER</c>/<c>PGPASSWORD</c>,
/// así que aquí se normaliza todo a un único formato.
///
/// Prioridad (la primera que exista gana):
/// <list type="number">
///   <item><c>ConnectionStrings:Default</c> — override explícito (dev local, compose, CI).</item>
///   <item>Modo migraciones (<c>ApplyMigrations=true</c>): <c>DATABASE_URL_UNPOOLED</c>,
///         <c>POSTGRES_URL_NON_POOLING</c>, <c>PGHOST_UNPOOLED</c>+<c>PG*</c>.
///         El DDL a través de PgBouncer en transaction mode es frágil.</item>
///   <item><c>DATABASE_URL</c> (pooled), <c>POSTGRES_URL</c>.</item>
///   <item><c>PGHOST</c> + <c>PGUSER</c> + <c>PGDATABASE</c> + <c>PGPASSWORD</c>.</item>
/// </list>
/// </summary>
public static class DatabaseConnectionString
{
    /// <summary>Override explícito (<c>ConnectionStrings__Default</c> en entorno).</summary>
    public const string ExplicitSource = "ConnectionStrings:Default";

    /// <summary>Origen pooled recomendado por Neon para el runtime.</summary>
    public const string DatabaseUrlSource = "DATABASE_URL";

    /// <summary>Alias pooled de las plantillas Vercel Postgres.</summary>
    public const string PostgresUrlSource = "POSTGRES_URL";

    /// <summary>Origen directo (sin PgBouncer) para migraciones.</summary>
    public const string DatabaseUrlUnpooledSource = "DATABASE_URL_UNPOOLED";

    /// <summary>Alias directo de las plantillas Vercel Postgres.</summary>
    public const string PostgresUrlNonPoolingSource = "POSTGRES_URL_NON_POOLING";

    private const int DefaultPort = 5432;

    /// <summary>
    /// Resuelve la conexión. <paramref name="read"/> abstrae el lector de
    /// configuración (variables de entorno incluidas) para poder probarlo sin
    /// arrancar la aplicación.
    /// </summary>
    /// <param name="explicitConnectionString">Valor de <c>ConnectionStrings:Default</c>, si existe.</param>
    /// <param name="preferUnpooled">true en modo migraciones (evita PgBouncer).</param>
    /// <param name="read">Lector de configuración por clave.</param>
    public static ResolvedConnectionString Resolve(
        string? explicitConnectionString,
        bool preferUnpooled,
        Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);

        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
            return Build(explicitConnectionString, ExplicitSource);

        if (preferUnpooled)
        {
            var directUrl = FirstUrl(read, DatabaseUrlUnpooledSource, PostgresUrlNonPoolingSource);
            if (directUrl is not null)
                return directUrl;

            var directPg = FromPgVariables(read, "PGHOST_UNPOOLED", "PGHOST_UNPOOLED/PGUSER/PGDATABASE");
            if (directPg is not null)
                return directPg;
        }

        var pooledUrl = FirstUrl(read, DatabaseUrlSource, PostgresUrlSource);
        if (pooledUrl is not null)
            return pooledUrl;

        var pg = FromPgVariables(read, "PGHOST", "PGHOST/PGUSER/PGDATABASE");
        if (pg is not null)
            return pg;

        // Falla rápido y sin valores: un mensaje accionable en vez de un 502 opaco.
        throw new InvalidOperationException(
            "No hay conexión a PostgreSQL configurada. Define ConnectionStrings__Default " +
            "o deja que el proveedor la inyecte: DATABASE_URL (preferida), POSTGRES_URL " +
            "o PGHOST + PGUSER + PGDATABASE + PGPASSWORD.");
    }

    /// <summary>
    /// Normaliza una cadena ya existente a formato <c>keyword=value</c>.
    /// Acepta URIs <c>postgresql://</c> / <c>postgres://</c> (formato de Neon) y
    /// cadenas Npgsql clásicas; en ambos casos el resultado es válido para Npgsql.
    /// </summary>
    public static string Normalize(string connectionString, string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var trimmed = connectionString.Trim();

        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return FromUri(trimmed, source);

        try
        {
            return new NpgsqlConnectionStringBuilder(trimmed).ConnectionString;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            // No se propaga el mensaje del builder: puede contener el valor original.
            throw new InvalidOperationException(
                $"La conexión definida en {source} no es válida: se espera una URI " +
                "postgresql:// o el formato keyword=value.", ex);
        }
    }

    private static ResolvedConnectionString Build(string raw, string source) =>
        new(Normalize(raw, source), source);

    private static ResolvedConnectionString? FirstUrl(Func<string, string?> read, params string[] sources)
    {
        foreach (var source in sources)
        {
            var value = read(source);
            if (!string.IsNullOrWhiteSpace(value))
                return Build(value, source);
        }

        return null;
    }

    /// <summary>Compone la cadena desde las variables sueltas de libpq.</summary>
    private static ResolvedConnectionString? FromPgVariables(
        Func<string, string?> read,
        string hostVariable,
        string source)
    {
        var host = read(hostVariable)?.Trim();
        var user = read("PGUSER")?.Trim();
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user))
            return null;

        var database = read("PGDATABASE")?.Trim();

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(read("PGPORT")?.Trim(), out var port) && port > 0 ? port : DefaultPort,
            Database = string.IsNullOrEmpty(database) ? user : database,
            Username = user,
        };

        // La contraseña no se recorta: puede contener espacios significativos.
        var password = read("PGPASSWORD");
        if (!string.IsNullOrEmpty(password))
            builder.Password = password;

        // Neon exige TLS; en local (o compose) el default (Prefer) sigue funcionando.
        if (RequiresTls(host))
            builder.SslMode = SslMode.Require;

        return new ResolvedConnectionString(builder.ConnectionString, source);
    }

    private static bool RequiresTls(string host) =>
        host.Contains("neon.tech", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Convierte <c>postgresql://user:pass@host:5432/db?sslmode=require</c> en
    /// <c>Host=…;Port=…;Database=…;Username=…;Password=…</c>.
    /// El parseo es manual (no <see cref="Uri.UserInfo"/>) para controlar el
    /// unescape de la contraseña y los hosts IPv6 entre corchetes.
    /// </summary>
    private static string FromUri(string raw, string source)
    {
        var schemeEnd = raw.IndexOf("://", StringComparison.Ordinal);
        var rest = raw[(schemeEnd + 3)..];

        var queryIndex = rest.IndexOf('?');
        var query = queryIndex < 0 ? string.Empty : rest[(queryIndex + 1)..];
        if (queryIndex >= 0)
            rest = rest[..queryIndex];

        var slashIndex = rest.IndexOf('/');
        var authority = slashIndex < 0 ? rest : rest[..slashIndex];
        var database = slashIndex < 0 ? string.Empty : rest[(slashIndex + 1)..];

        var atIndex = authority.LastIndexOf('@');
        var userInfo = atIndex < 0 ? string.Empty : authority[..atIndex];
        var hostPort = atIndex < 0 ? authority : authority[(atIndex + 1)..];

        var colonIndex = userInfo.IndexOf(':');
        var user = colonIndex < 0 ? userInfo : userInfo[..colonIndex];
        var password = colonIndex < 0 ? string.Empty : userInfo[(colonIndex + 1)..];

        var (host, port) = SplitHostPort(hostPort);

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user))
            throw new InvalidOperationException(
                $"La conexión definida en {source} no es válida: la URI debe incluir " +
                "usuario y host (postgresql://usuario:contraseña@host:puerto/base).");

        var username = Uri.UnescapeDataString(user);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port > 0 ? port : DefaultPort,
            Username = username,
            Database = string.IsNullOrEmpty(database)
                ? username
                : Uri.UnescapeDataString(database),
        };

        if (!string.IsNullOrEmpty(password))
            builder.Password = Uri.UnescapeDataString(password);

        ApplyQueryParameters(builder, query);
        return builder.ConnectionString;
    }

    private static (string Host, int Port) SplitHostPort(string hostPort)
    {
        if (hostPort.StartsWith('['))
        {
            // IPv6: [::1]:5432
            var close = hostPort.IndexOf(']');
            if (close < 0)
                return (hostPort, 0);

            var after = hostPort[(close + 1)..];
            var ipv6Port = after.StartsWith(':') && int.TryParse(after[1..], out var parsed) ? parsed : 0;
            return (hostPort[1..close], ipv6Port);
        }

        var lastColon = hostPort.LastIndexOf(':');
        if (lastColon <= 0)
            return (hostPort, 0);

        return int.TryParse(hostPort[(lastColon + 1)..], out var port)
            ? (hostPort[..lastColon], port)
            : (hostPort, 0);
    }

    /// <summary>
    /// Mapea los parámetros de la query de la URI a parámetros Npgsql.
    /// Los desconocidos (<c>pgbouncer=true</c>, <c>supa=…</c>, …) se ignoran a
    /// propósito: son marcas del proveedor, no opciones de conexión.
    /// </summary>
    private static void ApplyQueryParameters(NpgsqlConnectionStringBuilder builder, string query)
    {
        if (string.IsNullOrEmpty(query))
            return;

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator <= 0)
                continue;

            // ssl_mode / channel-bindig → sslmode / channelbinding
            var key = pair[..separator].Replace("_", string.Empty).Replace("-", string.Empty);
            var value = Uri.UnescapeDataString(pair[(separator + 1)..]);

            switch (key.ToLowerInvariant())
            {
                case "sslmode":
                    if (TryParseSslMode(value, out var sslMode))
                        builder.SslMode = sslMode;
                    break;

                case "channelbinding":
                    // Npgsql 10 expone el enum como Npgsql.ChannelBinding
                    // (ChannelBindingPolicy en versiones 8–9).
                    if (TryParseChannelBinding(value, out var policy))
                        builder.ChannelBinding = policy;
                    break;

                case "connecttimeout":
                    if (int.TryParse(value, out var timeout) && timeout > 0)
                        builder.Timeout = timeout;
                    break;

                case "applicationname":
                    if (!string.IsNullOrWhiteSpace(value))
                        builder.ApplicationName = value;
                    break;

                case "options":
                    if (!string.IsNullOrWhiteSpace(value))
                        builder.Options = value;
                    break;

                case "searchpath":
                    if (!string.IsNullOrWhiteSpace(value))
                        builder.SearchPath = value;
                    break;
            }
        }
    }

    private static bool TryParseSslMode(string value, out SslMode mode) =>
        Enum.TryParse(value.Replace("-", string.Empty).Replace("_", string.Empty), true, out mode);

    private static bool TryParseChannelBinding(string value, out ChannelBinding policy) =>
        Enum.TryParse(value.Replace("-", string.Empty).Replace("_", string.Empty), true, out policy);
}
