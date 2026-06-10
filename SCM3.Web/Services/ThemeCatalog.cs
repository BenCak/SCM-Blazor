namespace SCM3.Web.Services;

// Site-wide Telerik theme registry (root CLAUDE.md §13). Each entry is one of the
// "kendo-theme-*" stylesheets shipped in the Telerik.UI.for.Blazor package's static web
// assets. `Users.Theme` stores the Key and is resolved both at initial SSR (App.razor, to
// avoid a flash of the wrong theme) and applied live via JS interop (scm3.applyTheme) when
// the user picks a different theme from the switcher in MainLayout.
public sealed record ThemeOption(string Key, string DisplayName, string CssHref, bool IsDark = false);

public static class ThemeCatalog
{
    public const string DefaultThemeKey = "default-ocean-blue";

    public static readonly IReadOnlyList<ThemeOption> Themes =
    [
        new("default", "Default", "_content/Telerik.UI.for.Blazor/css/kendo-theme-default/all.css"),
        new("default-ocean-blue", "Default (Ocean Blue)", "_content/Telerik.UI.for.Blazor/css/kendo-theme-default/default-ocean-blue.css"),
        new("bootstrap", "Bootstrap", "_content/Telerik.UI.for.Blazor/css/kendo-theme-bootstrap/all.css"),
        new("fluent", "Fluent", "_content/Telerik.UI.for.Blazor/css/kendo-theme-fluent/all.css"),
        new("material", "Material", "_content/Telerik.UI.for.Blazor/css/kendo-theme-material/all.css"),
        new("meridian", "Meridian", "_content/Telerik.UI.for.Blazor/css/kendo-theme-meridian/all.css"),
        new("dark", "Dark", "_content/Telerik.UI.for.Blazor/css/kendo-theme-default/default-ocean-blue.css", IsDark: true),
    ];

    public static ThemeOption Resolve(string? key)
        => Themes.FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase))
           ?? Themes.First(t => t.Key == DefaultThemeKey);
}
