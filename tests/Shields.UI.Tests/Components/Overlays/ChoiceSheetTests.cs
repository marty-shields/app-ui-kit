using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Overlays;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Overlays;

public class ChoiceSheetTests : ComponentTestBase
{
    [SetUp]
    public void Prepare()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        Context.Services.AddSingleton<IBackButton>(new BackButton(NullLogger<BackButton>.Instance));
    }

    [Test]
    public void Closed_RendersNothing()
    {
        var cut = Render(open: false);

        Assert.That(cut.Markup.Trim(), Is.Empty);
    }

    [Test]
    public void Open_ShowsTheTitleMessageAndBothButtons()
    {
        var cut = Render(open: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=choice] h2").TextContent, Is.EqualTo("Keep it?"));
            Assert.That(cut.Find("[data-testid=choice-message]").TextContent, Is.EqualTo("Pick one."));
            Assert.That(cut.Find("[data-testid=choice-primary]").TextContent.Trim(), Is.EqualTo("Keep"));
            Assert.That(cut.Find("[data-testid=choice-primary]").ClassList, Does.Not.Contain("danger"));
            Assert.That(cut.Find("[data-testid=choice-danger]").TextContent.Trim(), Is.EqualTo("Throw away"));
            Assert.That(cut.Find("[data-testid=choice-danger]").ClassList, Does.Contain("danger"));
        });
    }

    [Test]
    public void Buttons_RaiseTheirOwnCallbackOnly()
    {
        var raised = new List<string>();
        var cut = Render(open: true, raised);

        cut.Find("[data-testid=choice-primary]").Click();
        cut.Find("[data-testid=choice-danger]").Click();

        Assert.That(raised, Is.EqualTo(new[] { "primary", "danger" }));
    }

    [Test]
    public void Scrim_RaisesOnCloseAndTheSheetStaysOpenUntilTheOwnerClearsIt()
    {
        var raised = new List<string>();
        var cut = Render(open: true, raised);

        cut.Find("[data-testid=choice-scrim]").Click();

        Assert.That(raised, Is.EqualTo(new[] { "close" }));
        Assert.That(cut.FindAll("[data-testid=choice]"), Has.Count.EqualTo(1));
    }

    private IRenderedComponent<ChoiceSheet> Render(bool open, List<string>? raised = null)
    {
        raised ??= [];
        return Context.Render<ChoiceSheet>(p => p
            .Add(x => x.Open, open)
            .Add(x => x.Title, "Keep it?")
            .Add(x => x.Message, "Pick one.")
            .Add(x => x.PrimaryText, "Keep")
            .Add(x => x.DangerText, "Throw away")
            .Add(x => x.OnPrimary, () => raised.Add("primary"))
            .Add(x => x.OnDanger, () => raised.Add("danger"))
            .Add(x => x.OnClose, () => raised.Add("close"))
            .Add(x => x.TestId, "choice"));
    }
}
