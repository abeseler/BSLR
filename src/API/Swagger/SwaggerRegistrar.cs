using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Beseler.API.Swagger;

internal static class SwaggerRegistrar
{
    public static void AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SwaggerDefaultValues>();
            options.EnableAnnotations();
        });
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
        });
    }

    public static void MapSwaggerUI(this WebApplication app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "_swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "_swagger";
            var descriptions = app.DescribeApiVersions();

            foreach (var description in descriptions)
            {
                var url = $"/_swagger/{description.GroupName}/swagger.json";
                var name = $"API {description.GroupName}";
                options.SwaggerEndpoint(url, name);
            }
        });
    }
}
