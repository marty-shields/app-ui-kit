using Shields.UI.Components.Actions;
using Shields.UI.Components.Navigation;

namespace Shields.UI.Tests.Components.Navigation;

public class PeriodStepperTests : ComponentTestBase
{
    [Test]
    public void ShowsTheLabelBetweenTwoArrows()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=stepper-label]").GetAttribute("aria-live"), Is.EqualTo("polite"));
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void PreviousArrowShowsChevronLeft_AndNextShowsChevron()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        Assert.That(cut.FindComponents<Icon>().Select(i => i.Instance.Name), Is.EqualTo(new[] { IconName.ChevronLeft, IconName.Chevron }));
    }

    [TestCase(PeriodKind.Week, "Previous week", "Next week")]
    [TestCase(PeriodKind.Month, "Previous month", "Next month")]
    [TestCase(PeriodKind.Year, "Previous year", "Next year")]
    public void Arrows_AreNamedAfterTheKind(PeriodKind kind, string previous, string next)
    {
        var cut = Render(kind, canPrevious: true, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-previous]").GetAttribute("aria-label"), Is.EqualTo(previous));
            Assert.That(cut.Find("[data-testid=stepper-next]").GetAttribute("aria-label"), Is.EqualTo(next));
        });
    }

    [Test]
    public void AtTheLatestPeriod_TheNextArrowIsDisabled()
    {
        var cut = Render(PeriodKind.Month, canPrevious: true, canNext: false);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void AtTheEarliestPeriod_ThePreviousArrowIsDisabled()
    {
        var cut = Render(PeriodKind.Year, canPrevious: false, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Arrows_RaiseTheirCallbacks()
    {
        var calls = new List<string>();
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true, () => calls.Add("next"), () => calls.Add("previous"));

        cut.Find("[data-testid=stepper-previous]").Click();
        cut.Find("[data-testid=stepper-next]").Click();

        Assert.That(calls, Is.EqualTo(new[] { "previous", "next" }));
    }

    [Test]
    public void WithoutATestId_NoTestIdsAreRendered()
    {
        var cut = Context.Render<PeriodStepper>(p => p
            .Add(x => x.Kind, PeriodKind.Week)
            .Add(x => x.Label, "16 – 22 Nov"));

        Assert.That(cut.FindAll("[data-testid]"), Is.Empty);
    }

    [Test]
    public void Label_IsMarkedWrapOk()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        Assert.That(cut.FindAll(".label[data-wrap-ok]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Css_WrapsTheLabel_AndKeepsTheArrowsWhole()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Navigation", "PeriodStepper.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(Rule(css, ".label"), Does.Contain("overflow-wrap: break-word;"));
            Assert.That(Rule(css, ".label"), Does.Not.Contain("overflow-wrap: anywhere;"));
            Assert.That(Rule(css, ".label"), Does.Contain("min-width: 0;"));
            Assert.That(Rule(css, ".label"), Does.Not.Contain("text-overflow"));
            Assert.That(Rule(css, ".label"), Does.Not.Contain("white-space: nowrap;"));
            Assert.That(Rule(css, ".stepper ::deep .icon-btn"), Does.Contain("flex: none;"));
        });
    }

    [Test]
    public void WhenTheParentDisablesThePressedArrow_FocusMovesToTheOtherArrow()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        cut.Find("[data-testid=stepper-next]").Click();
        cut.Render(p => p.Add(x => x.CanGoNext, false));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=stepper-previous]"));
    }

    [Test]
    public void WhenThePressedArrowStaysEnabled_FocusIsLeftAlone()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        cut.Find("[data-testid=stepper-next]").Click();

        Assert.That(Context.JSInterop.Invocations.Where(i => i.Identifier.EndsWith("focus")), Is.Empty);
    }

    private static string Rule(string css, string selector) =>
        System.Text.RegularExpressions.Regex.Match(css, System.Text.RegularExpressions.Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

    private IRenderedComponent<PeriodStepper> Render(PeriodKind kind, bool canPrevious, bool canNext, Action? onNext = null, Action? onPrevious = null) =>
        Context.Render<PeriodStepper>(p => p
            .Add(x => x.Kind, kind)
            .Add(x => x.Label, "16 – 22 Nov")
            .Add(x => x.CanGoPrevious, canPrevious)
            .Add(x => x.CanGoNext, canNext)
            .Add(x => x.OnNext, onNext ?? (() => { }))
            .Add(x => x.OnPrevious, onPrevious ?? (() => { }))
            .Add(x => x.TestId, "stepper"));
}
