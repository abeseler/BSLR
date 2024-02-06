using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Mappers;
using Beseler.Infrastructure.Data.Repositories;

namespace Beseler.Infrastructure.Data;

internal abstract class AggregateRepository(OutboxRepository repository)
{
    protected virtual async Task SaveAsync(Aggregate model, CancellationToken stoppingToken = default)
    {
        if (model.DomainEvents.Count == 0)
            return;

        var messages = model.DomainEvents.Select(x => x.ToOutboxMessage());
        await repository.InsertAllAsync(messages, stoppingToken);
        model.ClearDomainEvents();
    }
}
