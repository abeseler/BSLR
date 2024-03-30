using Beseler.API.Accounts.EventHandlers;
using Beseler.API.Swagger;
using Beseler.Domain.Accounts;
using Beseler.Domain.Common;
using Beseler.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog.Core;

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
            .Filter.ByExcluding("RequestPath like '/_swagger%'")
            .Filter.ByExcluding("RequestPath like '/_framework%'")
            .Enrich.FromLogContext());

        builder.Services
            .AddHttpContextAccessor()
            .AddValidatorsFromAssemblyContaining<Program>()
            .AddSingleton<ILogEventEnricher, HttpContextLogEnricher>()
            .AddTransient<IPasswordHasher<Account>, PasswordHasher<Account>>()
            .AddScoped<ETagConditions>()
            .AddScoped<CookieService>()
            .AddDomainEventHandler<SendVerificationEmailWhenAccountCreatedHandler, AccountCreatedDomainEvent>()
            .AddDomainEventHandler<SendAccountLockedEmailWhenAccountLockedHandler, AccountLockedDomainEvent>();

        builder.Services
            .AddSwaggerWithVersioning()
            .AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

        builder.Services
            .AddHostedService<OutboxMonitorService>()
            .AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        return builder;
    }

    private static IServiceCollection AddDomainEventHandler<THandler, TEvent>(this IServiceCollection services) where THandler : class, IDomainEventHandler where TEvent : DomainEvent
    {
        services.AddKeyedScoped<IDomainEventHandler, THandler>(typeof(TEvent).Name);
        return services;
    }
}