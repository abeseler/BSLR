using Beseler.API.Application;
using Beseler.Domain.Accounts.Events;
using Beseler.Infrastructure.Services;

namespace Beseler.API.Accounts.EventConsumers;

internal sealed class SendAccountLockedEmailWhenAccountLockedConsumer(IEmailService emailService) : IEventConsumer
{
    public async Task ConsumeAsync(string eventData, CancellationToken stoppingToken = default)
    {
        var dto = JsonSerializer.Deserialize<AccountLockedDomainEvent>(eventData);
        if (dto?.Email is null)
            throw new InvalidOperationException($"Domain event is missing data: {eventData}");

        var emailMessage = new EmailMessage
        {
            ToEmail = dto.Email,
            ToName = dto.Email,
            Subject = "Your Account Has Been Locked",
            Body = $"Your account has been locked. Reason: {dto.Reason} Please contact support for assistance.",
            BodyHtml = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Your Account Has Been Locked</title>
                </head>
                <body style="font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;">
                <div style="max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);">
                  <h2>Your Account Has Been Locked</h2>
                  <p>Your account has been locked for the following reason:</p>
                  <p>{dto.Reason}</p>
                  <p>Best regards,<br>The BSLR Team</p>
                </div>
                </body>
                </html>
                """
        };

        await emailService.SendAsync(emailMessage, stoppingToken);
    }
}
