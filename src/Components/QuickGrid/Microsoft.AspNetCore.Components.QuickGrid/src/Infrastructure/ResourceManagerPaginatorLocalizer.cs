// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Resources;

namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// A paginator localizer that automatically loads translations from embedded .resx files
/// based on <see cref="CultureInfo.CurrentUICulture"/>.
/// <para>
/// This is the default built-in localization provider for the Paginator component.
/// Translations are loaded from satellite assemblies or directly from embedded resources
/// using the specified <see cref="ResourceManager"/>.
/// </para>
/// </summary>
public sealed class ResourceManagerPaginatorLocalizer : IPaginatorLocalizer, IPaginatorLocalizer2
{
    /// <summary>
    /// The base name of the embedded resources.
    /// </summary>
    public static string ResourceBaseName { get; } = "Microsoft.AspNetCore.Components.QuickGrid.Resources.Paginator";

    private static readonly ResourceManager _defaultResourceManager = new(ResourceBaseName, typeof(ResourceManagerPaginatorLocalizer).Assembly);

    /// <summary>
    /// Gets a singleton instance of <see cref="ResourceManagerPaginatorLocalizer"/> that uses
    /// <see cref="CultureInfo.CurrentUICulture"/> to dynamically select the appropriate translation
    /// at render time.
    /// </summary>
    /// <remarks>
    /// The culture is read dynamically on each property access, not cached at initialization time.
    /// This ensures proper support for Blazor WebAssembly where the browser culture may differ
    /// from the server culture.
    /// </remarks>
    public static ResourceManagerPaginatorLocalizer Instance { get; } = new(_defaultResourceManager);

    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Creates a new <see cref="ResourceManagerPaginatorLocalizer"/> with the specified resource manager
    /// and using <see cref="CultureInfo.CurrentUICulture"/> for culture-aware string retrieval.
    /// </summary>
    /// <param name="resourceManager">The resource manager containing the localized strings.</param>
    public ResourceManagerPaginatorLocalizer(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }

    private string GetString(string name)
    {
        var value = _resourceManager.GetString(name, CultureInfo.CurrentUICulture);
        return value ?? name;
    }

    /// <inheritdoc />
    public string FirstPageAriaLabel => GetString(nameof(FirstPageAriaLabel));

    /// <inheritdoc />
    public string PreviousPageAriaLabel => GetString(nameof(PreviousPageAriaLabel));

    /// <inheritdoc />
    public string NextPageAriaLabel => GetString(nameof(NextPageAriaLabel));

    /// <inheritdoc />
    public string LastPageAriaLabel => GetString(nameof(LastPageAriaLabel));

    /// <inheritdoc />
    public string FirstPageTitle => GetString(nameof(FirstPageTitle));

    /// <inheritdoc />
    public string PreviousPageTitle => GetString(nameof(PreviousPageTitle));

    /// <inheritdoc />
    public string NextPageTitle => GetString(nameof(NextPageTitle));

    /// <inheritdoc />
    public string LastPageTitle => GetString(nameof(LastPageTitle));

    /// <inheritdoc />
    public string ItemSingularText => GetString(nameof(ItemSingularText));

    /// <inheritdoc />
    public string ItemPluralText => GetString(nameof(ItemPluralText));

    /// <inheritdoc />
    public string PageLabelFormat => GetString(nameof(PageLabelFormat));

    /// <inheritdoc />
    public string Items(int count)
    {
        var culture = CultureInfo.CurrentUICulture;

        // Try CLDR-style plural categories first
        var category = count switch
        {
            0 => "Zero",
            1 => "One",
            2 => "Two",
            _ => "Other"
        };
        var key = $"Items_{category}";
        var value = _resourceManager.GetString(key, culture);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Fallback to binary singular/plural
        return count == 1 ? ItemSingularText : ItemPluralText;
    }

    /// <inheritdoc />
    public string PageLabel(int currentPage, int totalPages)
        => string.Format(CultureInfo.InvariantCulture, PageLabelFormat, currentPage, totalPages);

    /// <inheritdoc />
    public string Summary(PaginationState state)
    {
        var count = state.TotalItemCount ?? 0;
        return $"{count} {(count == 1 ? ItemSingularText : ItemPluralText)}";
    }
}
