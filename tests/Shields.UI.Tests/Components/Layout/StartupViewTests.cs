using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class StartupViewTests : ComponentTestBase
{
    [Test]
    public void NotFailed_ShowsSpinner()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=startup-loading]").TextContent.Trim(), Is.EqualTo("Loading…"));
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=startup-retry]"), Is.Empty);
        });
    }

    [Test]
    public void LoadingLabel_ReplacesTheDefault()
    {
        var cut = Render(p => p.Add(x => x.LoadingLabel, "Opening…"));

        Assert.That(cut.Find("[data-testid=startup-loading]").TextContent.Trim(), Is.EqualTo("Opening…"));
    }

    [Test]
    public void Failed_ShowsTheMessageInsteadOfSpinner()
    {
        var cut = Render(p => p.Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=startup-error]").TextContent, Is.EqualTo("Couldn't open the database"));
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Is.Empty);
        });
    }

    [Test]
    public void FailedWithoutOnRetry_OffersNoTryAgain()
    {
        var cut = Render(p => p.Add(x => x.Failed, true));

        Assert.That(cut.FindAll("[data-testid=startup-retry]"), Is.Empty);
    }

    [Test]
    public void Failed_MarksTheViewStartupFailed_WithAndWithoutOnRetry()
    {
        var without = Render(p => p.Add(x => x.Failed, true));
        var with = Render(p => p.Add(x => x.Failed, true).Add(x => x.OnRetry, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(without.FindAll("[data-testid=startup-failed]"), Has.Count.EqualTo(1));
            Assert.That(with.FindAll("[data-testid=startup-failed]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void NotFailed_HasNoStartupFailedMarker() =>
        Assert.That(Render().FindAll("[data-testid=startup-failed]"), Is.Empty);

    [Test]
    public void FailedWithRetry_ShowsAPlainCentredFailure()
    {
        var cut = Render(p => p.Add(x => x.Failed, true).Add(x => x.OnRetry, () => { }));

        Assert.That(cut.Find("[data-testid=startup-failed] .failed").ClassList, Does.Contain("plain"));
    }

    [Test]
    public void Failed_TryAgainRaisesOnRetry()
    {
        var retries = 0;
        var cut = Render(p => p
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=startup-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [TestCase("My App", 1)]
    [TestCase(null, 0)]
    public void Title_ShowsOnlyWhenSet(string? title, int count)
    {
        var cut = Render(p => p.Add(x => x.Title, title));

        Assert.That(cut.FindAll(".title"), Has.Count.EqualTo(count));
    }

    [Test]
    public void Title_ShowsInATopBar()
    {
        var bar = Render(p => p.Add(x => x.Title, "My App")).Find("[data-testid=startup-top-bar]");

        Assert.Multiple(() =>
        {
            Assert.That(bar.TagName, Is.EqualTo("HEADER"));
            Assert.That(bar.QuerySelector(".title")!.TextContent, Is.EqualTo("My App"));
        });
    }

    [Test]
    public void WithoutTitle_ShowsNoTopBar() =>
        Assert.That(Render().FindAll("[data-testid=startup-top-bar]"), Is.Empty);

    [TestCase(".top-bar")]
    [TestCase(".title")]
    public void Css_MatchesAppScreen(string selector) =>
        Assert.That(CssRules.Declarations("StartupView", selector), Is.EqualTo(CssRules.Declarations("AppScreen", selector)));

    private IRenderedComponent<StartupView> Render(Action<ComponentParameterCollectionBuilder<StartupView>>? more = null) =>
        Context.Render<StartupView>(p =>
        {
            p.Add(x => x.FailedMessage, "Couldn't open the database");
            more?.Invoke(p);
        });
}
