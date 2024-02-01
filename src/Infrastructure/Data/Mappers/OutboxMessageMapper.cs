using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Models;
using Beseler.Infrastructure.Data.Repositories;
using System.Text.Json;

namespace Beseler.Infrastructure.Data.Mappers;

internal static class OutboxMessageMapper
{
    public static OutboxMessage ToOutboxMessage(this DomainEvent domainEvent)
    {
        return new()
        {
            ServiceId = OutboxRepository.ServiceId,
            MessageType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            CreatedOn = DateTime.UtcNow,
            RetriesRemaining = 3
        };
    }
}
