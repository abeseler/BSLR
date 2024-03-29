﻿using Serilog.Core;
using Serilog.Events;

namespace Beseler.API.Application.Services;

internal sealed class HttpContextLogEnricher(IHttpContextAccessor accessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (accessor.HttpContext is not { } http)
            return;

        if (http.User.Identity is { IsAuthenticated: true, Name: not null } user)
            logEvent.AddPropertyIfAbsent(new LogEventProperty("AccountId", new ScalarValue(user.Name)));
    }
}
