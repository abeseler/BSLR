using Beseler.Infrastructure.Data;
using Beseler.Infrastructure.Data.Repositories;
using Beseler.Infrastructure.Services;
using Beseler.Infrastructure.Services.Jwt;
using Beseler.Infrastructure.Services.SendGrid;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;

namespace Beseler.Infrastructure;

public static class Registrar
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var handler = new JsonWebTokenHandler();
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

        builder.Services.AddAuthorization(Policies.AuthorizationOptions);

        builder.Services
            .BindConfiguration<ConnectionStringOptions>()
            .BindConfiguration<JwtOptions>()
            .BindConfiguration<SendGridOptions>();

        builder.Services
            .AddSingleton<IDatabaseConnector, DatabaseConnector>()
            .AddSingleton<OutboxRepository>()
            .AddScoped<TokenRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IBudgetRepository, BudgetRepository>()
            .AddSingleton<TokenService>()
            .AddScoped<IEmailService, SendGridEmailService>();

        builder.Services.AddSendGrid(options =>
        {
            var key = builder.Configuration.GetValue<string?>("SendGrid:ApiKey");
            options.ApiKey = string.IsNullOrWhiteSpace(key) ? "TestKey" : key;
        });

        return builder;
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