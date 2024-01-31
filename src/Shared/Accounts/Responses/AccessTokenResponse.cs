namespace Beseler.Shared.Accounts.Responses;

public sealed record AccessTokenResponse(string TokenType, string AccessToken, DateTimeOffset ExpiresAt, string RefreshToken);
