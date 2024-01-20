using Asp.Versioning.Builder;
using HealthChecks.UI.Client;

namespace Beseler.API.Application;

internal static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app.MapHealthChecks("_health", new()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapGet("_ping", () => TypedResults.Text("pong"))
            .WithApiVersionSet(versionSet)
            .IsApiVersionNeutral()
            .ExcludeFromDescription();

        app.MapGet("coffee", () => TypedResults.Text("I'm a teapot!", statusCode: 418))
            .WithApiVersionSet(versionSet)
            .IsApiVersionNeutral()
            .ExcludeFromDescription();
    }
}
