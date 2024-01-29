using Asp.Versioning.Builder;
using Beseler.API.Accounts.Handlers;

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

        group.MapPost("register", RegisterAccountHandler.HandleAsync);

        return app;
    }
}
