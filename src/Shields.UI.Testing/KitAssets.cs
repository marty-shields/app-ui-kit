namespace Shields.UI.Testing;

/// <summary>The kit's scripts, embedded from <c>src/Shields.UI/wwwroot</c> when the package is built. The stylesheets are in <see cref="KitCss"/>.</summary>
public static class KitAssets
{
    public static string ThemeJs { get; } = EmbeddedText.Read("theme.js");
}
