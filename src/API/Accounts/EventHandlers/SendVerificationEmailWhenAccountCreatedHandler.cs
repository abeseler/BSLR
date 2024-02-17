using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Services;
using Beseler.Infrastructure.Services.Jwt;

namespace Beseler.API.Accounts.EventHandlers;

internal sealed class SendVerificationEmailWhenAccountCreatedHandler(TokenService tokenService, IAccountRepository repository, IEmailService emailService) : IDomainEventHandler
{
    public async Task HandleAsync(string payload, CancellationToken stoppingToken = default)
    {
        var email = JsonSerializer.Deserialize<AccountCreatedDomainEvent>(payload)?.Email
            ?? throw new InvalidOperationException($"Domain event is missing email: {payload}");

        var account = await repository.GetByEmailAsync(email, stoppingToken)
            ?? throw new InvalidOperationException($"Account not found: {email}");

        var token = tokenService.GenerateToken(account, TimeSpan.FromMinutes(10), [AppClaims.ConfirmEmailClaim(tokenService.Audience, account.Email)]);

        var emailMessage = new EmailMessage
        {
            ToEmail = account.Email,
            ToName = account.GivenName ?? account.Email,
            Subject = "Activate Your Account 🚀",
            Body = $"To confirm your email, navigate to the following url in your browser: {tokenService.Audience}?token={token}",
            BodyHtml = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Welcome to BSLR!</title>
                </head>
                <body style="font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;">
                <div style="max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);">
                  <h2>Welcome to BSLR! 🌟</h2>
                  <p>We're thrilled to have you join our community! To start your journey with us, please click the button below to verify your email address and activate your account:</p>
                  <p style="text-align: center;"><a href="{tokenService.Audience}/account/confirm-email?token={token.Token}" style="display: inline-block; padding: 12px 24px; background-color: #007bff; color: #fff; text-decoration: none; border-radius: 4px;">Verify My Email</a></p>
                  <p>If the button above doesn't work, you can also copy and paste the following link into your browser:</p>
                  <p>{tokenService.Audience}/account/confirm-email?token={token.Token}</p>
                  <p>If you didn't create an account with us, no worries! Just ignore this email, and your information will remain safe.</p>
                  <p>Happy exploring! 🎉</p>
                  <p>Best regards,<br>The BSLR Team</p>
                </div>
                </body>
                </html>
                """
        };

        await emailService.SendAsync(emailMessage, stoppingToken);
    }
}
