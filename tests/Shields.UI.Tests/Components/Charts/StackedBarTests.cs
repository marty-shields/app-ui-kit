using System.Text.RegularExpressions;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class StackedBarTests : ComponentTestBase
{
    private static readonly StackedBarSegment[] Three =
        [new("North", 30, DataSeries.One), new("South", 67, DataSeries.Two), new("East", 29, DataSeries.Three)];

    private IRenderedComponent<StackedBar> Render(IReadOnlyList<StackedBarSegment> segments, Action<ComponentParameterCollectionBuilder<StackedBar>>? more = null) =>
        Context.Render<StackedBar>(p =>
        {
            p.Add(x => x.Segments, segments).Add(x => x.Label, "Sales by region").Add(x => x.Unit, "g").Add(x => x.TestId, "bar");
            more?.Invoke(p);
        });

    private static string? Text(IRenderedComponent<StackedBar> cut) => cut.Find("[data-testid=bar]").GetAttribute("aria-label");

    [Test]
    public void Render_ShowsLabelsValuesAndSharesInTheTextAlternative()
    {
        var cut = Render(Three);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bar]").GetAttribute("role"), Is.EqualTo("img"));
            Assert.That(Text(cut), Is.EqualTo("Sales by region: North 30 g, 24%; South 67 g, 53%; East 29 g, 23%."));
            Assert.That(cut.Find("[data-testid=bar-label-0]").TextContent, Is.EqualTo("North 30 g"));
            Assert.That(cut.Find("[data-testid=bar-label-1]").TextContent, Is.EqualTo("South 67 g"));
            Assert.That(cut.Find("[data-testid=bar-label-2]").TextContent, Is.EqualTo("East 29 g"));
            Assert.That(cut.Find(".track").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(cut.Find(".labels").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Render_SizesSegmentsByShare()
    {
        var cut = Render([new("A", 10, DataSeries.One), new("B", 10, DataSeries.Two), new("C", 20, DataSeries.Three)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bar-segment-0]").GetAttribute("style"), Does.Contain("flex-grow: 25"));
            Assert.That(cut.Find("[data-testid=bar-segment-1]").GetAttribute("style"), Does.Contain("flex-grow: 25"));
            Assert.That(cut.Find("[data-testid=bar-segment-2]").GetAttribute("style"), Does.Contain("flex-grow: 50"));
        });
    }

    [Test]
    public void Render_GivesEachSegmentAndLabelItsSeriesClass()
    {
        var segments = Enum.GetValues<DataSeries>().Select(s => new StackedBarSegment($"S{(int)s}", 1, s)).ToArray();

        var cut = Render(segments);

        Assert.Multiple(() =>
        {
            for (var i = 0; i < segments.Length; i++)
            {
                var series = segments[i].Series.CssClass();
                Assert.That(cut.Find($"[data-testid=bar-segment-{i}]").ClassList, Does.Contain(series));
                Assert.That(cut.Find($"[data-testid=bar-label-{i}]").ClassList, Does.Contain(series));
            }
        });
    }

    [Test]
    public void Render_AllZero_ShowsNoSegmentsAndSaysSo()
    {
        var cut = Render([new("North", 0, DataSeries.One), new("South", 0, DataSeries.Two)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".segment"), Is.Empty);
            Assert.That(cut.Find(".track").ClassList, Does.Contain("none"));
            Assert.That(Text(cut), Is.EqualTo("Sales by region: North 0 g, South 0 g. Nothing yet."));
        });
    }

    [Test]
    public void Render_NoSegments_ShowsAnEmptyTrackAndEmptyText()
    {
        var cut = Render([], p => p.Add(x => x.EmptyText, "Nothing recorded"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".segment"), Is.Empty);
            Assert.That(Text(cut), Is.EqualTo("Sales by region: Nothing recorded."));
        });
    }

    [Test]
    public void Render_NegativeAndNonFiniteValues_DrawNoSegmentAndShowZero()
    {
        var cut = Render([new("A", -5, DataSeries.One), new("B", double.NaN, DataSeries.Two), new("C", 10, DataSeries.Three),
            new("D", double.PositiveInfinity, DataSeries.Four), new("E", double.NegativeInfinity, DataSeries.Five)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=bar-segment-0]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=bar-segment-1]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=bar-label-0]").TextContent, Is.EqualTo("A 0 g"));
            Assert.That(cut.FindAll("[data-testid=bar-segment-3]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=bar-segment-4]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=bar-label-1]").TextContent, Is.EqualTo("B 0 g"));
            Assert.That(cut.Find("[data-testid=bar-label-3]").TextContent, Is.EqualTo("D 0 g"));
            Assert.That(cut.Find("[data-testid=bar-label-4]").TextContent, Is.EqualTo("E 0 g"));
            Assert.That(Text(cut), Does.Contain("A 0 g, 0%").And.Contain("C 10 g, 100%"));
        });
    }

    [Test]
    public void Render_FractionalValues_KeepOneDecimal()
    {
        var cut = Render([new("A", 12.5, DataSeries.One), new("B", 10, DataSeries.Two)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bar-label-0]").TextContent, Is.EqualTo("A 12.5 g"));
            Assert.That(Text(cut), Does.Contain("A 12.5 g"));
        });
    }

    [Test]
    public void Render_LargeValues_GroupThousands()
    {
        var cut = Render([new("A", 12500, DataSeries.One), new("B", 1234.5, DataSeries.Two)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bar-label-0]").TextContent, Is.EqualTo("A 12,500 g"));
            Assert.That(cut.Find("[data-testid=bar-label-1]").TextContent, Is.EqualTo("B 1,234.5 g"));
        });
    }

    [Test]
    public void Render_TinyShare_StillGetsAPositiveGrow()
    {
        var cut = Render([new("A", 1e9, DataSeries.One), new("B", 1, DataSeries.Two)]);

        Assert.That(cut.Find("[data-testid=bar-segment-1]").GetAttribute("style"), Does.Contain("flex-grow: 0.001"));
    }

    [Test]
    public void Css_TrackUsesTheGapAndSurface_AndSegmentsKeepAMinimumWidth()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "StackedBar.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".track"), Does.Contain("gap: var(--stacked-bar-gap);").And.Contain("background-color: var(--color-surface);"));
            Assert.That(Rule(".track.none"), Does.Contain("background-color: var(--color-ring-track);"));
            Assert.That(Rule(".segment"), Does.Contain("min-width: var(--stacked-bar-segment-min-width);"));
        });
    }

    [Test]
    public void Render_LabelText_IsItsOwnSpan_SoTheValueSitsOutsideTheEllipsis()
    {
        var cut = Render(Three);

        var label = cut.Find("[data-testid=bar-label-0]");
        Assert.Multiple(() =>
        {
            Assert.That(label.QuerySelector(".label-text")!.TextContent, Is.EqualTo("North"));
            Assert.That(label.QuerySelector("strong")!.TextContent, Is.EqualTo("30 g"));
            Assert.That(label.QuerySelector(".label-text strong"), Is.Null);
        });
    }

    [Test]
    public void Css_OnlyTheLabelTextEllipsises_AndTheValueAndSwatchDoNotShrink()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "StackedBar.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".label-text"), Does.Contain("min-width: 0;").And.Contain("overflow: hidden;").And.Contain("text-overflow: ellipsis;"));
            Assert.That(Rule(".label"), Does.Not.Contain("overflow").And.Not.Contain("text-overflow"));
            Assert.That(Rule(".label strong"), Does.Contain("flex-shrink: 0;"));
            Assert.That(Rule(".label::before"), Does.Contain("flex-shrink: 0;"));
        });
    }

    [TestCase(1, 1, 1, "North 1 g, 34%", "South 1 g, 33%; East 1 g, 33%")]
    [TestCase(2, 2, 3, "North 2 g, 29%", "South 2 g, 28%; East 3 g, 43%")]
    public void Render_PercentagesAddUpToOneHundred(double a, double b, double c, string first, string rest)
    {
        var cut = Render([new("North", a, DataSeries.One), new("South", b, DataSeries.Two), new("East", c, DataSeries.Three)]);

        Assert.Multiple(() =>
        {
            Assert.That(Text(cut), Does.Contain(first));
            Assert.That(Text(cut), Does.Contain(rest));
        });
    }

    [Test]
    public void Render_OneSegmentZero_RendersOnlyTheOtherSegments()
    {
        var cut = Render([new("A", 10, DataSeries.One), new("B", 0, DataSeries.Two), new("C", 10, DataSeries.Three)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=bar-segment-1]"), Is.Empty);
            Assert.That(cut.FindAll(".segment"), Has.Count.EqualTo(2));
            Assert.That(cut.Find("[data-testid=bar-label-1]"), Is.Not.Null);
        });
    }

    [Test]
    public void Render_TinyTotal_StillFillsTheBar()
    {
        var cut = Render([new("A", 0.1, DataSeries.One)]);

        Assert.That(cut.Find("[data-testid=bar-segment-0]").GetAttribute("style"), Does.Contain("flex-grow: 100"));
    }

    [Test]
    public void Render_WithoutLabelOrUnit_LeavesThemOut()
    {
        var cut = Context.Render<StackedBar>(p => p
            .Add(x => x.Segments, [new("A", 1, DataSeries.One), new("B", 3, DataSeries.Two)]).Add(x => x.TestId, "bar"));

        Assert.Multiple(() =>
        {
            Assert.That(Text(cut), Is.EqualTo("A 1, 25%; B 3, 75%."));
            Assert.That(cut.Find("[data-testid=bar-label-0]").TextContent, Is.EqualTo("A 1"));
        });
    }

    [Test]
    public void Render_ShowLabelsFalse_HidesTheLabelsButKeepsTheTextAlternative()
    {
        var cut = Render(Three, p => p.Add(x => x.ShowLabels, false));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".labels"), Is.Empty);
            Assert.That(Text(cut), Does.StartWith("Sales by region: North 30 g"));
            Assert.That(cut.FindAll("[data-testid=bar-segment-0]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<StackedBar>(p => p.Add(x => x.Segments, Three));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }
}
