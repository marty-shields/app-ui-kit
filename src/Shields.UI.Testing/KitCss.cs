namespace Shields.UI.Testing;

/// <summary>The kit's tokens.css, base.css and scoped CSS bundle, embedded from <c>src/Shields.UI</c> when the package is built.</summary>
public static class KitCss
{
    public static string Tokens { get; } = EmbeddedText.Read("tokens.css");

    public static string Base { get; } = EmbeddedText.Read("base.css");

    /// <summary>The components' scoped styles (<c>Shields.UI.bundle.scp.css</c>), as the kit's static web assets serve them.</summary>
    public static string Bundle { get; } = EmbeddedText.Read("bundle.scp.css");
}
