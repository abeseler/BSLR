using Azure.Identity;
using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Data;
using Beseler.Infrastructure.Data.Repositories;
using Beseler.Infrastructure.Services;
using Beseler.Infrastructure.Services.Jwt;
using Beseler.Infrastructure.Services.SendGrid;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

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

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            options.TokenHandlers.Clear();
            options.TokenHandlers.Add(handler);

            var key = builder.Configuration.GetValue<string>("Jwt:Key") ?? "";
            options.TokenValidationParameters = new()
            {
                ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = JwtRegisteredClaimNames.Sub,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        builder.Services.AddAuthorization();

        builder.Services
            .BindConfiguration<ConnectionStringOptions>()
            .BindConfiguration<JwtOptions>()
            .BindConfiguration<SendGridOptions>();

        builder.Services
            .AddSingleton<IDatabaseConnector, DatabaseConnector>()
            .AddSingleton<OutboxRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddSingleton<TokenService>()
            .AddScoped<IEmailService, SendGridEmailService>();

        builder.Services.AddSendGrid(options =>
        {
            var key = builder.Configuration.GetValue<string?>("SendGrid:ApiKey");
            options.ApiKey = string.IsNullOrWhiteSpace(key) ? "TestKey" : key;
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