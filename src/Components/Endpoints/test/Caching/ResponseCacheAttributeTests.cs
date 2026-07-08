// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Endpoints.Caching;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests.Caching;

public class ResponseCacheAttributeTests
{
    [Fact]
    public void GetEffectiveProfile_AttributeValuesUsedDirectly()
    {
        var attribute = new ResponseCacheAttribute
        {
            Duration = 60,
            Location = ResponseCacheLocation.Client,
            NoStore = false,
            VaryByHeader = "Accept",
        };

        var profile = attribute.GetEffectiveProfile(new ResponseCacheOptions());

        Assert.Equal(60, profile.Duration);
        Assert.Equal(ResponseCacheLocation.Client, profile.Location);
        Assert.False(profile.NoStore);
        Assert.Equal("Accept", profile.VaryByHeader);
    }

    [Fact]
    public void GetEffectiveProfile_CacheProfileName_AppliesProfileValues()
    {
        var options = new ResponseCacheOptions();
        options.AddProfile("Default", new ResponseCacheProfile
        {
            Duration = 90,
            Location = ResponseCacheLocation.Any,
            VaryByHeader = "User-Agent",
        });

        var attribute = new ResponseCacheAttribute { CacheProfileName = "Default" };
        var profile = attribute.GetEffectiveProfile(options);

        Assert.Equal(90, profile.Duration);
        Assert.Equal(ResponseCacheLocation.Any, profile.Location);
        Assert.Equal("User-Agent", profile.VaryByHeader);
    }

    [Fact]
    public void GetEffectiveProfile_AttributeValuesOverrideProfile()
    {
        var options = new ResponseCacheOptions();
        options.AddProfile("Default", new ResponseCacheProfile
        {
            Duration = 90,
            Location = ResponseCacheLocation.Any,
        });

        var attribute = new ResponseCacheAttribute
        {
            CacheProfileName = "Default",
            Duration = 5, // explicitly set; should win
        };

        var profile = attribute.GetEffectiveProfile(options);

        Assert.Equal(5, profile.Duration);
        Assert.Equal(ResponseCacheLocation.Any, profile.Location); // not set on attribute, falls back to profile
    }

    [Fact]
    public void GetEffectiveProfile_UnknownCacheProfile_Throws()
    {
        var attribute = new ResponseCacheAttribute { CacheProfileName = "Missing" };
        Assert.Throws<InvalidOperationException>(() => attribute.GetEffectiveProfile(new ResponseCacheOptions()));
    }

    [Fact]
    public void GetEffectiveProfile_DefaultValuesDoNotThrow()
    {
        var attribute = new ResponseCacheAttribute();
        var profile = attribute.GetEffectiveProfile(new ResponseCacheOptions());

        Assert.Null(profile.Duration);
        Assert.Null(profile.Location);
        Assert.Null(profile.NoStore);
        Assert.Null(profile.VaryByHeader);
    }
}
