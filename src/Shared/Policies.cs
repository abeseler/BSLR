﻿using Beseler.Shared.Accounts;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.Shared;

public static class Policies
{
    public const string EmailVerified = nameof(EmailVerified);

    private static AuthorizationPolicy EmailVerifiedPolicy =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(PrivateClaims.EmailVerified)
            .Build();

    public static Action<AuthorizationOptions> AuthorizationOptions =>
        options =>
        {
            options.AddPolicy(EmailVerified, EmailVerifiedPolicy);
        };
}
