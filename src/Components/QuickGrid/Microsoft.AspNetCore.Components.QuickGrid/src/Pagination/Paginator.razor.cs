// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.AspNetCore.Components.Routing;

namespace Microsoft.AspNetCore.Components.QuickGrid;

/// <summary>
/// A component that provides a user interface for <see cref="PaginationState"/>.
/// </summary>
public partial class Paginator : IDisposable
{
    private readonly EventCallbackSubscriber<PaginationState> _totalItemCountChanged;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    private string QueryName => State.QueryName;

    /// <summary>
    /// Specifies the associated <see cref="PaginationState"/>. This parameter is required.
    /// </summary>
    [Parameter, EditorRequired] public PaginationState State { get; set; } = default!;

    /// <summary>
    /// Optionally supplies a template for rendering the page count summary.
    /// </summary>
    [Parameter] public RenderFragment? SummaryTemplate { get; set; }

    /// <summary>
    /// Gets or sets the aria-label text for the first page button.
    /// Defaults to "Go to first page".
    /// </summary>
    [Parameter] public string FirstPageAriaLabel { get; set; } = "Go to first page";

    /// <summary>
    /// Gets or sets the aria-label text for the previous page button.
    /// Defaults to "Go to previous page".
    /// </summary>
    [Parameter] public string PreviousPageAriaLabel { get; set; } = "Go to previous page";

    /// <summary>
    /// Gets or sets the aria-label text for the next page button.
    /// Defaults to "Go to next page".
    /// </summary>
    [Parameter] public string NextPageAriaLabel { get; set; } = "Go to next page";

    /// <summary>
    /// Gets or sets the aria-label text for the last page button.
    /// Defaults to "Go to last page".
    /// </summary>
    [Parameter] public string LastPageAriaLabel { get; set; } = "Go to last page";

    /// <summary>
    /// Gets or sets the title/tooltip text for the first page button.
    /// Defaults to "Go to first page".
    /// </summary>
    [Parameter] public string FirstPageTitle { get; set; } = "Go to first page";

    /// <summary>
    /// Gets or sets the title/tooltip text for the previous page button.
    /// Defaults to "Go to previous page".
    /// </summary>
    [Parameter] public string PreviousPageTitle { get; set; } = "Go to previous page";

    /// <summary>
    /// Gets or sets the title/tooltip text for the next page button.
    /// Defaults to "Go to next page".
    /// </summary>
    [Parameter] public string NextPageTitle { get; set; } = "Go to next page";

    /// <summary>
    /// Gets or sets the title/tooltip text for the last page button.
    /// Defaults to "Go to last page".
    /// </summary>
    [Parameter] public string LastPageTitle { get; set; } = "Go to last page";

    /// <summary>
    /// Gets or sets the singular form text for items (used when item count is 1).
    /// Defaults to "item".
    /// </summary>
    [Parameter] public string ItemSingularText { get; set; } = "item";

    /// <summary>
    /// Gets or sets the plural form text for items (used when item count is not 1).
    /// Defaults to "items".
    /// </summary>
    [Parameter] public string ItemPluralText { get; set; } = "items";

    /// <summary>
    /// Gets or sets the format string for the page summary display.
    /// The format accepts {0} for current page and {1} for total pages.
    /// Defaults to "Page {0} of {1}".
    /// </summary>
    [Parameter] public string PageLabelFormat { get; set; } = "Page {0} of {1}";

    /// <summary>
    /// Constructs an instance of <see cref="Paginator" />.
    /// </summary>
    public Paginator()
    {
        // The "total item count" handler doesn't need to do anything except cause this component to re-render
        _totalItemCountChanged = new(new EventCallback<PaginationState>(this, null));
        _queryParameterValueSupplier = new();
    }

    private readonly QueryParameterValueSupplier _queryParameterValueSupplier;

    private string GetPageUrl(int pageIndex)
    {
        int? pageValue = pageIndex == 0 ? null : pageIndex + 1;
        return NavigationManager.GetUriWithQueryParameter(QueryName, pageValue);
    }

    private Task GoFirstAsync() => GoToPageAsync(0);
    private Task GoPreviousAsync() => GoToPageAsync(State.CurrentPageIndex - 1);
    private Task GoNextAsync() => GoToPageAsync(State.CurrentPageIndex + 1);
    private Task GoLastAsync() => GoToPageAsync(State.LastPageIndex.GetValueOrDefault(0));

    private bool CanGoBack => State.CurrentPageIndex > 0;
    private bool CanGoForwards => State.CurrentPageIndex < State.LastPageIndex;
    private Task GoToPageAsync(int pageIndex)
    {
        NavigationManager.NavigateTo(GetPageUrl(pageIndex));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync()
    {
        _totalItemCountChanged.SubscribeOrMove(State.TotalItemCountChangedSubscribable);

        _queryParameterValueSupplier.ReadParametersFromQuery(QueryParameterValueSupplier.GetQueryString(NavigationManager.Uri));
        var pageFromQuery = ReadPageIndexFromQueryString() ?? 0;
        if (pageFromQuery != State.CurrentPageIndex)
        {
            return State.SetCurrentPageIndexAsync(pageFromQuery);
        }

        return Task.CompletedTask;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _queryParameterValueSupplier.ReadParametersFromQuery(QueryParameterValueSupplier.GetQueryString(NavigationManager.Uri));
        var pageFromQuery = ReadPageIndexFromQueryString() ?? 0;
        await InvokeAsync(async () =>
        {
            if (pageFromQuery != State.CurrentPageIndex)
            {
                await State.SetCurrentPageIndexAsync(pageFromQuery);
            }
            StateHasChanged();
        });
    }

    private int? ReadPageIndexFromQueryString()
    {
        var value = _queryParameterValueSupplier.GetQueryParameterValue(typeof(string), QueryName) as string;
        if (value is not null && int.TryParse(value, out var page) && page > 0)
        {
            return page - 1;
        }

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        _totalItemCountChanged.Dispose();
    }
}
