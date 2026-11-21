using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class ErrorViewTests : ComponentTestBase
{
    [Test]
    public void ShowsTheMessage() =>
        Assert.That(Context.Render<ErrorView>().Find("[data-testid=error-message]").TextContent, Is.EqualTo("Something went wrong"));

    [Test]
    public void Restart_RaisesOnRestart()
    {
        var restarted = 0;
        var cut = Context.Render<ErrorView>(p => p.Add(x => x.OnRestart, () => restarted++));

        cut.Find("[data-testid=error-restart]").Click();

        Assert.That(restarted, Is.EqualTo(1));
    }

    [TestCase("My App", 1)]
    [TestCase(null, 0)]
    public void Title_ShowsOnlyWhenSet(string? title, int count) =>
        Assert.That(Context.Render<ErrorView>(p => p.Add(x => x.Title, title)).FindAll(".title"), Has.Count.EqualTo(count));

    [Test]
    public void Title_ShowsInATopBar()
    {
        var bar = Context.Render<ErrorView>(p => p.Add(x => x.Title, "My App")).Find("[data-testid=error-top-bar]");

        Assert.Multiple(() =>
        {
            Assert.That(bar.TagName, Is.EqualTo("HEADER"));
            Assert.That(bar.QuerySelector(".title")!.TextContent, Is.EqualTo("My App"));
        });
    }

    [Test]
    public void WithoutTitle_ShowsNoTopBar() =>
        Assert.That(Context.Render<ErrorView>().FindAll("[data-testid=error-top-bar]"), Is.Empty);

    [Test]
    public void Restart_SitsInTheBottomBar() =>
        Assert.That(Context.Render<ErrorView>().Find("[data-testid=error-bottom-bar] [data-testid=error-restart]").ClassList, Does.Contain("full"));

    [TestCase(".top-bar")]
    [TestCase(".title")]
    public void Css_MatchesAppScreen(string selector) =>
        Assert.That(CssRules.Declarations("ErrorView", selector), Is.EqualTo(CssRules.Declarations("AppScreen", selector)));

    [TestCase("border-top")]
    [TestCase("background-color")]
    public void BottomCss_MatchesAppScreen(string property) =>
        Assert.That(CssRules.Declarations("ErrorView", ".bottom")[property], Is.EqualTo(CssRules.Declarations("AppScreen", ".bottom")[property]));

    [Test]
    public void BottomBarCss_PadsLikeAppScreensLastBottomBar() =>
        Assert.That(
            CssRules.Declarations("ErrorView", ".bottom-bar")["padding"],
            Is.EqualTo("var(--space-3) var(--space-4) calc(var(--space-3) + var(--inset-bottom))"));
}
