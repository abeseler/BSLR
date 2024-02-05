namespace Beseler.Shared.Accounts.Responses;

public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresOn);
