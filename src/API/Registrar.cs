using Beseler.API.Swagger;
using Beseler.Infrastructure.Data;

namespace Beseler.API;

public static class Registrar
{
    public static WebApplicationBuilder AddAPIServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "")
            .Filter.ByExcluding("RequestPath = '/_health'")
            .Filter.ByExcluding("RequestPath = '/_ping'")
            .Filter.ByExcluding("RequestPath = '/coffee'")
            .Filter.ByExcluding("RequestPath like '/_swagger%'")
            .Filter.ByExcluding("RequestPath like '/_framework%'")
            .Enrich.FromLogContext());

        builder.Services.AddSwaggerWithVersioning();
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        return builder;
    }
}
