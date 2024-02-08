using Beseler.API.Accounts.EventConsumers;
using Beseler.API.Application;
using Beseler.API.Application.Services;
using Beseler.API.Swagger;
using Beseler.Domain.Accounts;
using Beseler.Domain.Common;
using Beseler.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog.Core;
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
            .Filter.ByExcluding("RequestPath like '/_swagger%'")
            .Filter.ByExcluding("RequestPath like '/_framework%'")
            .Enrich.FromLogContext());

        builder.Services
            .AddHttpContextAccessor()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddSingleton<ILogEventEnricher, HttpContextLogEnricher>()
            .AddTransient<IPasswordHasher<Account>, PasswordHasher<Account>>()
            .AddScoped<CookieService>()
            .AddDomainEventHandler<SendVerificationEmailWhenAccountCreatedConsumer, AccountCreatedDomainEvent>()
            .AddDomainEventHandler<SendAccountLockedEmailWhenAccountLockedConsumer, AccountLockedDomainEvent>();

        builder.Services
            .AddSwaggerWithVersioning()
            .AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

        builder.Services
            .AddHostedService<OutboxService>()
            .AddRazorComponents().AddInteractiveWebAssemblyComponents();

        return builder;
    }

    private static IServiceCollection AddDomainEventHandler<TConsumer, TEvent>(this IServiceCollection services) where TConsumer : class, IDomainEventHandler where TEvent : DomainEvent
    {
        services.AddKeyedScoped<IDomainEventHandler, TConsumer>(typeof(TEvent).Name);
        return services;
    }
}