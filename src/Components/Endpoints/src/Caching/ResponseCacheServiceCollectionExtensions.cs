// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Endpoints.Caching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for configuring response cache header support for Blazor static server-side rendering.
/// </summary>
public static class ResponseCacheServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required to support the <see cref="ResponseCacheAttribute"/> on Blazor
    /// components. Call this from <c>builder.Services</c> if you want to register named cache
    /// profiles via <c>builder.Services.Configure&lt;ResponseCacheOptions&gt;(...)</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddResponseCacheHeaders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(ServiceDescriptor.Singleton<IConfigureOptions<ResponseCacheOptions>, ResponseCacheOptionsSetup>());
        return services;
    }

    private sealed class ResponseCacheOptionsSetup : IConfigureOptions<ResponseCacheOptions>
    {
        public void Configure(ResponseCacheOptions options)
        {
            // Default profile is intentionally not registered; users opt in by name.
        }
    }
}
