namespace Beseler.Shared.Accounts.Requests;

public sealed record RegisterAccountRequest(string Email, string GivenName, string FamilyName, string Password);
