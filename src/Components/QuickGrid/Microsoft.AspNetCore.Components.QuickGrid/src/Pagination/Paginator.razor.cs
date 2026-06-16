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

    [Inject]
    private IPaginatorLocalizer? PaginatorLocalizer { get; set; }

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
    /// Optionally supplies a custom localizer for pagination text localization.
    /// When set, this takes precedence over any injected <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public IPaginatorLocalizer? Localizer { get; set; }

    /// <summary>
    /// Optionally supplies a delegate to format the page label text.
    /// The delegate receives (currentPage, totalPages) using 1-based values.
    /// When set, this takes precedence over any values from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public Func<int, int, string>? PageLabelFormatter { get; set; }

    /// <summary>
    /// Optionally supplies a delegate to format the summary text.
    /// When set, this takes precedence over any values from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public Func<PaginationState, string>? SummaryFormatter { get; set; }

    /// <summary>
    /// Gets or sets the aria-label text for the first page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? FirstPageAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the aria-label text for the previous page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? PreviousPageAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the aria-label text for the next page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? NextPageAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the aria-label text for the last page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? LastPageAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the title/tooltip text for the first page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? FirstPageTitle { get; set; }

    /// <summary>
    /// Gets or sets the title/tooltip text for the previous page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? PreviousPageTitle { get; set; }

    /// <summary>
    /// Gets or sets the title/tooltip text for the next page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? NextPageTitle { get; set; }

    /// <summary>
    /// Gets or sets the title/tooltip text for the last page button.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? LastPageTitle { get; set; }

    /// <summary>
    /// Gets or sets the singular form text for items (used when item count is 1).
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? ItemSingularText { get; set; }

    /// <summary>
    /// Gets or sets the plural form text for items (used when item count is not 1).
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? ItemPluralText { get; set; }

    /// <summary>
    /// Gets or sets the format string for the page summary display.
    /// The format accepts {0} for current page and {1} for total pages.
    /// Overrides the value from <see cref="IPaginatorLocalizer"/>.
    /// </summary>
    [Parameter] public string? PageLabelFormat { get; set; }

    /// <summary>
    /// Gets the effective localizer to use for localization.
    /// Priority: 1. Explicit <see cref="Localizer"/> parameter, 2. Injected <see cref="IPaginatorLocalizer"/>, 3. Built-in culture-aware default.
    /// </summary>
    private IPaginatorLocalizer EffectiveLocalizer => Localizer ?? PaginatorLocalizer ?? ResourceManagerPaginatorLocalizer.Instance;

    /// <summary>
    /// Gets the resolved first page aria-label text.
    /// </summary>
    private string ResolvedFirstPageAriaLabel => FirstPageAriaLabel ?? EffectiveLocalizer.FirstPageAriaLabel;

    /// <summary>
    /// Gets the resolved previous page aria-label text.
    /// </summary>
    private string ResolvedPreviousPageAriaLabel => PreviousPageAriaLabel ?? EffectiveLocalizer.PreviousPageAriaLabel;

    /// <summary>
    /// Gets the resolved next page aria-label text.
    /// </summary>
    private string ResolvedNextPageAriaLabel => NextPageAriaLabel ?? EffectiveLocalizer.NextPageAriaLabel;

    /// <summary>
    /// Gets the resolved last page aria-label text.
    /// </summary>
    private string ResolvedLastPageAriaLabel => LastPageAriaLabel ?? EffectiveLocalizer.LastPageAriaLabel;

    /// <summary>
    /// Gets the resolved first page title text.
    /// </summary>
    private string ResolvedFirstPageTitle => FirstPageTitle ?? EffectiveLocalizer.FirstPageTitle;

    /// <summary>
    /// Gets the resolved previous page title text.
    /// </summary>
    private string ResolvedPreviousPageTitle => PreviousPageTitle ?? EffectiveLocalizer.PreviousPageTitle;

    /// <summary>
    /// Gets the resolved next page title text.
    /// </summary>
    private string ResolvedNextPageTitle => NextPageTitle ?? EffectiveLocalizer.NextPageTitle;

    /// <summary>
    /// Gets the resolved last page title text.
    /// </summary>
    private string ResolvedLastPageTitle => LastPageTitle ?? EffectiveLocalizer.LastPageTitle;

    /// <summary>
    /// Gets the resolved singular item text.
    /// </summary>
    private string ResolvedItemSingularText => ItemSingularText ?? EffectiveLocalizer.ItemSingularText;

    /// <summary>
    /// Gets the resolved plural item text.
    /// </summary>
    private string ResolvedItemPluralText => ItemPluralText ?? EffectiveLocalizer.ItemPluralText;

    /// <summary>
    /// Gets the resolved page label format string.
    /// </summary>
    private string ResolvedPageLabelFormat => PageLabelFormat ?? EffectiveLocalizer.PageLabelFormat;

    /// <summary>
    /// Gets the resolved items text (handles pluralization via advanced localizers when available).
    /// </summary>
    private string ResolvedItemsText
    {
        get
        {
            var count = State.TotalItemCount ?? 0;
            if (EffectiveLocalizer is Infrastructure.IPaginatorLocalizer2 advanced)
            {
                return advanced.Items(count);
            }

            return count == 1 ? ResolvedItemSingularText : ResolvedItemPluralText;
        }
    }

    /// <summary>
    /// Gets the resolved page label text.
    /// </summary>
    private string ResolvedPageLabelText
    {
        get
        {
            var current = State.CurrentPageIndex + 1;
            var total = (State.LastPageIndex ?? 0) + 1;
            if (PageLabelFormatter is not null)
            {
                return PageLabelFormatter(current, total);
            }
            if (EffectiveLocalizer is Infrastructure.IPaginatorLocalizer2 advanced)
            {
                return advanced.PageLabel(current, total);
            }
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, ResolvedPageLabelFormat, current, total);
        }
    }

    /// <summary>
    /// Gets the resolved summary text.
    /// </summary>
    private string ResolvedSummaryText
    {
        get
        {
            if (SummaryFormatter is not null)
            {
                return SummaryFormatter(State);
            }
            if (EffectiveLocalizer is Infrastructure.IPaginatorLocalizer2 advanced)
            {
                return advanced.Summary(State);
            }

            // Default summary: "<count> <items>"
            return $"{State.TotalItemCount} {ResolvedItemsText}";
        }
    }

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