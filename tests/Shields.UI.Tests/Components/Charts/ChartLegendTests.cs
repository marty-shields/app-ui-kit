using System.Text.RegularExpressions;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class ChartLegendTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsADotAndLabelForEachEntryInOrder()
    {
        var cut = Context.Render<ChartLegend>(p => p.Add(x => x.TestId, "legend").Add(x => x.Items,
        [
            new(LegendSwatch.Chip, "Under"), new(LegendSwatch.Accent, "On target"), new(LegendSwatch.Error, "Over"),
            new(LegendSwatch.Outline, "Not logged"), new(LegendSwatch.Line, "Target"),
            new(LegendSwatch.Band, "Range"),
        ]));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("li").Select(li => li.QuerySelector(".text")!.TextContent),
                Is.EqualTo(new[] { "Under", "On target", "Over", "Not logged", "Target", "Range" }));
            Assert.That(cut.FindAll(".swatch").Select(s => s.ClassList.Last()),
                Is.EqualTo(new[] { "chip", "accent", "error", "outline", "line", "band" }));
            Assert.That(cut.FindAll("li").Select(li => li.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "legend-0", "legend-1", "legend-2", "legend-3", "legend-4", "legend-5" }));
            Assert.That(cut.FindAll(".marker"), Is.Empty);
        });
    }

    [Test]
    public void Band_IsAnOutlineInTheBandTokenWithNoFill()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "ChartLegend.razor.css");
        var rule = Regex.Match(css, @"\.swatch\.band\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(rule, Does.Contain("width: calc(var(--legend-swatch-size) * 2);"));
            Assert.That(rule, Does.Not.Contain("background"));
            Assert.That(rule, Does.Contain("box-shadow: inset 0 0 0 var(--border-width) var(--color-chart-band);"));
            Assert.That(rule, Does.Contain("border-radius: calc(var(--radius-sm) / 4);"));
        });
    }

    [Test]
    public void Render_ItemWithAMarker_ShowsItBesideItsDotOnly()
    {
        var cut = Context.Render<ChartLegend>(p => p.Add(x => x.Items,
            [new(LegendSwatch.Error, "Over", "▲"), new(LegendSwatch.Chip, "Under")]));

        var items = cut.FindAll("li");
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Children.Select(c => c.ClassList.First()), Is.EqualTo(new[] { "swatch", "marker", "text" }));
            Assert.That(items[0].QuerySelector(".marker")!.TextContent, Is.EqualTo("▲"));
            Assert.That(items[0].QuerySelector(".marker")!.GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(items[1].QuerySelector(".marker"), Is.Null);
        });
    }

    [Test]
    public void Render_WithNoItems_RendersNothing()
    {
        var cut = Context.Render<ChartLegend>(p => p.Add(x => x.TestId, "legend"));

        Assert.That(cut.Markup.Trim(), Is.Empty);
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<ChartLegend>(p => p.Add(x => x.Items, [new(LegendSwatch.Accent, "On target")]));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }
}
