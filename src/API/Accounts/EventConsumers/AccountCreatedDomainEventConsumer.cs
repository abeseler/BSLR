using Beseler.API.Application;
using Beseler.Domain.Accounts.Events;
using Beseler.Infrastructure.Services;

namespace Beseler.API.Accounts.EventConsumers;

internal sealed class AccountCreatedDomainEventConsumer(IEmailService emailService) : IEventConsumer
{
    public async Task ConsumeAsync(string eventData, CancellationToken stoppingToken = default)
    {
        var domainEvent = JsonSerializer.Deserialize<AccountCreatedDomainEvent>(eventData);
        if (domainEvent?.Email is null)
            throw new InvalidOperationException($"Domain event is missing email: {eventData}");

        var emailMessage = new EmailMessage
        {
            ToEmail = domainEvent.Email,
            ToName = domainEvent.Email,
            Subject = "Confirm email address",
            Body = "Link to confirm email",
            BodyHtml = "<p>Link to confirm email</p>"
        };

        await emailService.SendAsync(emailMessage, stoppingToken);
    }
}
