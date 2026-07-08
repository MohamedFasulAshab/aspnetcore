// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Specifies the parameters necessary for setting appropriate headers in response caching for
/// Blazor components rendered via static server-side rendering.
/// </summary>
/// <remarks>
/// This attribute is consumed by the <c>UseResponseCacheHeaders</c> middleware. When applied to a
/// Blazor component, the middleware writes the corresponding <c>Cache-Control</c>, <c>Vary</c>, and
/// <c>Pragma</c> response headers.
/// </remarks>
/// <example>
/// <code>
/// &#64;page "/products"
/// &#64;attribute [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ResponseCacheAttribute : Attribute
{
    // Nullable backing fields let us tell "user set it explicitly" apart from "left at default",
    // which is impossible to do with a value-type property and the default keyword.
    private int? _duration;
    private ResponseCacheLocation? _location;
    private bool? _noStore;

    /// <summary>
    /// Gets or sets the duration in seconds for which the response is cached.
    /// This sets "max-age" in the "Cache-Control" header.
    /// </summary>
    public int Duration
    {
        get => _duration ?? 0;
        set => _duration = value;
    }

    /// <summary>
    /// Gets or sets the location where the data from a particular URL must be cached.
    /// Defaults to <see cref="ResponseCacheLocation.Any"/>.
    /// </summary>
    public ResponseCacheLocation Location
    {
        get => _location ?? ResponseCacheLocation.Any;
        set => _location = value;
    }

    /// <summary>
    /// Gets or sets a value which determines whether the data should be stored or not.
    /// When set to <c>true</c>, it sets the "Cache-Control" header to "no-store".
    /// Ignores <see cref="Location"/> for values other than <see cref="ResponseCacheLocation.None"/>.
    /// Ignores <see cref="Duration"/>.
    /// </summary>
    public bool NoStore
    {
        get => _noStore ?? false;
        set => _noStore = value;
    }

    /// <summary>
    /// Gets or sets the value for the Vary response header.
    /// </summary>
    public string? VaryByHeader { get; set; }

    /// <summary>
    /// Gets or sets the value of the cache profile name.
    /// </summary>
    /// <remarks>
    /// Resolved against <see cref="ResponseCacheOptions.CacheProfiles"/>.
    /// </remarks>
    public string? CacheProfileName { get; set; }

    /// <summary>
    /// Resolves the effective settings, layering attribute values on top of any matching
    /// <see cref="ResponseCacheProfile"/> registered in <see cref="ResponseCacheOptions.CacheProfiles"/>.
    /// </summary>
    /// <param name="options">The configured options.</param>
    /// <returns>A <see cref="ResponseCacheProfile"/> with the effective values.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CacheProfileName"/> is set but no matching profile is registered.
    /// </exception>
    public ResponseCacheProfile GetEffectiveProfile([NotNull] ResponseCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ResponseCacheProfile? selectedProfile = null;
        if (!string.IsNullOrEmpty(CacheProfileName))
        {
            if (!options.CacheProfiles.TryGetValue(CacheProfileName, out selectedProfile))
            {
                throw new InvalidOperationException(
                    $"The cache profile '{CacheProfileName}' was not found in ResponseCacheOptions.CacheProfiles.");
            }
        }

        // If the ResponseCacheAttribute parameters are set, they override the values from the
        // cache profile. The expression below checks the attribute first and falls back to the
        // profile (or null when no profile is selected).
        _duration = _duration ?? selectedProfile?.Duration;
        _noStore = _noStore ?? selectedProfile?.NoStore;
        _location = _location ?? selectedProfile?.Location;
        VaryByHeader = VaryByHeader ?? selectedProfile?.VaryByHeader;

        return new ResponseCacheProfile
        {
            Duration = _duration,
            Location = _location,
            NoStore = _noStore,
            VaryByHeader = VaryByHeader,
        };
    }
}
