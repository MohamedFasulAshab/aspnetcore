// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.Localization;

namespace BlazorUnitedApp.Localization;

/// <summary>
/// Adapter that exposes BlazorUnitedApp's own Paginator translations as an
/// <see cref="IStringLocalizer{Paginator}"/>. The QuickGrid Paginator does a
/// soft <c>Services.GetService&lt;IStringLocalizer&lt;Paginator&gt;&gt;()</c>
/// lookup at runtime, so any localizer we register under that interface is
/// picked up automatically. The default <see cref="IStringLocalizerFactory"/>
/// would resolve that lookup against the QuickGrid assembly (where these
/// translations do not live), so we wire the app's own embedded resources
/// into the factory by name and delegate.
/// </summary>
internal sealed class PaginatorLocalizer : IStringLocalizer<Paginator>
{
    private readonly IStringLocalizer _inner;

    public PaginatorLocalizer(IStringLocalizerFactory factory)
    {
        // The .resx files are embedded with LogicalName
        // "BlazorUnitedApp.Resources.Paginator.{culture}.resources", which is
        // exactly what ResourceManagerStringLocalizerFactory computes for
        // ("Paginator", "BlazorUnitedApp") with ResourcesPath = "Resources".
        _inner = factory.Create("Paginator", "BlazorUnitedApp");
    }

    public LocalizedString this[string name]
    {
        get
        {
            var v = _inner[name];
            System.Console.WriteLine($"[PaginatorLocalizer] {name} -> '{v.Value}' (RNF={v.ResourceNotFound}, searched={v.SearchedLocation})");

            Console.WriteLine(
            $"KEY={name}, VALUE={v.Value}, RNF={v.ResourceNotFound}");

            return v;
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var v = _inner[name, arguments];
            System.Console.WriteLine($"[PaginatorLocalizer] {name}(args={string.Join(",", arguments)}) -> '{v.Value}' (RNF={v.ResourceNotFound})");
            return v;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _inner.GetAllStrings(includeParentCultures);
}
