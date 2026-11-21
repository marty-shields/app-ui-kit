using System.Text.RegularExpressions;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class ProgressRingTests : ComponentTestBase
{
    [TestCase(0, 2100, "0", "0", "0 of 2,100")]
    [TestCase(659, 2100, "659", "0.3138", "659 of 2,100")]
    [TestCase(2100, 2100, "2,100", "1", "2,100 of 2,100")]
    public void Render_WithinMax_ShowsTheValueAndFill(double value, double max, string text, string fill, string description)
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, value).Add(x => x.Max, max).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo(text));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain($"--fill: {fill}"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("aria-label"), Is.EqualTo(description));
            Assert.That(cut.Find("[data-testid=ring]").ClassList, Does.Not.Contain("over"));
        });
    }

    [Test]
    public void Render_AboveMax_IsOverAndFull()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 2359).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring]").ClassList, Does.Contain("over"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 1"));
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("2,359"));
        });
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void Render_WithoutAPositiveMax_IsFullOnceAboveZeroAndNeverOver(double max)
    {
        var some = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 659).Add(x => x.Max, max).Add(x => x.TestId, "ring"));
        var none = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 0).Add(x => x.Max, max).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(some.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 1"));
            Assert.That(some.Find("[data-testid=ring]").ClassList, Does.Not.Contain("over"));
            Assert.That(some.Find("[data-testid=ring]").GetAttribute("aria-label"), Is.EqualTo("659"));
            Assert.That(none.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 0"));
        });
    }

    [Test]
    public void Render_NegativeValue_CountsAsZero()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, -50).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 0"));
        });
    }

    [Test]
    public void Render_FractionalValue_RoundsForDisplay()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 658.5).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("659"));
    }

    [Test]
    public void Compact_AddsTheCompactClassAndUsesTheCompactTokens()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "ProgressRing.razor.css");
        var normal = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 100).Add(x => x.TestId, "ring"));
        var compact = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 100).Add(x => x.Compact, true).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(normal.Find("[data-testid=ring]").ClassList, Does.Not.Contain("compact"));
            Assert.That(compact.Find("[data-testid=ring]").ClassList, Does.Contain("compact"));
            Assert.That(css.Split(".compact {")[1].Split('}')[0], Does.Contain("--size: var(--ring-size-compact)").And.Contain("--stroke: var(--ring-width-compact)"));
        });
    }

    [Test]
    public void Render_Caption_ShowsUnderTheValueAndInTheName()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 659).Add(x => x.Max, 2100).Add(x => x.Caption, "steps").Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-caption]").TextContent, Is.EqualTo("steps"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("aria-label"), Is.EqualTo("659 of 2,100 steps"));
        });
    }

    [Test]
    public void Render_WithoutCaption_ShowsNone()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 1).Add(x => x.TestId, "ring"));

        Assert.That(cut.FindAll("[data-testid=ring-caption]"), Is.Empty);
    }

    [Test]
    public void Render_ChildContent_ReplacesTheValueText()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 659).Add(x => x.Max, 2100).Add(x => x.TestId, "ring")
            .Add(x => x.ChildContent, "<b data-testid=\"left\">1,441</b>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=left]").TextContent, Is.EqualTo("1,441"));
            Assert.That(cut.FindAll("[data-testid=ring-value]"), Is.Empty);
        });
    }

    [Test]
    public void Render_AriaLabel_OverridesTheDefaultName()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 659).Add(x => x.Max, 2100).Add(x => x.Caption, "left").Add(x => x.AriaLabel, "1,441 left").Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("aria-label"), Is.EqualTo("1,441 left"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("role"), Is.EqualTo("img"));
            Assert.That(cut.Find(".centre").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [TestCase(999, "")]
    [TestCase(1234, "")]
    [TestCase(12345, "wider")]
    [TestCase(123456, "widest")]
    public void Render_StepsTheNumberDownByWidth(int value, string expected)
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, value).Add(x => x.TestId, "ring"));
        var classes = cut.Find("[data-testid=ring-value]").ClassList.Where(c => c != "value" && !c.StartsWith("b-", StringComparison.Ordinal));

        Assert.That(string.Join(" ", classes), Is.EqualTo(expected));
    }

    [Test]
    public void Render_WithoutTestId_RendersNoTestIds()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 1).Add(x => x.Caption, "steps"));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    [Test]
    public void Render_FractionAboveMaxThatRoundsToIt_IsNotOver()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 2100.3).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring]").ClassList, Does.Not.Contain("over"));
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("2,100"));
        });
    }

    [Test]
    public void Render_FractionThatRoundsToZeroWithoutMax_IsEmpty()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 0.3).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 0"));
        });
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Render_NonFiniteValue_CountsAsZero(double value)
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, value).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=ring]").GetAttribute("style"), Does.Contain("--fill: 0"));
            Assert.That(cut.Find("[data-testid=ring]").ClassList, Does.Not.Contain("over"));
        });
    }

    [Test]
    public void Render_VeryLargeValue_ShowsItInFullAndIsOver()
    {
        var cut = Context.Render<ProgressRing>(p => p.Add(x => x.Value, 5e12).Add(x => x.Max, 2100).Add(x => x.TestId, "ring"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=ring-value]").TextContent, Is.EqualTo("5,000,000,000,000"));
            Assert.That(cut.Find("[data-testid=ring]").ClassList, Does.Contain("over"));
        });
    }

    [Test]
    public void Css_ArcUsesTheArcTokens_AndOverUsesTheOverToken()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "ProgressRing.razor.css");
        string Rule(string selector) => Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Rule(".arc"), Does.Contain("var(--color-ring-arc)").And.Contain("var(--color-ring-track)"));
            Assert.That(Rule(".over .arc"), Does.Contain("var(--color-ring-arc-over)"));
        });
    }
}
