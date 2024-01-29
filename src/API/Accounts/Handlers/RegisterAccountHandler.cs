using Beseler.API.Accounts.Requests;
using Beseler.Domain.Accounts;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Beseler.API.Accounts.Handlers;

public static class RegisterAccountHandler
{
    public static async Task<IResult> HandleAsync(
        RegisterAccountRequest request,
        IValidator<RegisterAccountRequest> validator,
        IPasswordHasher<Account> passwordHasher,
        IAccountRepository repository,
        CancellationToken stoppingToken)
    {
        if (await validator.ValidateAsync(request, stoppingToken) is { IsValid: false } validationResult)
        {
            return TypedResults.BadRequest(validationResult.ToDictionary());
        }

        var passwordHash = passwordHasher.HashPassword(default!, request.Password);
        var account = Account.Create(request.Email, request.GivenName, request.FamilyName, passwordHash);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await repository.SaveAsync(account, stoppingToken);
        transactionScope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => TypedResults.UnprocessableEntity(error.Message));

    }
}
