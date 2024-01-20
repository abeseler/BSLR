using Microsoft.AspNetCore.Builder;

namespace Beseler.Domain;

public static class Registrar
{
    public static WebApplicationBuilder AddDomainServices(this WebApplicationBuilder builder)
    {
        return builder;
    }
}
