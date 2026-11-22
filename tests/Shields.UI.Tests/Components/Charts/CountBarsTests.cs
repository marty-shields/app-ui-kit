using System.Globalization;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class CountBarsTests : ComponentTestBase
{
    private IRenderedComponent<CountBars> Render(IReadOnlyList<int> counts, Action<ComponentParameterCollectionBuilder<CountBars>>? more = null) =>
        Context.Render<CountBars>(p =>
        {
            p.Add(x => x.Counts, counts).Add(x => x.Label, "Sessions per week").Add(x => x.TestId, "bars");
            more?.Invoke(p);
        });

    [Test]
    public void Figure_IsNamedAndTheDrawingHidden()
    {
        var cut = Render([1, 2]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("figure").GetAttribute("aria-label"), Is.EqualTo("Sessions per week"));
            Assert.That(cut.Find("svg").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Bars_AreProportionalToTheMost()
    {
        var heights = Render([1, 2, 4]).FindAll("rect").Select(r => double.Parse(r.GetAttribute("height")!, CultureInfo.InvariantCulture));

        Assert.That(heights, Is.EqualTo(new[] { 15d, 30d, 60d }));
    }

    [Test]
    public void TheLastBarIsTheCurrentOne_AndZeroDrawsNoBar()
    {
        var bars = Render([3, 0, 2, 5]).FindAll("rect");

        Assert.Multiple(() =>
        {
            Assert.That(bars, Has.Count.EqualTo(3));
            Assert.That(bars.Select(b => b.ClassList.Contains("current")), Is.EqualTo(new[] { false, false, true }));
        });
    }

    [Test]
    public void AllZeroOrNegative_ShowsTheEmptyText()
    {
        Assert.That(Render([0, -1]).FindAll("[data-testid=bars-none]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void AllZero_ShowsTheEmptyTextInsteadOfTheChart()
    {
        var cut = Render([0, 0], p => p.Add(x => x.EmptyText, "No sessions yet"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bars-none]").TextContent, Is.EqualTo("No sessions yet"));
            Assert.That(cut.FindAll("figure"), Is.Empty);
        });
    }

    [Test]
    public void NoCounts_ShowTheEmptyText()
    {
        Assert.That(Render([]).FindAll("[data-testid=bars-none]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void AxisLabels_ShowOnlyWhenSet_AndAreHiddenFromScreenReaders()
    {
        var with = Render([1, 2], p => p.Add(x => x.StartLabel, "1 week ago").Add(x => x.EndLabel, "This week"));
        var without = Render([1, 2]);

        Assert.Multiple(() =>
        {
            Assert.That(with.Find(".axis").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(with.FindAll(".axis span").Select(s => s.TextContent), Is.EqualTo(new[] { "1 week ago", "This week" }));
            Assert.That(without.FindAll(".axis span"), Is.Empty);
        });
    }

    [Test]
    public void TextAlternative_ListsEachBarWithTheValueText_OrTheBareCount()
    {
        var described = Render([2, 1], p => p.Add(x => x.ValueText, (i, n) => $"{(i == 0 ? "Last week" : "This week")}: {n} sessions"));
        var bare = Render([2, 1]);

        Assert.Multiple(() =>
        {
            Assert.That(described.FindAll("[data-testid=bars-values] li").Select(li => li.TextContent), Is.EqualTo(new[] { "Last week: 2 sessions", "This week: 1 sessions" }));
            Assert.That(bare.FindAll("[data-testid=bars-values] li").Select(li => li.TextContent), Is.EqualTo(new[] { "2", "1" }));
            Assert.That(bare.Find("[data-testid=bars-values]").ClassList, Does.Contain("visually-hidden"));
        });
    }
}
