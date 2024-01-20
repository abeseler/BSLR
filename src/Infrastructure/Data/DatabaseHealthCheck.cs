using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Beseler.Infrastructure.Data;

public sealed class DatabaseHealthCheck(IDatabaseConnector dbConnector) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await dbConnector.ConnectAsync(cancellationToken);
            await connection.QueryAsync("SELECT 1");
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
