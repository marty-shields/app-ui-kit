using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

/// <summary>Custom properties declared in tokens.css and base.css.</summary>
internal static partial class DesignTokens
{
    private static readonly string TokensCss = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");
    private static readonly string BaseCss = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "base.css");

    /// <summary>Names of every custom property declared in tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Names { get; } =
        Declaration().Matches(TokensCss + BaseCss).Select(m => m.Groups[1].Value).ToHashSet();

    /// <summary>Custom properties used through <c>var()</c> in a piece of CSS.</summary>
    public static IEnumerable<string> Uses(string css) => Use().Matches(css).Select(m => m.Groups[1].Value);

    /// <summary>Custom properties declared in a piece of CSS.</summary>
    public static IEnumerable<string> Declares(string css) => Declaration().Matches(css).Select(m => m.Groups[1].Value);

    [GeneratedRegex(@"^\s*(--[a-z0-9-]+)\s*:([^;]*);", RegexOptions.Multiline)]
    private static partial Regex Declaration();

    [GeneratedRegex(@"var\((--[a-z0-9-]+)")]
    private static partial Regex Use();
}
