using Azure.Identity;
using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Data;
using Beseler.Infrastructure.Data.Repositories;
using Beseler.Infrastructure.Services;
using Beseler.Infrastructure.Services.SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

namespace Beseler.Infrastructure;

public static class Registrar
{
    private static readonly string? _appConfigurationEndpoint = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_ENDPOINT", EnvironmentVariableTarget.Process);

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        if (_appConfigurationEndpoint is not null)
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(_appConfigurationEndpoint), new ManagedIdentityCredential())
                    .Select("BeselerNet:*", LabelFilter.Null)
                    .TrimKeyPrefix("BeselerNet:")
                    .ConfigureRefresh(options =>
                    {
                        options.Register("BeselerNet:Sentinel", refreshAll: true);
                    });
            });
            builder.Services.AddAzureAppConfiguration();
        }

        builder.Services
            .BindConfiguration<ConnectionStringOptions>()
            .BindConfiguration<SendGridOptions>();

        builder.Services
            .AddSingleton<IDatabaseConnector, DatabaseConnector>()
            .AddScoped<OutboxRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IEmailService, SendGridEmailService>();

        builder.Services.AddSendGrid(options =>
        {
            options.ApiKey = builder.Configuration.GetValue<string>("SendGrid:ApiKey");
        });

        return builder;
    }

    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        if (_appConfigurationEndpoint is not null)
        {
            app.UseAzureAppConfiguration();
        }
    }

    private static IServiceCollection BindConfiguration<T>(this IServiceCollection services) where T : class, IConfigurationSection
    {
        services.AddOptions<T>().BindConfiguration(T.SectionName);
        return services;
    }
}

internal interface IConfigurationSection
{
    static abstract string SectionName { get; }
}