namespace Beseler.API.Application;

internal interface IEventConsumer
{
    Task ConsumeAsync(string eventData, CancellationToken stoppingToken = default);
}
