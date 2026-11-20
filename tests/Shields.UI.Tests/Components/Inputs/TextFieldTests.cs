using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class TextFieldTests : ComponentTestBase
{
    private IRenderedComponent<TextField> Render(string? value = "1 slice", Action<string>? changed = null, string? invalid = null, bool disabled = false) =>
        Context.Render<TextField>(p => p
            .Add(x => x.Label, "Serving name")
            .Add(x => x.Value, value)
            .Add(x => x.MaxLength, 60)
            .Add(x => x.Placeholder, "Serving name")
            .Add(x => x.InvalidMessage, invalid)
            .Add(x => x.Disabled, disabled)
            .Add(x => x.TestId, "name")
            .Add(x => x.ValueChanged, changed ?? (_ => { })));

    [Test]
    public void Renders_ATextInputWithItsValueTestIdMaxLengthPlaceholderAndAHiddenLabel()
    {
        var cut = Render();

        var input = cut.Find("input");
        Assert.Multiple(() =>
        {
            Assert.That((input.GetAttribute("type"), input.GetAttribute("value"), input.GetAttribute("data-testid")), Is.EqualTo(("text", "1 slice", "name")));
            Assert.That((input.GetAttribute("maxlength"), input.GetAttribute("placeholder")), Is.EqualTo(("60", "Serving name")));
            Assert.That(cut.Find(".visually-hidden").TextContent, Is.EqualTo("Serving name"));
        });
    }

    [Test]
    public void Typing_RaisesTheTextOnEveryKeystroke()
    {
        var typed = new List<string>();
        var cut = Render("", typed.Add);

        cut.Find("input").Input("A");
        cut.Find("input").Input("A slice");

        Assert.That(typed, Is.EqualTo(new[] { "A", "A slice" }));
    }

    [Test]
    public void InvalidMessage_ShowsAnAnnouncedErrorLinkedToTheInput()
    {
        var cut = Render(invalid: "Too long");

        var message = cut.Find("[data-testid=name-error]");
        Assert.Multiple(() =>
        {
            Assert.That(message.TextContent, Is.EqualTo("Too long"));
            Assert.That(message.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("input").GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(cut.Find("input").GetAttribute("aria-describedby"), Is.EqualTo(message.Id));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    public void NoInvalidMessage_ShowsNoError(string? invalid)
    {
        var cut = Render(invalid: invalid);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[role=alert]"), Is.Empty);
            Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public void TypingTheParentPutsBack_RedrawsOnceSoTheTypedTextGoes()
    {
        var cut = Render("abc");
        var renders = cut.RenderCount;

        cut.Find("input").Input("abcd");

        Assert.Multiple(() =>
        {
            Assert.That(cut.RenderCount - renders, Is.EqualTo(2));
            Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("abc"));
        });
    }

    [Test]
    public void TypingTheParentTakes_DoesNotRedraw()
    {
        IRenderedComponent<TextField> cut = null!;
        cut = Render("abc", changed: v => cut.Render(p => p.Add(x => x.Value, v)));
        var renders = cut.RenderCount;

        cut.Find("input").Input("abcd");

        Assert.Multiple(() =>
        {
            Assert.That(cut.RenderCount - renders, Is.EqualTo(2));
            Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("abcd"));
        });
    }

    [Test]
    public async Task TypingTheParentPutsBack_FocusesTheInputOnce()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render("abc");

        await cut.Find("input").InputAsync("abcd");
        cut.WaitForState(() => FocusCalls() == 1);
        await WaitForRendererAsync();

        Assert.That(FocusCalls(), Is.EqualTo(1));
    }

    [Test]
    public async Task ReferenceStaleForSixRenders_FocusesOnceTheReferenceChanges()
    {
        var cut = RenderSlow(staleFor: 6);

        await PutBackAsync(cut, "abcd", RendersAfterOnePutBack);

        Assert.That(FocusCalls(), Is.EqualTo(1));
    }

    [Test]
    public async Task ReferenceNeverChanges_GivesUpWithoutFocusingAfterTenChecks()
    {
        var cut = RenderSlow(staleFor: 1000);
        var renders = cut.Instance.Renders;

        // The render after the keystroke, the redraw, then one render per check up to the cap.
        await PutBackAsync(cut, "abcd", renders + 12);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Instance.Renders - renders, Is.EqualTo(12));
            Assert.That(FocusCalls(), Is.Zero);
        });
    }

    [Test]
    public async Task TwoPutBacksEachStaleForSixRenders_FocusTwice()
    {
        var cut = RenderSlow(staleFor: 6);

        await PutBackAsync(cut, "abcd", RendersAfterOnePutBack);
        await PutBackAsync(cut, "abcde", RendersAfterOnePutBack + PutBackRenders);

        Assert.That(FocusCalls(), Is.EqualTo(2));
    }

    // The initial render, then per put-back: the render after the keystroke, the redraw and one render per stale check (6), the last of which sees the new reference.
    private const int RendersAfterOnePutBack = 1 + PutBackRenders;
    private const int PutBackRenders = 2 + 6;

    // Waits for the render count, then for the dispatcher to finish the focus call made after that render.
    private async Task PutBackAsync(IRenderedComponent<SlowReferenceTextField> cut, string typed, int renders)
    {
        await cut.Find("input").InputAsync(typed);
        cut.WaitForState(() => cut.Instance.Renders >= renders);
        await WaitForRendererAsync();
    }

    private IRenderedComponent<SlowReferenceTextField> RenderSlow(int staleFor)
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        return Context.Render<SlowReferenceTextField>(p => p
            .Add(x => x.Label, "Serving name")
            .Add(x => x.Value, "abc")
            .Add(x => x.ValueChanged, (string _) => { })
            .Add(x => x.StaleFor, staleFor));
    }

    private int FocusCalls() => Context.JSInterop.Invocations.Count(i => i.Identifier == "Blazor._internal.domWrapper.focus");

    /// <summary>After each put-back, keeps handing out the old element reference for <see cref="StaleFor"/> checks, as the browser does until it has drawn the new input.</summary>
    private sealed class SlowReferenceTextField : TextField
    {
        private int reads;
        private int epoch;

        [Inject]
        private IJSRuntime JS { get; set; } = null!;

        [Parameter]
        public int StaleFor { get; set; }

        public int Renders { get; private set; }

        internal override ElementReference CurrentInput
        {
            get
            {
                // Read 0 notes the old input, reads 1..StaleFor still see it, the next sees the new one.
                var id = reads++ > StaleFor ? $"drawn-{epoch}" : $"old-{epoch}";
                if (reads > StaleFor + 1)
                {
                    reads = 0;
                    epoch++;
                }

                return new ElementReference(id, new WebElementReferenceContext(JS));
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            Renders++;
            base.BuildRenderTree(builder);
        }
    }

    [Test]
    public void Disabled_DisablesTheInput() =>
        Assert.That(Render(disabled: true).Find("input").HasAttribute("disabled"), Is.True);
}
