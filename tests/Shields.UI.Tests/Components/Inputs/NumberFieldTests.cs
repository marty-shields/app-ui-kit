using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Components.Inputs;

public class NumberFieldTests : ComponentTestBase
{
    private const string Module = "./_content/Shields.UI/Components/Inputs/NumberField.razor.js";

    [Test]
    public void Renders_ANumericInputWithAHiddenLabelAndTheUnit()
    {
        var cut = Render(p => p.Add(x => x.Value, 150m));

        var input = cut.Find("[data-testid=num]");
        Assert.Multiple(() =>
        {
            Assert.That(input.GetAttribute("value"), Is.EqualTo("150"));
            Assert.That(input.GetAttribute("inputmode"), Is.EqualTo("decimal"));
            Assert.That(cut.Find(".visually-hidden").TextContent, Is.EqualTo("Amount"));
            Assert.That(cut.Find("label").GetAttribute("for"), Is.EqualTo(input.Id));
            Assert.That(cut.Find("[data-testid=num-unit]").TextContent, Is.EqualTo("g"));
            Assert.That(cut.FindAll("button"), Is.Empty);
            Assert.That(input.HasAttribute("aria-invalid"), Is.False);
            Assert.That(input.HasAttribute("aria-required"), Is.False);
            Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
        });
    }

    [TestCase(false, "numeric")]
    [TestCase(true, "decimal")]
    public void AllowDecimal_SetsInputMode(bool allowDecimal, string inputMode)
    {
        var cut = Render(p => p.Add(x => x.AllowDecimal, allowDecimal));

        Assert.That(cut.Find("input").GetAttribute("inputmode"), Is.EqualTo(inputMode));
    }

    [Test]
    public void ShowLabel_ShowsTheLabelAboveTheField()
    {
        var cut = Render(p => p.Add(x => x.ShowLabel, true));

        Assert.That(cut.Find("label").ClassList, Does.Not.Contain("visually-hidden"));
    }

    [Test]
    public void Required_MarksTheLabelAndTheInput()
    {
        var cut = Render(p => p.Add(x => x.ShowLabel, true).Add(x => x.Required, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("label").TextContent, Is.EqualTo("Amount *"));
            Assert.That(cut.Find("input").GetAttribute("aria-required"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Typing_RaisesValueChangedWithTheNumber()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p.Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num]").Input("12.5");
        cut.Find("[data-testid=num]").Input("12,5");
        cut.Find("[data-testid=num]").Input("abc");
        cut.Find("[data-testid=num]").Input("");

        Assert.That(changed, Is.EqualTo(new decimal?[] { 12.5m, 12.5m, null, null }));
    }

    [Test]
    public void Typing_RaisesTextChangedWithTheRawTextBeforeValueChanged()
    {
        var events = new List<string>();
        var cut = Render(p => p
            .Add(x => x.TextChanged, (string t) => events.Add($"text:{t}"))
            .Add(x => x.ValueChanged, (decimal? v) => events.Add($"value:{v?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null"}")));

        cut.Find("[data-testid=num]").Input("12,5");
        cut.Find("[data-testid=num]").Input("abc");
        cut.Find("[data-testid=num]").Input("");

        Assert.That(events, Is.EqualTo(new[] { "text:12,5", "value:12.5", "text:abc", "value:null", "text:", "value:null" }));
    }

    [Test]
    public void Typing_AGroupedNumber_IsNotANumber()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p.Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num]").Input("1,000.5");
        cut.Find("[data-testid=num]").Input("1.000,5");
        cut.Find("[data-testid=num]").Input("1 000");
        cut.Find("[data-testid=num]").Input("-2.5");

        Assert.That(changed, Is.EqualTo(new decimal?[] { null, null, null, -2.5m }));
    }

    [Test]
    public void Typing_KeepsTheTypedTextWhileTheParentTakesTheNumber()
    {
        var cut = RenderBound(null);

        cut.Find("[data-testid=num]").Input("1.");

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo("1."));
    }

    [Test]
    public void Value_SetByTheParent_ReplacesTheText()
    {
        var cut = Render(p => p.Add(x => x.Value, 5m));

        cut.Render(p => p.Add(x => x.Value, 7m));

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo("7"));
    }

    [Test]
    public void Typing_WithAnAsyncParent_PassesTheTypedTextEvenIfTheParentSetsAValueMeanwhile()
    {
        var gate = new TaskCompletionSource();
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.TextChanged, async (string _) => await gate.Task)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        _ = cut.Find("[data-testid=num]").InputAsync(new ChangeEventArgs { Value = "12" });
        cut.Render(p => p.Add(x => x.Value, 9m));
        gate.SetResult();

        cut.WaitForAssertion(() => Assert.That(changed, Is.EqualTo(new decimal?[] { 12m })));
    }

    [Test]
    public void Value_SetToNullByTheParent_ClearsTextThatIsNotANumber()
    {
        var cut = Render(p => p.Add(x => x.Value, 5m));

        cut.Find("[data-testid=num]").Input("abc");
        cut.Render(p => p.Add(x => x.Value, null));

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo(""));
    }

    [Test]
    public void Typing_NotANumber_IsKeptWhenTheParentTakesTheNull()
    {
        var cut = RenderBound(5m);

        cut.Find("[data-testid=num]").Input("abc");

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo("abc"));
    }

    [TestCase(-10, 160, 140)]
    [TestCase(0, 150, 150)]
    public void Stepper_UsesTheAbsoluteValueOfStep(int step, int expectedPlus, int expectedMinus)
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Step, (decimal)step)
            .Add(x => x.Value, 150m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-plus]").Click();
        cut.Find("[data-testid=num-minus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { expectedPlus, expectedMinus }));
    }

    [Test]
    public void Stepper_NearTheLimitsOfDecimal_ClampsInsteadOfThrowing()
    {
        var changed = new List<decimal?>();
        var high = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Value, decimal.MaxValue - 0.5m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));
        var low = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Value, decimal.MinValue + 0.5m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        high.Find("[data-testid=num-plus]").Click();
        low.Find("[data-testid=num-minus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { decimal.MaxValue, decimal.MinValue }));
    }

    [Test]
    public void ClickingTheBox_FocusesTheInput()
    {
        var cut = Render();

        cut.Find(".unit").Click();

        Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(1));
    }

    [Test]
    public void ClickingTheBox_WhenDisabled_DoesNotFocus()
    {
        var cut = Render(p => p.Add(x => x.Disabled, true));

        cut.Find(".box").Click();

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void Box_AddsNoSecondAccessibleName()
    {
        var cut = Render();

        Assert.That(cut.FindAll("label"), Has.Count.EqualTo(1));
    }

    [TestCase("Enter", 1)]
    [TestCase("a", 0)]
    public void Key_RaisesOnEnterForEnterOnly(string key, int raised)
    {
        var count = 0;
        var cut = Render(p => p.Add(x => x.OnEnter, () => count++));

        cut.Find("input").KeyDown(key);

        Assert.That(count, Is.EqualTo(raised));
    }

    [Test]
    public void Enter_WithoutOnEnter_DoesNothing()
    {
        var cut = Render();

        Assert.DoesNotThrow(() => cut.Find("input").KeyDown("Enter"));
    }

    [Test]
    public void FocusAsync_FocusesTheInput()
    {
        var cut = Render();

        _ = cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(1));
    }

    [Test]
    public void Stepper_DoesNotRaiseTextChanged()
    {
        var texts = new List<string>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Value, 5m)
            .Add(x => x.TextChanged, (string t) => texts.Add(t)));

        cut.Find("[data-testid=num-plus]").Click();
        cut.Find("[data-testid=num-minus]").Click();

        Assert.That(texts, Is.Empty);
    }

    [Test]
    public void Stepper_ShowsBothButtonsWithNamesAndTestIds()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.Value, 50m));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=num-minus]").GetAttribute("aria-label"), Is.EqualTo("Decrease Amount"));
            Assert.That(cut.Find("[data-testid=num-plus]").GetAttribute("aria-label"), Is.EqualTo("Increase Amount"));
        });
    }

    [Test]
    public void Stepper_RaisesValueChangedByTheStep()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Step, 10m)
            .Add(x => x.Value, 150m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-plus]").Click();
        cut.Find("[data-testid=num-minus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { 160m, 140m }));
    }

    [Test]
    public void Stepper_Plus_FromEmpty_GoesToTheMinimumPlusTheStep()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Minimum, 1m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-plus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { 2m }));
    }

    [Test]
    public void Stepper_NeverGoesBelowTheMinimum()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Step, 10m)
            .Add(x => x.Minimum, 5m)
            .Add(x => x.Value, 12m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-minus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { 5m }));
    }

    [Test]
    public void Stepper_AtTheMinimum_DisablesMinusOnly()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.Minimum, 5m).Add(x => x.Value, 5m));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=num-minus]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=num-plus]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Stepper_Plus_FromBelowTheMinimum_GoesToTheMinimum()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Minimum, 5m)
            .Add(x => x.Value, 2m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-plus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { 5m }));
    }

    [Test]
    public void Stepper_Minus_FromEmptyWithALowMinimum_StepsDownFromTheMinimumAndStopsAtIt()
    {
        var changed = new List<decimal?>();
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Minimum, -100m)
            .Add(x => x.Step, 10m)
            .Add(x => x.ValueChanged, (decimal? v) => changed.Add(v)));

        cut.Find("[data-testid=num-minus]").Click();

        Assert.That(changed, Is.EqualTo(new decimal?[] { -100m }));
    }

    [Test]
    public void Stepper_BelowTheMinimum_DisablesMinus()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.Minimum, 5m).Add(x => x.Value, 3m));

        Assert.That(cut.Find("[data-testid=num-minus]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Stepper_WhenTheParentDoesNotTakeTheValue_KeepsShowingTheParentsValue()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.Value, 5m));

        cut.Find("[data-testid=num-plus]").Click();

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo("5"));
    }

    [Test]
    public void Stepper_WhenTheParentTakesTheValue_ShowsIt()
    {
        var cut = RenderBound(5m);

        cut.Find("[data-testid=num-plus]").Click();

        Assert.That(cut.Find("[data-testid=num]").GetAttribute("value"), Is.EqualTo("6"));
    }

    [Test]
    public void InvalidMessage_ShowsTheMessageAnnouncedAndLinkedToTheInput()
    {
        var cut = Render(p => p.Add(x => x.InvalidMessage, "Enter 10 g or more."));

        var message = cut.Find("[data-testid=num-error]");
        var input = cut.Find("[data-testid=num]");
        Assert.Multiple(() =>
        {
            Assert.That(message.TextContent, Is.EqualTo("Enter 10 g or more."));
            Assert.That(message.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(message.ClassList, Does.Contain("message"));
            Assert.That(input.GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(input.GetAttribute("aria-describedby"), Is.EqualTo(message.Id));
            Assert.That(message.Id, Is.Not.Empty);
            Assert.That(cut.Find(".box").ClassList, Does.Contain("invalid"));
        });
    }

    [Test]
    public void Invalid_MarksTheFieldWithoutAMessage()
    {
        var cut = Render(p => p.Add(x => x.Invalid, true));

        var input = cut.Find("input");
        Assert.Multiple(() =>
        {
            Assert.That(input.GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(input.HasAttribute("aria-describedby"), Is.False);
            Assert.That(cut.Find(".box").ClassList, Does.Contain("invalid"));
            Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
        });
    }

    [Test]
    public void Valid_DoesNotMarkTheField()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".box").ClassList, Does.Not.Contain("invalid"));
            Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public void Disabled_DisablesTheInputAndBothButtons()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.Disabled, true).Add(x => x.Value, 50m));

        Assert.That(cut.FindAll("input, button").Select(e => e.HasAttribute("disabled")), Is.EqualTo(new[] { true, true, true }));
    }

    [Test]
    public void InteractiveElements_AllHaveTestIds()
    {
        var cut = Render(p => p.Add(x => x.Stepper, true));

        Assert.That(cut.FindAll("input, button").Select(e => e.GetAttribute("data-testid")), Is.EqualTo(new[] { "num-minus", "num", "num-plus" }));
    }

    [Test]
    public void WithoutATestId_TheInputUsesTheDefault()
    {
        var cut = Context.Render<NumberField>(p => p.Add(x => x.Label, "Amount"));

        Assert.That(cut.Find("input").GetAttribute("data-testid"), Is.EqualTo("number-field"));
    }

    [Test]
    public void Box_HasATestIdFromTheTestId()
    {
        var cut = Render();

        Assert.That(cut.Find("[data-testid=num-box]").ClassList, Does.Contain("box").And.Contain("input-box"));
    }

    [Test]
    public void WithoutATestId_TheBoxUsesTheDefault()
    {
        var cut = Context.Render<NumberField>(p => p.Add(x => x.Label, "Amount"));

        Assert.That(cut.FindAll("[data-testid=number-field-box]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Compact_AddsTheCompactBoxClassOnlyWhenSet()
    {
        var normal = Render();
        var compact = Render(p => p.Add(x => x.Compact, true));

        Assert.Multiple(() =>
        {
            Assert.That(normal.Find("[data-testid=num-box]").ClassList, Does.Not.Contain("input-box-compact"));
            Assert.That(compact.Find("[data-testid=num-box]").ClassList, Does.Contain("input-box-compact"));
        });
    }

    [Test]
    public void Css_TheCompactBoxPadsOneSpaceUnitAndTheSharedBoxShowsTheFocusOutline()
    {
        var tokens = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");
        var shared = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "base.css");

        Assert.Multiple(() =>
        {
            Assert.That(tokens, Does.Contain("--input-box-padding-inline: var(--space-3);").And.Contain("--input-box-padding-inline-compact: var(--space-1);"));
            Assert.That(shared.Split(".input-box-compact {")[1].Split('}')[0], Does.Contain("padding-inline: var(--input-box-padding-inline-compact)"));
            Assert.That(shared.Split(".input-box:focus-within {")[1].Split('}')[0], Does.Contain("outline: var(--focus-width) solid var(--color-focus)").And.Contain("outline-offset: var(--focus-offset)"));
        });
    }

    [Test]
    public void Text_WhenSet_IsShownAsTypedInsteadOfTheValue()
    {
        var cut = Render(p => p.Add(x => x.Value, null).Add(x => x.Text, "22,75"));

        Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("22,75"));
    }

    [TestCase("abc")]
    [TestCase("007")]
    public void Text_WinsOverAValueThatDiffers(string text)
    {
        var cut = Render(p => p.Add(x => x.Value, 5m).Add(x => x.Text, text));

        Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo(text));
    }

    [Test]
    public void Text_WhenNull_ShowsTheValue()
    {
        var cut = Render(p => p.Add(x => x.Value, 7m));

        Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("7"));
    }

    [Test]
    public void Text_TypingRaisesTextChangedThenValueChanged()
    {
        var calls = new List<string>();
        var cut = Render(p => p
            .Add(x => x.Text, "")
            .Add(x => x.TextChanged, t => calls.Add("text:" + t))
            .Add(x => x.ValueChanged, v => calls.Add("value:" + v)));

        cut.Find("input").Input("22,75");

        Assert.That(calls, Is.EqualTo(new[] { "text:22,75", "value:22.75" }));
    }

    [Test]
    public void Text_Stepper_RaisesTextChangedWithTheNewNumber()
    {
        var texts = new List<string>();
        decimal? value = null;
        var cut = Render(p => p
            .Add(x => x.Stepper, true)
            .Add(x => x.Value, 5m)
            .Add(x => x.Text, "5")
            .Add(x => x.TextChanged, t => texts.Add(t))
            .Add(x => x.ValueChanged, v => value = v));

        cut.Find("[data-testid=num-plus]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(texts, Is.EqualTo(new[] { "6" }));
            Assert.That(value, Is.EqualTo(6m));
        });
    }

    [Test]
    public void Text_Stepper_WithoutText_DoesNotRaiseTextChanged()
    {
        var texts = new List<string>();
        var cut = Render(p => p.Add(x => x.Stepper, true).Add(x => x.TextChanged, t => texts.Add(t)));

        cut.Find("[data-testid=num-plus]").Click();

        Assert.That(texts, Is.Empty);
    }

    [Test]
    public void Css_ShowsErrorsInTheErrorTone()
    {
        var number = RepoFiles.Read("src", "Shields.UI", "Components", "Inputs", "NumberField.razor.css");
        var field = RepoFiles.Read("src", "Shields.UI", "Components", "Inputs", "FieldInput.razor.css");

        Assert.Multiple(() =>
        {
            var invalid = number.Split(".box.invalid {")[1].Split('}')[0];
            Assert.That(invalid, Does.Contain("border-color: var(--color-error)").And.Contain("box-shadow: inset").And.Contain("var(--color-error)"));
            Assert.That(invalid, Does.Not.Contain("border-width:"));
            Assert.That(field.Split(".message {")[1].Split('}')[0], Does.Contain("color: var(--color-error)"));
        });
    }

    [Test]
    public void AutoFocus_AsksTheModuleToFocusTheInputUntilItHasFocus()
    {
        var module = Context.JSInterop.SetupModule(Module);
        module.SetupVoid("focusWhenReady", _ => true);

        Render(p => p.Add(x => x.AutoFocus, true));

        Assert.That(module.Invocations["focusWhenReady"], Has.Count.EqualTo(1));
    }

    [Test]
    public void AutoFocusFails_LogsAWarningWithEvent3701AndShowsTheField()
    {
        var logger = new RecordingLogger<NumberField>();
        Context.Services.AddSingleton<ILogger<NumberField>>(logger);
        Context.JSInterop.SetupModule(Module).SetupVoid("focusWhenReady", _ => true).SetException(new JSException("Focus failed"));

        var cut = Render(p => p.Add(x => x.AutoFocus, true).Add(x => x.Value, 123.45m));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=num]"), Has.Count.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { (9601, LogLevel.Warning, "Focusing the number field failed") }));
            Assert.That(logger.Records.Single().Id, Is.EqualTo((int)KitEvent.NumberFieldFocusFailed));
            Assert.That(logger.LastException, Is.TypeOf<JSException>());
        });
    }

    [Test]
    public void AutoFocusAfterDisconnect_LogsNothing()
    {
        var logger = new RecordingLogger<NumberField>();
        Context.Services.AddSingleton<ILogger<NumberField>>(logger);
        Context.JSInterop.SetupModule(Module).SetupVoid("focusWhenReady", _ => true).SetException(new JSDisconnectedException("Gone"));

        var cut = Render(p => p.Add(x => x.AutoFocus, true));

        Assert.Multiple(() =>
        {
            Assert.That(logger.Records, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=num]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void AutoFocus_WhenDisabled_NoScriptRuns()
    {
        Render(p => p.Add(x => x.AutoFocus, true).Add(x => x.Disabled, true));

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void WithoutAutoFocus_NoScriptRuns()
    {
        Render();

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    // A parent that binds the value, so the field gets it back in the same render as the keystroke.
    private sealed class BoundHost : ComponentBase
    {
        [Parameter] public decimal? Value { get; set; }

        [Parameter] public decimal? Initial { get; set; }

        protected override void OnInitialized() => Value = Initial;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<NumberField>(0);
            builder.AddAttribute(1, nameof(NumberField.Label), "Amount");
            builder.AddAttribute(2, nameof(NumberField.TestId), "num");
            builder.AddAttribute(3, nameof(NumberField.Stepper), true);
            builder.AddAttribute(4, nameof(NumberField.Value), Value);
            builder.AddAttribute(5, nameof(NumberField.ValueChanged), EventCallback.Factory.Create<decimal?>(this, v => Value = v));
            builder.CloseComponent();
        }
    }

    private IRenderedComponent<BoundHost> RenderBound(decimal? initial) =>
        Context.Render<BoundHost>(p => p.Add(x => x.Initial, initial));

    private IRenderedComponent<NumberField> Render(Action<ComponentParameterCollectionBuilder<NumberField>>? configure = null) =>
        Context.Render<NumberField>(p =>
        {
            p.Add(x => x.Label, "Amount").Add(x => x.Unit, "g").Add(x => x.TestId, "num");
            configure?.Invoke(p);
        });
}
