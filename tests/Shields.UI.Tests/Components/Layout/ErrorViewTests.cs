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
}
