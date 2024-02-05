using Beseler.Shared.Accounts;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.Shared;

public static class Policies
{
    public static AuthorizationPolicy EmailVerified =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(PrivateClaims.EmailVerified)
            .Build();

    public static Action<AuthorizationOptions> AuthorizationOptions => 
        options =>
        {
            options.AddPolicy(nameof(EmailVerified), EmailVerified);
        };
}
