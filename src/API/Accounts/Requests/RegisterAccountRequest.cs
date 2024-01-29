namespace Beseler.API.Accounts.Requests;

public sealed record RegisterAccountRequest(string Email, string GivenName, string FamilyName, string Password);
