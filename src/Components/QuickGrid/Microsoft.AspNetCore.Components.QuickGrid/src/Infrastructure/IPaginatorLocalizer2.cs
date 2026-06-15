// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// Optional advanced pagination localizer for cultures requiring richer formatting.
/// Implement this in addition to <see cref="IPaginatorLocalizer"/> to provide
/// pluralization-aware item text, culture-specific page labels, and a full summary string.
/// </summary>
public interface IPaginatorLocalizer2 : IPaginatorLocalizer
{
    /// <summary>
    /// Returns the localized item text for the specified <paramref name="count"/>.
    /// Implementations should apply appropriate pluralization rules for the target culture.
    /// </summary>
    /// <param name="count">The item count.</param>
    /// <returns>The localized item term (e.g., "item"/"items" or other plural forms).</returns>
    string Items(int count);

    /// <summary>
    /// Returns the localized page label text.
    /// </summary>
    /// <param name="currentPage">The current page number (1-based).</param>
    /// <param name="totalPages">The total number of pages (1-based).</param>
    /// <returns>The localized page label text.</returns>
    string PageLabel(int currentPage, int totalPages);

    /// <summary>
    /// Returns a fully localized summary string for the supplied <see cref="PaginationState"/>.
    /// </summary>
    /// <param name="state">The pagination state.</param>
    /// <returns>The localized summary string.</returns>
    string Summary(Microsoft.AspNetCore.Components.QuickGrid.PaginationState state);
}
