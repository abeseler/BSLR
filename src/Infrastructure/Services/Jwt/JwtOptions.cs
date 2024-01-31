namespace Beseler.Infrastructure.Services.Jwt;

public sealed class JwtOptions : IConfigurationSection
{
    public static string SectionName => "Jwt";

    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Key { get; set; }
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeHours { get; set; }
}
