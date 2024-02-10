using System.Security.Claims;

namespace Beseler.Shared.Accounts;

public static class PrivateClaims
{
    public const string EmailVerified = "email_verified";
    public static string ConfirmEmail(string audience) => $"{audience}/confirm_email";
    public static string ResetPassword(string audience) => $"{audience}/reset_password";

    public static bool HasClaim(this ClaimsPrincipal principal, string claim) => principal.HasClaim(c => c.Type == claim);
}
