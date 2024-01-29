using Asp.Versioning.Builder;
using HealthChecks.UI.Client;

namespace Beseler.API.Application;

internal static class ApplicationEndpoints
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app, ApiVersionSet versions)
    {
        app.MapHealthChecks("_health", new()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapGet("_ping", () => TypedResults.Text("pong"))
            .WithApiVersionSet(versions)
            .IsApiVersionNeutral()
            .ExcludeFromDescription();

        app.MapGet("coffee", () => TypedResults.Text("I'm a teapot!", statusCode: 418))
            .WithApiVersionSet(versions)
            .IsApiVersionNeutral()
            .ExcludeFromDescription();

        return app;
    }
}
