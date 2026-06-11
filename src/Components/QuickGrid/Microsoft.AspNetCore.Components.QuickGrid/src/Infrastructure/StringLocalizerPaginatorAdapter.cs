// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// An <see cref="IPaginatorLocalizer"/> adapter that wraps an <see cref="IStringLocalizer"/>
/// to integrate with ASP.NET Core's localization system.
/// </summary>
/// <remarks>
/// This adapter enables the use of resource files (.resx) with <see cref="IStringLocalizer"/>
/// for localizing QuickGrid pagination text.
///
/// <para>
/// To use this adapter, register it in your service collection:
/// <code>
/// builder.Services.AddQuickGridLocalization();
/// </code>
/// </para>
/// </remarks>
public class StringLocalizerPaginatorAdapter : IPaginatorLocalizer
{
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Resource base name used for looking up QuickGrid pagination localization strings.
    /// </summary>
    public static string ResourceBaseName { get; } = "Microsoft.AspNetCore.Components.QuickGrid.Resources.Paginator";

    /// <summary>
    /// Creates a new instance of <see cref="StringLocalizerPaginatorAdapter"/>.
    /// </summary>
    /// <param name="localizer">The underlying string localizer to use for translations.</param>
    public StringLocalizerPaginatorAdapter(IStringLocalizer localizer)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    /// <inheritdoc />
    public string FirstPageAriaLabel => _localizer[nameof(FirstPageAriaLabel)];

    /// <inheritdoc />
    public string PreviousPageAriaLabel => _localizer[nameof(PreviousPageAriaLabel)];

    /// <inheritdoc />
    public string NextPageAriaLabel => _localizer[nameof(NextPageAriaLabel)];

    /// <inheritdoc />
    public string LastPageAriaLabel => _localizer[nameof(LastPageAriaLabel)];

    /// <inheritdoc />
    public string FirstPageTitle => _localizer[nameof(FirstPageTitle)];

    /// <inheritdoc />
    public string PreviousPageTitle => _localizer[nameof(PreviousPageTitle)];

    /// <inheritdoc />
    public string NextPageTitle => _localizer[nameof(NextPageTitle)];

    /// <inheritdoc />
    public string LastPageTitle => _localizer[nameof(LastPageTitle)];

    /// <inheritdoc />
    public string ItemSingularText => _localizer[nameof(ItemSingularText)];

    /// <inheritdoc />
    public string ItemPluralText => _localizer[nameof(ItemPluralText)];

    /// <inheritdoc />
    public string PageLabelFormat => _localizer[nameof(PageLabelFormat)];
}

/// <summary>
/// Extension methods for registering QuickGrid localization services.
/// </summary>
public static class PaginatorLocalizerExtensions
{
    /// <summary>
    /// Adds QuickGrid pagination localization services using the specified <see cref="IStringLocalizer"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="localizer">The <see cref="IStringLocalizer"/> to use for translations.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// This method registers a <see cref="StringLocalizerPaginatorAdapter"/> that implements <see cref="IPaginatorLocalizer"/>.
    /// The underlying localizer will be resolved from the service provider when needed.
    /// </remarks>
    public static IServiceCollection AddQuickGridLocalization(
        [NotNull] this IServiceCollection services,
        [NotNull] IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(localizer);

        services.AddSingleton<IPaginatorLocalizer>(new StringLocalizerPaginatorAdapter(localizer));
        return services;
    }

    /// <summary>
    /// Adds QuickGrid pagination localization services using the specified <see cref="IStringLocalizerFactory"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="factory">The <see cref="IStringLocalizerFactory"/> to use for creating localizers.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddQuickGridLocalization(
        [NotNull] this IServiceCollection services,
        [NotNull] IStringLocalizerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton<IPaginatorLocalizer>(sp =>
        {
            var localizer = factory.Create(typeof(StringLocalizerPaginatorAdapter));
            return new StringLocalizerPaginatorAdapter(localizer);
        });
        return services;
    }

    /// <summary>
    /// Adds QuickGrid pagination localization services using the specified resource type
    /// and the application's <see cref="IStringLocalizerFactory"/>.
    /// </summary>
    /// <typeparam name="T">A type whose namespace and assembly are used to determine the default resource base name.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddQuickGridLocalization<T>(
        [NotNull] this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPaginatorLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<IStringLocalizerFactory>();
            var localizer = factory.Create(typeof(T));
            return new StringLocalizerPaginatorAdapter(localizer);
        });
        return services;
    }

    /// <summary>
    /// Adds QuickGrid pagination localization services using a custom <see cref="IPaginatorLocalizer"/> implementation.
    /// </summary>
    /// <typeparam name="T">The <see cref="IPaginatorLocalizer"/> implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddQuickGridCustomLocalization<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        [NotNull] this IServiceCollection services)
        where T : class, IPaginatorLocalizer
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPaginatorLocalizer, T>();
        return services;
    }
}
