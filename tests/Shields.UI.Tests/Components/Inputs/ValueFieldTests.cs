using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class ValueFieldTests : ComponentTestBase
{
    [SetUp]
    public void RegisterAClock() => Context.Services.AddSingleton<TimeProvider>(new ManualClock());

    [Test]
    public void Duration_RendersThreeBoxes() =>
        Assert.That(Render(ValueKind.Duration).FindAll("input"), Has.Count.EqualTo(3));

    [TestCase(ValueKind.Whole, "numeric")]
    [TestCase(ValueKind.Fractional, "decimal")]
    public void NumberKinds_SetTheKeyboard(ValueKind kind, string inputMode) =>
        Assert.That(Render(kind).Find("input").GetAttribute("inputmode"), Is.EqualTo(inputMode));

    [Test]
    public void Label_NamesTheField() =>
        Assert.That(Render(ValueKind.Fractional).Find("label").TextContent.Trim(), Is.EqualTo("Value"));

    [Test]
    public void Unit_ShowsAfterTheNumber() =>
        Assert.That(Render(ValueKind.Fractional, p => p.Add(x => x.Unit, "kg")).Find("[data-testid=field-unit]").TextContent, Is.EqualTo("kg"));

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Fractional)]
    [TestCase(ValueKind.Duration)]
    public void Invalid_MarksTheInput(ValueKind kind) =>
        Assert.That(Render(kind, p => p.Add(x => x.Invalid, true)).FindAll("input").Select(i => i.HasAttribute("aria-invalid")), Is.All.True);

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Duration)]
    public void NotInvalid_LeavesTheInputUnmarked(ValueKind kind) =>
        Assert.That(Render(kind).FindAll("input").Select(i => i.HasAttribute("aria-invalid")), Is.All.False);

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Duration)]
    public void InvalidMessage_ShowsAnAlert(ValueKind kind) =>
        Assert.That(Render(kind, p => p.Add(x => x.InvalidMessage, "Enter a value")).Find("[role=alert]").TextContent, Is.EqualTo("Enter a value"));

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Duration)]
    public void Required_MarksTheInput(ValueKind kind) =>
        Assert.That(Render(kind, p => p.Add(x => x.Required, true)).FindAll("input").Select(i => i.GetAttribute("aria-required")), Is.All.EqualTo("true"));

    [Test]
    public void Input_RaisesValueChanged()
    {
        string? value = null;
        var cut = Render(ValueKind.Whole, p => p.Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("input").Input("10");

        Assert.That(value, Is.EqualTo("10"));
    }

    [TestCase("1.")]
    [TestCase("1,5")]
    [TestCase("abc")]
    [TestCase("-")]
    [TestCase(" 5")]
    [TestCase(".5")]
    public void TypedText_StaysInTheBoxAndIsRaisedAsTyped(string typed)
    {
        string? raised = null;
        var cut = Context.Render<ValueField>(p => p
            .Add(x => x.Kind, ValueKind.Fractional).Add(x => x.Label, "Weight").Add(x => x.TestId, "field")
            .Add(x => x.Value, raised)
            .Add(x => x.ValueChanged, (string? v) => raised = v));

        cut.Find("input").Input(typed);
        cut.Render(p => p.Add(x => x.Value, raised));

        Assert.Multiple(() =>
        {
            Assert.That(raised, Is.EqualTo(typed));
            Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo(typed));
        });
    }

    [TestCase("22,75")]
    [TestCase("abc")]
    public void ValueOverAnyText_ShowsItAsGiven(string given) =>
        Assert.That(Render(ValueKind.Fractional, p => p.Add(x => x.Value, given)).Find("input").GetAttribute("value"), Is.EqualTo(given));

    [Test]
    public void ValueGiven_ShowsInTheBox() =>
        Assert.That(Render(ValueKind.Fractional, p => p.Add(x => x.Value, "62.5")).Find("input").GetAttribute("value"), Is.EqualTo("62.5"));

    [Test]
    public void ClearedInput_RaisesNull()
    {
        string? value = "5";
        var cut = Render(ValueKind.Whole, p => p.Add(x => x.Value, "5").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("input").Input("");

        Assert.That(value, Is.Null);
    }

    [Test]
    public void Enter_RaisesOnEnter()
    {
        var entered = false;
        var cut = Render(ValueKind.Whole, p => p.Add(x => x.OnEnter, () => entered = true));

        cut.Find("input").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        Assert.That(entered, Is.True);
    }

    [Test]
    public void Duration_BoxInputRaisesValueChanged()
    {
        string? value = null;
        var cut = Render(ValueKind.Duration, p => p.Add(x => x.Value, "0::").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("[data-testid=field-mm]").Input("45");

        Assert.That(value, Is.EqualTo("0:45:"));
    }

    [TestCase(ValueKind.Whole, "field")]
    [TestCase(ValueKind.Duration, "field-h")]
    public void TestId_GoesOnTheInputOrPrefixesTheBoxes(ValueKind kind, string expected) =>
        Assert.That(Render(kind).Find("input").GetAttribute("data-testid"), Is.EqualTo(expected));

    [Test]
    public void Compact_MakesTheNumberBoxCompact()
    {
        var compact = Render(ValueKind.Whole, p => p.Add(x => x.Compact, true));
        var normal = Render(ValueKind.Whole);

        Assert.Multiple(() =>
        {
            Assert.That(compact.Find("[data-testid=field-box]").ClassList, Does.Contain("input-box-compact"));
            Assert.That(normal.Find("[data-testid=field-box]").ClassList, Does.Not.Contain("input-box-compact"));
        });
    }

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Duration)]
    public void Disabled_DisablesEveryInput(ValueKind kind) =>
        Assert.That(Render(kind, p => p.Add(x => x.Disabled, true)).FindAll("input").Select(i => i.HasAttribute("disabled")), Is.All.True);

    [TestCase(ValueKind.Whole)]
    [TestCase(ValueKind.Duration)]
    public void ShowLabelOff_KeepsTheLabelForScreenReadersOnly(ValueKind kind)
    {
        var hidden = Render(kind, p => p.Add(x => x.ShowLabel, false)).Find("label").ClassList.Contains("visually-hidden");
        var shown = Render(kind).Find("label").ClassList.Contains("visually-hidden");

        Assert.Multiple(() =>
        {
            Assert.That(hidden, Is.True);
            Assert.That(shown, Is.False);
        });
    }

    [TestCase(ValueKind.Whole, "field")]
    [TestCase(ValueKind.Duration, "field-h")]
    public async Task FocusAsync_FocusesTheFirstInput(ValueKind kind, string testId)
    {
        var cut = Render(kind);

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Assert.That(Focuses().Single().Arguments[0], Is.Not.Null);
        Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find($"[data-testid={testId}]"));
    }

    [Test]
    public async Task FocusAsync_AfterTheKindChanges_FocusesTheCurrentControl()
    {
        var cut = Render(ValueKind.Whole);
        cut.Render(p => p.Add(x => x.Kind, ValueKind.Duration));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=field-h]"));
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    private IRenderedComponent<ValueField> Render(ValueKind kind, Action<ComponentParameterCollectionBuilder<ValueField>>? more = null) =>
        Context.Render<ValueField>(p =>
        {
            p.Add(x => x.Kind, kind).Add(x => x.Label, "Value").Add(x => x.TestId, "field");
            more?.Invoke(p);
        });
}
