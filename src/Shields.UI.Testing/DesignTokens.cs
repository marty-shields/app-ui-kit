using System.Text.RegularExpressions;

namespace Shields.UI.Testing;

/// <summary>Custom properties declared in the kit's tokens.css and base.css. The overloads taking CSS text read an app's own file the same way.</summary>
public static partial class DesignTokens
{
    /// <summary>Names of every custom property declared in the kit's tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Names { get; } = Declares(KitCss.Tokens + KitCss.Base).ToHashSet();

    /// <summary>Names of the custom properties declared in the kit's tokens.css, in file order.</summary>
    public static IReadOnlyList<string> TokensCssNames { get; } = Declares(KitCss.Tokens).ToList();

    /// <summary>Light and dark value of a colour token in the kit's tokens.css.</summary>
    public static ThemedColour Colour(string name) => Colour(KitCss.Tokens, name);

    /// <summary>Light and dark value of a colour token: a <c>light-dark()</c> pair or one hex value for both.</summary>
    public static ThemedColour Colour(string css, string name)
    {
        var value = Value(css, name);
        var pair = LightDark().Match(value);
        if (pair.Success)
        {
            return new(ParseHex(name, pair.Groups[1].Value.Trim()), ParseHex(name, pair.Groups[2].Value.Trim()));
        }

        var single = ParseHex(name, value);
        return new(single, single);
    }

    /// <summary>Trimmed value of a custom property declared in the kit's tokens.css.</summary>
    public static string Value(string name) => Value(KitCss.Tokens, name);

    /// <summary>Trimmed value of a custom property declared in a piece of CSS.</summary>
    public static string Value(string css, string name) =>
        Declaration().Matches(css).Single(m => m.Groups[1].Value == name).Groups[2].Value.Trim();

    /// <summary>Custom properties used through <c>var()</c> in a piece of CSS.</summary>
    public static IEnumerable<string> Uses(string css) => Use().Matches(css).Select(m => m.Groups[1].Value);

    /// <summary>Custom properties declared in a piece of CSS.</summary>
    public static IEnumerable<string> Declares(string css) => Declaration().Matches(css).Select(m => m.Groups[1].Value);

    private static Rgb ParseHex(string name, string text) =>
        Hex().IsMatch(text)
            ? Rgb.Parse(text)
            : throw new FormatException($"{name}: expected a single #rrggbb colour but found '{text}'.");

    [GeneratedRegex(@"^\s*(--[a-z0-9-]+)\s*:([^;]*);", RegexOptions.Multiline)]
    private static partial Regex Declaration();

    [GeneratedRegex(@"^light-dark\(([^,()]*),([^,()]*)\)$")]
    private static partial Regex LightDark();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex Hex();

    [GeneratedRegex(@"var\((--[a-z0-9-]+)")]
    private static partial Regex Use();
}
