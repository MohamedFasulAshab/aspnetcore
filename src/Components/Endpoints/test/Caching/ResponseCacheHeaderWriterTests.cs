// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Endpoints.Caching;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests.Caching;

public class ResponseCacheHeaderWriterTests
{
    [Fact]
    public void Apply_DurationAndAny_SetsPublicMaxAge()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Duration = 60, Location = ResponseCacheLocation.Any });

        Assert.Equal("public,max-age=60", ctx.Response.Headers.CacheControl.ToString());
        Assert.False(ctx.Response.Headers.ContainsKey("Pragma"));
        Assert.False(ctx.Response.Headers.ContainsKey("Vary"));
    }

    [Fact]
    public void Apply_DurationAndClient_SetsPrivateMaxAge()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Duration = 30, Location = ResponseCacheLocation.Client });

        Assert.Equal("private,max-age=30", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public void Apply_NoStoreTrue_OverridesEverything()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { NoStore = true, Duration = 60, Location = ResponseCacheLocation.Any });

        Assert.Equal("no-store", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public void Apply_NoStoreTrueAndLocationNone_AddsNoCacheAndPragma()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { NoStore = true, Location = ResponseCacheLocation.None });

        Assert.Equal("no-store,no-cache", ctx.Response.Headers.CacheControl.ToString());
        Assert.Equal("no-cache", ctx.Response.Headers.Pragma.ToString());
    }

    [Fact]
    public void Apply_LocationNoneWithoutDuration_SetsNoCacheAndPragma()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Location = ResponseCacheLocation.None });

        Assert.Equal("no-cache", ctx.Response.Headers.CacheControl.ToString());
        Assert.Equal("no-cache", ctx.Response.Headers.Pragma.ToString());
    }

    [Fact]
    public void Apply_VaryByHeader_SetsVary()
    {
        var ctx = NewContext();
        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Duration = 30, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding" });

        Assert.Equal("Accept-Encoding", ctx.Response.Headers.Vary.ToString());
    }

    [Fact]
    public void Apply_OverwritesExistingCacheControlHeader()
    {
        var ctx = NewContext();
        ctx.Response.Headers.CacheControl = "must-revalidate";

        ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Duration = 10, Location = ResponseCacheLocation.Any });

        Assert.Equal("public,max-age=10", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public void Apply_MissingDurationAndNotNoStore_Throws()
    {
        var ctx = NewContext();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Location = ResponseCacheLocation.Any }));
        Assert.Contains("Duration", ex.Message);
    }

    [Fact]
    public void Apply_NegativeDuration_Throws()
    {
        var ctx = NewContext();
        Assert.Throws<InvalidOperationException>(() =>
            ResponseCacheHeaderWriter.Apply(ctx, new ResponseCacheProfile { Duration = -1, Location = ResponseCacheLocation.Any }));
    }

    private static HttpContext NewContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }
}
