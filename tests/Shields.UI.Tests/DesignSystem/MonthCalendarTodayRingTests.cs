using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class MonthCalendarTodayRingTests
{
    private const double Graphic = ContrastCheck.LargeTextOrControl;

    private static readonly string Css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "MonthCalendar.razor.css");

    private static readonly string TokensCss = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");

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

    [Test]
    public void TodayRing_IsOutsideTheCell_GapInsideRing_AndLeavesRoomForNeighbours()
    {
        var spreads = Spreads();

        Assert.That(Css, Does.Not.Match(@"\.cell\.today\s*\{[^}]*inset"));
        Assert.That(spreads, Has.Count.EqualTo(2));
        Assert.That(spreads[0], Is.GreaterThan(0), "gap is visible");
        Assert.That(spreads[1], Is.GreaterThan(spreads[0]), "ring is drawn outside the gap");
        Assert.That(spreads[1], Is.LessThan(Px("--space-1")), "ring stops short of the neighbouring cells");
    }

    [Test]
    public void TodayRing_FocusOutline_SitsBeyondTheRing()
    {
        var rule = Regex.Match(Css, @"\.cell\.today:focus-visible\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
        var offset = Regex.Match(rule, @"outline-offset:(?<value>[^;]*);").Groups["value"].Value;

        Assert.That(Length(offset), Is.GreaterThanOrEqualTo(Spreads()[1]));
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

    [TestCase(".marks .cell.marked .num", "background-color: var(--color-primary);")]
    [TestCase(".marks .cell.marked", "color: var(--color-on-primary);")]
    [TestCase(".marks .num", "border: var(--border-width) dashed var(--color-border-strong);")]
    public void MarksStyle_UsesThePrimaryAndBorderTokens(string selector, string declaration)
    {
        var rule = Regex.Match(Css, @"(?:^|[},/])\s*" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups["body"].Value;

        Assert.That(rule, Does.Contain(declaration));
    }

    private static List<double> Spreads()
    {
        var rule = Regex.Match(Css, @"\.cell\.today\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
        var shadow = Regex.Match(rule, @"box-shadow:(?<value>[^;]*);").Groups["value"].Value;
        return [.. shadow.Split(',', StringSplitOptions.TrimEntries)
            .Select(layer => Length(Regex.Match(layer, @"^0 0 0 (?<spread>.+) var\(--color-[\w-]+\)$").Groups["spread"].Value))];
    }

    // Sums the px tokens in a length such as "var(--a)" or "calc(var(--a) + var(--b))".
    private static double Length(string expression) =>
        TokenRef().Matches(expression).Sum(m => Px(m.Groups[1].Value));

    private static double Px(string token) =>
        double.Parse(Regex.Match(TokensCss, $@"{Regex.Escape(token)}:\s*(?<px>[\d.]+)px;").Groups["px"].Value, System.Globalization.CultureInfo.InvariantCulture);

    private static List<string> Ring()
    {
        var rule = Regex.Match(Css, @"\.cell\.today\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
        var shadow = Regex.Match(rule, @"box-shadow:(?<value>[^;]*);").Groups["value"].Value;
        return [.. ColourToken().Matches(shadow).Select(m => m.Groups[1].Value)];
    }

    [GeneratedRegex(@"var\((--(?!color-)[\w-]+)\)")]
    private static partial Regex TokenRef();

    [GeneratedRegex(@"var\((--color-[\w-]+)\)")]
    private static partial Regex ColourToken();
}
