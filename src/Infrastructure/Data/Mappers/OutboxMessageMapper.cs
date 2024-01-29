using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Models;
using System.Text.Json;

namespace Beseler.Infrastructure.Data.Mappers;

internal static class OutboxMessageMapper
{
    public static OutboxMessage ToOutboxMessage(this DomainEvent domainEvent)
    {
        return new()
        {
            OutboxMessageId = Guid.NewGuid(),
            MessageType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            CreatedOn = DateTime.UtcNow,
            RetriesRemaining = 3
        };
    }
}
