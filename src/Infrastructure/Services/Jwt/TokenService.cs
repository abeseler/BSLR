using Beseler.Domain.Accounts;
using Beseler.Shared.Accounts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Token = (System.Guid Id, System.DateTime ExpiresOn, string Token);

namespace Beseler.Infrastructure.Services.Jwt;

public sealed class TokenService
{
    public const string RefreshTokenCookieKey = "X-Refresh-Token";
    private readonly SymmetricSecurityKey _symmetricSecurityKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JsonWebTokenHandler _handler;
    private readonly JwtOptions _options;
    public string Issuer => _options.Issuer;
    public string Audience => _options.Audience;

    public TokenService(IOptions<JwtOptions> options)
    {
        var key = Encoding.UTF8.GetBytes(options.Value.Key!);
        _options = options.Value;
        _handler = new();
        _handler.InboundClaimTypeMap.Clear();
        _symmetricSecurityKey = new(key);
        _signingCredentials = new(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        _validationParameters = new()
        {
            IssuerSigningKey = _symmetricSecurityKey,
            ValidIssuer = options.Value.Issuer,
            ValidAudience = options.Value.Audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<ClaimsPrincipal?> ValidateAsync(string token)
    {
        var result = await _handler.ValidateTokenAsync(token, _validationParameters);
        if (result.IsValid is false)
            return null;

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }

    public Token GenerateAccessToken(Account account)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = GetDefaultClaims(account, tokenId);
        var token = WriteToken(claims, expiresOn);

        return (tokenId, expiresOn, token);
    }

    public Token GenerateRefreshToken(Account account)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.AddHours(_options.RefreshTokenLifetimeHours);
        var claims = GetDefaultClaims(account, tokenId);
        var token = WriteToken(claims, expiresOn);

        return (tokenId, expiresOn, token);
    }

    public Token GenerateToken(Account account, TimeSpan lifetime, Dictionary<string, string>? additionalClaims = null)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.Add(lifetime);
        var claims = GetDefaultClaims(account, tokenId);

        foreach (var (key, value) in additionalClaims ?? [])
        {
            claims.Add(new(key, value));
        }

        var token = WriteToken(claims, expiresOn);
        return (tokenId, expiresOn, token);
    }

    private string WriteToken(List<Claim> claims, DateTime expiresOn)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = expiresOn,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = _signingCredentials
        };

        return _handler.CreateToken(descriptor);
    }

    private static List<Claim> GetDefaultClaims(Account account, Guid tokenId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(JwtRegisteredClaimNames.Sub, account.AccountId.ToString())
        };

        if (account.IsVerified)
            claims.Add(new(PrivateClaims.EmailVerified, account.IsVerified.ToString()));

        return claims;
    }
}
