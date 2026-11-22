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
    public void Render_ShowsTheHintWithTestIdBeforeTheTotal()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Lunch")
            .Add(x => x.Hint, "from 11:00")
            .Add(x => x.Total, "521 kcal")
            .Add(x => x.TestId, "head"));

        var hint = cut.Find("[data-testid=head-hint]");
        Assert.Multiple(() =>
        {
            Assert.That(hint.TextContent, Is.EqualTo("from 11:00"));
            Assert.That(hint.ClassList, Does.Contain("hint"));
            Assert.That(hint.ParentElement!.NextElementSibling!.GetAttribute("data-testid"), Is.EqualTo("head-total"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Render_WithNullOrBlankHint_RendersNoHint(string? hint)
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Lunch")
            .Add(x => x.Hint, hint)
            .Add(x => x.TestId, "head"));

        Assert.That(cut.FindAll(".hint,[data-testid=head-hint]"), Is.Empty);
    }

    [Test]
    public void Render_PlainHeader_ShowsTheHintInsideTheHeading()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Lunch")
            .Add(x => x.Hint, "from 11:00")
            .Add(x => x.TestId, "head"));

        Assert.That(cut.Find("h2 span.toggle [data-testid=head-hint]").TextContent, Is.EqualTo("from 11:00"));
    }

    [Test]
    public void Render_CollapsibleHeader_PutsTheHintInTheButtonSoItIsInTheAccessibleName()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Lunch")
            .Add(x => x.Hint, "from 11:00")
            .Add(x => x.ExpandedChanged, _ => { })
            .Add(x => x.TestId, "head"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button [data-testid=head-hint]").TextContent, Is.EqualTo("from 11:00"));
            Assert.That(cut.Find("button").TextContent, Does.Contain("from 11:00"));
            Assert.That(cut.Find("button").HasAttribute("aria-describedby"), Is.False);
        });
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<SectionHeader>(p => p
            .Add(x => x.Title, "Morning")
            .Add(x => x.Badge, "Now")
            .Add(x => x.Hint, "from 11:00")
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
    public void Css_LeadGroupWrapsSoTheHintDropsUnderTheTitleWhenNarrow()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Lists", "SectionHeader.razor.css");
        var lead = Regex.Match(css, @"\.lead\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(lead, Does.Match(@"flex-wrap\s*:\s*wrap"));
            Assert.That(lead, Does.Match(@"flex\s*:\s*1\s*;"));
            Assert.That(lead, Does.Match(@"min-width\s*:\s*0"));
        });
    }

    [Test]
    public void Css_HintIsMutedSmallAndNeverWraps()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Lists", "SectionHeader.razor.css");
        var hint = Regex.Match(css, @"\.hint\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(hint, Does.Match(@"white-space\s*:\s*nowrap"));
            Assert.That(hint, Does.Match(@"flex-shrink\s*:\s*0"));
            Assert.That(hint, Does.Match(@"color\s*:\s*var\(--color-text-muted\)"));
            Assert.That(hint, Does.Match(@"font-size\s*:\s*var\(--font-size-sm\)"));
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

    [Test]
    public void Sticky_AddsTheStickyClass_AndIsOffByDefault()
    {
        var sticky = Context.Render<SectionHeader>(p => p.Add(x => x.Title, "November").Add(x => x.Total, "4").Add(x => x.Sticky, true));
        var plain = Context.Render<SectionHeader>(p => p.Add(x => x.Title, "November"));

        Assert.Multiple(() =>
        {
            Assert.That(sticky.Find(".header").ClassList, Does.Contain("sticky"));
            Assert.That(plain.Find(".header").ClassList, Does.Not.Contain("sticky"));
        });
    }

    [Test]
    public void Css_Sticky_StaysAtTheTopOnThePageColourOverAppScreensPadding()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Lists", "SectionHeader.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("position: sticky;"));
            Assert.That(css, Does.Contain("top: var(--section-header-sticky-top);"));
            Assert.That(css, Does.Contain("background-color: var(--color-bg);"));
            Assert.That(KitCss.Tokens, Does.Contain("--section-header-sticky-top: calc(-1 * var(--space-4));"));
        });
    }
}
