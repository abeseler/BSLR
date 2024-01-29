using Beseler.Infrastructure.Data.Models;
using Dapper;
using System.Data;

namespace Beseler.Infrastructure.Data.Repositories;

internal sealed class OutboxRepository(IDatabaseConnector connector)
{
    public async Task InsertAllAsync(IEnumerable<OutboxMessage> messages, CancellationToken stoppingToken = default)
    {
        var dt = new DataTable();
        dt.Columns.Add("OutboxMessageId");
        dt.Columns.Add("MessageType");
        dt.Columns.Add("Payload");
        dt.Columns.Add("CreatedOn");
        dt.Columns.Add("RetriesRemaining");

        foreach (var message in messages)
        {
            dt.Rows.Add(message.OutboxMessageId, message.MessageType, message.Payload, message.CreatedOn, message.RetriesRemaining);
        }

        var parameters = new DynamicParameters();
        parameters.Add("Messages", dt.AsTableValuedParameter("dbo.TVP_OutboxMessage"));

        using var connection = await connector.ConnectAsync(stoppingToken);
        _ = await connection.ExecuteAsync("""
            INSERT INTO dbo.OutboxMessage (
                OutboxMessageId,
                MessageType,
                Payload,
                CreatedOn,
                RetriesRemaining)
            SELECT
                m.OutboxMessageId,
                m.MessageType,
                m.Payload,
                m.CreatedOn,
                m.RetriesRemaining
            FROM @Messages m
            WHERE NOT EXISTS (SELECT 1 FROM dbo.OutboxMessage om WHERE om.OutboxMessageId = m.OutboxMessageId);
            """, parameters);
    }
}
