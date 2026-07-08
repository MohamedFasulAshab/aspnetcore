// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Writes <c>Cache-Control</c>, <c>Vary</c> and <c>Pragma</c> response headers based on a
/// <see cref="ResponseCacheProfile"/>. This is the Blazor-side mirror of the MVC
/// <c>ResponseCacheFilterExecutor</c>.
/// </summary>
internal static class ResponseCacheHeaderWriter
{
    /// <summary>
    /// Applies the headers described by <paramref name="profile"/> to
    /// <paramref name="httpContext"/>.Response.Headers.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
    /// <param name="profile">The effective profile to apply.</param>
    public static void Apply(HttpContext httpContext, ResponseCacheProfile profile)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(profile);

        var resolvedDuration = profile.Duration ?? 0;
        var resolvedLocation = profile.Location ?? ResponseCacheLocation.Any;
        var resolvedNoStore = profile.NoStore ?? false;
        var resolvedVaryByHeader = profile.VaryByHeader;

        // Validate: Duration is required unless NoStore is set or Location is None.
        if (!(resolvedNoStore
            || resolvedLocation == ResponseCacheLocation.None
            || profile.Duration is not null))
        {
            throw new InvalidOperationException(
                $"The '{nameof(ResponseCacheAttribute)}.{nameof(ResponseCacheAttribute.Duration)}' property must be set " +
                $"unless '{nameof(ResponseCacheAttribute.NoStore)}' is true or " +
                $"'{nameof(ResponseCacheAttribute.Location)}' is '{nameof(ResponseCacheLocation.None)}'.");
        }

        if (resolvedDuration < 0)
        {
            throw new InvalidOperationException(
                $"The '{nameof(ResponseCacheAttribute)}.{nameof(ResponseCacheAttribute.Duration)}' property must be greater than or equal to zero.");
        }

        var headers = httpContext.Response.Headers;

        // Clear headers that we are about to set, so the attribute is authoritative.
        headers.Remove(HeaderNames.Vary);
        headers.Remove(HeaderNames.CacheControl);
        headers.Remove(HeaderNames.Pragma);

        if (!string.IsNullOrEmpty(resolvedVaryByHeader))
        {
            headers.Vary = resolvedVaryByHeader;
        }

        if (resolvedNoStore)
        {
            headers.CacheControl = "no-store";

            // Cache-Control: no-store, no-cache is valid.
            if (resolvedLocation == ResponseCacheLocation.None)
            {
                headers.AppendCommaSeparatedValues(HeaderNames.CacheControl, "no-cache");
                headers.Pragma = "no-cache";
            }
        }
        else
        {
            string cacheControlValue;
            if (resolvedLocation == ResponseCacheLocation.None && profile.Duration is null)
            {
                cacheControlValue = "no-cache";
                headers.Pragma = "no-cache";
            }
            else
            {
                cacheControlValue = resolvedLocation switch
                {
                    ResponseCacheLocation.Any => "public",
                    ResponseCacheLocation.Client => "private",
                    ResponseCacheLocation.None => "no-cache",
                    _ => throw new InvalidOperationException($"Unknown {nameof(ResponseCacheLocation)} value: {resolvedLocation}"),
                };
                cacheControlValue = $"{cacheControlValue},max-age={resolvedDuration}";
            }

            headers.CacheControl = cacheControlValue;
        }
    }
}
