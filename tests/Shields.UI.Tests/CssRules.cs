using System.Text.RegularExpressions;

namespace Shields.UI.Tests;

/// <summary>Declarations of a rule in a Layout component's scoped stylesheet, by property.</summary>
internal static partial class CssRules
{
    /// <summary>Property values of the rule whose selector is exactly the one given, ignoring comments, order and spacing.</summary>
    public static IReadOnlyDictionary<string, string> Declarations(string component, string selector)
    {
        var css = CommentPattern().Replace(RepoFiles.Read("src", "Shields.UI", "Components", "Layout", component + ".razor.css"), "");
        var match = Regex.Match(css, @"(?:^|})\s*" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}");
        Assert.That(match.Success, Is.True, $"{component}.razor.css has no rule for {selector}");

        return match.Groups["body"].Value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.Split(':', 2, StringSplitOptions.TrimEntries))
            .ToDictionary(d => d[0], d => WhitespacePattern().Replace(d[1], " "));
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();
}
