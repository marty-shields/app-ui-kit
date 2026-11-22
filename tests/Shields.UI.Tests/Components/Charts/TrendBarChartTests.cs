using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class TrendBarChartTests : ComponentTestBase
{
    private static readonly DateOnly Monday = new(2026, 11, 9);

    private readonly List<DateOnly> selected = [];

    [SetUp]
    public void ClearSelection() => selected.Clear();

    private static TrendBarPoint Bar(int day, double? value, string? label = null) =>
        new(Monday.AddDays(day), label ?? Monday.AddDays(day).ToString("ddd", CultureInfo.InvariantCulture), value);

    private IRenderedComponent<TrendBarChart> Render(IReadOnlyList<TrendBarPoint> bars, double? target = 2000) =>
        Context.Render<TrendBarChart>(p => p
            .Add(x => x.Bars, bars).Add(x => x.Target, target).Add(x => x.Unit, "pts").Add(x => x.TestId, "chart")
            .Add(x => x.OnSelect, EventCallback.Factory.Create<DateOnly>(this, selected.Add)));

    private static double Number(IElement element, string attribute) =>
        double.Parse(element.GetAttribute(attribute)!, CultureInfo.InvariantCulture);

    private static double Height(IRenderedComponent<TrendBarChart> cut, int day) =>
        Number(cut.Find($"[data-testid=chart-column-{Monday.AddDays(day):yyyy-MM-dd}]"), "data-height");

    private static readonly Regex TopArc = new(@"A \S+ \S+ 0 0 1 \S+ (?<y>\S+)", RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>The y of the top of the column as drawn in its path, which is where the first rounded corner ends.</summary>
    private static double DrawnTop(IRenderedComponent<TrendBarChart> cut, int day)
    {
        var d = cut.Find($"[data-testid=chart-column-{Monday.AddDays(day):yyyy-MM-dd}]").GetAttribute("d")!;
        return double.Parse(TopArc.Match(d).Groups["y"].Value, CultureInfo.InvariantCulture);
    }

    [Test]
    public void Render_DrawsEachColumnFromTheBaselineUpToItsValueWithARoundedTop()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        var tickY = (int value) => Number(cut.Find($"[data-testid=chart-grid-{value}]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(DrawnTop(cut, 0), Is.EqualTo(tickY(1000)).Within(0.02));
            Assert.That(DrawnTop(cut, 1), Is.EqualTo(tickY(2000)).Within(0.02));
            Assert.That(baseline - DrawnTop(cut, 1), Is.EqualTo(2 * (baseline - DrawnTop(cut, 0))).Within(0.05));
            Assert.That(cut.Find("[data-testid=chart-column-2026-11-09]").GetAttribute("d"), Does.Match(@"^M \S+ \S+ V \S+ A "));
        });
    }

    [Test]
    public void Render_ScalesBarHeightsWithValuesOnOneYScale()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        var (small, tall) = (Height(cut, 0), Height(cut, 1));
        Assert.Multiple(() =>
        {
            Assert.That(small, Is.GreaterThan(0));
            Assert.That(tall / small, Is.EqualTo(2).Within(0.02));
        });
    }

    [Test]
    public void Render_HeightsFollowTheTicks()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        var tickY = (int value) => Number(cut.Find($"[data-testid=chart-grid-{value}]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(baseline - tickY(1000), Is.EqualTo(Height(cut, 0)).Within(0.02));
            Assert.That(baseline - tickY(2000), Is.EqualTo(Height(cut, 1)).Within(0.02));
        });
    }

    [Test]
    public void Render_BandEdgesSitAtNinetyAndOneHundredTenPercentOfTarget()
    {
        // Bars of exactly 90% and 110% of the 2,000 target mark where the band's bottom and top edges should be.
        var cut = Render([Bar(0, 1800), Bar(1, 2200)]);

        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        var top = Number(cut.Find("[data-testid=chart-band-top]"), "y1");
        var bottom = Number(cut.Find("[data-testid=chart-band-bottom]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(baseline - top, Is.EqualTo(Height(cut, 1)).Within(0.02));
            Assert.That(baseline - bottom, Is.EqualTo(Height(cut, 0)).Within(0.02));
        });
    }

    [Test]
    public void Render_Band_IsTwoEdgeLinesAndNoFill()
    {
        var cut = Render([Bar(0, 1800)]);

        var band = cut.Find("[data-testid=chart-band]");
        Assert.Multiple(() =>
        {
            Assert.That(band.QuerySelectorAll("line.band-edge"), Has.Count.EqualTo(2));
            Assert.That(band.QuerySelectorAll("rect, path, polygon"), Is.Empty, "an outline has nothing to fill");
            Assert.That(cut.FindAll("rect.band"), Is.Empty);
        });
    }

    [Test]
    public void TargetStyle_KeepsItsDashesAndStrokeInScreenPixelsWhateverTheChartsWidth()
    {
        var css = Rule(".target");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("stroke-width: var(--border-width-error);"));
            Assert.That(css, Does.Contain("stroke-dasharray: var(--chart-dash);"));
            Assert.That(css, Does.Contain("vector-effect: non-scaling-stroke;"), "the legend's Line swatch is a fixed size, so the chart's dashes must not scale with the card");
        });
    }

    [Test]
    public void BandStyle_StrokesTheEdgesInTheBandColourAtTheBorderWidthWithNoFill()
    {
        var css = Rule(".band-edge");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("stroke: var(--color-chart-band);"));
            Assert.That(css, Does.Contain("stroke-width: var(--border-width);"));
            Assert.That(css, Does.Contain("vector-effect: non-scaling-stroke;"));
            Assert.That(css, Does.Contain("shape-rendering: crispEdges;"), "a 1px stroke at a fractional y is otherwise blurred across two pixel rows");
            Assert.That(css, Does.Not.Contain("fill"));
        });
    }

    [Test]
    public void TargetStyle_IsDashedWithTheChartDash()
    {
        Assert.That(Rule(".target"), Does.Contain("stroke-dasharray: var(--chart-dash);"));
    }

    [TestCase(".column", "--color-chip")]
    [TestCase(".bar.on .column", "--color-accent")]
    [TestCase(".bar.over .column", "--color-error")]
    public void BarStyle_UsesTheStatusColour(string selector, string token)
    {
        Assert.That(Rule(selector), Does.Contain($"fill: var({token});"));
    }

    private static string Rule(string selector)
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "TrendBarChart.razor.css");
        var match = Regex.Match(css, @"(?:^|[},/])\s*" + Regex.Escape(selector) + @"(?:,[^{]*)?\s*\{(?<body>[^}]*)\}", RegexOptions.None, TimeSpan.FromSeconds(1));
        Assert.That(match.Success, Is.True, $"no rule for {selector}");
        return match.Groups["body"].Value;
    }

    [Test]
    public void Render_DrawsADashedTargetLineAtTheTarget()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        var line = cut.Find("[data-testid=chart-target]");
        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(line.ClassList, Does.Contain("target"));
            Assert.That(baseline - Number(line, "y1"), Is.EqualTo(Height(cut, 1)).Within(0.02));
        });
    }

    [TestCase(null)]
    [TestCase(0.0)]
    public void Render_WithoutATarget_DrawsNoBandOrLineAndNoStatus(double? target)
    {
        var cut = Render([Bar(0, 1000)], target);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-band]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-target]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,000 pts"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("plain"));
        });
    }

    [TestCase(1000, "under", "under target")]
    [TestCase(1799, "under", "under target")]
    [TestCase(1800, "on", "on target")]
    [TestCase(2200, "on", "on target")]
    [TestCase(2201, "over", "over target")]
    public void Render_GivesEachBarAStatusClassAndAccessibleName(double value, string statusClass, string word)
    {
        var cut = Render([Bar(0, value)]);

        var bar = cut.Find("[data-testid=chart-bar-2026-11-09]");
        Assert.Multiple(() =>
        {
            Assert.That(bar.ClassList, Does.Contain(statusClass));
            Assert.That(bar.GetAttribute("aria-label"),
                Is.EqualTo($"Mon 9 Nov, {value.ToString("N0", CultureInfo.InvariantCulture)} pts, {word}"));
        });
    }

    [Test]
    public void Render_JudgesABarAgainstItsOwnTarget()
    {
        var cut = Render([new TrendBarPoint(Monday, "Mon", 2500, Target: 2400), Bar(1, 2500)], target: 2000);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("on"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-10]").ClassList, Does.Contain("over"));
        });
    }

    [Test]
    public void Render_WithShowOverMarkerFalse_DrawsNoMarkerAndLowersTheValueLabelByTheMarkersLift()
    {
        var with = Render([Bar(0, 2500), Bar(1, 2000)]);
        var without = Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, 2500), Bar(1, 2000)]).Add(x => x.Target, 2000).Add(x => x.TestId, "chart").Add(x => x.ShowOverMarker, false));

        Assert.Multiple(() =>
        {
            Assert.That(with.FindAll("[data-testid^=chart-marker]"), Has.Count.EqualTo(1));
            Assert.That(without.FindAll("[data-testid^=chart-marker]"), Is.Empty);
            Assert.That(Number(without.Find("[data-testid=chart-value-label]"), "y") - Number(with.Find("[data-testid=chart-value-label]"), "y"), Is.EqualTo(9).Within(0.01), "the label drops from 15 to 6 above the bar");
            Assert.That(without.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Does.Contain("over target"));
            Assert.That(without.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("over"));
        });
    }

    [Test]
    public void Render_MarksOverBarsWithMoreThanColour()
    {
        var cut = Render([Bar(0, 2500), Bar(1, 2000), Bar(2, 1000)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-marker-2026-11-09]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-marker-2026-11-10]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-marker-2026-11-11]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Does.Contain("over target"));
        });
    }

    [Test]
    public void Render_Bars_AreFocusableButtons()
    {
        var cut = Render([Bar(0, 1000), Bar(1, null), Bar(2, 2000)]);

        var bars = cut.FindAll("g.bar");
        Assert.Multiple(() =>
        {
            Assert.That(bars, Has.Count.EqualTo(2));
            Assert.That(bars.Select(b => b.GetAttribute("role")), Is.All.EqualTo("button"));
            Assert.That(bars.Select(b => b.GetAttribute("tabindex")), Is.All.EqualTo("0"));
            Assert.That(bars.Select(b => b.ClassList.Contains("selectable")), Is.All.True);
        });
    }

    [Test]
    public void Render_BarNotSelectable_IsPlainAndKeepsItsName()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000) with { Selectable = false }]);

        var plain = cut.Find("[data-testid=chart-bar-2026-11-10]");
        Assert.Multiple(() =>
        {
            Assert.That(plain.GetAttribute("role"), Is.EqualTo("img"));
            Assert.That(plain.HasAttribute("tabindex"), Is.False);
            Assert.That(plain.ClassList, Does.Not.Contain("selectable"));
            Assert.That(plain.GetAttribute("aria-label"), Does.StartWith("Tue 10 Nov, 2,000"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("role"), Is.EqualTo("button"));
        });
    }

    [Test]
    public void ClickAndKey_OnBarNotSelectable_RaiseNothing()
    {
        var cut = Render([Bar(0, 1000) with { Selectable = false }]);

        cut.Find("[data-testid=chart-bar-2026-11-09]").Click();
        cut.Find("[data-testid=chart-bar-2026-11-09]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(selected, Is.Empty);
    }

    [Test]
    public void Click_RaisesOnSelectWithTheBarsDate()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        cut.Find("[data-testid=chart-bar-2026-11-10]").Click();

        Assert.That(selected, Is.EqualTo(new[] { new DateOnly(2026, 11, 10) }));
    }

    [Test]
    public void Key_Enter_RaisesOnSelectWithTheBarsDate()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2000)]);

        cut.Find("[data-testid=chart-bar-2026-11-09]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(selected, Is.EqualTo(new[] { new DateOnly(2026, 11, 9) }));
    }

    [Test]
    public void Key_Space_RaisesOnSelectWithTheBarsDate()
    {
        var cut = Render([Bar(0, 1000)]);

        cut.Find("[data-testid=chart-bar-2026-11-09]").KeyDown(new KeyboardEventArgs { Key = " " });

        Assert.That(selected, Is.EqualTo(new[] { new DateOnly(2026, 11, 9) }));
    }

    [Test]
    public void Render_WithoutOnSelect_BarsAreNotInteractive()
    {
        var cut = Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, 1000), Bar(1, 2000)]).Add(x => x.Target, 2000).Add(x => x.Unit, "pts").Add(x => x.TestId, "chart"));

        var bars = cut.FindAll("g.bar");
        Assert.Multiple(() =>
        {
            Assert.That(bars, Has.Count.EqualTo(2));
            Assert.That(bars.Select(b => b.GetAttribute("role")), Is.All.EqualTo("img"));
            Assert.That(bars.Select(b => b.HasAttribute("tabindex")), Is.All.False);
            Assert.That(bars.Select(b => b.ClassList.Contains("selectable")), Is.All.False);
            Assert.That(bars[0].GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,000 pts, under target"));
        });
    }

    [Test]
    public void Key_Other_DoesNothing()
    {
        var cut = Render([Bar(0, 1000)]);

        cut.Find("[data-testid=chart-bar-2026-11-09]").KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.That(selected, Is.Empty);
    }

    [Test]
    public void Render_LabelsOnlyTheHighestBar()
    {
        var cut = Render([Bar(0, 1000), Bar(1, 2345), Bar(2, 1500)]);

        var labels = cut.FindAll("[data-testid=chart-value-label]");
        Assert.Multiple(() =>
        {
            Assert.That(labels, Has.Count.EqualTo(1));
            Assert.That(labels[0].TextContent, Is.EqualTo("2,345"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-10]").QuerySelector("[data-testid=chart-value-label]"), Is.Not.Null);
        });
    }

    [TestCase(1000, 1500, new[] { "0", "500", "1k", "1.5k", "2k" })]
    [TestCase(2000, 2000, new[] { "0", "1k", "2k", "3k" })]
    [TestCase(3000, 2500, new[] { "0", "1k", "2k", "3k" })]
    [TestCase(3100, 2000, new[] { "0", "1k", "2k", "3k", "4k" })]
    [TestCase(4000, 2000, new[] { "0", "1k", "2k", "3k", "4k" })]
    [TestCase(4001, 2000, new[] { "0", "2k", "4k", "6k" })]
    [TestCase(6000, 2000, new[] { "0", "2k", "4k", "6k" })]
    [TestCase(9000, 2000, new[] { "0", "5k", "10k" })]
    [TestCase(74000, 2000, new[] { "0", "20k", "40k", "60k", "80k" })]
    public void Render_ShowsAFewNiceYTicksFromZero(double tallest, double target, string[] expected)
    {
        // A 2,000 target puts the band's top at 2,200, which needs the 3k tick.
        var cut = Render([Bar(0, tallest)], target);

        Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(expected));
    }

    [Test]
    public void Render_WithoutATargetAndLowBars_FitsTheScaleToTheBars()
    {
        var cut = Render([Bar(0, 800)], target: null);

        Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "200", "400", "600", "800" }));
    }

    [Test]
    public void Render_CalorieSizedTarget_GetsKiloTicksAboveTheBand()
    {
        var cut = Render([Bar(0, 2000), Bar(1, 2600)], target: 2400);

        Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "1k", "2k", "3k" }));
    }

    [Test]
    public void Render_SmallValuesAroundATarget_GetTheirOwnScale()
    {
        var cut = Render([Bar(0, 30), Bar(1, 60), Bar(2, 45)], target: 50);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "20", "40", "60" }));
            Assert.That(cut.FindAll("[data-testid=chart-grid-60]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Render_MaxSeven_StepsByTwoToEight()
    {
        var cut = Render([Bar(0, 7)], target: null);

        Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "2", "4", "6", "8" }));
    }

    [Test]
    public void Render_FractionalValues_GetFractionalTicksWithInvariantIds()
    {
        var cut = Render([Bar(0, 0.8)], target: null);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "0.2", "0.4", "0.6", "0.8" }));
            Assert.That(cut.FindAll("[data-testid=\"chart-grid-0.6\"]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=\"chart-tick-0.8\"]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Render_Millions_UseMTicks()
    {
        var cut = Render([Bar(0, 3_000_000)], target: null);

        Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent), Is.EqualTo(new[] { "0", "1M", "2M", "3M" }));
    }

    [Test]
    public void Render_ValueFarBeyondTheScale_IsDrawnNoTallerThanTheTopTick()
    {
        var cut = Render([Bar(0, 1e30)], target: null);

        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        var topTick = cut.FindAll("line.grid").Min(l => Number(l, "y1"));
        Assert.That(Height(cut, 0), Is.EqualTo(baseline - topTick).Within(0.02));
    }

    [Test]
    public void Render_PerBarTarget_IsHeldByTheScaleButNotDrawnAsALine()
    {
        var cut = Render([new TrendBarPoint(Monday, "Mon", 1000, Target: 9000), Bar(1, 1000)], target: null);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("text.tick").Select(t => t.TextContent).Last(), Is.EqualTo("10k"));
            Assert.That(cut.FindAll("[data-testid=chart-target]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-band]"), Is.Empty);
        });
    }

    [Test]
    public void Render_TooltipWiderThanTheChart_DoesNotThrowAndCentresTheText()
    {
        var cut = Context.Render<TrendBarChart>(p => p
            .Add(x => x.Bars, [Bar(0, 1200)]).Add(x => x.DateFormat, "dddd d MMMM yyyy 'in the year' yyyy").Add(x => x.Unit, "steps taken that day").Add(x => x.TestId, "chart"));

        Assert.That(Number(cut.Find("text.tip"), "x"), Is.EqualTo(160));
    }

    [Test]
    public void Render_WithoutATestId_AddsNoTestIds()
    {
        var cut = Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, 1000), Bar(1, null)]).Add(x => x.Target, 2000));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    [Test]
    public void Render_OutlierBar_StillFitsInsideTheChartWithFewTicks()
    {
        var cut = Render([Bar(0, 74000), Bar(1, 1500)]);

        var top = Number(cut.Find("[data-testid=chart-grid-80000]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("text.tick"), Has.Count.LessThanOrEqualTo(6));
            Assert.That(DrawnTop(cut, 0), Is.GreaterThan(top));
            Assert.That(top, Is.GreaterThanOrEqualTo(40), "room above the scale for the tooltip, the value and the over marker");
        });
    }

    [TestCase(0)]
    [TestCase(11)]
    public void Render_TooltipAtEitherEnd_StaysInsideTheChart(int index)
    {
        var bars = Enumerable.Range(0, 12).Select(i => new TrendBarPoint(new DateOnly(2026, 1 + i, 1), "Month", i == 0 || i == 11 ? 1200 : 1000)).ToList();
        var cut = Context.Render<TrendBarChart>(p => p
            .Add(x => x.Bars, bars).Add(x => x.Target, 2000).Add(x => x.DateFormat, "MMMM yyyy").Add(x => x.TestId, "chart"));

        var tip = cut.FindAll("g.bar")[index].QuerySelector("text.tip")!;
        var half = tip.TextContent.Length * 9 / 2.0;
        var x = Number(tip, "x");
        Assert.Multiple(() =>
        {
            Assert.That(x - half, Is.GreaterThanOrEqualTo(0), "the text starts inside the left edge of the 320 wide chart");
            Assert.That(x + half, Is.LessThanOrEqualTo(320), "and ends inside the right edge");
            Assert.That(tip.GetAttribute("text-anchor"), Is.EqualTo("middle"));
        });
    }

    [Test]
    public void Render_TooltipOfAMiddleBar_SitsOverItsBar()
    {
        var cut = Render(Enumerable.Range(0, 7).Select(i => Bar(i, 1000)).ToList());

        var bar = cut.FindAll("g.bar")[3];
        var slot = (312 - 34) / 7.0;
        Assert.That(Number(bar.QuerySelector("text.tip")!, "x"), Is.EqualTo(34 + 3.5 * slot).Within(0.02));
    }

    [Test]
    public void Render_MissingValue_LeavesAnEmptySlot()
    {
        var cut = Render([Bar(0, 1000), Bar(1, null), Bar(2, 0), Bar(3, 1500)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("g.bar"), Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=chart-gap-2026-11-10]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-gap-2026-11-11]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-column-2026-11-10]"), Is.Empty);
            Assert.That(cut.FindAll("text.axis-label").Select(t => t.TextContent), Is.EqualTo(new[] { "Mon", "Tue", "Wed", "Thu" }));
        });
    }

    [Test]
    public void Render_EmptyChart_KeepsAxesAndSaysSo()
    {
        var cut = Render([Bar(0, null), Bar(1, null)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-empty]").TextContent, Is.EqualTo("Nothing recorded yet"));
            Assert.That(cut.FindAll("g.bar"), Is.Empty);
            Assert.That(cut.FindAll("text.tick"), Is.Not.Empty);
        });
    }

    [Test]
    public void Render_TooltipHoldsTheDateAndValueForHoverAndFocus()
    {
        var cut = Render([Bar(0, 2500)]);

        Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09] text.tip").TextContent, Is.EqualTo("Mon 9 Nov, 2,500 pts"));
    }

    [Test]
    public void Render_LongLabels_AreShortenedUnderTheBarButKeptInTheName()
    {
        var cut = Render(Enumerable.Range(0, 7).Select(i => Bar(i, 1000, "Monday afternoon")).ToList());

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("text.axis-label")[0].TextContent, Is.EqualTo("Mo…"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Does.StartWith("Mon 9 Nov"));
        });
    }

    [Test]
    public void Render_TwelveBars_ShowOneLetterEach()
    {
        var cut = Render(Enumerable.Range(0, 12).Select(i => Bar(i, 1000, "Mar")).ToList());

        Assert.That(cut.FindAll("text.axis-label").Select(t => t.TextContent), Has.All.EqualTo("M"));
    }

    [Test]
    public void Render_DayNumbersOfAMonth_AreNeverCut()
    {
        var bars = Enumerable.Range(0, 31).Select(i => Bar(i, 1000, (i + 1).ToString(CultureInfo.InvariantCulture))).ToList();
        var cut = Render(bars);

        Assert.That(cut.FindAll("text.axis-label").Select(t => t.TextContent), Is.EqualTo(Enumerable.Range(1, 31).Select(i => i.ToString(CultureInfo.InvariantCulture))));
    }

    [Test]
    public void Render_SevenBarsLabelledMon_ShowTheWholeLabel()
    {
        var cut = Render(Enumerable.Range(0, 7).Select(i => Bar(i, 1000, "Mon")).ToList());

        Assert.That(cut.FindAll("text.axis-label").Select(t => t.TextContent), Has.All.EqualTo("Mon"));
    }

    [Test]
    public void Render_DateFormatShapesTheName()
    {
        var cut = Context.Render<TrendBarChart>(p => p
            .Add(x => x.Bars, [new TrendBarPoint(new DateOnly(2026, 3, 1), "Mar", 1500)]).Add(x => x.DateFormat, "MMMM yyyy").Add(x => x.Unit, "pts").Add(x => x.TestId, "chart"));

        Assert.That(cut.Find("g.bar").GetAttribute("aria-label"), Is.EqualTo("March 2026, 1,500 pts"));
    }

    [Test]
    public void Render_NoBarsAtAll_DoesNotThrow()
    {
        var cut = Render([]);

        Assert.That(cut.Find("[data-testid=chart-empty]"), Is.Not.Null);
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Render_NonFiniteValue_LeavesAnEmptySlotAndStillRenders(double value)
    {
        var cut = Render([Bar(0, value), Bar(1, 1500)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-gap-2026-11-09]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-column-2026-11-09]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-column-2026-11-10]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Render_OnlyNonFiniteValues_ShowsTheEmptyChart()
    {
        var cut = Render([Bar(0, double.NaN), Bar(1, double.PositiveInfinity)], target: null);

        Assert.That(cut.Find("[data-testid=chart-empty]"), Is.Not.Null);
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Render_NonFiniteTarget_DrawsNoBandAndNoStatus(double target)
    {
        var cut = Render([Bar(0, 1500)], target);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-band]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-target]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-column-2026-11-09]"), Is.Not.Null);
        });
    }

    [Test]
    public void ABarWithANote_IsFaded_AndTheNoteEndsItsTextAlternative()
    {
        var cut = Render([Bar(0, 2000), Bar(1, 2000) with { Note = "based on 2 days" }]);

        var plain = cut.Find($"[data-testid=chart-bar-{Monday:yyyy-MM-dd}]");
        var thin = cut.Find($"[data-testid=chart-bar-{Monday.AddDays(1):yyyy-MM-dd}]");

        Assert.Multiple(() =>
        {
            Assert.That(plain.ClassList, Does.Not.Contain("thin"));
            Assert.That(thin.ClassList, Does.Contain("thin"));
            Assert.That(thin.GetAttribute("aria-label"), Does.EndWith(", based on 2 days"));
            Assert.That(plain.GetAttribute("aria-label"), Does.Not.Contain("based on"));
        });
    }

    [Test]
    public void Render_WithoutAUnit_NamesTheBarWithoutOne()
    {
        var cut = Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, 1000)]).Add(x => x.TestId, "chart"));

        Assert.That(cut.Find("g.bar").GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,000"));
    }

    [Test]
    public void Render_EmptyTextAndLabel_AreShown()
    {
        var cut = Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, null)]).Add(x => x.EmptyText, "No data").Add(x => x.Label, "Steps").Add(x => x.TestId, "chart"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-empty]").TextContent, Is.EqualTo("No data"));
            Assert.That(cut.Find("[data-testid=chart]").GetAttribute("aria-label"), Is.EqualTo("Steps"));
        });
    }

    private IRenderedComponent<TrendBarChart> RenderPast(IReadOnlyList<TrendBarPoint> bars, string? earlierLabel = null) =>
        Context.Render<TrendBarChart>(p =>
        {
            p.Add(x => x.Bars, bars).Add(x => x.Target, 2000).Add(x => x.Unit, "pts").Add(x => x.TestId, "chart").Add(x => x.Style, TrendBarStyle.PastAndCurrent);
            if (earlierLabel is not null)
            {
                p.Add(x => x.EarlierLabel, earlierLabel);
            }
        });

    private static TrendBarPoint Past(int day, double? value, bool current = false, bool labelled = false, double? earlier = null) =>
        Bar(day, value) with { Current = current, Labelled = labelled, Earlier = earlier };

    [Test]
    public void PastAndCurrent_CurrentBarIsNowAndOthersArePast_WithoutTargetOrStatus()
    {
        var cut = RenderPast([Past(0, 1500), Past(1, 2600, current: true)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("past"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-10]").ClassList, Does.Contain("now"));
            Assert.That(cut.FindAll("[data-testid=chart-target],[data-testid=chart-band],[data-testid^=chart-marker]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-10]").GetAttribute("aria-label"), Is.EqualTo("Tue 10 Nov, 2,600 pts, current"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,500 pts"));
        });
    }

    [Test]
    public void PastAndCurrent_CurrentAxisLabelIsMarked()
    {
        var cut = RenderPast([Past(0, 1500), Past(1, 2600, current: true)]);

        Assert.That(cut.FindAll(".axis-label.current").Select(e => e.TextContent), Is.EqualTo(new[] { "Tue" }));
    }

    [Test]
    public void PastAndCurrent_PrintsTheValueOfLabelledBarsOnly()
    {
        var cut = RenderPast([Past(0, 1500), Past(1, 2600, labelled: true), Past(2, 900, labelled: true)]);

        Assert.That(cut.FindAll("[data-testid^=chart-value-label]").Select(e => e.TextContent), Is.EqualTo(new[] { "2,600", "900" }));
    }

    [Test]
    public void PastAndCurrent_WithEarlierValues_DrawsAPairAndNamesTheEarlierValue()
    {
        var cut = RenderPast([Past(0, 1500, earlier: 1200), Past(1, 2600, current: true, earlier: null), Past(2, null, earlier: 800)], "last week");

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".column.earlier"), Has.Count.EqualTo(2));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,500 pts, last week 1,200 pts"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-11]").GetAttribute("aria-label"), Is.EqualTo("Wed 11 Nov, last week 800 pts"));
            Assert.That(cut.FindAll("[data-testid=chart-gap-2026-11-11]"), Is.Empty, "a day with only an earlier value is not a gap");
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("now"));
            Assert.That(Number(cut.Find("[data-testid=chart-earlier-2026-11-09]"), "data-height"), Is.GreaterThan(0));
        });
    }

    [Test]
    public void PastAndCurrent_EarlierAndValueShareOneScale()
    {
        var cut = RenderPast([Past(0, 500, earlier: 5000)]);

        var earlier = Number(cut.Find("[data-testid=chart-earlier-2026-11-09]"), "data-height");
        Assert.That(earlier, Is.EqualTo(10 * Height(cut, 0)).Within(0.1), "5000 stands 10 times as tall as 500, and fits the chart");
    }

    [Test]
    public void PastAndCurrent_AllEmpty_ShowsTheEmptyText()
    {
        var cut = RenderPast([Past(0, null), Past(1, null)]);

        Assert.That(cut.Find("[data-testid=chart-empty]").TextContent, Is.EqualTo("Nothing recorded yet"));
    }

    [Test]
    public void StatusStyle_IgnoresCurrentLabelledAndEarlier()
    {
        var cut = Render([Past(0, 2600, current: true, labelled: true, earlier: 900), Past(1, 1000)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").ClassList, Does.Contain("over"));
            Assert.That(cut.FindAll(".column.earlier"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Does.Not.Contain("current"));
            Assert.That(cut.FindAll("[data-testid=chart-value-label]"), Has.Count.EqualTo(1));
        });
    }

    [TestCase(".bar.past .column", "--color-chart-past")]
    [TestCase(".column.earlier", "--color-chart-past")]
    [TestCase(".bar.now .column:not(.earlier)", "--color-accent")]
    public void PastAndCurrentStyle_UsesThePastAndAccentColours(string selector, string token)
    {
        Assert.That(Rule(selector), Does.Contain($"fill: var({token});"));
    }

    [Test]
    public void PastAndCurrent_Scale_IgnoresTheChartTargetAndEachBarsOwn()
    {
        var cut = RenderPast([Past(0, 500), Past(1, 1000) with { Target = 9000 }]);

        var ticks = cut.FindAll("[data-testid^=chart-tick-]").Select(e => double.Parse(e.GetAttribute("data-testid")!["chart-tick-".Length..], CultureInfo.InvariantCulture));
        var top = Number(cut.Find("[data-testid=chart-grid-1000]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(ticks.Max(), Is.LessThan(2200));
            Assert.That(DrawnTop(cut, 1), Is.EqualTo(top).Within(0.02));
            Assert.That(Height(cut, 1), Is.EqualTo(2 * Height(cut, 0)).Within(0.05));
        });
    }

    [Test]
    public void StatusStyle_EarlierOnlyBar_IsAGapAndWithoutAColumn()
    {
        var cut = Render([Past(0, null, earlier: 800)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-gap-2026-11-09]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-bar-2026-11-09]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=chart-empty]").TextContent, Is.EqualTo("Nothing recorded yet"));
        });
    }

    [Test]
    public void PastAndCurrent_LabelledBarWithOnlyAnEarlierValue_PrintsNoValueLabel()
    {
        var cut = RenderPast([Past(0, null, labelled: true, earlier: 800)]);

        Assert.That(cut.FindAll("[data-testid^=chart-value-label]"), Is.Empty);
    }

    [Test]
    public void PastAndCurrent_Paired_ValueLabelIsCentredOnTheCurrentColumnAboveTheTallerOne()
    {
        var cut = RenderPast([Past(0, 500, labelled: true, earlier: 1500), Past(1, 2000, labelled: true, earlier: 100)]);

        var column = (int day) =>
        {
            var d = cut.Find($"[data-testid=chart-column-{Monday.AddDays(day):yyyy-MM-dd}]").GetAttribute("d")!.Split(' ');
            var left = double.Parse(d[1], CultureInfo.InvariantCulture);
            var right = double.Parse(d[^5], CultureInfo.InvariantCulture);
            return (left + right) / 2;
        };
        var label = (int day) => cut.Find($"[data-testid=chart-value-label-{Monday.AddDays(day):yyyy-MM-dd}]");
        var baseline = Number(cut.Find("[data-testid=chart-baseline]"), "y1");
        Assert.Multiple(() =>
        {
            Assert.That(Number(label(0), "x"), Is.EqualTo(column(0)).Within(0.05));
            Assert.That(Number(label(1), "x"), Is.EqualTo(column(1)).Within(0.05));
            Assert.That(baseline - Number(label(0), "y"), Is.GreaterThan(Number(cut.Find("[data-testid=chart-earlier-2026-11-09]"), "data-height")), "above the taller earlier column");
            Assert.That(baseline - Number(label(1), "y"), Is.GreaterThan(Height(cut, 1)));
        });
    }

    [Test]
    public void PastAndCurrent_Paired_TooltipAndNameCarryTheEarlierValue()
    {
        var cut = RenderPast([Past(0, 1500, earlier: 1200), Past(1, null, earlier: 800), Past(2, 900)], "last week");

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09] .tip").TextContent, Is.EqualTo("Mon 9 Nov, 1,500 pts, last week 1,200 pts"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-10] .tip").TextContent, Is.EqualTo("Tue 10 Nov, last week 800 pts"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-11] .tip").TextContent, Is.EqualTo("Wed 11 Nov, 900 pts"));
            Assert.That(cut.Find("[data-testid=chart-bar-2026-11-09]").GetAttribute("aria-label"), Is.EqualTo("Mon 9 Nov, 1,500 pts, last week 1,200 pts"));
        });
    }

    private IRenderedComponent<TrendBarChart> RenderMax(IReadOnlyList<TrendBarPoint> bars, double? max, TrendBarStyle style) =>
        Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, bars).Add(x => x.Max, max).Add(x => x.Style, style).Add(x => x.TestId, "chart"));

    private static IEnumerable<double> TickValues(IRenderedComponent<TrendBarChart> cut) =>
        cut.FindAll("[data-testid^=chart-tick-]").Select(e => double.Parse(e.GetAttribute("data-testid")!["chart-tick-".Length..], CultureInfo.InvariantCulture));

    [TestCase(TrendBarStyle.Status)]
    [TestCase(TrendBarStyle.PastAndCurrent)]
    public void Max_SetsTheTopOfTheScale_AndCapsTallerBars(TrendBarStyle style)
    {
        var cut = RenderMax([Bar(0, 500), Bar(1, 1000), Bar(2, 3000)], 1000, style);

        Assert.Multiple(() =>
        {
            Assert.That(TickValues(cut), Is.EqualTo(new[] { 0d, 500, 1000 }));
            Assert.That(Height(cut, 1), Is.EqualTo(2 * Height(cut, 0)).Within(0.05));
            Assert.That(Height(cut, 2), Is.EqualTo(Height(cut, 1)).Within(0.05));
        });
    }

    [TestCase(TrendBarStyle.Status)]
    [TestCase(TrendBarStyle.PastAndCurrent)]
    public void Max_ThatIsNotATick_IsTheTopAndTicksStopBelowIt(TrendBarStyle style)
    {
        var cut = RenderMax([Bar(0, 35), Bar(1, 70)], 70, style);

        Assert.Multiple(() =>
        {
            Assert.That(TickValues(cut), Is.EqualTo(new[] { 0d, 20, 40, 60 }));
            Assert.That(Height(cut, 1), Is.EqualTo(2 * Height(cut, 0)).Within(0.05));
            Assert.That(Height(cut, 1), Is.EqualTo(132).Within(0.05));
        });
    }

    [TestCase(TrendBarStyle.Status)]
    [TestCase(TrendBarStyle.PastAndCurrent)]
    public void Max_HoldsTheScaleWhateverTheValues(TrendBarStyle style)
    {
        var low = RenderMax([Bar(0, 10)], 1000, style);
        var high = RenderMax([Bar(0, 900)], 1000, style);

        Assert.Multiple(() =>
        {
            Assert.That(TickValues(low), Is.EqualTo(TickValues(high)));
            Assert.That(Height(high, 0) / Height(low, 0), Is.EqualTo(90).Within(0.5));
        });
    }

    [TestCase(null)]
    [TestCase(0.0)]
    [TestCase(-5.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Max_NotUsable_FitsTheScaleToTheTallestBar(double? max)
    {
        var cut = RenderMax([Bar(0, 1000), Bar(1, 2000)], max, TrendBarStyle.Status);

        Assert.That(TickValues(cut), Is.EqualTo(new[] { 0d, 500, 1000, 1500, 2000 }));
    }

    private IRenderedComponent<TrendBarChart> RenderMaxTarget(double target, double max, TrendBarStyle style) =>
        Context.Render<TrendBarChart>(p => p.Add(x => x.Bars, [Bar(0, 1000)]).Add(x => x.Target, target).Add(x => x.Max, max).Add(x => x.Style, style).Add(x => x.TestId, "chart"));

    [TestCase(TrendBarStyle.Status)]
    [TestCase(TrendBarStyle.PastAndCurrent)]
    public void Max_BelowTheTarget_DrawsNoTargetLineAndNoBandEdge(TrendBarStyle style)
    {
        var cut = RenderMaxTarget(2000, 1500, style);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-target]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-band-top]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=chart-band-bottom]"), Is.Empty);
        });
    }

    [Test]
    public void Max_BelowTheBandTop_KeepsTheTargetLineAndLowerEdgeOnly()
    {
        var cut = RenderMaxTarget(1400, 1500, TrendBarStyle.Status);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=chart-target]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-band-bottom]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=chart-band-top]"), Is.Empty);
        });
    }

    [TestCase(TrendBarStyle.Status)]
    [TestCase(TrendBarStyle.PastAndCurrent)]
    public void Max_HugeButFinite_KeepsTicksAndHeightsFinite(TrendBarStyle style)
    {
        var cut = RenderMax([Bar(0, 1000)], 1e300, style);

        Assert.Multiple(() =>
        {
            Assert.That(TickValues(cut), Is.All.Matches<double>(double.IsFinite));
            Assert.That(TickValues(cut).Max(), Is.LessThanOrEqualTo(2e16));
            Assert.That(Height(cut, 0), Is.Not.NaN.And.GreaterThanOrEqualTo(0));
        });
    }
}
