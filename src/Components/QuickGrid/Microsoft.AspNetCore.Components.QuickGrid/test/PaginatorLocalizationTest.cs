// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Tests for IPaginatorLocalizer interface and implementations.
/// Covers: issue #42338 - QuickGrid pagination localization
/// </summary>
public class PaginatorLocalizationTest
{
    /// <summary>
    /// Validates DefaultPaginatorLocalizer provides correct English strings.
    /// </summary>
    [Fact]
    public void DefaultPaginatorLocalizer_ReturnsCorrectEnglishStrings()
    {
        // Arrange & Act
        var localizer = DefaultPaginatorLocalizer.Instance;

        // Assert - verify all 11 properties have expected English values
        Assert.Equal("Go to first page", localizer.FirstPageAriaLabel);
        Assert.Equal("Go to previous page", localizer.PreviousPageAriaLabel);
        Assert.Equal("Go to next page", localizer.NextPageAriaLabel);
        Assert.Equal("Go to last page", localizer.LastPageAriaLabel);
        Assert.Equal("Go to first page", localizer.FirstPageTitle);
        Assert.Equal("Go to previous page", localizer.PreviousPageTitle);
        Assert.Equal("Go to next page", localizer.NextPageTitle);
        Assert.Equal("Go to last page", localizer.LastPageTitle);
        Assert.Equal("item", localizer.ItemSingularText);
        Assert.Equal("items", localizer.ItemPluralText);
        Assert.Equal("Page {0} of {1}", localizer.PageLabelFormat);
    }

    /// <summary>
    /// Validates DefaultPaginatorLocalizer is a singleton by checking Instance returns same object.
    /// </summary>
    [Fact]
    public void DefaultPaginatorLocalizer_IsSingleton()
    {
        // Arrange & Act
        var instance1 = DefaultPaginatorLocalizer.Instance;
        var instance2 = DefaultPaginatorLocalizer.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    /// <summary>
    /// Validates that IPaginatorLocalizer interface is properly implemented.
    /// </summary>
    [Fact]
    public void DefaultPaginatorLocalizer_ImplementsIPaginatorLocalizer()
    {
        // Arrange & Act
        IPaginatorLocalizer localizer = DefaultPaginatorLocalizer.Instance;

        // Assert - verify interface contract
        Assert.NotNull(localizer);
        Assert.Equal("Go to first page", localizer.FirstPageAriaLabel);
        Assert.Equal("item", localizer.ItemSingularText);
        Assert.Equal("items", localizer.ItemPluralText);
        Assert.Equal("Page {0} of {1}", localizer.PageLabelFormat);
    }

    /// <summary>
    /// Validates that a custom IPaginatorLocalizer implementation works correctly.
    /// </summary>
    [Fact]
    public void CustomPaginatorLocalizer_WorksCorrectly()
    {
        // Arrange
        var customLocalizer = new TestPaginatorLocalizer();

        // Assert - verify custom values are returned
        Assert.Equal(" Prima ", customLocalizer.FirstPageAriaLabel);
        Assert.Equal(" Precedente ", customLocalizer.PreviousPageAriaLabel);
        Assert.Equal(" Successiva ", customLocalizer.NextPageAriaLabel);
        Assert.Equal(" Ultima ", customLocalizer.LastPageAriaLabel);
        Assert.Equal(" elemento ", customLocalizer.ItemSingularText);
        Assert.Equal(" elementi ", customLocalizer.ItemPluralText);
        Assert.Equal(" Pagina {0} di {1} ", customLocalizer.PageLabelFormat);
    }

    /// <summary>
    /// Validates page label format contains placeholders.
    /// </summary>
    [Fact]
    public void PageLabelFormat_ContainsFormatPlaceholders()
    {
        // Arrange & Act
        var localizer = DefaultPaginatorLocalizer.Instance;

        // Assert
        Assert.Contains("{0}", localizer.PageLabelFormat);
        Assert.Contains("{1}", localizer.PageLabelFormat);
    }

    /// <summary>
    /// Validates singular text is returned for count of 1 (semantic verification).
    /// </summary>
    [Fact]
    public void ItemSingularText_IsSingular()
    {
        // Arrange & Act
        var localizer = DefaultPaginatorLocalizer.Instance;

        // Assert - singular text should not contain "s" suffix typically used for plural
        Assert.False(localizer.ItemSingularText.EndsWith("s", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Validates plural text is different from singular text.
    /// </summary>
    [Fact]
    public void ItemPluralText_DiffersFromSingularText()
    {
        // Arrange & Act
        var localizer = DefaultPaginatorLocalizer.Instance;

        // Assert - English plural adds "s"
        Assert.NotEqual(localizer.ItemSingularText, localizer.ItemPluralText);
    }
}

/// <summary>
/// Custom test implementation of IPaginatorLocalizer for testing.
/// </summary>
file class TestPaginatorLocalizer : IPaginatorLocalizer
{
    public string FirstPageAriaLabel => " Prima ";
    public string PreviousPageAriaLabel => " Precedente ";
    public string NextPageAriaLabel => " Successiva ";
    public string LastPageAriaLabel => " Ultima ";
    public string FirstPageTitle => " Prima ";
    public string PreviousPageTitle => " Precedente ";
    public string NextPageTitle => " Successiva ";
    public string LastPageTitle => " Ultima ";
    public string ItemSingularText => " elemento ";
    public string ItemPluralText => " elementi ";
    public string PageLabelFormat => " Pagina {0} di {1} ";
}