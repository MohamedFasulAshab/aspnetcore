// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// Default English implementation of <see cref="IPaginatorLocalizer"/>.
/// This is used as the fallback when no other localizer is configured.
/// </summary>
public class DefaultPaginatorLocalizer : IPaginatorLocalizer, IPaginatorLocalizer2
{
    /// <summary>
    /// Gets the singleton instance of the default localizer.
    /// </summary>
    public static DefaultPaginatorLocalizer Instance { get; } = new();

    /// <inheritdoc />
    public string FirstPageAriaLabel => "Go to first page";

    /// <inheritdoc />
    public string PreviousPageAriaLabel => "Go to previous page";

    /// <inheritdoc />
    public string NextPageAriaLabel => "Go to next page";

    /// <inheritdoc />
    public string LastPageAriaLabel => "Go to last page";

    /// <inheritdoc />
    public string FirstPageTitle => "Go to first page";

    /// <inheritdoc />
    public string PreviousPageTitle => "Go to previous page";

    /// <inheritdoc />
    public string NextPageTitle => "Go to next page";

    /// <inheritdoc />
    public string LastPageTitle => "Go to last page";

    /// <inheritdoc />
    public string ItemSingularText => "item";

    /// <inheritdoc />
    public string ItemPluralText => "items";

    /// <inheritdoc />
    public string PageLabelFormat => "Page {0} of {1}";

    /// <inheritdoc />
    public string Items(int count) => count == 1 ? ItemSingularText : ItemPluralText;

    /// <inheritdoc />
    public string PageLabel(int currentPage, int totalPages) => string.Format(System.Globalization.CultureInfo.InvariantCulture, PageLabelFormat, currentPage, totalPages);

    /// <inheritdoc />
    public string Summary(Microsoft.AspNetCore.Components.QuickGrid.PaginationState state)
    {
        var count = state.TotalItemCount ?? 0;
        return $"{count} {(count == 1 ? ItemSingularText : ItemPluralText)}";
    }
}
