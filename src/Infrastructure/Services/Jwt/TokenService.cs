using Beseler.Domain.Accounts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Token = (System.Guid Id, System.DateTime ExpiresOn, string Token);

namespace Beseler.Infrastructure.Services.Jwt;

public sealed class TokenService
{
    private readonly SymmetricSecurityKey _symmetricSecurityKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JwtSecurityTokenHandler _handler;
    private readonly JwtOptions _options;

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

    public ClaimsPrincipal? Validate(string token) => _handler.ValidateToken(token, _validationParameters, out _);

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

    private string WriteToken(List<Claim> claims, DateTime expiresOn)
    {
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresOn,
            signingCredentials: _signingCredentials);

        return _handler.WriteToken(token);
    }

    private static List<Claim> GetDefaultClaims(Account account, Guid tokenId)
    {
        return
        [
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
            new(CustomClaimTypes.EmailVerified, account.IsVerified.ToString())
        ];
    }
}
