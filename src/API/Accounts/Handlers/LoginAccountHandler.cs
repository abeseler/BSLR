﻿using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Services.Jwt;
using Beseler.Shared.Accounts.Requests;
using Beseler.Shared.Accounts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Beseler.API.Accounts.Handlers;

internal static class LoginAccountHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
    LoginAccountRequest request,
    TokenService tokenService,
    IPasswordHasher<Account> passwordHasher,
    IAccountRepository repository,
    CancellationToken stoppingToken)
    {
        var account = await repository.GetByEmailAsync(request.Email, stoppingToken);
        if (account is null || account.IsLocked)
            return TypedResults.Unauthorized();

        var verificationResult = passwordHasher.VerifyHashedPassword(account, account.SecretHash ?? "", request.Secret);
        if (verificationResult is not PasswordVerificationResult.Success)
            return TypedResults.Unauthorized();

        var (_, expiresOn, accessToken) = tokenService.GenerateAccessToken(account);
        var (_, _, refreshToken) = tokenService.GenerateRefreshToken(account);

        var response = new AccessTokenResponse("Bearer", accessToken, expiresOn, refreshToken);

        account.Login();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await repository.SaveAsync(account, stoppingToken);
        scope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.Ok(response),
            onFailure: error => TypedResults.Problem(error.Message));
    }
}