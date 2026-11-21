using Shields.UI.Components.Feedback;

namespace Shields.UI.Tests.Components.Feedback;

public class BannerTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheContentWithTheSelectedTone()
    {
        var cut = Context.Render<Banner>(p => p
            .AddChildContent("Adding to Lunch")
            .Add(x => x.TestId, "banner"));

        var banner = cut.Find("[data-testid=banner]");
        Assert.Multiple(() =>
        {
            Assert.That(banner.TextContent, Is.EqualTo("Adding to Lunch"));
            Assert.That(banner.ClassList, Does.Contain("selected"));
        });
    }

    [Test]
    public void Tone_Selected_PutsItsClassOnTheBanner()
    {
        var cut = Context.Render<Banner>(p => p.Add(x => x.Tone, BannerTone.Selected));

        Assert.That(cut.Find(".banner").ClassList, Does.Contain("selected"));
    }

    [Test]
    public void Render_WithoutTestId_WritesNoTestId()
    {
        var cut = Context.Render<Banner>(p => p.AddChildContent("x"));

        Assert.That(cut.Find(".banner").HasAttribute("data-testid"), Is.False);
    }

    [Test]
    public void Css_UsesTheSelectedBackgroundLargeRadiusAndBoldText()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Feedback", "Banner.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(TouchTargetCheck.Declarations(css, ".selected"), Has.Member("background-color: var(--color-selected)"));
            Assert.That(TouchTargetCheck.Declarations(css, ".banner"), Has.Member("border-radius: var(--radius-lg)").And.Member("font-weight: var(--font-weight-bold)"));
        });
    }
}
