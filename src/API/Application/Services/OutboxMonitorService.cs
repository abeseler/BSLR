using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Models;
using Beseler.Infrastructure.Data.Repositories;

namespace Beseler.API.Application.Services;

internal sealed class OutboxMonitorService(IServiceProvider services, OutboxRepository repository, ILogger<OutboxMonitorService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var messages = await repository.GetAllAsync(stoppingToken);
                foreach (var message in messages)
                {
                    await ProcessMessage(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {ServiceName}", nameof(OutboxMonitorService));
            }
        }
    }

    public override Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {Service}: {ServiceId}", nameof(OutboxMonitorService), OutboxRepository.ServiceId);
        return base.StartAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping {Service}: {ServiceId}", nameof(OutboxMonitorService), OutboxRepository.ServiceId);
        return base.StopAsync(stoppingToken);
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        var handlers = scope.ServiceProvider.GetKeyedServices<IDomainEventHandler>(message.MessageType);
        var tasks = handlers.Select(h => h.HandleAsync(message.Payload, stoppingToken));
        try
        {
            await TaskExt.WhenAll(tasks);
            await repository.DeleteAsync(message, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred processing message: {MessageId}", message.OutboxMessageId);
            var updated = message with { RetriesRemaining = message.RetriesRemaining - 1 };
            await repository.UpdateAsync(updated, stoppingToken);
        }
    }
}
