// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// Abstraction for localizing QuickGrid pagination UI strings.
/// Implement this interface to provide custom translations or integrate with any localization system.
/// </summary>
/// <remarks>
/// The default implementation (<see cref="DefaultPaginatorLocalizer"/>) provides English strings.
/// For ASP.NET Core integration, use <see cref="StringLocalizerPaginatorAdapter"/> with IStringLocalizer.
/// </remarks>
public interface IPaginatorLocalizer
{
    /// <summary>
    /// Gets the aria-label text for the first page navigation button.
    /// </summary>
    string FirstPageAriaLabel { get; }

    /// <summary>
    /// Gets the aria-label text for the previous page navigation button.
    /// </summary>
    string PreviousPageAriaLabel { get; }

    /// <summary>
    /// Gets the aria-label text for the next page navigation button.
    /// </summary>
    string NextPageAriaLabel { get; }

    /// <summary>
    /// Gets the aria-label text for the last page navigation button.
    /// </summary>
    string LastPageAriaLabel { get; }

    /// <summary>
    /// Gets the title/tooltip text for the first page navigation button.
    /// </summary>
    string FirstPageTitle { get; }

    /// <summary>
    /// Gets the title/tooltip text for the previous page navigation button.
    /// </summary>
    string PreviousPageTitle { get; }

    /// <summary>
    /// Gets the title/tooltip text for the next page navigation button.
    /// </summary>
    string NextPageTitle { get; }

    /// <summary>
    /// Gets the title/tooltip text for the last page navigation button.
    /// </summary>
    string LastPageTitle { get; }

    /// <summary>
    /// Gets the singular form text for items (used when item count is 1).
    /// </summary>
    string ItemSingularText { get; }

    /// <summary>
    /// Gets the plural form text for items (used when item count is not 1).
    /// </summary>
    string ItemPluralText { get; }

    /// <summary>
    /// Gets the format string for the page summary display.
    /// The format accepts {0} for current page and {1} for total pages.
    /// Example: "Page {0} of {1}" produces "Page 2 of 5".
    /// </summary>
    string PageLabelFormat { get; }
}
