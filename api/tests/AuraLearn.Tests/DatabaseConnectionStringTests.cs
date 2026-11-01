using AuraLearn.Infrastructure.Configuration;
using Npgsql;

namespace AuraLearn.Tests;

/// <summary>
/// La cadena de conexión debe salir de las variables que inyecta la integración
/// Neon ↔ Vercel, normalizada a <c>keyword=value</c> (Npgsql no acepta URIs).
/// </summary>
public class DatabaseConnectionStringTests
{
    private static Func<string, string?> Env(params (string Key, string? Value)[] values) =>
        key =>
        {
            foreach (var (name, value) in values)
            {
                if (name == key)
                    return value;
            }

            return null;
        };

    [Fact]
    public void Resolve_prioriza_ConnectionStrings_Default_sobre_las_variables_del_proveedor()
    {
        var result = DatabaseConnectionString.Resolve(
            "Host=localhost;Database=auralearn_dev;Username=auralearn;Password=auralearn_dev",
            preferUnpooled: false,
            read: Env(("DATABASE_URL", "postgresql://neon:pw@ep-x.neon.tech/neondb")));

        Assert.Equal(DatabaseConnectionString.ExplicitSource, result.Source);
        Assert.Equal("localhost", new NpgsqlConnectionStringBuilder(result.Value).Host);
    }

    [Fact]
    public void Resolve_convierte_la_URI_pooled_de_Neon_a_keyword_value()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: false,
            read: Env(("DATABASE_URL",
                "postgresql://auralearn:p%40ss%3Bword@ep-cool-1234-pooler.us-east-2.aws.neon.tech/neondb" +
                "?sslmode=require&channel_binding=require&pgbouncer=true")));

        Assert.Equal(DatabaseConnectionString.DatabaseUrlSource, result.Source);

        var builder = new NpgsqlConnectionStringBuilder(result.Value);
        Assert.Equal("ep-cool-1234-pooler.us-east-2.aws.neon.tech", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("neondb", builder.Database);
        Assert.Equal("auralearn", builder.Username);
        Assert.Equal("p@ss;word", builder.Password);
        Assert.Equal(SslMode.Require, builder.SslMode);
        Assert.Equal(ChannelBinding.Require, builder.ChannelBinding);
    }

    [Fact]
    public void Resolve_usa_POSTGRES_URL_cuando_falta_DATABASE_URL()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: false,
            read: Env(("POSTGRES_URL", "postgresql://user:pw@ep-y.neon.tech/db")));

        Assert.Equal(DatabaseConnectionString.PostgresUrlSource, result.Source);
        Assert.Equal("ep-y.neon.tech", new NpgsqlConnectionStringBuilder(result.Value).Host);
    }

    [Fact]
    public void Resolve_compone_la_cadena_desde_las_variables_PG_cuando_no_hay_URL()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: false,
            read: Env(
                ("PGHOST", "ep-pg.neon.tech"),
                ("PGUSER", "auralearn"),
                ("PGDATABASE", "neondb"),
                ("PGPASSWORD", "secreto")));

        Assert.Equal("PGHOST/PGUSER/PGDATABASE", result.Source);

        var builder = new NpgsqlConnectionStringBuilder(result.Value);
        Assert.Equal("ep-pg.neon.tech", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("neondb", builder.Database);
        Assert.Equal("auralearn", builder.Username);
        Assert.Equal("secreto", builder.Password);
        // Neon exige TLS: se activa solo, sin pedir sslmode en la variable.
        Assert.Equal(SslMode.Require, builder.SslMode);
    }

    [Fact]
    public void Resolve_no_fuerza_TLS_cuando_el_host_no_es_Neon()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: false,
            read: Env(
                ("PGHOST", "localhost"),
                ("PGPORT", "5433"),
                ("PGUSER", "auralearn"),
                ("PGDATABASE", "auralearn_dev"),
                ("PGPASSWORD", "auralearn_dev")));

        var builder = new NpgsqlConnectionStringBuilder(result.Value);
        Assert.Equal(5433, builder.Port);
        Assert.Equal(SslMode.Prefer, builder.SslMode);
    }

    [Fact]
    public void Resolve_en_modo_migraciones_prefiere_la_URL_directa()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: true,
            read: Env(
                ("DATABASE_URL", "postgresql://user:pw@ep-x-pooler.neon.tech/neondb"),
                ("DATABASE_URL_UNPOOLED", "postgresql://user:pw@ep-x.neon.tech/neondb")));

        Assert.Equal(DatabaseConnectionString.DatabaseUrlUnpooledSource, result.Source);
        Assert.Equal("ep-x.neon.tech", new NpgsqlConnectionStringBuilder(result.Value).Host);
    }

    [Fact]
    public void Resolve_en_modo_migraciones_acepta_el_alias_POSTGRES_URL_NON_POOLING()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: true,
            read: Env(("POSTGRES_URL_NON_POOLING", "postgresql://user:pw@ep-z.neon.tech/db")));

        Assert.Equal(DatabaseConnectionString.PostgresUrlNonPoolingSource, result.Source);
    }

    [Fact]
    public void Resolve_en_modo_migraciones_usa_PGHOST_UNPOOLED()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: true,
            read: Env(
                ("PGHOST", "ep-pooler.neon.tech"),
                ("PGHOST_UNPOOLED", "ep-direct.neon.tech"),
                ("PGUSER", "auralearn"),
                ("PGPASSWORD", "secreto")));

        Assert.Equal("PGHOST_UNPOOLED/PGUSER/PGDATABASE", result.Source);

        var builder = new NpgsqlConnectionStringBuilder(result.Value);
        Assert.Equal("ep-direct.neon.tech", builder.Host);
        // Sin PGDATABASE se usa el nombre del usuario (comportamiento de Postgres).
        Assert.Equal("auralearn", builder.Database);
    }

    [Fact]
    public void Resolve_en_modo_migraciones_cae_a_la_pooled_si_no_hay_directa()
    {
        var result = DatabaseConnectionString.Resolve(
            null,
            preferUnpooled: true,
            read: Env(("DATABASE_URL", "postgresql://user:pw@ep-x-pooler.neon.tech/neondb")));

        Assert.Equal(DatabaseConnectionString.DatabaseUrlSource, result.Source);
    }

    [Fact]
    public void Resolve_falla_rapido_si_no_hay_ninguna_fuente()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConnectionString.Resolve(null, preferUnpooled: false, read: Env()));

        Assert.Contains("DATABASE_URL", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__Default", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_acepta_keyword_value_y_respeta_los_caracteres_especiales()
    {
        // Npgsql exige comillas cuando el valor contiene ';' o comillas (que se
        // duplican). El resolver no debe alterar la cadena clásica.
        var result = DatabaseConnectionString.Resolve(
            "Host=localhost;Database=auralearn_dev;Username=auralearn;Password=\"a;b\"\"c\"",
            preferUnpooled: false,
            read: Env());

        var builder = new NpgsqlConnectionStringBuilder(result.Value);
        Assert.Equal("a;b\"c", builder.Password);
    }

    [Fact]
    public void Normalize_ignora_parametros_de_proveedor_desconocidos()
    {
        var value = DatabaseConnectionString.Normalize(
            "postgresql://user:pw@ep-x.neon.tech/db?sslmode=require&supa=base-pooler.x&connect_timeout=20",
            "TEST");

        var builder = new NpgsqlConnectionStringBuilder(value);
        Assert.Equal(SslMode.Require, builder.SslMode);
        Assert.Equal(20, builder.Timeout);
    }

    [Fact]
    public void Normalize_rechaza_una_cadena_invalida_sin_filtrar_el_valor()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConnectionString.Normalize("esto:no:es:una:cadena", "TEST"));

        Assert.DoesNotContain("esto:no:es", exception.Message, StringComparison.Ordinal);
        Assert.Contains("TEST", exception.Message, StringComparison.Ordinal);
    }
}
