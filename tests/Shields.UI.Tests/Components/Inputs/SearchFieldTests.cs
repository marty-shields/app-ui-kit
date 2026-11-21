using Microsoft.AspNetCore.Components.Web;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class SearchFieldTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsASearchInputWithTheSearchIconInTheInputBox()
    {
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search foods")
            .Add(x => x.Placeholder, "Search foods")
            .Add(x => x.Value, "oats")
            .Add(x => x.TestId, "search"));

        var input = cut.Find("input");
        Assert.Multiple(() =>
        {
            Assert.That(input.GetAttribute("type"), Is.EqualTo("search"));
            Assert.That(input.GetAttribute("aria-label"), Is.EqualTo("Search foods"));
            Assert.That(input.GetAttribute("placeholder"), Is.EqualTo("Search foods"));
            Assert.That(input.GetAttribute("value"), Is.EqualTo("oats"));
            Assert.That(input.GetAttribute("autocomplete"), Is.EqualTo("off"));
            Assert.That(input.GetAttribute("enterkeyhint"), Is.EqualTo("search"));
            Assert.That(cut.Find(".input-box svg"), Is.Not.Null);
            Assert.That(cut.Find(".input-box input"), Is.Not.Null);
        });
    }

    [Test]
    public void TestId_IsOnTheInputAndNotTheBox()
    {
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.TestId, "search"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=search]").TagName, Is.EqualTo("INPUT"));
            Assert.That(cut.FindAll("[data-testid]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Input_RaisesValueChangedWithTheTypedText()
    {
        string? typed = null;
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.ValueChanged, v => typed = v));

        cut.Find("input").Input("che");

        Assert.That(typed, Is.EqualTo("che"));
    }

    [Test]
    public void Enter_RaisesOnEnter()
    {
        var entered = 0;
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.OnEnter, () => entered++));

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(entered, Is.EqualTo(1));
    }

    [Test]
    public void OtherKeys_DoNotRaiseOnEnter()
    {
        var entered = 0;
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.OnEnter, () => entered++));

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.That(entered, Is.Zero);
    }

    [Test]
    public void Trailing_ShowsAfterADecorativeVerticalDivider()
    {
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.Trailing, "<button type=\"button\" data-testid=\"scan\">Scan</button>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".trailing [data-testid=scan]").TextContent, Is.EqualTo("Scan"));
            Assert.That(cut.Find(".divider-slot > .divider.vertical").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(cut.Find(".divider-slot").NextElementSibling!.ClassList, Does.Contain("trailing"));
        });
    }

    [Test]
    public void Render_WithoutTrailing_ShowsNoDividerAndNoSlot()
    {
        var cut = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search"));

        Assert.That(cut.FindAll(".divider-slot,.trailing"), Is.Empty);
    }

    [Test]
    public void Input_SitsInsideTheInputBoxSoItsFocusOutlineShows()
    {
        var cut = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search"));

        Assert.That(cut.Find("input").ParentElement!.ClassList, Does.Contain("input-box"));
    }

    [Test]
    public void TypingTheParentPutsBack_RedrawsSoTheTypedTextGoes()
    {
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.Value, "abc")
            .Add(x => x.ValueChanged, (string _) => { }));
        var renders = cut.RenderCount;

        cut.Find("input").Input("abcd");

        Assert.Multiple(() =>
        {
            Assert.That(cut.RenderCount - renders, Is.EqualTo(2));
            Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("abc"));
        });
    }

    [Test]
    public async Task TypingTheParentPutsBack_FocusesTheInputOnce()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.Value, "abc")
            .Add(x => x.ValueChanged, (string _) => { }));

        await cut.Find("input").InputAsync("abcd");
        cut.WaitForState(() => FocusCalls() == 1);
        await WaitForRendererAsync();

        Assert.That(FocusCalls(), Is.EqualTo(1));
    }

    [Test]
    public void TypingTheParentTakes_KeepsTheTypedText()
    {
        IRenderedComponent<SearchField> cut = null!;
        cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.Value, "abc")
            .Add(x => x.ValueChanged, v => cut.Render(q => q.Add(x => x.Value, v))));

        cut.Find("input").Input("abcd");

        Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("abcd"));
    }

    private int FocusCalls() => Context.JSInterop.Invocations.Count(i => i.Identifier == "Blazor._internal.domWrapper.focus");
}
