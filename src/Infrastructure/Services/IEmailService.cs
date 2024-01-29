namespace Beseler.Infrastructure.Services;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken stoppingToken = default);
}

public sealed record EmailMessage
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required string BodyHtml { get; init; }
}