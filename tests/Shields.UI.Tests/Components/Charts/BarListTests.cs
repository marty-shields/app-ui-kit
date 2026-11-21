using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class BarListTests : ComponentTestBase
{
    private IRenderedComponent<BarList> Render(IReadOnlyList<BarListItem> items, double? max = null) =>
        Context.Render<BarList>(p => p.Add(x => x.Items, items).Add(x => x.Max, max).Add(x => x.Label, "Sales by region").Add(x => x.TestId, "list"));

    private static string? Style(IRenderedComponent<BarList> cut, int index) => cut.Find($"[data-testid=list-bar-{index}]").GetAttribute("style");

    [Test]
    public void Render_ShowsEachRowsLabelAndValueText()
    {
        var cut = Render([new("North", 420, "420 units"), new("South", 1234), new("East", 1234.5)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list]").GetAttribute("aria-label"), Is.EqualTo("Sales by region"));
            Assert.That(cut.Find("[data-testid=list-row-0]"), Is.Not.Null);
            Assert.That(cut.Find("[data-testid=list-label-0]").TextContent, Is.EqualTo("North"));
            Assert.That(cut.Find("[data-testid=list-value-0]").TextContent, Is.EqualTo("420 units"));
            Assert.That(cut.Find("[data-testid=list-value-1]").TextContent, Is.EqualTo("1,234"));
            Assert.That(cut.Find("[data-testid=list-value-2]").TextContent, Is.EqualTo("1,235"));
        });
    }

    [Test]
    public void Render_SizesBarsAgainstTheLargestValue()
    {
        var cut = Render([new("A", 200), new("B", 800), new("C", 0)]);

        Assert.Multiple(() =>
        {
            Assert.That(Style(cut, 0), Is.EqualTo("--fill: 0.25"));
            Assert.That(Style(cut, 1), Is.EqualTo("--fill: 1"));
            Assert.That(Style(cut, 2), Is.EqualTo("--fill: 0"));
        });
    }

    [Test]
    public void Render_WithMax_SizesBarsAgainstIt()
    {
        var cut = Render([new("Nov", 15), new("Dec", 45)], max: 30);

        Assert.Multiple(() =>
        {
            Assert.That(Style(cut, 0), Is.EqualTo("--fill: 0.5"));
            Assert.That(Style(cut, 1), Is.EqualTo("--fill: 1"));
        });
    }

    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Render_WithoutAUsableMax_SizesBarsAgainstTheLargestValue(double max)
    {
        var cut = Render([new("A", 10), new("B", 40)], max);

        Assert.Multiple(() =>
        {
            Assert.That(Style(cut, 0), Is.EqualTo("--fill: 0.25"));
            Assert.That(Style(cut, 1), Is.EqualTo("--fill: 1"));
        });
    }

    [Test]
    public void Render_AllZero_FillsNothing()
    {
        var cut = Render([new("A", 0), new("B", 0)]);

        Assert.That(cut.FindAll(".fill").Select(f => f.GetAttribute("style")), Has.All.EqualTo("--fill: 0"));
    }

    [Test]
    public void Render_NegativeAndNonFiniteValues_FillNoBar()
    {
        var cut = Render([new("A", -5), new("B", double.NaN), new("C", double.PositiveInfinity), new("D", 10)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".fill").Take(3).Select(f => f.GetAttribute("style")), Has.All.EqualTo("--fill: 0"));
            Assert.That(Style(cut, 3), Is.EqualTo("--fill: 1"));
            Assert.That(cut.Find("[data-testid=list-value-0]").TextContent, Is.EqualTo("-5"));
            Assert.That(cut.Find("[data-testid=list-value-1]").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=list-value-2]").TextContent, Is.EqualTo("0"));
        });
    }

    [Test]
    public void Render_BarsAreHiddenFromScreenReaders()
    {
        var cut = Render([new("A", 1)]);

        Assert.That(cut.Find(".track").GetAttribute("aria-hidden"), Is.EqualTo("true"));
    }

    [Test]
    public void Render_NoRows_SaysSo()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.EmptyText, "Nothing recorded yet").Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-empty]").TextContent, Is.EqualTo("Nothing recorded yet"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    [Test]
    public void Render_NoRowsAndNoEmptyText_SaysNothingYet()
    {
        var cut = Context.Render<BarList>();

        Assert.That(cut.Find("p").TextContent, Is.EqualTo("Nothing yet"));
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 1)]));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    [Test]
    public void Css_FillUsesTheArcToken_AndTrackTheRingTrack()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "BarList.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".fill"), Does.Contain("background-color: var(--color-ring-arc);"));
            Assert.That(Rule(".track"), Does.Contain("background-color: var(--color-ring-track);"));
        });
    }

    [Test]
    public void Render_WithAHandler_ListIsSelectable_AndWithoutOneIsNot()
    {
        var with = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 1, Selectable: false)]).Add(x => x.OnSelect, EventCallback.Factory.Create<int>(this, _ => { })));
        var without = Render([new("A", 1)]);

        Assert.Multiple(() =>
        {
            Assert.That(with.Find("ul").ClassList, Does.Contain("selectable"));
            Assert.That(without.Find("ul").ClassList, Does.Not.Contain("selectable"));
        });
    }

    [Test]
    public void Render_ZeroRow_GetsTheZeroClass_AndOthersDoNot()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 0, "Not trained"), new BarListItem("B", -2), new BarListItem("C", 3)]).Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-value-0]").ClassList, Does.Contain("zero"));
            Assert.That(cut.Find("[data-testid=list-value-1]").ClassList, Does.Contain("zero"));
            Assert.That(cut.Find("[data-testid=list-value-2]").ClassList, Does.Not.Contain("zero"));
        });
    }

    [Test]
    public void Css_ZeroValue_IsRegularWeightAndStaysMuted()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "BarList.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".value.zero"), Does.Contain("font-weight: var(--font-weight-regular);").And.Not.Contain("color"));
            Assert.That(Rule(".value"), Does.Contain("color: var(--color-text-muted);").And.Contain("font-weight: var(--font-weight-bold);"));
        });
    }

    [Test]
    public void Css_SelectableRowsAreATouchTargetTall_AndInlineBarsUseTheirOwnHeight()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "BarList.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".selectable .row"), Does.Contain("min-height: var(--touch-target);"));
            Assert.That(Rule(".inline .track"), Does.Contain("height: var(--bar-list-bar-height-inline);"));
            Assert.That(Rule(".track"), Does.Contain("height: var(--bar-list-bar-height);"));
        });
    }

    [Test]
    public void Render_ByDefault_IsStackedWithNormalColumnsAndNoButtons()
    {
        var cut = Render([new("A", 1)]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("ul").ClassList, Does.Not.Contain("inline").And.Not.Contain("label-wide").And.Not.Contain("label-narrow").And.Not.Contain("value-narrow"));
            Assert.That(cut.FindAll("button"), Is.Empty);
        });
    }

    [Test]
    public void Render_Inline_AndTheColumnWidthsSetClasses()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 1)]).Add(x => x.Inline, true)
            .Add(x => x.LabelWidth, BarListLabelWidth.Wide).Add(x => x.ValueWidth, BarListValueWidth.Narrow));
        var narrow = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 1)]).Add(x => x.LabelWidth, BarListLabelWidth.Narrow));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("ul").ClassList, Does.Contain("inline").And.Contain("label-wide").And.Contain("value-narrow"));
            Assert.That(narrow.Find("ul").ClassList, Does.Contain("label-narrow"));
        });
    }

    [Test]
    public void Render_Unit_FollowsEachValueForScreenReadersOnly()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 5)]).Add(x => x.Unit, "sessions").Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-value-0]").TextContent.Trim(), Is.EqualTo("5 sessions"));
            Assert.That(cut.Find("[data-testid=list-value-0] .visually-hidden").TextContent.Trim(), Is.EqualTo("sessions"));
        });
    }

    [Test]
    public void Render_WithOnSelect_SelectableRowsAreButtonsThatRaiseTheirPosition()
    {
        var selected = new List<int>();
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 3), new BarListItem("B", 2, Selectable: false), new BarListItem("C", 1)])
            .Add(x => x.OnSelect, EventCallback.Factory.Create<int>(this, selected.Add)).Add(x => x.TestId, "list"));

        cut.Find("[data-testid=list-select-2]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button"), Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=list-select-1]"), Is.Empty, "a row that is not selectable is plain");
            Assert.That(selected, Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void Css_InlineColumns_UseTheBarListWidthTokens()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "BarList.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("--label-width: var(--bar-list-label-width);"));
            Assert.That(css, Does.Contain("--label-width: var(--bar-list-label-width-wide);"));
            Assert.That(css, Does.Contain("--label-width: var(--bar-list-label-width-narrow);"));
            Assert.That(css, Does.Contain("--value-width: var(--bar-list-value-width-narrow);"));
        });
    }

    private IRenderedComponent<BarList> RenderBy(IReadOnlyList<BarListItem> items, BarListTestIdBy by, bool tappable = false) =>
        Context.Render<BarList>(p =>
        {
            p.Add(x => x.Items, items).Add(x => x.TestIdBy, by).Add(x => x.TestId, "list");
            if (tappable)
            {
                p.Add(x => x.OnSelect, EventCallback.Factory.Create<int>(this, _ => { }));
            }
        });

    private static IEnumerable<string?> Ids(IRenderedComponent<BarList> cut, string part) =>
        cut.FindAll($"[data-testid^=list-{part}-]").Select(e => e.GetAttribute("data-testid"));

    [Test]
    public void TestIdBy_Slug_NamesEveryRowPartByLabelSlug()
    {
        var cut = RenderBy([new("Upper Body", 3), new("  Legs & Core! ", 2)], BarListTestIdBy.Slug, tappable: true);

        Assert.Multiple(() =>
        {
            Assert.That(Ids(cut, "row"), Is.EqualTo(new[] { "list-row-upper-body", "list-row-legs-core" }));
            Assert.That(Ids(cut, "select"), Is.EqualTo(new[] { "list-select-upper-body", "list-select-legs-core" }));
            Assert.That(Ids(cut, "label"), Is.EqualTo(new[] { "list-label-upper-body", "list-label-legs-core" }));
            Assert.That(Ids(cut, "value"), Is.EqualTo(new[] { "list-value-upper-body", "list-value-legs-core" }));
            Assert.That(Ids(cut, "bar"), Is.EqualTo(new[] { "list-bar-upper-body", "list-bar-legs-core" }));
        });
    }

    [Test]
    public void TestIdBy_Slug_NumbersRepeatedSlugsFromTwo()
    {
        var cut = RenderBy([new("Core", 1), new("core", 2), new("CORE!", 3), new("Core 2", 4)], BarListTestIdBy.Slug);

        Assert.That(Ids(cut, "row"), Is.EqualTo(new[] { "list-row-core", "list-row-core-2", "list-row-core-3", "list-row-core-2-2" }));
    }

    [Test]
    public void TestIdBy_Slug_GivesALabelWithNoLettersOrDigitsAnId()
    {
        var cut = RenderBy([new("!!!", 1), new("???", 2)], BarListTestIdBy.Slug);

        Assert.That(Ids(cut, "row"), Is.EqualTo(new[] { "list-row-item", "list-row-item-2" }));
    }

    [Test]
    public void TestIdBy_Default_KeepsThePositionIds()
    {
        var cut = RenderBy([new("Core", 1), new("Core", 2)], BarListTestIdBy.Position);

        Assert.That(Ids(cut, "row"), Is.EqualTo(new[] { "list-row-0", "list-row-1" }));
    }

    [Test]
    public void TestIdBy_Slug_FollowsTheItemsWhenTheyChange()
    {
        var cut = RenderBy([new("A", 1)], BarListTestIdBy.Slug);

        cut.Render(p => p.Add(x => x.Items, [new BarListItem("B", 1), new BarListItem("A", 1)]));

        Assert.That(Ids(cut, "row"), Is.EqualTo(new[] { "list-row-b", "list-row-a" }));
    }

    [Test]
    public void AllZeroText_ReplacesTheBarsWhenEveryValueIsZero()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 0), new BarListItem("B", 0)]).Add(x => x.AllZeroText, "Nothing logged").Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            var text = cut.Find("[data-testid=list-all-zero]");
            Assert.That(text.TextContent, Is.EqualTo("Nothing logged"));
            Assert.That(text.ClassList, Is.EquivalentTo(Context.Render<BarList>(p => p.Add(x => x.TestId, "e")).Find("[data-testid=e-empty]").ClassList));
            Assert.That(cut.FindAll("li, .fill"), Is.Empty);
        });
    }

    [Test]
    public void AllZeroText_ShowsWhenEveryValueCountsAsZero()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 0), new BarListItem("B", double.NaN), new BarListItem("C", -3)]).Add(x => x.AllZeroText, "Nothing logged").Add(x => x.TestId, "list"));

        Assert.That(cut.Find("[data-testid=list-all-zero]").TextContent, Is.EqualTo("Nothing logged"));
    }

    [Test]
    public void AllZeroText_WithOneNonZeroValue_ShowsTheBars()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.Items, [new BarListItem("A", 0), new BarListItem("B", 1)]).Add(x => x.AllZeroText, "Nothing logged").Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("li"), Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=list-all-zero]"), Is.Empty);
        });
    }

    [Test]
    public void AllZeroText_WithNoRows_ShowsTheEmptyTextInstead()
    {
        var cut = Context.Render<BarList>(p => p.Add(x => x.AllZeroText, "Nothing logged").Add(x => x.EmptyText, "No items").Add(x => x.TestId, "list"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-empty]").TextContent, Is.EqualTo("No items"));
            Assert.That(cut.FindAll("[data-testid=list-all-zero]"), Is.Empty);
        });
    }

    [Test]
    public void WithoutAllZeroText_AnAllZeroListShowsItsRows()
    {
        var cut = Render([new("A", 0), new("B", 0)]);

        Assert.That(cut.FindAll("li"), Has.Count.EqualTo(2));
    }
}
