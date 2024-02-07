namespace Beseler.API.Application;

internal interface IEventConsumer
{
    Task ConsumeAsync(string payload, CancellationToken stoppingToken = default);
}
