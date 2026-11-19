using System.Text.RegularExpressions;
using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class CardTests : ComponentTestBase
{
    [Test]
    public void ShowsTheTitleAsAHeadingWithTheAsideAndContent()
    {
        var cut = Context.Render<Card>(p => p
            .Add(x => x.Title, "Records")
            .Add(x => x.Aside, "1 this week")
            .Add(x => x.ChildContent, "<p class=\"inside\">Hello</p>")
            .Add(x => x.TestId, "card"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("h2").TextContent, Is.EqualTo("Records"));
            Assert.That(cut.Find("[data-testid=card-aside]").TextContent, Is.EqualTo("1 this week"));
            Assert.That(cut.Find(".inside").TextContent, Is.EqualTo("Hello"));
        });
    }

    [Test]
    public void WithoutAnAside_ShowsNone()
    {
        var cut = Context.Render<Card>(p => p.Add(x => x.Title, "Weeks").Add(x => x.TestId, "card"));

        Assert.That(cut.FindAll("[data-testid=card-aside]"), Is.Empty);
    }

    [Test]
    public void WithoutATitle_ShowsOnlyTheContent()
    {
        var cut = Context.Render<Card>(p => p
            .Add(x => x.Aside, "ignored")
            .AddChildContent("<span>Eaten 659</span>")
            .Add(x => x.TestId, "card"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=card] span").TextContent, Is.EqualTo("Eaten 659"));
            Assert.That(cut.FindAll("h2, .header, .aside"), Is.Empty);
        });
    }

    [Test]
    public void Css_TakesSurfaceBorderAndRadiusFromTokens()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "Card.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(Rule(css, ".card"), Does.Contain("background-color: var(--color-surface);"));
            Assert.That(Rule(css, ".card"), Does.Contain("border: var(--border-width) solid var(--color-border);"));
            Assert.That(Rule(css, ".card"), Does.Contain("border-radius: var(--radius-lg);"));
        });
    }

    [Test]
    public void Title_IsBoldAndTheMdSize_Which16px()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "Card.razor.css");
        var tokens = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");

        Assert.Multiple(() =>
        {
            Assert.That(Rule(css, ".title"), Does.Contain("font-size: var(--font-size-md);"));
            Assert.That(Rule(css, ".title"), Does.Contain("font-weight: var(--font-weight-bold);"));
            Assert.That(tokens, Does.Contain("--font-size-md: 1rem;"));
        });
    }

    private static string Rule(string css, string selector) =>
        Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
}
