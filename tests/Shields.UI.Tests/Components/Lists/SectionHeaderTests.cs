using System.Text.RegularExpressions;
using Shields.UI.Components.Actions;
using Shields.UI.Components.Lists;

namespace Shields.UI.Tests.Components.Lists;

public class SectionHeaderTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheTitleInAHeading()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=head-title]").TextContent, Is.EqualTo("Morning"));
            Assert.That(cut.Find("h2 [data-testid=head-title]"), Is.Not.Null);
            Assert.That(cut.FindAll("[data-testid=head-total],[data-testid=head-badge]"), Is.Empty);
        });
    }

    [Test]
    public void Render_ShowsTheTotal()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Total, "1,250 total")
            .Add(x => x.TestId, "head"));

        Assert.That(cut.Find("[data-testid=head-total]").TextContent, Is.EqualTo("1,250 total"));
    }

    [Test]
    public void Render_ShowsTheBadgeAfterTheTitle()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Badge, "Now")
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=head-badge]").TextContent, Is.EqualTo("Now"));
            Assert.That(cut.Find("[data-testid=head-title]").NextElementSibling!.GetAttribute("data-testid"), Is.EqualTo("head-badge"));
        });
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Badge, "Now")
            .Add(x => x.Total, "3"));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    [Test]
    public void Render_WithoutCallback_IsAPlainHeading()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Expanded, true)
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button"), Is.Empty);
            Assert.That(cut.FindAll("[aria-expanded]"), Is.Empty);
            Assert.That(cut.FindAll("svg"), Is.Empty);
            Assert.That(cut.Find("h2 [data-testid=head-title]").TextContent, Is.EqualTo("Morning"));
        });
    }

    [TestCase(true, "true")]
    [TestCase(false, "false")]
    public void Render_WithCallback_IsAButtonWithAriaExpanded(bool expanded, string expected)
    {
        var cut = RenderCollapsible(expanded);

        var toggle = cut.Find("h2 button[data-testid=head-toggle]");
        Assert.That(toggle.GetAttribute("aria-expanded"), Is.EqualTo(expected));
    }

    [TestCase(true, "panel-1")]
    [TestCase(false, null)]
    public void Render_WithControlsId_SetsAriaControlsOnlyWhileExpanded(bool expanded, string? expected)
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Expanded, expanded)
            .Add(x => x.ControlsId, "panel-1")
            .Add(x => x.ExpandedChanged, _ => { })
            .Add(x => x.TestId, "head"));

        Assert.That(cut.Find("button").GetAttribute("aria-controls"), Is.EqualTo(expected));
    }

    [Test]
    public void Render_WithoutControlsId_WritesNoAriaControls()
    {
        var cut = RenderCollapsible(true);

        Assert.That(cut.Find("button").HasAttribute("aria-controls"), Is.False);
    }

    [TestCase(true, IconName.Up)]
    [TestCase(false, IconName.Down)]
    public void Render_ChevronPointsUpWhenExpandedAndDownWhenNot(bool expanded, IconName expected)
    {
        var cut = RenderCollapsible(expanded);

        Assert.That(cut.FindComponent<Icon>().Instance.Name, Is.EqualTo(expected));
    }

    [Test]
    public void Render_WithCallback_KeepsTitleBadgeAndTotalInsideTheButton()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Badge, "Now")
            .Add(x => x.Total, "3")
            .Add(x => x.ExpandedChanged, _ => { })
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button [data-testid=head-title]").TextContent, Is.EqualTo("Morning"));
            Assert.That(cut.Find("button [data-testid=head-badge]").TextContent, Is.EqualTo("Now"));
            Assert.That(cut.Find("button [data-testid=head-total]").TextContent, Is.EqualTo("3"));
        });
    }

    [Test]
    public void Render_VeryLongTitle_WrapsAndTotalStaysOnOneLine()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Lists", "SectionHeader.razor.css");
        var total = Regex.Match(css, @"\.total\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, new string('W', 200) + " morning snack with a very long name")
            .Add(x => x.Total, "1,250 total")
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=head-title]").HasAttribute("data-wrap-ok"), Is.True);
            Assert.That(total, Does.Match(@"white-space\s*:\s*nowrap"));
        });
    }

    [Test]
    public void Click_RaisesExpandedChangedWithTheOppositeValue()
    {
        var raised = new List<bool>();
        var cut = RenderCollapsible(false, raised.Add);

        cut.Find("[data-testid=head-toggle]").Click();

        Assert.That(raised, Is.EqualTo(new[] { true }));
    }

    [Test]
    public void Click_WhenTheParentRebinds_TogglesBothWays()
    {
        var raised = new List<bool>();
        var cut = RenderCollapsible(false, raised.Add);

        cut.Find("button").Click();
        cut.Render(p => p.Add(x => x.Expanded, raised[^1]));
        Assert.That(cut.Find("button").GetAttribute("aria-expanded"), Is.EqualTo("true"));

        cut.Find("button").Click();
        cut.Render(p => p.Add(x => x.Expanded, raised[^1]));

        Assert.Multiple(() =>
        {
            Assert.That(raised, Is.EqualTo(new[] { true, false }));
            Assert.That(cut.Find("button").GetAttribute("aria-expanded"), Is.EqualTo("false"));
        });
    }

    private IRenderedComponent<SectionHeader> RenderCollapsible(bool expanded, Action<bool>? onChanged = null) =>
        Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Expanded, expanded)
            .Add(x => x.ExpandedChanged, onChanged ?? (_ => { }))
            .Add(x => x.TestId, "head"));
}
