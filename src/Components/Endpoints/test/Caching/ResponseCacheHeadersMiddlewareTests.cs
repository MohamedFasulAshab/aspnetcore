// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Endpoints.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests.Caching;

public class ResponseCacheHeadersMiddlewareTests
{
    [Fact]
    public async Task EndpointHasAttribute_HeadersAreWritten()
    {
        var attribute = new ResponseCacheAttribute
        {
            Duration = 30,
            Location = ResponseCacheLocation.Client,
            VaryByHeader = "Accept-Encoding",
        };

        var ctx = await InvokeAsync(attribute, options: null);

        Assert.Equal("private,max-age=30", ctx.Response.Headers.CacheControl.ToString());
        Assert.Equal("Accept-Encoding", ctx.Response.Headers.Vary.ToString());
        Assert.False(ctx.Response.Headers.ContainsKey("Pragma"));
    }

    [Fact]
    public async Task EndpointHasAttributeNoStore_HeadersAreWritten()
    {
        var attribute = new ResponseCacheAttribute { NoStore = true };

        var ctx = await InvokeAsync(attribute, options: null);

        Assert.Equal("no-store", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task EndpointHasAttributeWithProfile_ProfileValuesAreApplied()
    {
        var attribute = new ResponseCacheAttribute { CacheProfileName = "Default" };
        var options = new ResponseCacheOptions();
        options.AddProfile("Default", new ResponseCacheProfile
        {
            Duration = 120,
            Location = ResponseCacheLocation.Any,
        });

        var ctx = await InvokeAsync(attribute, options);

        Assert.Equal("public,max-age=120", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task EndpointDoesNotHaveAttribute_HeadersAreNotWritten()
    {
        var ctx = await InvokeAsync(attribute: null, options: null);

        Assert.False(ctx.Response.Headers.ContainsKey("Cache-Control"));
        Assert.False(ctx.Response.Headers.ContainsKey("Vary"));
        Assert.False(ctx.Response.Headers.ContainsKey("Pragma"));
    }

    [Fact]
    public async Task NoEndpoint_HeadersAreNotWritten()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var middleware = new ResponseCacheHeadersMiddleware(
            _ => Task.CompletedTask,
            Options.Create(new ResponseCacheOptions()));

        var ctx = new DefaultHttpContext { RequestServices = services };
        ctx.Response.Body = new MemoryStream();

        await middleware.Invoke(ctx);

        Assert.False(ctx.Response.Headers.ContainsKey("Cache-Control"));
    }

    private static async Task<HttpContext> InvokeAsync(ResponseCacheAttribute? attribute, ResponseCacheOptions? options)
    {
        var services = new ServiceCollection()
            .AddOptions()
            .BuildServiceProvider();

        var optionsWrapper = Options.Create(options ?? new ResponseCacheOptions());

        // The middleware reads Endpoint metadata before calling _next, so we must pre-set
        // the endpoint the same way UseRouting would have.
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new ResponseCacheHeadersMiddleware(next, optionsWrapper);

        var ctx = new DefaultHttpContext { RequestServices = services };
        ctx.Response.Body = new MemoryStream();
        ctx.Request.Path = "/";

        var metadata = new List<object> { new HttpMethodMetadata(new[] { "GET" }) };
        if (attribute is not null)
        {
            metadata.Add(attribute);
        }

        var endpoint = new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse("/"),
            order: 0,
            new EndpointMetadataCollection(metadata),
            displayName: null);
        ctx.SetEndpoint(endpoint);

        await middleware.Invoke(ctx);

        return ctx;
    }
}
