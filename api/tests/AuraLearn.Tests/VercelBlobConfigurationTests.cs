using AuraLearn.Infrastructure.Configuration;

namespace AuraLearn.Tests;

/// <summary>
/// Las credenciales de Blob se reutilizan con los nombres estándar de Vercel
/// (<c>BLOB_STORE_ID</c> / <c>BLOB_READ_WRITE_TOKEN</c>), que son también los que
/// usa el frontend para firmar las subidas.
/// </summary>
public class VercelBlobConfigurationTests
{
    private const string StoreId = "BJe6tww3JlHPu28Y";
    private const string Token = "vercel_blob_rw_BJe6tww3JlHPu28Y_Myb6NagCn6M9HVKMdAjfJbRgofNhpE";

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
    public void Resolve_usa_las_variables_estandar_y_normaliza_el_prefijo_store()
    {
        var credentials = VercelBlobConfiguration.Resolve(Env(
            ("BLOB_STORE_ID", "store_BJe6tww3JlHPu28Y"),
            ("BLOB_READ_WRITE_TOKEN", Token)));

        // La URL del store es <id>.private.blob.vercel-storage.com: sin prefijo.
        Assert.Equal(StoreId, credentials.StoreId);
        Assert.Equal(Token, credentials.Token);
        Assert.Equal("private", credentials.Access);
    }

    [Fact]
    public void Resolve_acepta_el_store_id_sin_prefijo()
    {
        var credentials = VercelBlobConfiguration.Resolve(Env(
            ("BLOB_STORE_ID", StoreId),
            ("BLOB_READ_WRITE_TOKEN", Token)));

        Assert.Equal(StoreId, credentials.StoreId);
    }

    [Fact]
    public void Resolve_deriva_el_store_id_del_token_cuando_falta_la_variable()
    {
        var credentials = VercelBlobConfiguration.Resolve(Env(
            ("BLOB_READ_WRITE_TOKEN", Token)));

        Assert.Equal(StoreId, credentials.StoreId);
    }

    [Fact]
    public void Resolve_tolera_tokens_cuyo_store_id_incluye_el_prefijo()
    {
        var credentials = VercelBlobConfiguration.Resolve(Env(
            ("BLOB_READ_WRITE_TOKEN", $"vercel_blob_rw_store_{StoreId}_secreto")));

        Assert.Equal(StoreId, credentials.StoreId);
    }

    [Fact]
    public void Resolve_lee_el_modo_del_store_desde_Storage_VercelBlobAccess()
    {
        var credentials = VercelBlobConfiguration.Resolve(Env(
            ("BLOB_READ_WRITE_TOKEN", Token),
            ("Storage:VercelBlobAccess", "public")));

        Assert.Equal("public", credentials.Access);
    }

    [Fact]
    public void Resolve_falla_si_falta_el_token_aunque_haya_store_id()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VercelBlobConfiguration.Resolve(Env(("BLOB_STORE_ID", "store_BJe6tww3JlHPu28Y"))));

        Assert.Contains("BLOB_READ_WRITE_TOKEN", exception.Message, StringComparison.Ordinal);
        Assert.Contains("VERCEL_OIDC_TOKEN", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_falla_si_el_store_id_no_coincide_con_el_del_token()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VercelBlobConfiguration.Resolve(Env(
                ("BLOB_STORE_ID", "store_OtroStoreDistinto"),
                ("BLOB_READ_WRITE_TOKEN", Token))));

        // Los ids no son secretos: el mensaje los muestra para poder corregirlos.
        Assert.Contains("OtroStoreDistinto", exception.Message, StringComparison.Ordinal);
        Assert.Contains(StoreId, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_falla_si_el_token_no_tiene_el_formato_esperado()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VercelBlobConfiguration.Resolve(Env(("BLOB_READ_WRITE_TOKEN", "token-opaco"))));

        Assert.Contains("vercel_blob_rw_", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StoreIdFromToken_devuelve_null_para_valores_vacios()
    {
        Assert.Null(VercelBlobConfiguration.StoreIdFromToken(string.Empty));
        Assert.Null(VercelBlobConfiguration.StoreIdFromToken("vercel_blob_rw_"));
    }

    [Fact]
    public void ResolveStoreId_compara_ignorando_el_prefijo_store()
    {
        var storeId = VercelBlobConfiguration.ResolveStoreId(Token, $"store_{StoreId}");

        Assert.Equal(StoreId, storeId);
    }
}
