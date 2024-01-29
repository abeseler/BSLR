using Beseler.API.Application.Services;
using Beseler.API.Swagger;
using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

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

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddTransient<IPasswordHasher<Account>, PasswordHasher<Account>>();

        builder.Services.AddHostedService<OutboxService>();

        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        return builder;
    }
}
