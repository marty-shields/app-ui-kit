using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class SegmentedControlTests : ComponentTestBase
{
    private static readonly IReadOnlyList<RadioOption<int>> Options =
    [
        new(1, "One", "one"),
        new(2, "Two", "two", "Not shown"),
        new(3, "Three", "three"),
    ];

    [Test]
    public void Renders_AHiddenLabelAndEachOptionWithItsTestId()
    {
        var cut = Render(value: 1);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("fieldset").GetAttribute("data-testid"), Is.EqualTo("segments"));
            Assert.That(cut.Find("legend").TextContent, Is.EqualTo("Choose"));
            Assert.That(cut.Find("legend").ClassList, Does.Contain("visually-hidden"));
            Assert.That(cut.FindAll(".text").Select(t => t.TextContent), Is.EqualTo(new[] { "One", "Two", "Three" }));
            Assert.That(cut.FindAll("input[type=radio]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "one", "two", "three" }));
            Assert.That(cut.Markup, Does.Not.Contain("Not shown"));
        });
    }

    [Test]
    public void Value_ChecksItsOptionOnly()
    {
        var cut = Render(value: 2);

        Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { false, true, false }));
    }

    [Test]
    public void Value_MatchingNoOption_ChecksNone()
    {
        var cut = Render(value: 4);

        Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { false, false, false }));
    }

    [Test]
    public void AriaLabel_NamesItsRadioAndOthersKeepTheirLabel()
    {
        var cut = Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Range")
            .Add(x => x.Options, [new RadioOption<int>(1, "3M", "three", AriaLabel: "Last 3 months"), new RadioOption<int>(2, "All", "all")])
            .Add(x => x.Value, 1));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=three]").GetAttribute("aria-label"), Is.EqualTo("Last 3 months"));
            Assert.That(cut.Find("[data-testid=all]").HasAttribute("aria-label"), Is.False);
            Assert.That(cut.FindAll(".text").Select(t => t.TextContent), Is.EqualTo(new[] { "3M", "All" }));
        });
    }

    [Test]
    public void Options_ShareOneNamePerControl()
    {
        var first = Render(value: 1);
        var second = Render(value: 1);

        var names = first.FindAll("input").Select(i => i.GetAttribute("name")).Distinct().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(second.Find("input").GetAttribute("name"), Is.Not.EqualTo(names[0]));
        });
    }

    [Test]
    public void TappingASegment_RaisesValueChangedWithItsValue()
    {
        var changed = new List<int>();
        var cut = Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (int v) => changed.Add(v)));

        cut.Find("[data-testid=three]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public void TappingASegment_TheParentDoesNotTake_RedrawsTheRadiosOnce()
    {
        var cut = Render(value: 1);
        var renders = cut.RenderCount;

        cut.Find("[data-testid=three]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.RenderCount - renders, Is.EqualTo(2));
            Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { true, false, false }));
        });
    }

    [Test]
    public void TappingASegment_TheParentTakes_DoesNotRedrawTheRadios()
    {
        var cut = RenderTracking(1);
        var renders = cut.RenderCount;
        cut.Find("[data-testid=three]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.RenderCount - renders, Is.LessThanOrEqualTo(2));
            Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { false, false, true }));
        });
    }

    [Test]
    public async Task TappingASegment_TheParentTakesAfterAnAsyncHandler_DoesNotRedrawTheRadios()
    {
        var release = new TaskCompletionSource();
        IRenderedComponent<SegmentedControl<int>>? cut = null;
        cut = Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, async (int v) =>
            {
                await release.Task;
                cut!.Render(q => q.Add(x => x.Value, v));
            }));
        var before = cut.RenderCount;
        var tap = cut.Find("[data-testid=three]").ChangeAsync(new ChangeEventArgs { Value = true });
        cut.Render();
        var whileWaiting = cut.RenderCount - before;
        release.SetResult();
        await tap;

        Assert.Multiple(() =>
        {
            Assert.That(whileWaiting, Is.EqualTo(2), "renders while the parent had not answered");
            Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { false, false, true }));
        });
    }

    [Test]
    public void TappingASegment_WhenValueChangedThrows_StillChecksTheParentsValue()
    {
        var cut = Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (int _) => throw new InvalidOperationException("parent failed")));

        Assert.That(() => cut.Find("[data-testid=three]").Change(true), Throws.InvalidOperationException);
        cut.Render();

        Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { true, false, false }));
    }

    [Test]
    public void NullValue_ChecksTheOptionWhoseValueIsNull()
    {
        var cut = Context.Render<SegmentedControl<int?>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, [new RadioOption<int?>(1, "One", "one"), new RadioOption<int?>(null, "All", "all")])
            .Add(x => x.Value, null)
            .Add(x => x.ValueChanged, (int? _) => { }));

        Assert.That(cut.FindAll("input").Select(i => i.HasAttribute("checked")), Is.EqualTo(new[] { false, true }));
    }

    [Test]
    public async Task TappingASegment_TheParentDeclines_PutsFocusBackEvenWhenTheRadioReferencesAreSlowToUpdate()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Context.Render<SlowReferenceRadios>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, [new RadioOption<int>(1, "One", "one"), new RadioOption<int>(2, "Two", "two")])
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (int _) => { }));

        await cut.Find("input").ChangeAsync(new ChangeEventArgs { Value = true });

        cut.WaitForAssertion(() => Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(1)));
    }

    /// <summary>Hands out the same element reference for the chosen radio for the first renders after a pick, as the browser does until it has drawn the new radios.</summary>
    private sealed class SlowReferenceRadios : RadioGroupBase<int>
    {
        private const int StaleRenders = 3;
        private int renders;

        [Inject]
        private IJSRuntime JS { get; set; } = null!;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            renders++;
            var id = renders <= StaleRenders ? "stale" : $"drawn-{renders}";
            Radios[0] = new ElementReference(id, new WebElementReferenceContext(JS));
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "radio");
            builder.AddAttribute(2, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => Pick(2)));
            builder.SetKey(Generation);
            builder.CloseElement();
        }
    }

    private IRenderedComponent<SegmentedControl<int>> RenderTracking(int value)
    {
        IRenderedComponent<SegmentedControl<int>>? cut = null;
        cut = Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, (int v) => cut!.Render(q => q.Add(x => x.Value, v))));
        return cut;
    }

    private IRenderedComponent<SegmentedControl<int>> Render(int value) =>
        Context.Render<SegmentedControl<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, (int _) => { })
            .Add(x => x.TestId, "segments"));
}
