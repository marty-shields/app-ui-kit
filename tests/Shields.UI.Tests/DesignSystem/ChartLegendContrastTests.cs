using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class ChartLegendContrastTests
{
    private const double Text = 4.5;
    private const double Graphic = 3.0;

    private static readonly string Css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "ChartLegend.razor.css");

    private static IEnumerable<TestCaseData> Cases() =>
        new[] { ("marker", "color", Text), ("swatch.accent", "background-color", Graphic), ("swatch.chip", "background-color", Graphic), ("swatch.error", "background-color", Graphic), ("swatch.outline", "border", Graphic), ("swatch.line", "border-top", Graphic) }
            .SelectMany(c => new[] { "--color-bg", "--color-surface" }.SelectMany(background => Enum.GetValues<ColourScheme>()
                .Select(scheme => new TestCaseData(c.Item1, c.Item2, background, c.Item3, scheme).SetArgDisplayNames($".{c.Item1} {c.Item2} on {background} ({scheme})"))));

    [TestCaseSource(nameof(Cases))]
    public void LegendColour_IsReadableOnTheBackground(string selector, string property, string background, double minimum, ColourScheme scheme)
    {
        var token = Declaration(selector, property);
        var fg = DesignTokens.Colour(token).In(scheme);
        var bg = DesignTokens.Colour(background).In(scheme);

        Assert.That(Rgb.Contrast(fg, bg), Is.GreaterThanOrEqualTo(minimum), $"{token} {fg} on {bg}");
    }

    private static string Declaration(string selector, string property) =>
        Declared().Matches(Css)
            .Where(m => m.Groups[1].Value.Trim() == "." + selector)
            .Select(m => Regex.Match(m.Groups[2].Value, @"(?<![-\w])" + property + @":[^;]*var\((--[\w-]+)\)"))
            .Single(m => m.Success).Groups[1].Value;

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex Declared();
}
