// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Middleware that reads a <see cref="ResponseCacheAttribute"/> from the matched endpoint's
/// metadata and writes the corresponding <c>Cache-Control</c> / <c>Vary</c> / <c>Pragma</c>
/// response headers.
/// </summary>
/// <remarks>
/// Place this middleware after <c>UseRouting</c> (so an endpoint is matched) and after
/// <c>UseAntiforgery</c> (so the antiforgery tokens are issued without being clobbered) and
/// before <c>MapRazorComponents&lt;App&gt;()</c>.
/// </remarks>
public sealed class ResponseCacheHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<ResponseCacheOptions> _options;

    /// <summary>
    /// Creates a new <see cref="ResponseCacheHeadersMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The response cache options.</param>
    public ResponseCacheHeadersMiddleware(
        RequestDelegate next,
        IOptions<ResponseCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        _options = options;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
    /// <returns>A <see cref="Task"/> that completes when the middleware has completed.</returns>
    public Task Invoke(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var endpoint = httpContext.GetEndpoint();
        if (endpoint is not null)
        {
            var attribute = endpoint.Metadata.GetMetadata<ResponseCacheAttribute>();
            if (attribute is not null)
            {
                var profile = attribute.GetEffectiveProfile(_options.Value);
                ResponseCacheHeaderWriter.Apply(httpContext, profile);
            }
        }

        return _next(httpContext);
    }
}
