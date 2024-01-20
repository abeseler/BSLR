using System.Data;

namespace Beseler.Infrastructure.Data;

public interface IDatabaseConnector
{
    public Task<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default);
}
