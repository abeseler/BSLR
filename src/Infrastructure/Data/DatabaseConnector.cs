using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace Beseler.Infrastructure.Data;

internal sealed class DatabaseConnector(IOptionsMonitor<ConnectionStringOptions> connectionStrings) : IDatabaseConnector
{
    public async Task<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(connectionStrings.CurrentValue.Database);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
