using AuraLearn.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuraLearn.Api.HealthChecks;

/// <summary>
/// Readiness de la base de datos: comprueba que EF Core puede abrir conexión
/// contra PostgreSQL. Sin esto, <c>/health</c> respondía Healthy aunque Neon
/// estuviera caído o la connection string fuera inválida (solo probaba que el
/// proceso estuviera vivo).
///
/// Se implementa a mano con <c>CanConnectAsync</c> para no añadir el paquete
/// AspNetCore.HealthChecks.NpgSql: es una sola llamada y EF ya está en el grafo.
///
/// ⚠️ Efecto buscado: en Neon (auto-suspend) este check despierta la BD, así
/// que un monitor externo que lo sondee cada minuto evita el cold start. Por eso
/// el check va etiquetado como <c>ready</c> y la ruta de liveness no lo ejecuta.
/// </summary>
public sealed class DatabaseHealthCheck(AuraLearnDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL accesible")
                : HealthCheckResult.Unhealthy("PostgreSQL inaccesible");
        }
        catch (Exception ex)
        {
            // El mensaje va al reporte; nunca se registra la connection string.
            return HealthCheckResult.Unhealthy("Error al conectar con PostgreSQL", ex);
        }
    }
}
