using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class RadioGroupBaseTests : ComponentTestBase
{
    private const string Focus = "Blazor._internal.domWrapper.focus";

    // The group looks again after each render until the radio's reference changes, ten times at most; one decline renders 3 + 10 times when it never does.
    private const int RendersWhenNeverDrawn = 13;

    [Test]
    public async Task ParentDeclines_ReferencesStaleForSeveralRenders_PutsFocusBackOnce()
    {
        var cut = RenderGroup(staleFor: 6);

        await Tap(cut);
        await SettleAsync(cut, renders: RendersAfterOneDecline);

        Assert.That(FocusCalls(), Is.EqualTo(1));
    }

    [Test]
    public async Task ParentDeclinesTwice_EachDeclineGetsItsOwnRetries()
    {
        var cut = RenderGroup(staleFor: 6);

        await Tap(cut);
        await SettleAsync(cut, renders: RendersAfterOneDecline);
        await Tap(cut);
        await SettleAsync(cut, renders: RendersAfterOneDecline + 8);

        Assert.That(FocusCalls(), Is.EqualTo(2));
    }

    [Test]
    public async Task ParentDeclines_ReferencesNeverUpdate_GivesUpWithoutFocusing()
    {
        var cut = RenderGroup(staleFor: 1000);

        await Tap(cut);
        cut.WaitForState(() => cut.Instance.Renders >= RendersWhenNeverDrawn);
        for (var i = 0; i < 5; i++)
        {
            await WaitForRendererAsync();
        }

        Assert.Multiple(() =>
        {
            Assert.That(cut.Instance.Renders, Is.EqualTo(RendersWhenNeverDrawn));
            Assert.That(FocusCalls(), Is.Zero);
        });
    }

    // Initial render, the render after the tap, the new radios (stale for 6 renders), then the render that shows the drawn radios.
    private const int RendersAfterOneDecline = 9;

    // Waits for the render count, then for the dispatcher to finish the focus call made after that render.
    private async Task SettleAsync(IRenderedComponent<SlowReferenceRadios> cut, int renders)
    {
        cut.WaitForState(() => cut.Instance.Renders >= renders);
        await WaitForRendererAsync();
    }

    private int FocusCalls() => Context.JSInterop.Invocations.Count(i => i.Identifier == Focus);

    private static Task Tap(IRenderedComponent<SlowReferenceRadios> cut) =>
        cut.Find("input").ChangeAsync(new ChangeEventArgs { Value = true });

    private IRenderedComponent<SlowReferenceRadios> RenderGroup(int staleFor)
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        return Context.Render<SlowReferenceRadios>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, [new RadioOption<int>(1, "One", "one"), new RadioOption<int>(2, "Two", "two")])
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (int _) => { })
            .Add(x => x.StaleFor, staleFor));
    }

    /// <summary>After each pick, hands out the previous element reference for the chosen radio for <see cref="StaleFor"/> renders of the new radios, as the browser does until it has drawn them.</summary>
    private sealed class SlowReferenceRadios : RadioGroupBase<int>
    {
        private int generation = -1;
        private int rendersInGeneration;
        private string reference = "initial";

        [Inject]
        private IJSRuntime JS { get; set; } = null!;

        [Parameter]
        public int StaleFor { get; set; }

        public int Renders { get; private set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            Renders++;
            if (generation != Generation)
            {
                generation = Generation;
                rendersInGeneration = 0;
            }

            if (generation > 0 && ++rendersInGeneration > StaleFor)
            {
                reference = $"drawn-{generation}-{rendersInGeneration}";
            }

            Radios[0] = new ElementReference(reference, new WebElementReferenceContext(JS));
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "radio");
            builder.AddAttribute(2, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => Pick(2)));
            builder.SetKey(Generation);
            builder.CloseElement();
        }
    }
}
