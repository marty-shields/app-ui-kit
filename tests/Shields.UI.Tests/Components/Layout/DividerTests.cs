using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class DividerTests : ComponentTestBase
{
    [Test]
    public void Render_IsAHorizontalSeparatorByDefault()
    {
        var cut = Context.Render<Divider>(p => p.Add(x => x.TestId, "rule"));

        var rule = cut.Find("[data-testid=rule]");
        Assert.Multiple(() =>
        {
            Assert.That(rule.GetAttribute("role"), Is.EqualTo("separator"));
            Assert.That(rule.GetAttribute("aria-orientation"), Is.EqualTo("horizontal"));
            Assert.That(rule.HasAttribute("aria-hidden"), Is.False);
            Assert.That(rule.ClassList, Does.Contain("horizontal").And.Not.Contain("vertical"));
        });
    }

    [Test]
    public void Vertical_IsAVerticalSeparator()
    {
        var cut = Context.Render<Divider>(p => p.Add(x => x.Vertical, true));

        var rule = cut.Find("[role=separator]");
        Assert.Multiple(() =>
        {
            Assert.That(rule.GetAttribute("aria-orientation"), Is.EqualTo("vertical"));
            Assert.That(rule.ClassList, Does.Contain("vertical").And.Not.Contain("horizontal"));
        });
    }

    [Test]
    public void Decorative_HidesTheLineFromScreenReaders()
    {
        var cut = Context.Render<Divider>(p => p.Add(x => x.Decorative, true).Add(x => x.Vertical, true));

        var rule = cut.Find(".divider");
        Assert.Multiple(() =>
        {
            Assert.That(rule.GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(rule.HasAttribute("role"), Is.False);
            Assert.That(rule.HasAttribute("aria-orientation"), Is.False);
        });
    }

    [Test]
    public void Render_WithoutTestId_WritesNoTestId()
    {
        var cut = Context.Render<Divider>();

        Assert.That(cut.Find(".divider").HasAttribute("data-testid"), Is.False);
    }

    [Test]
    public void Css_VerticalStretchesInAFlexRow_AndBothUseTheBorderColour()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "Divider.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(TouchTargetCheck.Declarations(css, ".vertical"), Has.Member("align-self: stretch"));
            Assert.That(TouchTargetCheck.Declarations(css, ".divider"), Has.Member("background-color: var(--color-border)"));
        });
    }
}
