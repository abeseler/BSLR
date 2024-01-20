using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Beseler.API.Swagger;

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IWebHostEnvironment _environment;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IWebHostEnvironment environment)
    {
        _provider = provider;
        _environment = environment;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, _environment.EnvironmentName));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description, string environmentName)
    {
        var text = new StringBuilder("**Environment:** `");
        text.Append(environmentName);
        text.Append("`\nAn example application with OpenAPI, Swashbuckle, and API versioning.");
        var info = new OpenApiInfo()
        {
            Title = "Example Service",
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            text.Append(" This API version has been deprecated.");
        }

        if (description.SunsetPolicy is SunsetPolicy policy)
        {
            if (policy.Date is DateTimeOffset when)
            {
                text.Append(" The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                for (var i = 0; i < policy.Links.Count; i++)
                {
                    var link = policy.Links[i];

                    if (link.Type == "text/html")
                    {
                        text.AppendLine();

                        if (link.Title.HasValue)
                        {
                            text.Append(link.Title.Value).Append(": ");
                        }

                        text.Append(link.LinkTarget.OriginalString);
                    }
                }
            }
        }

        info.Description = text.ToString();

        return info;
    }
}
