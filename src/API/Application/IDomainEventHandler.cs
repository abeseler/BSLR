namespace Beseler.API.Application;

internal interface IDomainEventHandler
{
    Task HandleAsync(string payload, CancellationToken stoppingToken = default);
}
