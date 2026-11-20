using System.Text.RegularExpressions;

namespace Shields.UI.Testing;

/// <summary>No literal colours or sizes outside a tokens file, and only declared tokens used. Each method returns what it found; empty means the check holds.</summary>
public static partial class TokensOnlyCheck
{
    // Literal values every app may keep: the safe-area fallbacks and the visually-hidden helper in base.css.
    private static readonly HashSet<string> BaseAllowed =
    [
        "base.css: --inset-top: env(safe-area-inset-top, 0px);",
        "base.css: --inset-bottom: env(safe-area-inset-bottom, 0px);",
        "base.css: width: 1px;",
        "base.css: height: 1px;",
        "base.css: margin: -1px;",
    ];

    /// <summary>Lines of a stylesheet with a literal colour or size, as <c>file.css: line</c>, less the allowed lines.</summary>
    /// <param name="allowed">Further lines to accept, in the same <c>file.css: line</c> form.</param>
    public static IReadOnlyList<string> Literals(string css, string fileName, IEnumerable<string>? allowed = null)
    {
        var accepted = BaseAllowed.Concat(allowed ?? []).ToHashSet();
        return StripComments(css).Split('\n')
            .Select(line => line.Trim())
            .Where(line => Literal().IsMatch(line))
            .Select(line => $"{fileName}: {line}")
            .Where(line => !accepted.Contains(line))
            .ToList();
    }

    /// <summary>Literals of every <c>.css</c> file under a folder except tokens.css, which is where literals belong.</summary>
    public static IReadOnlyList<string> LiteralsInFolder(string folder, IEnumerable<string>? allowed = null) =>
        CssFiles.Find(folder, "*.css")
            .Where(file => Path.GetFileName(file) != "tokens.css")
            .SelectMany(file => Literals(File.ReadAllText(file), Path.GetFileName(file), allowed))
            .ToList();

    /// <summary>Inline <c>style="..."</c> values in markup that hold a literal colour or size.</summary>
    public static IReadOnlyList<string> InlineStyleLiterals(string markup) =>
        InlineStyle().Matches(markup)
            .Select(m => m.Groups[1].Value)
            .Where(style => Literal().IsMatch(style))
            .ToList();

    /// <summary>Inline style literals of every <c>.razor</c> file under a folder, as <c>file.razor: style</c>.</summary>
    public static IReadOnlyList<string> InlineStyleLiteralsInFolder(string folder) =>
        CssFiles.Find(folder, "*.razor")
            .SelectMany(file => InlineStyleLiterals(File.ReadAllText(file)).Select(style => $"{Path.GetFileName(file)}: {style}"))
            .ToList();

    /// <summary>Tokens a stylesheet reads through <c>var()</c> that are neither declared in it nor in <paramref name="known"/>, which defaults to the kit's tokens.</summary>
    public static IReadOnlyList<string> UnknownTokens(string css, IEnumerable<string>? known = null) =>
        DesignTokens.Uses(css).Except(known ?? DesignTokens.Names).Except(DesignTokens.Declares(css)).Distinct().ToList();

    /// <summary>Lines using the <c>background</c> shorthand, which resets the pressed layer's background-image.</summary>
    public static IReadOnlyList<string> BackgroundShorthands(string css) =>
        StripComments(css).Split('\n')
            .Select(line => line.Trim())
            .Where(line => BackgroundShorthand().IsMatch(line))
            .ToList();

    private static string StripComments(string css) => Comment().Replace(css, "");

    [GeneratedRegex(@"(?<![-\w])background\s*:")]
    private static partial Regex BackgroundShorthand();

    // Hex, colour functions, named colours in a value, sizes with units, and percentages other than 100%.
    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|\b(rgba?|hsla?|hwb|lab|lch|oklab|oklch|color|color-mix)\(|(?<=:[^;{}]*)\b(" + NamedColours + @")\b|\d(px|rem|em|vh|vw|vmin|vmax|pt)\b|(?<![\d.])(?!100%)\d+(\.\d+)?%", RegexOptions.IgnoreCase)]
    private static partial Regex Literal();

    private const string NamedColours =
        "aliceblue|antiquewhite|aqua|aquamarine|azure|beige|bisque|black|blanchedalmond|blue|blueviolet|brown|burlywood|" +
        "cadetblue|chartreuse|chocolate|coral|cornflowerblue|cornsilk|crimson|cyan|darkblue|darkcyan|darkgoldenrod|darkgray|" +
        "darkgreen|darkgrey|darkkhaki|darkmagenta|darkolivegreen|darkorange|darkorchid|darkred|darksalmon|darkseagreen|" +
        "darkslateblue|darkslategray|darkslategrey|darkturquoise|darkviolet|deeppink|deepskyblue|dimgray|dimgrey|dodgerblue|" +
        "firebrick|floralwhite|forestgreen|fuchsia|gainsboro|ghostwhite|gold|goldenrod|gray|green|greenyellow|grey|honeydew|" +
        "hotpink|indianred|indigo|ivory|khaki|lavender|lavenderblush|lawngreen|lemonchiffon|lightblue|lightcoral|lightcyan|" +
        "lightgoldenrodyellow|lightgray|lightgreen|lightgrey|lightpink|lightsalmon|lightseagreen|lightskyblue|lightslategray|" +
        "lightslategrey|lightsteelblue|lightyellow|lime|limegreen|linen|magenta|maroon|mediumaquamarine|mediumblue|" +
        "mediumorchid|mediumpurple|mediumseagreen|mediumslateblue|mediumspringgreen|mediumturquoise|mediumvioletred|" +
        "midnightblue|mintcream|mistyrose|moccasin|navajowhite|navy|oldlace|olive|olivedrab|orange|orangered|orchid|" +
        "palegoldenrod|palegreen|paleturquoise|palevioletred|papayawhip|peachpuff|peru|pink|plum|powderblue|purple|" +
        "rebeccapurple|red|rosybrown|royalblue|saddlebrown|salmon|sandybrown|seagreen|seashell|sienna|silver|skyblue|" +
        "slateblue|slategray|slategrey|snow|springgreen|steelblue|tan|teal|thistle|tomato|turquoise|violet|wheat|white|" +
        "whitesmoke|yellow|yellowgreen";

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex("\\bstyle=\"([^\"]*)\"")]
    private static partial Regex InlineStyle();
}
