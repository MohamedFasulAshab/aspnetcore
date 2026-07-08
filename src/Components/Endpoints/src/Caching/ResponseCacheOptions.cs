// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Options for the response cache header middleware used with Blazor static server-side rendering.
/// </summary>
public sealed class ResponseCacheOptions
{
    /// <summary>
    /// Gets the set of named cache profiles that can be referenced by
    /// <see cref="ResponseCacheAttribute"/> via <c>CacheProfileName</c>.
    /// </summary>
    public IDictionary<string, ResponseCacheProfile> CacheProfiles { get; } =
        new Dictionary<string, ResponseCacheProfile>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a named cache profile.
    /// </summary>
    /// <param name="name">The name of the profile.</param>
    /// <param name="profile">The profile.</param>
    public void AddProfile(string name, ResponseCacheProfile profile)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(profile);
        CacheProfiles[name] = profile;
    }
}
