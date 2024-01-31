using Asp.Versioning.Builder;
using Beseler.API.Accounts.Handlers;
using Beseler.Shared.Accounts.Responses;

namespace Beseler.API.Accounts;

internal static class AccountEndpoints
{
    private const string BaseRoute = "api/accounts";

    public static WebApplication MapAccountEndpoints(this WebApplication app, ApiVersionSet versions)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Accounts")
            .WithApiVersionSet(versions)
            .MapToApiVersion(1);

        group.MapPost("register", RegisterAccountHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        group.MapPost("login", LoginAccountHandler.HandleAsync)
            .Produces<AccessTokenResponse>(200)
            .Produces(400);

        group.MapPost("refresh", RefreshTokenHandler.HandleAsync)
            .Produces<AccessTokenResponse>(200)
            .Produces(400);

        group.MapPost("confirm-email", ConfirmEmailHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        return app;
    }
}
