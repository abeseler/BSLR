using Beseler.Infrastructure.Data.Models;
using Dapper;
using System.Data;

namespace Beseler.Infrastructure.Data.Repositories;

public sealed class OutboxRepository(IDatabaseConnector connector)
{
    public static readonly Guid ServiceId = Guid.NewGuid();

    public async Task<OutboxMessage[]> GetAllAsync(CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ServiceId", ServiceId);

        using var connection = await connector.ConnectAsync(stoppingToken);
        var messages = await connection.QueryAsync<OutboxMessage>("""
            SELECT *
            FROM dbo.OutboxMessage WITH (NOLOCK)
            WHERE RetriesRemaining > 0
                AND ServiceId = @ServiceId;
            """, parameters);
        return messages.ToArray();
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(message);
        using var connection = await connector.ConnectAsync(stoppingToken);
        _ = await connection.ExecuteAsync("""
            UPDATE dbo.OutboxMessage
            SET RetriesRemaining = @RetriesRemaining
            WHERE OutboxMessageId = @OutboxMessageId;
            """, parameters);
    }

    public async Task DeleteAsync(OutboxMessage message, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(message);
        using var connection = await connector.ConnectAsync(stoppingToken);
        _ = await connection.ExecuteAsync("DELETE FROM dbo.OutboxMessage WHERE OutboxMessageId = @OutboxMessageId", parameters);
    }

    public async Task InsertAllAsync(IEnumerable<OutboxMessage> messages, CancellationToken stoppingToken = default)
    {
        var dt = new DataTable();
        dt.Columns.Add("ServiceId");
        dt.Columns.Add("MessageType");
        dt.Columns.Add("Payload");
        dt.Columns.Add("CreatedOn");
        dt.Columns.Add("RetriesRemaining");

        foreach (var message in messages)
        {
            dt.Rows.Add(ServiceId, message.MessageType, message.Payload, message.CreatedOn, message.RetriesRemaining);
        }

        var parameters = new DynamicParameters();
        parameters.Add("Messages", dt.AsTableValuedParameter("dbo.TVP_OutboxMessage"));

        using var connection = await connector.ConnectAsync(stoppingToken);
        _ = await connection.ExecuteAsync("""
            INSERT INTO dbo.OutboxMessage (
                ServiceId,
                MessageType,
                Payload,
                CreatedOn,
                RetriesRemaining)
            SELECT
                m.ServiceId,
                m.MessageType,
                m.Payload,
                m.CreatedOn,
                m.RetriesRemaining
            FROM @Messages m;
            """, parameters);
    }
}
