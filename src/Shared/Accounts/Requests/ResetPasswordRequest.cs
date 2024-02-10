namespace Beseler.Shared.Accounts.Requests;

public sealed record ResetPasswordRequest(string Token, string Password);
