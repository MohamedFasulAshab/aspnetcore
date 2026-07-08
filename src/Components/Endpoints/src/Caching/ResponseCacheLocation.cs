// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Endpoints.Caching;

/// <summary>
/// Determines the value for the "Cache-Control" header in the response.
/// </summary>
public enum ResponseCacheLocation
{
    /// <summary>
    /// Cached in both proxies and the client.
    /// Sets the "Cache-Control" header to "public".
    /// </summary>
    Any = 0,

    /// <summary>
    /// Cached only in the client.
    /// Sets the "Cache-Control" header to "private".
    /// </summary>
    Client = 1,

    /// <summary>
    /// "Cache-Control" and "Pragma" headers are set to "no-cache".
    /// </summary>
    None = 2,
}
