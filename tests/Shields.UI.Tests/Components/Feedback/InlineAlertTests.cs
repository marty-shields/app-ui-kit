using Shields.UI.Components.Feedback;

namespace Shields.UI.Tests.Components.Feedback;

public class InlineAlertTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheMessageAsAnAlert()
    {
        var cut = Context.Render<InlineAlert>(p => p
            .Add(x => x.Message, "Couldn't save")
            .Add(x => x.TestId, "alert"));

        var alert = cut.Find("[data-testid=alert]");
        Assert.Multiple(() =>
        {
            Assert.That(alert.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(alert.TextContent.Trim(), Is.EqualTo("Couldn't save"));
            Assert.That(alert.ClassList, Does.Contain("error"));
        });
    }

    [Test]
    public void ChildContent_ShowsAfterTheMessage()
    {
        var cut = Context.Render<InlineAlert>(p => p
            .Add(x => x.Message, "Couldn't save.")
            .AddChildContent("<b>Try again</b>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[role=alert] b").TextContent, Is.EqualTo("Try again"));
            Assert.That(cut.Find("[role=alert]").TextContent, Does.StartWith("Couldn't save."));
        });
    }

    [Test]
    public void ChildContent_AloneIsTheWholeAlert()
    {
        var cut = Context.Render<InlineAlert>(p => p.AddChildContent("Only this"));

        Assert.That(cut.Find("[role=alert]").TextContent.Trim(), Is.EqualTo("Only this"));
    }

    [Test]
    public void Tone_Error_IsTheDefaultAndPutsItsClassOnTheBox()
    {
        var cut = Context.Render<InlineAlert>(p => p.Add(x => x.Message, "x"));

        Assert.That(cut.Find("[role=alert]").ClassList, Does.Contain("error"));
    }

    [Test]
    public void Render_WithoutTestId_WritesNoTestId()
    {
        var cut = Context.Render<InlineAlert>(p => p.Add(x => x.Message, "x"));

        Assert.That(cut.Find("[role=alert]").HasAttribute("data-testid"), Is.False);
    }

    [Test]
    public void Message_IsEncodedNotRenderedAsMarkup()
    {
        var cut = Context.Render<InlineAlert>(p => p.Add(x => x.Message, "<img src=x onerror=alert(1)>"));

        Assert.That(cut.FindAll("img"), Is.Empty);
    }

    [Test]
    public void Render_WithNoMessageAndNoContent_RendersNothing()
    {
        var cut = Context.Render<InlineAlert>();

        Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
    }

    [Test]
    public void Css_HasTheSurfaceBackgroundAndTheErrorBorderAndText()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Feedback", "InlineAlert.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(TouchTargetCheck.Declarations(css, ".alert"), Has.Member("background-color: var(--color-surface)"));
            Assert.That(TouchTargetCheck.Declarations(css, ".error"), Has.Member("border-color: var(--color-error)").And.Member("color: var(--color-error)"));
        });
    }

    [Test]
    public void Messages_ShowAsOneListInOneAlert_SkippingBlanks()
    {
        var cut = Context.Render<InlineAlert>(p => p
            .Add(x => x.Messages, new[] { "Weight is required", " ", "Reps must be a whole number" })
            .Add(x => x.TestId, "errors"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[role=alert]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=errors-messages] li").Select(li => li.TextContent), Is.EqualTo(new[] { "Weight is required", "Reps must be a whole number" }));
        });
    }

    [Test]
    public void Messages_ComeAfterTheMessage_AndBeforeTheContent()
    {
        var cut = Context.Render<InlineAlert>(p => p
            .Add(x => x.Message, "Couldn't save.")
            .Add(x => x.Messages, new[] { "One" })
            .AddChildContent("<b>Retry</b>"));

        var text = cut.Find("[role=alert]").TextContent;
        Assert.That(text.IndexOf("Couldn't save.", StringComparison.Ordinal), Is.LessThan(text.IndexOf("One", StringComparison.Ordinal)));
        Assert.That(text.IndexOf("One", StringComparison.Ordinal), Is.LessThan(text.IndexOf("Retry", StringComparison.Ordinal)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void NoUsableMessages_AndNothingElse_RendersNothing(string? only)
    {
        var cut = Context.Render<InlineAlert>(p => p.Add(x => x.Messages, only is null ? null : new[] { only }));
        var none = Context.Render<InlineAlert>(p => p.Add(x => x.Messages, Array.Empty<string>()));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Markup.Trim(), Is.Empty);
            Assert.That(none.Markup.Trim(), Is.Empty);
        });
    }
}
