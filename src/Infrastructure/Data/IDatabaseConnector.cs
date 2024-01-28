using System.Data;

namespace Beseler.Infrastructure.Data;

public interface IDatabaseConnector
{
    Task<IDbConnection> ConnectAsync(CancellationToken stoppingToken = default);
}
