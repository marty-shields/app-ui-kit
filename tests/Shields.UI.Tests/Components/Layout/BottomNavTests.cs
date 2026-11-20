using Shields.UI.Components.Actions;
using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class BottomNavTests : ComponentTestBase
{
    private static readonly BottomNavItem Home = new("Home", IconName.Today, "home-link");
    private static readonly BottomNavItem Stats = new("Stats", IconName.BarChart, "stats-link");
    private static readonly BottomNavItem More = new("More", IconName.Settings, "more-link");

    private IRenderedComponent<BottomNav> Render(IReadOnlyList<BottomNavItem> items, BottomNavItem? current, Action<ComponentParameterCollectionBuilder<BottomNav>>? more = null) =>
        Context.Render<BottomNav>(p =>
        {
            p.Add(x => x.Items, items).Add(x => x.Current, current).Add(x => x.AddLabel, "New").Add(x => x.AddIcon, IconName.Plus);
            more?.Invoke(p);
        });

    [Test]
    public void Render_ShowsTheGivenItemsAroundTheRaisedButton()
    {
        var cut = Render([Home, Stats], Home);

        var buttons = cut.FindAll("nav button").Select(b => (b.GetAttribute("data-testid"), b.TextContent.Trim(), b.QuerySelectorAll("svg").Length));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("nav").GetAttribute("aria-label"), Is.EqualTo("Main"));
            Assert.That(cut.Find("nav").GetAttribute("data-testid"), Is.EqualTo("bottom-nav"));
            Assert.That(buttons, Is.EqualTo(new[] { ("home-link", "Home", 1), ("add-button", "New", 1), ("stats-link", "Stats", 1) }));
        });
    }

    [Test]
    public void Render_OddCount_PutsTheExtraItemLeftOfTheRaisedButton()
    {
        var cut = Render([Home, Stats, More], Home);

        var add = cut.Find("[data-testid=add-button]");

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".side").Select(s => s.QuerySelectorAll("button").Select(b => b.GetAttribute("data-testid"))),
                Is.EqualTo(new[] { new[] { "home-link", "stats-link" }, new[] { "more-link" } }));
            Assert.That(add.PreviousElementSibling, Is.SameAs(cut.FindAll(".side")[0]));
        });
    }

    [Test]
    public void Render_MarksOnlyTheCurrentItem_AndNeverTheRaisedButton()
    {
        var cut = Render([Home, Stats], Stats);

        var marked = cut.FindAll("[aria-current]").Select(b => (b.GetAttribute("data-testid"), b.GetAttribute("aria-current"), b.ClassList.Contains("current")));

        Assert.Multiple(() =>
        {
            Assert.That(marked, Is.EqualTo(new[] { ("stats-link", "page", true) }));
            Assert.That(cut.Find("[data-testid=add-button]").HasAttribute("aria-current"), Is.False);
        });
    }

    [Test]
    public void Render_WithNoCurrent_MarksNothing() =>
        Assert.That(Render([Home, Stats], null).FindAll("[aria-current]"), Is.Empty);

    [Test]
    public void Render_AddHasPopup_IsOnlyOnTheRaisedButtonWhenGiven()
    {
        var without = Render([Home, Stats], Home);
        var with = Render([Home, Stats], Home, p => p.Add(x => x.AddHasPopup, "dialog"));

        Assert.Multiple(() =>
        {
            Assert.That(without.Find("[data-testid=add-button]").HasAttribute("aria-haspopup"), Is.False);
            Assert.That(with.Find("[data-testid=add-button]").GetAttribute("aria-haspopup"), Is.EqualTo("dialog"));
            Assert.That(with.FindAll("[aria-haspopup]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TappingAnItem_RaisesOnNavigateWithIt()
    {
        var navigated = new List<BottomNavItem>();
        var cut = Render([Home, Stats], Home, p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=stats-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { Stats }));
    }

    [Test]
    public void TappingTheCurrentItem_RaisesNothing()
    {
        var navigated = new List<BottomNavItem>();
        var cut = Render([Home, Stats], Home, p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=home-link]").Click();

        Assert.That(navigated, Is.Empty);
    }

    [Test]
    public void TappingTheRaisedButton_RaisesOnAddAndNavigatesNowhere()
    {
        var navigated = new List<BottomNavItem>();
        var added = 0;
        var cut = Render([Home, Stats], Home, p => p.Add(x => x.OnNavigate, navigated.Add).Add(x => x.OnAdd, () => added++));

        cut.Find("[data-testid=add-button]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.EqualTo(1));
            Assert.That(navigated, Is.Empty);
        });
    }
}
