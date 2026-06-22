// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

public class PaginatorLocalizationTest
{
    [Fact]
    public async Task PaginatorUsesLocalizedStringsFromStringLocalizerFactory()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");

            var serviceProvider = new ServiceCollection()
                .AddSingleton<NavigationManager, TestNavigationManager>()
                .AddSingleton<IStringLocalizerFactory>(new TestQuickGridLocalizerFactory(
                    new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
                    {
                        ["es"] = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["Paginator_ItemsSummary"] = "{0:N0} artículos",
                            ["Paginator_Page"] = "Página",
                            ["Paginator_Of"] = "de",
                            ["Paginator_GoToFirstPage"] = "Ir a la primera página",
                            ["Paginator_GoToPreviousPage"] = "Ir a la página anterior",
                            ["Paginator_GoToNextPage"] = "Ir a la página siguiente",
                            ["Paginator_GoToLastPage"] = "Ir a la última página",
                        }
                    }))
                .BuildServiceProvider();

            var renderer = new TestRenderer(serviceProvider);
            var paginator = renderer.InstantiateComponent<Paginator>();
            var componentId = renderer.AssignRootComponentId(paginator);

            var state = new PaginationState();
            typeof(PaginationState).GetProperty("QueryName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(state, "page");
            await state.SetTotalItemCountAsync(1200);

            renderer.RenderRootComponent(componentId, ParameterView.FromDictionary(new Dictionary<string, object>
            {
                [nameof(Paginator.State)] = state,
            }));

            var frames = renderer.GetCurrentRenderTreeFrames(componentId).AsEnumerable();
            var textContents = new List<string>();
            var attributeValues = new List<string>();

            foreach (var frame in frames)
            {
                if (frame.FrameType == RenderTreeFrameType.Text)
                {
                    textContents.Add(frame.TextContent);
                }
                else if (frame.FrameType == RenderTreeFrameType.Attribute)
                {
                    attributeValues.Add(frame.AttributeValue?.ToString() ?? string.Empty);
                }
            }

            Assert.Contains("Página", textContents);
            Assert.Contains("de", textContents);
            Assert.Contains("1.200 artículos", textContents);

            Assert.Contains("Ir a la primera página", attributeValues);
            Assert.Contains("Ir a la página anterior", attributeValues);
            Assert.Contains("Ir a la página siguiente", attributeValues);
            Assert.Contains("Ir a la última página", attributeValues);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }
}

internal sealed class TestQuickGridLocalizerFactory : IStringLocalizerFactory
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _translations;

    public TestQuickGridLocalizerFactory(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> translations)
    {
        _translations = translations;
    }

    public IStringLocalizer Create(Type resourceSource) => new TestQuickGridLocalizer(_translations);

    public IStringLocalizer Create(string baseName, string location) => new TestQuickGridLocalizer(_translations);
}

internal sealed class TestQuickGridLocalizer : IStringLocalizer
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _translations;

    public TestQuickGridLocalizer(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> translations)
    {
        _translations = translations;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (_translations.TryGetValue(language, out var cultureTranslations) && cultureTranslations.TryGetValue(name, out var value))
            {
                return new LocalizedString(name, value, resourceNotFound: false);
            }

            return new LocalizedString(name, name, resourceNotFound: true);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localized = this[name];
            if (localized.ResourceNotFound)
            {
                return localized;
            }

            return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, localized.Value, arguments), resourceNotFound: false);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
}
