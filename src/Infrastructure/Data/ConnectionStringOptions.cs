namespace Beseler.Infrastructure.Data;

internal sealed class ConnectionStringOptions
{
    public const string SectionName = "ConnectionStrings";

    public required string Database { get; set; }
}
