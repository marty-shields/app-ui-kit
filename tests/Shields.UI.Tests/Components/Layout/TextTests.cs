using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components.Layout;

public class TextTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheContentInASpanWithTheBodyLook()
    {
        var cut = Context.Render<Text>(p => p.AddChildContent("Hello"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("span").TextContent, Is.EqualTo("Hello"));
            Assert.That(cut.Find("span").ClassList, Is.EquivalentTo(new[] { "text" }));
        });
    }

    [TestCase(TextVariant.Body, "text")]
    [TestCase(TextVariant.Label, "label")]
    [TestCase(TextVariant.Muted, "muted")]
    [TestCase(TextVariant.MutedSmall, "muted-small")]
    [TestCase(TextVariant.Strong, "strong")]
    [TestCase(TextVariant.Figure, "figure")]
    [TestCase(TextVariant.Error, "error")]
    public void Variant_PutsItsClassOnTheElement(TextVariant variant, string expected)
    {
        var cut = Context.Render<Text>(p => p.Add(x => x.Variant, variant).AddChildContent("x"));

        Assert.That(cut.Find("span").ClassList, Does.Contain(expected).And.Contain("text"));
    }

    [TestCase(TextElement.Span, "SPAN")]
    [TestCase(TextElement.P, "P")]
    [TestCase(TextElement.Div, "DIV")]
    [TestCase(TextElement.H2, "H2")]
    [TestCase(TextElement.H3, "H3")]
    [TestCase(TextElement.H4, "H4")]
    [TestCase(TextElement.H5, "H5")]
    [TestCase(TextElement.H6, "H6")]
    public void As_RendersTheElementWithTheLookAndTheContent(TextElement element, string tag)
    {
        var cut = Context.Render<Text>(p => p
            .Add(x => x.As, element)
            .Add(x => x.Variant, TextVariant.Label)
            .AddChildContent("Section"));

        var root = cut.Find("[class~=label]");
        Assert.Multiple(() =>
        {
            Assert.That(root.TagName, Is.EqualTo(tag));
            Assert.That(root.TextContent, Is.EqualTo("Section"));
            Assert.That(cut.Nodes, Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void TestId_IsWrittenToTheElement()
    {
        var cut = Context.Render<Text>(p => p.Add(x => x.TestId, "note").AddChildContent("x"));

        Assert.That(cut.Find("[data-testid=note]").TextContent, Is.EqualTo("x"));
    }

    [Test]
    public void Render_WithoutTestId_WritesNoTestId()
    {
        var cut = Context.Render<Text>(p => p.AddChildContent("x"));

        Assert.That(cut.Find("span").HasAttribute("data-testid"), Is.False);
    }

    [Test]
    public void OtherAttributes_AreWrittenToTheElement_AndAClassIsAddedToTheOwn()
    {
        var cut = Context.Render<Text>(p => p
            .Add(x => x.Variant, TextVariant.Muted)
            .AddUnmatched("id", "intro")
            .AddUnmatched("class", "centred")
            .AddUnmatched("data-testid", "ignored")
            .Add(x => x.TestId, "kept")
            .AddChildContent("x"));

        var span = cut.Find("span");
        Assert.Multiple(() =>
        {
            Assert.That(span.Id, Is.EqualTo("intro"));
            Assert.That(span.ClassList, Is.EquivalentTo(new[] { "text", "muted", "centred" }));
            Assert.That(span.GetAttribute("data-testid"), Is.EqualTo("kept"));
        });
    }

    [Test]
    public void Strong_NestedInMuted_KeepsTheNormalTextColour()
    {
        var cut = Context.Render<Text>(p => p
            .Add(x => x.Variant, TextVariant.Muted)
            .AddChildContent<Text>(inner => inner.Add(x => x.Variant, TextVariant.Strong).AddChildContent("kcal")));

        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "Text.razor.css");
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("span.muted span.strong").TextContent, Is.EqualTo("kcal"));
            Assert.That(TouchTargetCheck.Declarations(css, ".strong"), Has.Member("color: var(--color-text)").And.Member("font-weight: var(--font-weight-bold)"));
        });
    }

    [Test]
    public void Css_ErrorLabelAndFigureHaveTheirLooks()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "Text.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(TouchTargetCheck.Declarations(css, ".error"), Has.Member("color: var(--color-error)"));
            Assert.That(
                TouchTargetCheck.Declarations(css, ".label"),
                Has.Member("font-size: var(--font-size-sm)")
                    .And.Member("font-weight: var(--font-weight-bold)")
                    .And.Member("text-transform: uppercase")
                    .And.Member("letter-spacing: var(--letter-spacing-label)"));
            Assert.That(TouchTargetCheck.Declarations(css, ".figure"), Has.Member("font-size: var(--font-size-xl)").And.Member("font-weight: var(--font-weight-bold)"));
        });
    }
}
