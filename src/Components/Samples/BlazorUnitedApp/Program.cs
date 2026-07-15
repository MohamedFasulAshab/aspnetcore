// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Globalization;
using BlazorUnitedApp;
using BlazorUnitedApp.Data;
using BlazorUnitedApp.Localization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents();

// Register localization. The QuickGrid Paginator will detect IStringLocalizer<Paginator>
// from the DI container and use these .resx files to localize its UI.
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// The default IStringLocalizer<Paginator> registration would resolve against the
// QuickGrid assembly, where BlazorUnitedApp's translations do not live. Provide
// an adapter that points the Paginator's soft-lookup at our own resources.
builder.Services.AddSingleton<IStringLocalizer<Paginator>, PaginatorLocalizer>();

builder.Services.AddSingleton<WeatherForecastService>();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ta"),
    new CultureInfo("fr"),
    new CultureInfo("es")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture =
        new RequestCulture("en");

    options.SupportedCultures =
        supportedCultures;

    options.SupportedUICultures =
        supportedCultures;

    options.RequestCultureProviders.Insert(
        0,
        new CookieRequestCultureProvider());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/culture/set",
(
    HttpContext context,
    string culture,
    string redirectUri
) =>
{
    Console.WriteLine(
        $"SET CULTURE => {culture}");

    var cookieValue =
        CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(culture));

    Console.WriteLine(
        $"COOKIE VALUE => {cookieValue}");

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        cookieValue,
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            Path = "/"
        });

    context.Response.Redirect(redirectUri);

    return Task.CompletedTask;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
