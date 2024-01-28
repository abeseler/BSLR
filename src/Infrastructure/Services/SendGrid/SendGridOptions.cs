namespace Beseler.Infrastructure.Services.SendGrid;

internal sealed class SendGridOptions : IConfigurationSection
{
    public static string SectionName => "SendGrid";

    public string? ApiKey { get; set; }
}
