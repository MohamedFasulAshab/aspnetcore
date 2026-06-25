// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Components.QuickGrid;

/// <summary>
/// Base localizer that applications can override to provide QuickGrid translations.
/// </summary>
public class QuickGridLocalizer
{

/// <summary>
/// Gets the localized string for the specified key.
/// </summary>
/// <param name="key">The localization key.</param>
/// <returns>
/// A <see cref="Microsoft.Extensions.Localization.LocalizedString"/> for the requested key.
/// </returns>
public virtual LocalizedString this[string key] => new(key, key, resourceNotFound: true);

/// <summary>
/// Gets the localized string for the specified key and formats it with the supplied arguments.
/// </summary>
/// <param name="key">The localization key.</param>
/// <param name="arguments">Arguments used to format the localized string.</param>
/// <returns>
/// A <see cref="Microsoft.Extensions.Localization.LocalizedString"/> for the requested key.
/// </returns>
public virtual LocalizedString this[string key, params object[] arguments]
{
    get
    {
        var localizedString = this[key];

        if (arguments is null || arguments.Length == 0)
        {
            return localizedString;
        }

        var formattedValue = string.Format(CultureInfo.CurrentCulture, localizedString.Value, arguments);
        return new LocalizedString(localizedString.Name, formattedValue, localizedString.ResourceNotFound);
    }
}

}
