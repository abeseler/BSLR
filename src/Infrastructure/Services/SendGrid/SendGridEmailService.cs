using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Beseler.Infrastructure.Services.SendGrid;

internal sealed class SendGridEmailService(IOptions<SendGridOptions> options, ISendGridClient client, ILogger<SendGridEmailService> logger) : IEmailService
{
    public async Task SendAsync(EmailMessage message, CancellationToken stoppingToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            logger.LogWarning("SendGrid ApiKey is not set");
            return;
        }

        var emailMessage = new SendGridMessage
        {
            From = new EmailAddress(options.Value.SenderEmail, options.Value.SenderName),
            Subject = message.Subject,
            PlainTextContent = message.Body,
            HtmlContent = message.BodyHtml
        };
        emailMessage.AddTo(new EmailAddress(message.ToEmail, message.ToName));

        if ((await client.SendEmailAsync(emailMessage, stoppingToken)) is { IsSuccessStatusCode: false } response)
        {
            var responseBody = await response.Body.ReadAsStringAsync(stoppingToken);
            logger.LogError("Failed to send email: {Response}", responseBody);
            throw new InvalidOperationException("Failed to send email");
        }
    }
}
