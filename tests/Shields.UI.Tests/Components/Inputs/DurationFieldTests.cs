using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class DurationFieldTests : ComponentTestBase
{
    private ManualClock clock = null!;

    [SetUp]
    public void UseAManualClock()
    {
        clock = new ManualClock();
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    [Test]
    public void Render_ShowsHoursMinutesSeconds()
    {
        var cut = Render();

        Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "duration-h", "duration-mm", "duration-ss" }));
    }

    [Test]
    public void Render_NamesEveryBoxAndTheGroup()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("aria-label")), Is.EqualTo(new[] { "Hours", "Minutes", "Seconds" }));
            Assert.That(cut.Find("[role=group]").GetAttribute("aria-label"), Is.EqualTo("Duration"));
            Assert.That(cut.Find("label").GetAttribute("for"), Is.EqualTo(cut.Find("[data-testid=duration-h]").Id));
        });
    }

    [Test]
    public void Boxes_AreInTheSharedInputBoxWithCompactPadding_AndCompactGoesTighter()
    {
        var normal = Render().FindAll(".box");
        var boxes = Render(p => p.Add(x => x.Compact, true)).FindAll(".box");

        Assert.Multiple(() =>
        {
            Assert.That(boxes, Has.Count.EqualTo(3));
            Assert.That(boxes.Select(b => b.ClassList.Contains("input-box") && b.ClassList.Contains("input-box-compact")), Is.All.True);
            Assert.That(normal.Select(b => b.ClassList.Contains("input-box") && b.ClassList.Contains("input-box-compact")), Is.All.True);
            Assert.That(Render().Find(".parts").ClassList, Does.Not.Contain("compact"));
            Assert.That(Render(p => p.Add(x => x.Compact, true)).Find(".parts").ClassList, Does.Contain("compact"));
            Assert.That(boxes.Select(b => b.GetAttribute("data-testid")), Is.EqualTo(new[] { "duration-h-box", "duration-mm-box", "duration-ss-box" }));
        });
    }

    [Test]
    public void Value_FillsBoxes() =>
        Assert.That(Render(p => p.Add(x => x.Value, "1:02:03")).FindAll("input").Select(i => i.GetAttribute("value")), Is.EqualTo(new[] { "1", "02", "03" }));

    [Test]
    public void BoxInput_RaisesJoinedValue()
    {
        string? value = null;
        var cut = Render(p => p.Add(x => x.Value, "0::").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("[data-testid=duration-mm]").Input("45");

        Assert.That(value, Is.EqualTo("0:45:"));
    }

    [Test]
    public void AllBoxesEmpty_RaisesNull()
    {
        string? value = "0::";
        var cut = Render(p => p.Add(x => x.Value, "0::").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("[data-testid=duration-h]").Input("");

        Assert.That(value, Is.Null);
    }

    [Test]
    public void Invalid_MarksEveryBox()
    {
        var cut = Render(p => p.Add(x => x.Invalid, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".box").Select(b => b.ClassList.Contains("invalid")), Is.All.True);
            Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("aria-invalid")), Is.All.EqualTo("true"));
            Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
        });
    }

    [Test]
    public void InvalidMessage_ShowsAnAlertDescribingEveryBox()
    {
        var cut = Render(p => p.Add(x => x.InvalidMessage, "Seconds must be under 60"));

        var message = cut.Find("[data-testid=duration-error]");
        Assert.Multiple(() =>
        {
            Assert.That(message.TextContent, Is.EqualTo("Seconds must be under 60"));
            Assert.That(message.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("aria-describedby")), Is.All.EqualTo(message.Id));
            Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("aria-invalid")), Is.All.EqualTo("true"));
        });
    }

    [Test]
    public void NoMessage_LeavesNoDescribedBy() =>
        Assert.That(Render().FindAll("input").Select(i => i.HasAttribute("aria-describedby")), Is.All.False);

    [TestCase(true, "true")]
    [TestCase(false, null)]
    public void Required_MarksEveryBox(bool required, string? expected) =>
        Assert.That(Render(p => p.Add(x => x.Required, required)).FindAll("input").Select(i => i.GetAttribute("aria-required")), Is.All.EqualTo(expected));

    [Test]
    public void Disabled_DisablesEveryBox() =>
        Assert.That(Render(p => p.Add(x => x.Disabled, true)).FindAll("input").Select(i => i.HasAttribute("disabled")), Is.All.True);

    [TestCase(true)]
    [TestCase(false)]
    public void ShowLabel_ControlsWhetherTheLabelIsSeen(bool show) =>
        Assert.That(Render(p => p.Add(x => x.ShowLabel, show)).Find("label").ClassList.Contains("visually-hidden"), Is.EqualTo(!show));

    [TestCase("1::", "1:00:00")]
    [TestCase("::22", "0:00:22")]
    [TestCase("1:5:3", "1:05:03")]
    public async Task FocusLeavesPartlyFilledField_FillsIn(string value, string expected)
    {
        string? raised = null;
        var cut = Render(p => p.Add(x => x.Value, value).Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        clock.Advance(DurationField.FocusMoveWait);
        await leaving;

        Assert.That(raised, Is.EqualTo(expected));
    }

    [Test]
    public void Css_Compact_DropsPaddingAndGapAndShrinksTheDigits()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Inputs", "DurationField.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain(".parts.compact .box {\n    padding-inline: 0;"));
            Assert.That(css, Does.Contain(".parts.compact .input {\n    font-size: var(--font-size-md);"));
            Assert.That(css, Does.Contain(".parts.compact {\n    gap: 0;"));
        });
    }

    [Test]
    public void FocusMoveWait_IsTheTenMillisecondsTheFieldWaitsBeforeFillingIn()
    {
        Assert.That(DurationField.FocusMoveWait, Is.EqualTo(TimeSpan.FromMilliseconds(10)));
    }

    [Test]
    public async Task FocusLeaves_FillsInOnlyOnceFocusMoveWaitHasPassed()
    {
        string? raised = null;
        var cut = Render(p => p.Add(x => x.Value, "1::").Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        clock.Advance(DurationField.FocusMoveWait - TimeSpan.FromMilliseconds(1));
        await cut.InvokeAsync(() => { });
        Assert.That(raised, Is.Null);
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await leaving;

        Assert.That(raised, Is.EqualTo("1:00:00"));
    }

    [TestCase(null)]
    [TestCase("0:20:00")]
    public async Task FocusLeavesEmptyOrComplete_RaisesNothing(string? value)
    {
        var raised = false;
        var cut = Render(p => p.Add(x => x.Value, value).Add(x => x.ValueChanged, (string? _) => raised = true));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        clock.Advance(DurationField.FocusMoveWait);
        await leaving;

        Assert.That(raised, Is.False);
    }

    [Test]
    public async Task FocusMovesToAnotherBox_DoesNotFillIn()
    {
        string? raised = null;
        var cut = Render(p => p.Add(x => x.Value, "1::").Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        await cut.Find("[role=group]").FocusInAsync(new FocusEventArgs());
        clock.Advance(DurationField.FocusMoveWait);
        await leaving;

        Assert.That(raised, Is.Null);
    }

    [Test]
    public void Boxes_UseNumericKeyboardAndTwoDigits()
    {
        var inputs = Render().FindAll("input");

        Assert.Multiple(() =>
        {
            Assert.That(inputs.Select(i => i.GetAttribute("inputmode")), Is.All.EqualTo("numeric"));
            Assert.That(inputs.Select(i => i.GetAttribute("maxlength")), Is.All.EqualTo("2"));
        });
    }

    [Test]
    public void TestId_PrefixesTheBoxes() =>
        Assert.That(Render(p => p.Add(x => x.TestId, "time")).FindAll("input").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "time-h", "time-mm", "time-ss" }));

    [TestCase("1:2", "0:12:")]
    [TestCase("a1b", "0:1:")]
    public void PastedText_KeepsOnlyDigits(string pasted, string expected)
    {
        string? value = null;
        var cut = Render(p => p.Add(x => x.Value, "0::").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("[data-testid=duration-mm]").Input(pasted);

        Assert.That(value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Disposed_BeforeTheFocusWaitEnds_RaisesNothing()
    {
        string? raised = null;
        var cut = Render(p => p.Add(x => x.Value, "1::").Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        await Context.DisposeComponentsAsync();
        clock.Advance(DurationField.FocusMoveWait);
        await leaving;

        Assert.That(raised, Is.Null);
    }

    [Test]
    public void ClickingABox_FocusesIt()
    {
        var cut = Render();


        cut.Find("[data-testid=duration-mm-box]").Click();

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void ClickingABox_WhenDisabled_DoesNotFocus()
    {
        var cut = Render(p => p.Add(x => x.Disabled, true));

        cut.Find("[data-testid=duration-h-box]").Click();

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public async Task FocusAsync_FocusesTheHoursBox()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=duration-h]"));
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    private IRenderedComponent<DurationField> Render(Action<ComponentParameterCollectionBuilder<DurationField>>? more = null) =>
        Context.Render<DurationField>(p =>
        {
            p.Add(x => x.Label, "Duration");
            more?.Invoke(p);
        });
}
