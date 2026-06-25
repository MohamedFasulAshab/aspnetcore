// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Components.QuickGrid;

internal sealed class InternalQuickGridLocalizer
{
    private readonly QuickGridLocalizer? _quickGridLocalizer;
    private readonly IStringLocalizer _defaultLocalizer;

    public InternalQuickGridLocalizer(
        IStringLocalizerFactory localizerFactory,
        QuickGridLocalizer? quickGridLocalizer = null)
    {
        _quickGridLocalizer = quickGridLocalizer;

        _defaultLocalizer = localizerFactory.Create(
            "Microsoft.AspNetCore.Components.QuickGrid.Resources.QuickGridLocalization",
            typeof(InternalQuickGridLocalizer).Assembly.GetName().Name!);
    }

    public LocalizedString this[string key] => Get(key);

    public LocalizedString this[string key, params object[] arguments] => Get(key, arguments);

    private LocalizedString Get(string key, params object[] arguments)
    {
        if (_quickGridLocalizer is not null)
        {
            var customValue = arguments is { Length: > 0 }
                ? _quickGridLocalizer[key, arguments]
                : _quickGridLocalizer[key];

            if (!customValue.ResourceNotFound)
            {
                return customValue;
            }
        }

        return arguments is { Length: > 0 }
            ? _defaultLocalizer[key, arguments]
            : _defaultLocalizer[key];
    }
}