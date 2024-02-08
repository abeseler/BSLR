using Microsoft.AspNetCore.Diagnostics;

namespace Beseler.API.Application.Services;

internal static class StatusCodeMiddleware
{
    public static IApplicationBuilder UseStatusCodeMiddleware(this IApplicationBuilder app)
    {
        app.UseStatusCodePagesWithReExecute("/", "?statusCode={0}");
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api")
                || context.Request.Path.StartsWithSegments("/_swagger")
                || context.Request.Path.StartsWithSegments("/_health")
                || context.Request.Path.StartsWithSegments("/_ping"))
            {
                var feature = context.Features.Get<IStatusCodePagesFeature>();
                if (feature is not null)
                    feature.Enabled = false;
            }
            await next.Invoke();
        });
        return app;
    }
}
