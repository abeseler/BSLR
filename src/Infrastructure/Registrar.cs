using Azure.Identity;
using Beseler.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

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

        builder.Services.AddOptions<ConnectionStringOptions>().BindConfiguration(ConnectionStringOptions.SectionName);

        builder.Services.AddSingleton<IDatabaseConnector, DatabaseConnector>();

        return builder;
    }

    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        if (_appConfigurationEndpoint is not null)
        {
            app.UseAzureAppConfiguration();
        }
    }
}
