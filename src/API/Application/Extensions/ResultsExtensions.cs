namespace Beseler.API.Application.Extensions;

public static class ResultsExtensions
{
    public static IResult NotModified(this IResultExtensions resultExtensions) => new NotModifiedResult();
    public static IResult PreconditionFailed(this IResultExtensions resultExtensions) => new PreconditionFailedResult();
    public static IResult PreconditionRequired(this IResultExtensions resultExtensions) => new PreconditionRequiredResult();
}

public sealed class NotModifiedResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
        return Task.CompletedTask;
    }
}

public sealed class PreconditionFailedResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
        return Task.CompletedTask;
    }
}

public sealed class PreconditionRequiredResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
        return Task.CompletedTask;
    }
}
