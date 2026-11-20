using Shields.UI.Components.Lists;

namespace Shields.UI.Tests.Components.Lists;

public class StatTileTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsLabelFirstThenValue()
    {
        var cut = Context.Render<StatTile>(p => p
            .Add(x => x.Label, "This week")
            .Add(x => x.Value, "3")
            .Add(x => x.TestId, "tile"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=tile-label]").TextContent, Is.EqualTo("This week"));
            Assert.That(cut.Find("[data-testid=tile-value]").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find(".tile").Children.Select(c => c.ClassList.First()), Is.EqualTo(new[] { "label", "value" }));
            Assert.That(cut.FindAll("[data-testid=tile-change],[data-testid=tile-caption],[data-testid=tile-note]"), Is.Empty);
        });
    }

    [Test]
    public void Render_ChangeUp_ShowsAnUpArrowAndTheAmount()
    {
        var cut = RenderWithChange(new StatChange(ChangeDirection.Up, "35m"));

        var change = cut.Find("[data-testid=tile-change]");
        Assert.Multiple(() =>
        {
            Assert.That(change.ClassList, Does.Contain("up"));
            Assert.That(change.QuerySelector("svg.success"), Is.Not.Null);
            Assert.That(change.TextContent.Trim(), Is.EqualTo("up 35m"));
        });
    }

    [Test]
    public void Render_ChangeDown_ShowsADownArrowAndTheAmount()
    {
        var cut = RenderWithChange(new StatChange(ChangeDirection.Down, "6"));

        var change = cut.Find("[data-testid=tile-change]");
        Assert.Multiple(() =>
        {
            Assert.That(change.ClassList, Does.Contain("down"));
            Assert.That(change.QuerySelector("svg.error"), Is.Not.Null);
            Assert.That(change.TextContent.Trim(), Is.EqualTo("down 6"));
        });
    }

    [Test]
    public void Render_NoChange_ShowsAMutedDashAndNoArrow()
    {
        var cut = RenderWithChange(new StatChange(ChangeDirection.None, "ignored"));

        var change = cut.Find("[data-testid=tile-change]");
        Assert.Multiple(() =>
        {
            Assert.That(change.ClassList, Does.Contain("same"));
            Assert.That(change.QuerySelector("svg"), Is.Null);
            Assert.That(change.QuerySelector("[aria-hidden=true]")!.TextContent, Is.EqualTo("—"));
            Assert.That(change.QuerySelector(".visually-hidden")!.TextContent, Is.EqualTo("no change"));
            Assert.That(change.TextContent, Does.Not.Contain("ignored"));
        });
    }

    [Test]
    public void Render_Caption_ShowsItUnderTheChange()
    {
        var cut = Context.Render<StatTile>(p => p
            .Add(x => x.Label, "Volume")
            .Add(x => x.Value, "12.4 t")
            .Add(x => x.Change, new StatChange(ChangeDirection.Up, "8%"))
            .Add(x => x.Caption, "strength")
            .Add(x => x.TestId, "tile"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=tile-caption]").TextContent, Is.EqualTo("strength"));
            Assert.That(cut.Find(".tile").Children.Select(c => c.ClassList.First()), Is.EqualTo(new[] { "label", "value", "change", "caption" }));
        });
    }

    [Test]
    public void Render_NotEnoughData_ReplacesTheValueDropsTheChangeAndKeepsTheCaption()
    {
        var cut = Context.Render<StatTile>(p => p
            .Add(x => x.Label, "Average")
            .Add(x => x.Value, "1,980")
            .Add(x => x.Change, new StatChange(ChangeDirection.Up, "5%"))
            .Add(x => x.Caption, "avg kcal a day")
            .Add(x => x.NotEnoughData, true)
            .Add(x => x.TestId, "tile"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=tile-value]").TextContent, Is.EqualTo("–"));
            Assert.That(cut.Find("[data-testid=tile-caption]").TextContent, Is.EqualTo("avg kcal a day"));
            Assert.That(cut.Find("[data-testid=tile-note]").TextContent, Is.EqualTo("Not enough data yet"));
            Assert.That(cut.FindAll("[data-testid=tile-change]"), Is.Empty);
            Assert.That(cut.Markup, Does.Not.Contain("1,980"));
        });
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<StatTile>(p => p
            .Add(x => x.Label, "Workouts")
            .Add(x => x.Value, "3")
            .Add(x => x.Caption, "this week")
            .Add(x => x.NotEnoughData, true));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    private IRenderedComponent<StatTile> RenderWithChange(StatChange change) =>
        Context.Render<StatTile>(p => p
            .Add(x => x.Label, "Time")
            .Add(x => x.Value, "3h")
            .Add(x => x.Change, change)
            .Add(x => x.TestId, "tile"));
}
