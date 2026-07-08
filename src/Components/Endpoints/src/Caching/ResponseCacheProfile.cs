// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Represents a named set of response cache settings that can be referenced by
/// <see cref="ResponseCacheAttribute"/> via <c>CacheProfileName</c>.
/// </summary>
public sealed class ResponseCacheProfile
{
    /// <summary>
    /// The duration in seconds for which the response is cached.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// The location where the data from a particular URL must be cached.
    /// </summary>
    public ResponseCacheLocation? Location { get; set; }

    /// <summary>
    /// Whether the data should be stored or not.
    /// </summary>
    public bool? NoStore { get; set; }

    /// <summary>
    /// The value for the Vary response header.
    /// </summary>
    public string? VaryByHeader { get; set; }
}
