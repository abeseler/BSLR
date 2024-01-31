using Beseler.API.Application;
using Beseler.Domain.Accounts;
using Beseler.Domain.Accounts.Events;
using Beseler.Infrastructure.Services;
using Beseler.Infrastructure.Services.Jwt;

namespace Beseler.API.Accounts.EventConsumers;

internal sealed class SendVerificationEmailWhenAccountCreatedConsumer(TokenService tokenService, IAccountRepository repository, IEmailService emailService) : IEventConsumer
{
    public async Task ConsumeAsync(string eventData, CancellationToken stoppingToken = default)
    {
        var email = JsonSerializer.Deserialize<AccountCreatedDomainEvent>(eventData)?.Email
            ?? throw new InvalidOperationException($"Domain event is missing email: {eventData}");

        var account = await repository.GetByEmailAsync(email, stoppingToken)
            ?? throw new InvalidOperationException($"Account not found: {email}");

        var token = tokenService.GenerateRefreshToken(account);

        var emailMessage = new EmailMessage
        {
            ToEmail = account.Email,
            ToName = account.Email,
            Subject = "Confirm email address",
            Body = "Link to confirm email",
            BodyHtml = $"<p>Link to confirm email</p><p>{token}</p>"
        };

        await emailService.SendAsync(emailMessage, stoppingToken);
    }
}
