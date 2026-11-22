using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class FieldInputTests : ComponentTestBase
{
    [Test]
    public void Renders_TheLabelForTheInputAndTheContent()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("label").GetAttribute("for"), Is.EqualTo("reps-input"));
            Assert.That(cut.Find("label").TextContent, Is.EqualTo("Reps"));
            Assert.That(cut.Find("label").ClassList, Does.Not.Contain("visually-hidden"));
            Assert.That(cut.Find("#reps-input"), Is.Not.Null);
            Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
        });
    }

    [Test]
    public void Required_AddsAnAsteriskHiddenFromScreenReaders()
    {
        var cut = Render(p => p.Add(x => x.Required, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("label").TextContent, Is.EqualTo("Reps\u00a0*"));
            Assert.That(cut.Find(".required").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void ShowLabelFalse_HidesTheLabelVisually()
    {
        var cut = Render(p => p.Add(x => x.ShowLabel, false));

        Assert.That(cut.Find("label").ClassList, Does.Contain("visually-hidden"));
    }

    [Test]
    public void Message_IsAnnouncedWithItsIdAndTestId()
    {
        var cut = Render(p => p
            .Add(x => x.Message, "Enter reps")
            .Add(x => x.MessageId, "reps-message")
            .Add(x => x.MessageTestId, "reps-error"));

        var message = cut.Find("[data-testid=reps-error]");
        Assert.Multiple(() =>
        {
            Assert.That(message.TextContent, Is.EqualTo("Enter reps"));
            Assert.That(message.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(message.Id, Is.EqualTo("reps-message"));
            Assert.That(message.ClassList, Does.Contain("message"));
        });
    }

    [Test]
    public void EmptyMessage_ShowsNothing()
    {
        var cut = Render(p => p.Add(x => x.Message, ""));

        Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
    }

    private IRenderedComponent<FieldInput> Render(Action<ComponentParameterCollectionBuilder<FieldInput>>? configure = null) =>
        Context.Render<FieldInput>(p =>
        {
            p.Add(x => x.Label, "Reps").Add(x => x.InputId, "reps-input").AddChildContent("<input id=\"reps-input\" />");
            configure?.Invoke(p);
        });
}
