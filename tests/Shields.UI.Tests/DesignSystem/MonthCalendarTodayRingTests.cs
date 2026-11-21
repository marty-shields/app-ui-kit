using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class MonthCalendarTodayRingTests
{
    private const double Graphic = ContrastCheck.LargeTextOrControl;

    private static readonly string Css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "MonthCalendar.razor.css");

    // The ring is an inner gap (touching the cell's fill) and an outer ring (touching what the cell sits on).
    private static readonly string[] Fills = ["--color-chip", "--color-accent", "--color-error"];
    private static readonly string[] Surfaces = ["--color-bg", "--color-surface"];

    private static IEnumerable<TestCaseData> FillCases() =>
        Fills.SelectMany(fill => Enum.GetValues<ColourScheme>()
            .Select(scheme => new TestCaseData(fill, scheme).SetArgDisplayNames($"inner ring on {fill} ({scheme})")));

    private static IEnumerable<TestCaseData> SurfaceCases() =>
        Surfaces.SelectMany(surface => Enum.GetValues<ColourScheme>()
            .Select(scheme => new TestCaseData(surface, scheme).SetArgDisplayNames($"outer ring on {surface} ({scheme})")));

    [Test]
    public void TodayRing_IsAnInnerGapAndAnOuterRing()
    {
        Assert.That(Ring(), Has.Count.EqualTo(2));
    }

    [TestCaseSource(nameof(FillCases))]
    public void TodayRing_InnerRing_ContrastsWithTheFill(string fill, ColourScheme scheme)
    {
        var ring = DesignTokens.Colour(Ring()[0]).In(scheme);
        var background = DesignTokens.Colour(fill).In(scheme);

        Assert.That(Rgb.Contrast(ring, background), Is.GreaterThanOrEqualTo(Graphic), $"{Ring()[0]} {ring} on {fill} {background}");
    }

    [TestCaseSource(nameof(SurfaceCases))]
    public void TodayRing_OuterRing_ContrastsWithTheSurface(string surface, ColourScheme scheme)
    {
        var ring = DesignTokens.Colour(Ring()[1]).In(scheme);
        var background = DesignTokens.Colour(surface).In(scheme);

        Assert.That(Rgb.Contrast(ring, background), Is.GreaterThanOrEqualTo(Graphic), $"{Ring()[1]} {ring} on {surface} {background}");
    }

    private static List<string> Ring()
    {
        var rule = Regex.Match(Css, @"\.cell\.today\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
        var shadow = Regex.Match(rule, @"box-shadow:(?<value>[^;]*);").Groups["value"].Value;
        return [.. ColourToken().Matches(shadow).Select(m => m.Groups[1].Value)];
    }

    [GeneratedRegex(@"var\((--color-[\w-]+)\)")]
    private static partial Regex ColourToken();
}
