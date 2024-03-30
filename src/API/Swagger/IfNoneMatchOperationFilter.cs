using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Beseler.API.Swagger;

public sealed class IfNoneMatchOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "If-None-Match",
            In = ParameterLocation.Header,
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Required = false
        });
    }
}
