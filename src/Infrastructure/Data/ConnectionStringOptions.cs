namespace Beseler.Infrastructure.Data;

internal sealed class ConnectionStringOptions : IConfigurationSection
{
    public static string SectionName => "ConnectionStrings";

    public required string Database { get; set; }
}
