using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Actions;
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
    public void Render_WithActions_ShowsThemUnderTheTextInTheirOwnRow()
    {
        var cut = Context.Render<Banner>(p => p
            .AddChildContent("Workout in progress")
            .Add(x => x.Actions, (RenderFragment)(b =>
            {
                b.OpenComponent<AppButton>(0);
                b.AddAttribute(1, "TestId", "resume");
                b.CloseComponent();
            }))
            .Add(x => x.TestId, "banner"));

        var banner = cut.Find("[data-testid=banner]");
        Assert.Multiple(() =>
        {
            Assert.That(banner.ClassList, Does.Contain("with-actions"));
            Assert.That(cut.Find("[data-testid=banner-actions]").ClassList, Does.Contain("actions"));
            Assert.That(cut.Find(".actions [data-testid=resume]"), Is.Not.Null);
            Assert.That(banner.FirstChild!.TextContent, Is.EqualTo("Workout in progress"));
        });
    }

    [Test]
    public void Render_WithoutActions_WritesNoActionsRow()
    {
        var cut = Context.Render<Banner>(p => p.AddChildContent("x").Add(x => x.TestId, "banner"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".actions"), Is.Empty);
            Assert.That(cut.Find(".banner").ClassList, Does.Not.Contain("with-actions"));
        });
    }

    [Test]
    public void Render_WithActionsAndNoTestId_WritesNoTestIdOnTheRow()
    {
        var cut = Context.Render<Banner>(p => p.Add(x => x.Actions, (RenderFragment)(b => b.AddContent(0, "a"))));

        Assert.That(cut.Find(".actions").HasAttribute("data-testid"), Is.False);
    }

    [Test]
    public void Css_PutsTheActionsInAWrappingRowAndPadsABannerThatHasThem()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Feedback", "Banner.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(TouchTargetCheck.Declarations(css, ".actions"), Has.Member("display: flex").And.Member("flex-wrap: wrap").And.Member("gap: var(--space-2)"));
            Assert.That(TouchTargetCheck.Declarations(css, ".with-actions"), Has.Member("padding: var(--space-3)"));
        });
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
