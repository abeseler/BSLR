using Beseler.Infrastructure.Data.Models;
using Beseler.Infrastructure.Data.Repositories;

namespace Beseler.API.Application.Services;

internal sealed class OutboxService(IServiceProvider services, OutboxRepository repository, ILogger<OutboxService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var messages = await repository.GetAllAsync(stoppingToken);
                logger.LogInformation("Outbox messages to process: {MessageCount}", messages.Length);
                foreach (var message in messages)
                {
                    await ProcessMessage(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {ServiceName}", nameof(OutboxService));
            }
        }
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        var consumers = scope.ServiceProvider.GetKeyedServices<IEventConsumer>(message.MessageType);
        try
        {
            foreach (var consumer in consumers)
            {
                await consumer.ConsumeAsync(message.Payload, stoppingToken);
            }

            await repository.DeleteAsync(message, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred processing message {MessageId}", message.OutboxMessageId);
            var updated = message with { RetriesRemaining = message.RetriesRemaining - 1 };
            await repository.UpdateAsync(updated, stoppingToken);
        }
    }
}
