using Beseler.Domain.Common;
using Microsoft.Net.Http.Headers;

namespace Beseler.API.Application.Services;

internal sealed class ETagConditions
{
    private readonly HttpResponse? _response;
    private readonly IList<EntityTagHeaderValue> _ifMatch;
    private readonly IList<EntityTagHeaderValue> _ifNoneMatch;
    public ETagConditions(IHttpContextAccessor accessor)
    {
        _response = accessor.HttpContext?.Response;
        var headers = accessor.HttpContext?.Request.GetTypedHeaders();
        _ifMatch = headers?.IfMatch ?? [];
        _ifNoneMatch = headers?.IfNoneMatch ?? [];
    }
    public bool MissingIfMatchHeader => _ifMatch.Count == 0;
    public bool IfMatch(ITagged entity) => _ifMatch.Any(x => Match(entity.ETag, x.Tag));
    public bool IfNoneMatch(ITagged entity) => _ifNoneMatch.All(x => x.Tag != "*" && !Match(entity.ETag, x.Tag));
    public void SetETagHeader(ITagged entity)
    {
        if (_response is not null)
            _response.Headers.ETag = $"\"{entity.ETag}\"";
    }
    private static bool Match(ReadOnlySpan<char> entityTag, ReadOnlySpan<char> tag) =>
        tag.Length == entityTag.Length + 2 && tag[0] == '"' && tag[^1] == '"' && entityTag[1..^1].SequenceEqual(tag);
}
