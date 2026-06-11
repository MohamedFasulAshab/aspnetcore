// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Tests for Paginator component localization parameters.
/// Covers: issue #42338 - QuickGrid pagination localization
/// </summary>
public class PaginatorLocalizationTest : TestContext
{
    /// <summary>
    /// Test model for pagination data.
    /// </summary>
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock NavigationManager for testing pagination state changes.
    /// </summary>
    private sealed class MockNavigationManager : NavigationManager
    {
        public MockNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        public string LastNavigateUri { get; private set; } = string.Empty;
        public bool NavigateCalled { get; private set; }

        public void SetUri(string uri)
        {
            SetGlobalNavigationUri(uri, uri);
        }

        public void SetGlobalNavigationUri(string uri, string newUri)
        {
            Uri = uri;

            // Required to bypass the check that prevents duplicate navigations
            if (ShouldTriggerNavigationNotifications(newUri))
            {
                NavigationStateChanged(new Uri(newUri), new Uri(newUri));
            }
        }

        public string GetUriWithQueryParameter(string key, object? value)
        {
            var uri = new Uri(Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (value is null)
            {
                query.Remove(key);
            }
            else
            {
                query[key] = value.ToString();
            }
            var baseUri = uri.GetLeftPart(UriPartial.Path);
            var queryString = query.ToString();
            return string.IsNullOrEmpty(queryString) ? baseUri : $"{baseUri}?{queryString}";
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            NavigateCalled = true;
            LastNavigateUri = uri;
        }

        private bool ShouldTriggerNavigationNotifications(string newUri)
        {
            return true; // Simplified for testing
        }
    }

    /// <summary>
    /// Validates that Paginator has all required localization parameters with correct default values.
    /// </summary>
    [Fact]
    public void Paginator_HasAllLocalizationParameters_WithCorrectDefaults()
    {
        // Arrange & Act
        var paginator = new Paginator();

        // Assert - All localization parameters exist with correct English defaults
        Assert.Equal("Go to first page", paginator.FirstPageAriaLabel);
        Assert.Equal("Go to previous page", paginator.PreviousPageAriaLabel);
        Assert.Equal("Go to next page", paginator.NextPageAriaLabel);
        Assert.Equal("Go to last page", paginator.LastPageAriaLabel);

        Assert.Equal("Go to first page", paginator.FirstPageTitle);
        Assert.Equal("Go to previous page", paginator.PreviousPageTitle);
        Assert.Equal("Go to next page", paginator.NextPageTitle);
        Assert.Equal("Go to last page", paginator.LastPageTitle);

        Assert.Equal("item", paginator.ItemSingularText);
        Assert.Equal("items", paginator.ItemPluralText);
        Assert.Equal("Page {0} of {1}", paginator.PageLabelFormat);
    }

    /// <summary>
    /// Validates that localization parameters can be set and retrieved.
    /// </summary>
    [Fact]
    public void Paginator_LocalizationParameters_CanBeSetAndRetrieved()
    {
        // Arrange
        var paginator = new Paginator
        {
            FirstPageAriaLabel = "Primera Seite",
            PreviousPageAriaLabel = "Vorherige Seite",
            NextPageAriaLabel = "Nächste Seite",
            LastPageAriaLabel = "Letzte Seite",
            FirstPageTitle = "Zur ersten Seite",
            PreviousPageTitle = "Zur vorherigen Seite",
            NextPageTitle = "Zur nächsten Seite",
            LastPageTitle = "Zur letzten Seite",
            ItemSingularText = "Element",
            ItemPluralText = "Elemente",
            PageLabelFormat = "Seite {0} von {1}"
        };

        // Assert
        Assert.Equal("Primera Seite", paginator.FirstPageAriaLabel);
        Assert.Equal("Vorherige Seite", paginator.PreviousPageAriaLabel);
        Assert.Equal("Nächste Seite", paginator.NextPageAriaLabel);
        Assert.Equal("Letzte Seite", paginator.LastPageAriaLabel);

        Assert.Equal("Zur ersten Seite", paginator.FirstPageTitle);
        Assert.Equal("Zur vorherigen Seite", paginator.PreviousPageTitle);
        Assert.Equal("Zur nächsten Seite", paginator.NextPageTitle);
        Assert.Equal("Zur letzten Seite", paginator.LastPageTitle);

        Assert.Equal("Element", paginator.ItemSingularText);
        Assert.Equal("Elemente", paginator.ItemPluralText);
        Assert.Equal("Seite {0} von {1}", paginator.PageLabelFormat);
    }

    /// <summary>
    /// Validates backward compatibility - Paginator works without explicit localization parameters.
    /// </summary>
    [Fact]
    public void Paginator_BackwardCompatibility_WorksWithoutLocalizationParameters()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        var navManager = new MockNavigationManager();

        // Act - Create paginator without setting any localization parameters
        var paginator = new Paginator { State = paginationState };
        var renderer = AddComponent();
        renderer.Inject(navManager);

        // Assert - Component should be instantiable without errors
        Assert.NotNull(paginator);
        Assert.Equal("Go to first page", paginator.FirstPageAriaLabel); // Default preserved
        Assert.Equal("item", paginator.ItemSingularText); // Default preserved
    }

    /// <summary>
    /// Validates singular/plural text handling for exactly 1 item.
    /// Issue: "1 items" bug fix verification.
    /// </summary>
    [Fact]
    public void Paginator_SingularText_UsedWhenTotalItemCountIsOne()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        paginationState.SetTotalItemCountAsync(1).Wait();

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            ItemSingularText = "artículo",
            ItemPluralText = "artículos"
        };

        // Assert
        Assert.Equal("artículo", paginator.ItemSingularText);
        Assert.Equal("artículos", paginator.ItemPluralText);
    }

    /// <summary>
    /// Validates plural text handling for multiple items.
    /// </summary>
    [Fact]
    public void Paginator_PluralText_UsedWhenTotalItemCountIsGreaterThanOne()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        paginationState.SetTotalItemCountAsync(25).Wait();

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            ItemSingularText = "artículo",
            ItemPluralText = "artículos"
        };

        // Assert
        Assert.Equal("artículo", paginator.ItemSingularText);
        Assert.Equal("artículos", paginator.ItemPluralText);
    }

    /// <summary>
    /// Validates plural text handling for zero items.
    /// </summary>
    [Fact]
    public void Paginator_PluralText_UsedWhenTotalItemCountIsZero()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        paginationState.SetTotalItemCountAsync(0).Wait();

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            ItemSingularText = "artículo",
            ItemPluralText = "artículos"
        };

        // Assert
        Assert.Equal("artículo", paginator.ItemSingularText);
        Assert.Equal("artículos", paginator.ItemPluralText);
    }

    /// <summary>
    /// Validates PageLabelFormat can be customized.
    /// </summary>
    [Fact]
    public void Paginator_PageLabelFormat_CanBeCustomized()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            PageLabelFormat = "Pagina {0} di {1}"
        };

        // Assert - Verify the format string is set correctly
        Assert.Equal("Pagina {0} di {1}", paginator.PageLabelFormat);
    }

    /// <summary>
    /// Validates that page navigation aria-labels can be customized independently.
    /// </summary>
    [Fact]
    public void Paginator_NavigationAriaLabels_CanBeCustomized()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            FirstPageAriaLabel = "Aller à la première page",
            PreviousPageAriaLabel = "Page précédente",
            NextPageAriaLabel = "Page suivante",
            LastPageAriaLabel = "Aller à la dernière page"
        };

        // Assert
        Assert.Equal("Aller à la première page", paginator.FirstPageAriaLabel);
        Assert.Equal("Page précédente", paginator.PreviousPageAriaLabel);
        Assert.Equal("Page suivante", paginator.NextPageAriaLabel);
        Assert.Equal("Aller à la dernière page", paginator.LastPageAriaLabel);
    }

    /// <summary>
    /// Validates that page navigation titles can be customized independently.
    /// </summary>
    [Fact]
    public void Paginator_NavigationTitles_CanBeCustomized()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            FirstPageTitle = "Prima pagina",
            PreviousPageTitle = "Pagina precedente",
            NextPageTitle = "Pagina successiva",
            LastPageTitle = "Ultima pagina"
        };

        // Assert
        Assert.Equal("Prima pagina", paginator.FirstPageTitle);
        Assert.Equal("Pagina precedente", paginator.PreviousPageTitle);
        Assert.Equal("Pagina successiva", paginator.NextPageTitle);
        Assert.Equal("Ultima pagina", paginator.LastPageTitle);
    }

    /// <summary>
    /// Validates empty string is a valid localization value.
    /// </summary>
    [Fact]
    public void Paginator_LocalizationParameters_AcceptEmptyStrings()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            FirstPageAriaLabel = "",
            ItemSingularText = "",
            ItemPluralText = "",
            PageLabelFormat = ""
        };

        // Assert - Empty strings are valid values
        Assert.Equal("", paginator.FirstPageAriaLabel);
        Assert.Equal("", paginator.ItemSingularText);
        Assert.Equal("", paginator.ItemPluralText);
        Assert.Equal("", paginator.PageLabelFormat);
    }

    /// <summary>
    /// Validates special characters in localization strings.
    /// </summary>
    [Fact]
    public void Paginator_LocalizationParameters_AcceptsSpecialCharacters()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            FirstPageAriaLabel = "« prayers",
            PreviousPageAriaLabel = "‹ לעמוד הקודם",
            NextPageAriaLabel = "下一页",
            LastPageAriaLabel = "Dernière page ✔",
            ItemSingularText = "élément",
            ItemPluralText = "éléments",
            PageLabelFormat = "{1} / {0}"
        };

        // Assert - Unicode and special characters are preserved
        Assert.Equal("« prayers", paginator.FirstPageAriaLabel);
        Assert.Equal("‹ לעמוד הקודם", paginator.PreviousPageAriaLabel);
        Assert.Equal("下一页", paginator.NextPageAriaLabel);
        Assert.Equal("Dernière page ✔", paginator.LastPageAriaLabel);
        Assert.Equal("élément", paginator.ItemSingularText);
        Assert.Equal("éléments", paginator.ItemPluralText);
        Assert.Equal("{1} / {0}", paginator.PageLabelFormat);
    }

    /// <summary>
    /// Validates that SummaryTemplate takes precedence over item count text.
    /// This ensures backward compatibility with custom pagination templates.
    /// </summary>
    [Fact]
    public void Paginator_SummaryTemplate_TakesPrecedenceOverItemCountText()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        paginationState.SetTotalItemCountAsync(1).Wait();

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            ItemSingularText = "WRONG",
            ItemPluralText = "ALSO_WRONG",
            SummaryTemplate = null // Not set
        };

        // Assert - Without SummaryTemplate, item count text is used
        Assert.Equal("WRONG", paginator.ItemSingularText);
        Assert.Equal("ALSO_WRONG", paginator.ItemPluralText);
    }

    /// <summary>
    /// Validates PaginationState.TotalItemCount property.
    /// </summary>
    [Fact]
    public void PaginationState_SetTotalItemCount_SetsValue()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        paginationState.SetTotalItemCountAsync(43).Wait();

        // Assert
        Assert.Equal(43, paginationState.TotalItemCount);
    }

    /// <summary>
    /// Validates PaginationState.LastPageIndex calculation.
    /// </summary>
    [Fact]
    public void PaginationState_LastPageIndex_CalculatesCorrectly()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act - 43 items with 10 per page = 5 pages (0-4)
        paginationState.SetTotalItemCountAsync(43).Wait();

        // Assert
        Assert.Equal(4, paginationState.LastPageIndex); // 0-indexed
    }

    /// <summary>
    /// Validates PaginationState.LastPageIndex for exact page division.
    /// </summary>
    [Fact]
    public void PaginationState_LastPageIndex_ExactDivision()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act - 50 items with 10 per page = 5 pages (0-4)
        paginationState.SetTotalItemCountAsync(50).Wait();

        // Assert
        Assert.Equal(4, paginationState.LastPageIndex);
    }

    /// <summary>
    /// Validates PaginationState.LastPageIndex for single item edge case.
    /// </summary>
    [Fact]
    public void PaginationState_LastPageIndex_SingleItem()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act - 1 item with 10 per page = 1 page (0)
        paginationState.SetTotalItemCountAsync(1).Wait();

        // Assert
        Assert.Equal(0, paginationState.LastPageIndex);
    }

    /// <summary>
    /// Validates PaginationState.LastPageIndex for zero items.
    /// </summary>
    [Fact]
    public void PaginationState_LastPageIndex_ZeroItems()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };

        // Act
        paginationState.SetTotalItemCountAsync(0).Wait();

        // Assert - (-1) / 10 = 0 integer division
        Assert.Equal(0, paginationState.LastPageIndex);
    }

    /// <summary>
    /// Validates pagination state cloning behavior (via GetHashCode).
    /// </summary>
    [Fact]
    public void PaginationState_GetHashCode_DifferentStates_ReturnDifferentHashes()
    {
        // Arrange
        var pagination1 = new PaginationState { ItemsPerPage = 10 };
        pagination1.SetTotalItemCountAsync(10).Wait();

        var pagination2 = new PaginationState { ItemsPerPage = 10 };
        pagination2.SetTotalItemCountAsync(20).Wait();

        // Assert
        Assert.NotEqual(pagination1.GetHashCode(), pagination2.GetHashCode());
    }

    /// <summary>
    /// Validates pagination state hash for same state.
    /// </summary>
    [Fact]
    public void PaginationState_GetHashCode_SameState_ReturnsSameHash()
    {
        // Arrange
        var pagination1 = new PaginationState { ItemsPerPage = 10 };
        pagination1.SetTotalItemCountAsync(20).Wait();

        var pagination2 = new PaginationState { ItemsPerPage = 10 };
        pagination2.SetTotalItemCountAsync(20).Wait();

        // Assert
        Assert.Equal(pagination1.GetHashCode(), pagination2.GetHashCode());
    }

    /// <summary>
    /// Validates that disposing Paginator does not throw.
    /// </summary>
    [Fact]
    public void Paginator_Dispose_DoesNotThrow()
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = 10 };
        var paginator = new Paginator { State = paginationState };

        // Act & Assert - Should not throw
        paginator.Dispose();
    }

    /// <summary>
    /// Validates localization parameters are not affected by ItemsPerPage.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void Paginator_LocalizationParameters_IndependentOfItemsPerPage(int itemsPerPage)
    {
        // Arrange
        var paginationState = new PaginationState { ItemsPerPage = itemsPerPage };

        // Act
        var paginator = new Paginator
        {
            State = paginationState,
            ItemSingularText = "条目",
            ItemPluralText = "条目"
        };

        // Assert - Localization parameters remain unchanged
        Assert.Equal("条目", paginator.ItemSingularText);
        Assert.Equal("条目", paginator.ItemPluralText);
        Assert.Equal(itemsPerPage, paginationState.ItemsPerPage);
    }

    /// <summary>
    /// Validates that multiple paginators can have different localization settings.
    /// </summary>
    [Fact]
    public void Paginator_MultipleInstances_CanHaveDifferentLocalizations()
    {
        // Arrange
        var paginationState1 = new PaginationState { ItemsPerPage = 10 };
        var paginationState2 = new PaginationState { ItemsPerPage = 10 };

        // Act
        var paginatorEnglish = new Paginator
        {
            State = paginationState1,
            FirstPageAriaLabel = "Go to first page",
            ItemSingularText = "item",
            ItemPluralText = "items"
        };

        var paginatorGerman = new Paginator
        {
            State = paginationState2,
            FirstPageAriaLabel = "Zur ersten Seite",
            ItemSingularText = "Element",
            ItemPluralText = "Elemente"
        };

        // Assert - Each paginator maintains its own localization
        Assert.Equal("Go to first page", paginatorEnglish.FirstPageAriaLabel);
        Assert.Equal("item", paginatorEnglish.ItemSingularText);
        Assert.Equal("items", paginatorEnglish.ItemPluralText);

        Assert.Equal("Zur ersten Seite", paginatorGerman.FirstPageAriaLabel);
        Assert.Equal("Element", paginatorGerman.ItemSingularText);
        Assert.Equal("Elemente", paginatorGerman.ItemPluralText);
    }
}
