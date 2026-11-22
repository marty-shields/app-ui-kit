using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class NoteFieldTests : ComponentTestBase
{
    private IRenderedComponent<NoteField> Render(Action<ComponentParameterCollectionBuilder<NoteField>>? configure = null) =>
        Context.Render<NoteField>(p =>
        {
            p.Add(x => x.Label, "Note").Add(x => x.MaxLength, 500).Add(x => x.TestId, "note");
            configure?.Invoke(p);
        });

    [Test]
    public void Empty_RendersNothing()
    {
        var cut = Render();

        Assert.That(cut.FindAll("*"), Is.Empty);
    }

    [Test]
    public void BlankText_RendersNothing()
    {
        var cut = Render(p => p.Add(x => x.Value, "  "));

        Assert.That(cut.FindAll("*"), Is.Empty);
    }
    [Test]
    public void Editing_ShowsLabelCounterAndATextBoxWithNoLengthLimit()
    {
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "Belt on"));

        var textarea = cut.Find("textarea");
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".label").TextContent, Is.EqualTo("Note"));
            Assert.That(cut.Find("[data-testid=note-count]").TextContent, Is.EqualTo("7/500"));
            Assert.That(textarea.GetAttribute("data-testid"), Is.EqualTo("note-input"));
            Assert.That(textarea.GetAttribute("value"), Is.EqualTo("Belt on"));
            Assert.That(textarea.HasAttribute("maxlength"), Is.False);
            Assert.That(textarea.ClassList, Does.Not.Contain("invalid"));
            Assert.That(cut.FindAll("[data-testid=note-error]"), Is.Empty);
        });
    }

    [Test]
    public void Editing_OnFirstRender_DoesNotMoveFocus()
    {
        Render(p => p.Add(x => x.Editing, true));

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void Editing_SetByTheParentLater_MovesFocusIntoTheTextBox()
    {
        var cut = Render();

        cut.Render(p => p.Add(x => x.Editing, true));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("textarea"));
    }

    [TestCase(0, "0/500", false)]
    [TestCase(500, "500/500", false)]
    [TestCase(501, "501/500", true)]
    public void Counter_ShowsTheLengthAndTurnsRedOverTheLimit(int length, string text, bool over)
    {
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, length == 0 ? null : new string('a', length)));

        var counter = cut.Find("[data-testid=note-count]");
        Assert.Multiple(() =>
        {
            Assert.That(counter.TextContent, Is.EqualTo(text));
            Assert.That(counter.ClassList.Contains("over"), Is.EqualTo(over));
        });
    }

    [Test]
    public void Typing_RaisesValueChangedAndUpdatesTheCounter()
    {
        string? value = null;
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("textarea").Input("Belt on from set 3");

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.EqualTo("Belt on from set 3"));
            Assert.That(cut.Find("[data-testid=note-count]").TextContent, Is.EqualTo("18/500"));
        });
    }

    [Test]
    public void Typing_KeepsTheSpacesTypedSoFar()
    {
        string? value = null;
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("textarea").Input("Belt on ");

        Assert.That(value, Is.EqualTo("Belt on "));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Typing_Blank_RaisesNull(string text)
    {
        string? value = "old";
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "old").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("textarea").Input(text);

        Assert.That(value, Is.Null);
    }

    [Test]
    public void Blur_TrimsTheNoteAndCollapsesToOneLine()
    {
        string? value = null;
        var editing = new List<bool>();
        var cut = Render(p => p
            .Add(x => x.Editing, true)
            .Add(x => x.ValueChanged, (string? v) => value = v)
            .Add(x => x.EditingChanged, (bool e) => editing.Add(e)));
        cut.Find("textarea").Input("  Belt on  ");

        cut.Find("textarea").Blur();

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.EqualTo("Belt on"));
            Assert.That(editing, Is.EqualTo(new[] { false }));
            Assert.That(cut.FindAll("textarea"), Is.Empty);
            Assert.That(cut.Find("[data-testid=note-open] .text").TextContent, Is.EqualTo("Belt on"));
        });
    }

    [Test]
    public void Blur_BlankNote_BecomesNullAndShowsNothing()
    {
        string? value = "Belt on";
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "Belt on").Add(x => x.ValueChanged, (string? v) => value = v));
        cut.Find("textarea").Input(" ");

        cut.Find("textarea").Blur();

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.Null);
            Assert.That(cut.FindAll("*"), Is.Empty);
        });
    }

    [Test]
    public void Blur_UnchangedNote_RaisesNoValueChange()
    {
        var changes = 0;
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "Belt on").Add(x => x.ValueChanged, (string? _) => changes++));

        cut.Find("textarea").Blur();

        Assert.That(changes, Is.Zero);
    }

    [Test]
    public void Collapsed_ShowsTheNoteOnOneLine()
    {
        var cut = Render(p => p.Add(x => x.Value, "Belt on from set 3"));

        var open = cut.Find("[data-testid=note-open]");
        Assert.Multiple(() =>
        {
            Assert.That(System.Text.RegularExpressions.Regex.Replace(open.TextContent, @"\s+", " ").Trim(), Is.EqualTo("Edit Note: Belt on from set 3"));
            Assert.That(open.QuerySelector(".text")!.TextContent, Is.EqualTo("Belt on from set 3"));
            Assert.That(cut.FindAll("textarea"), Is.Empty);
        });
    }

    [Test]
    public void Collapsed_Tap_OpensTheTextBoxWithTheNoteAndFocus()
    {
        var editing = new List<bool>();
        var cut = Render(p => p.Add(x => x.Value, "Belt on").Add(x => x.EditingChanged, (bool e) => editing.Add(e)));

        cut.Find("[data-testid=note-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(editing, Is.EqualTo(new[] { true }));
            Assert.That(cut.Find("textarea").GetAttribute("value"), Is.EqualTo("Belt on"));
            Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("textarea"));
        });
    }

    [Test]
    public void WithoutAParentBinding_OpensAndCollapsesOnItsOwn()
    {
        var cut = Render(p => p.Add(x => x.Editing, true));

        cut.Find("textarea").Input("Belt on");
        cut.Find("textarea").Blur();

        Assert.That(cut.Find("[data-testid=note-open] .text").TextContent, Is.EqualTo("Belt on"));
    }

    [Test]
    public void Error_Editing_MarksTheTextBoxInvalidAndShowsTheMessage()
    {
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, new string('a', 501)).Add(x => x.Error, "Note must be 500 characters or less"));

        var textarea = cut.Find("textarea");
        var error = cut.Find("[data-testid=note-error]");
        Assert.Multiple(() =>
        {
            Assert.That(textarea.ClassList, Does.Contain("invalid"));
            Assert.That(textarea.GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(textarea.GetAttribute("aria-describedby"), Does.EndWith(error.GetAttribute("id")!));
            Assert.That(error.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(error.GetAttribute("id"), Does.Contain("-error"));
            Assert.That(error.TextContent, Is.EqualTo("Note must be 500 characters or less"));
        });
    }

    [Test]
    public void ErrorId_IsPerInstance_EvenWithoutATestId()
    {
        var first = Context.Render<NoteField>(p => p.Add(x => x.Label, "Note").Add(x => x.Editing, true).Add(x => x.Error, "Too long"));
        var second = Context.Render<NoteField>(p => p.Add(x => x.Label, "Note").Add(x => x.Editing, true).Add(x => x.Error, "Too long"));

        Assert.That(first.Find("p.error").GetAttribute("id"), Is.Not.EqualTo(second.Find("p.error").GetAttribute("id")));
    }

    [Test]
    public void Typing_OnlyWhitespace_KeepsItInTheTextBox_WhileTheValueIsNull()
    {
        string? value = "Belt on";
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "Belt on").Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("textarea").Input("   ");

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.Null);
            Assert.That(cut.Find("textarea").GetAttribute("value"), Is.EqualTo("   "));
        });
    }

    [Test]
    public void Count_IsOutsideTheLabel_AndDescribesTheTextBox()
    {
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, "Belt on"));

        var count = cut.Find("[data-testid=note-count]");
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("label [data-testid=note-count]"), Is.Empty);
            Assert.That(cut.Find("label").TextContent, Is.EqualTo("Note"));
            Assert.That(cut.Find("label").GetAttribute("for"), Is.EqualTo(cut.Find("textarea").GetAttribute("id")));
            Assert.That(cut.Find("textarea").GetAttribute("aria-describedby"), Does.Contain(count.GetAttribute("id")!));
        });
    }

    [Test]
    public void Error_Collapsed_MarksTheLineInvalidAndShowsTheMessage()
    {
        var cut = Render(p => p.Add(x => x.Value, new string('a', 501)).Add(x => x.Error, "Note must be 500 characters or less"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=note-open]").ClassList, Does.Contain("invalid"));
            Assert.That(cut.Find("[data-testid=note-error]").TextContent.Trim(), Is.EqualTo("Note must be 500 characters or less"));
        });
    }

    [Test]
    public void Error_WithoutANote_ShowsNothing()
    {
        var cut = Render(p => p.Add(x => x.Error, "Note must be 500 characters or less"));

        Assert.That(cut.FindAll("[data-testid=note-error]"), Is.Empty);
    }

    [Test]
    public async Task FocusAsync_OpensTheTextBoxAndFocusesIt()
    {
        var cut = Render(p => p.Add(x => x.Value, "Belt on"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("textarea"));
    }

    [Test]
    public async Task FocusAsync_WhileEditing_FocusesTheTextBox()
    {
        var cut = Render(p => p.Add(x => x.Editing, true));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("textarea"));
    }

    [Test]
    public void WithoutMaxLength_TheCounterShowsTheLengthAlone_AndNothingIsOver()
    {
        var cut = Context.Render<NoteField>(p => p.Add(x => x.Label, "Note").Add(x => x.Editing, true).Add(x => x.Value, new string('a', 900)).Add(x => x.TestId, "note"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=note-count]").TextContent, Is.EqualTo("900"));
            Assert.That(cut.Find("[data-testid=note-count]").ClassList, Does.Not.Contain("over"));
            Assert.That(cut.Find("textarea").HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public void OverTheLimit_MarksTheTextBoxInvalidForScreenReadersEvenWithoutAMessage()
    {
        var cut = Render(p => p.Add(x => x.Editing, true).Add(x => x.Value, new string('a', 501)));

        Assert.That(cut.Find("textarea").GetAttribute("aria-invalid"), Is.EqualTo("true"));
    }

    [Test]
    public void Css_TheErrorOutlineAndTheOverCountUseTheErrorColour()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Inputs", "NoteField.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain(".invalid {\n    border: var(--border-width-error) solid var(--color-error);"));
            Assert.That(css, Does.Contain(".over {\n    color: var(--color-error);"));
        });
    }
}
