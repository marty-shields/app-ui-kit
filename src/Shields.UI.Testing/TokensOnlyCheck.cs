using System.Text.RegularExpressions;

namespace Shields.UI.Testing;

/// <summary>No literal colours or sizes outside a tokens file, and only declared tokens used. Each method returns what it found; empty means the check holds.</summary>
public static partial class TokensOnlyCheck
{
    // Literal lines the kit's own base.css keeps: the safe-area fallbacks and the visually-hidden helper.
    private static readonly string[] BaseAllowed =
    [
        "--inset-top: env(safe-area-inset-top, 0px);",
        "--inset-bottom: env(safe-area-inset-bottom, 0px);",
        "width: 1px;",
        "height: 1px;",
        "margin: -1px;",
    ];

    /// <summary>Lines of a stylesheet with a literal colour or size, as <c>name: line</c>, less the allowed lines. The kit's own base.css keeps its safe-area and visually-hidden literals; an app's stylesheet does not.</summary>
    /// <param name="name">How the file is reported: its file name, or its path inside the checked folder.</param>
    /// <param name="allowed">Lines to accept, in the same <c>name: line</c> form.</param>
    public static IReadOnlyList<string> Literals(string css, string name, IEnumerable<string>? allowed = null)
    {
        var accepted = (allowed ?? []).ToHashSet();
        if (IsKitBase(css))
        {
            accepted.UnionWith(BaseAllowed.Select(line => $"{name}: {line}"));
        }

        return SourceFiles.StripComments(css).Split('\n')
            .Select(line => line.Trim())
            .Where(line => Literal().IsMatch(line))
            .Select(line => $"{name}: {line}")
            .Where(line => !accepted.Contains(line))
            .ToList();
    }

    /// <summary>Literals of every <c>.css</c> file under a folder except tokens.css, which is where literals belong. Files are reported, and allowed, by path relative to the folder with <c>/</c> separators.</summary>
    public static IReadOnlyList<string> LiteralsInFolder(string folder, IEnumerable<string>? allowed = null) =>
        SourceFiles.Find(folder, "*.css")
            .Where(file => Path.GetFileName(file) != "tokens.css")
            .SelectMany(file => Literals(File.ReadAllText(file), SourceFiles.Relative(folder, file), allowed))
            .ToList();

    /// <summary>Inline <c>style="..."</c> values in markup that hold a literal colour or size.</summary>
    public static IReadOnlyList<string> InlineStyleLiterals(string markup) =>
        InlineStyle().Matches(markup)
            .Select(m => m.Groups[1].Value)
            .Where(style => Literal().IsMatch(style))
            .ToList();

    /// <summary>Inline style literals of every <c>.razor</c> file under a folder, as <c>file.razor: style</c>.</summary>
    public static IReadOnlyList<string> InlineStyleLiteralsInFolder(string folder) =>
        SourceFiles.Find(folder, "*.razor")
            .SelectMany(file => InlineStyleLiterals(File.ReadAllText(file)).Select(style => $"{Path.GetFileName(file)}: {style}"))
            .ToList();

    /// <summary>Tokens a stylesheet reads through <c>var()</c> that are not declared in it, in the kit, or in <paramref name="known"/>, which adds an app's own tokens to the kit's.</summary>
    public static IReadOnlyList<string> UnknownTokens(string css, IEnumerable<string>? known = null) =>
        DesignTokens.Uses(css).Except(DesignTokens.Names).Except(known ?? []).Except(DesignTokens.Declares(css)).Distinct().ToList();

    /// <summary>Lines using the <c>background</c> shorthand, which resets the pressed layer's background-image.</summary>
    public static IReadOnlyList<string> BackgroundShorthands(string css) =>
        SourceFiles.StripComments(css).Split('\n')
            .Select(line => line.Trim())
            .Where(line => BackgroundShorthand().IsMatch(line))
            .ToList();

    private static bool IsKitBase(string css) => Normalise(css) == Normalise(KitCss.Base);

    private static string Normalise(string text) => text.Replace("\r\n", "\n");

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

    [GeneratedRegex("\\bstyle=\"([^\"]*)\"")]
    private static partial Regex InlineStyle();
}
