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

    [Test]
    public void ResultText_ShowsAPoliteLiveLineUnderTheBox()
    {
        var cut = Context.Render<SearchField>(p => p
            .Add(x => x.AriaLabel, "Search")
            .Add(x => x.ResultText, "12 results")
            .Add(x => x.TestId, "search"));

        var line = cut.Find("[data-testid=search-results]");
        Assert.Multiple(() =>
        {
            Assert.That(line.TextContent, Is.EqualTo("12 results"));
            Assert.That(line.GetAttribute("aria-live"), Is.EqualTo("polite"));
            Assert.That(line.GetAttribute("role"), Is.EqualTo("status"));
        });
    }

    [Test]
    public void ResultText_Empty_KeepsAnEmptyLiveLine_AndNullShowsNone()
    {
        var empty = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search").Add(x => x.ResultText, ""));
        var none = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search"));

        Assert.Multiple(() =>
        {
            Assert.That(empty.Find(".results").TextContent, Is.Empty);
            Assert.That(none.FindAll(".results"), Is.Empty);
        });
    }

    [Test]
    public void MaxLength_SetsTheInputLimit_AndNullLeavesItOff()
    {
        var limited = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search").Add(x => x.MaxLength, 60));
        var open = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search"));

        Assert.Multiple(() =>
        {
            Assert.That(limited.Find("input").GetAttribute("maxlength"), Is.EqualTo("60"));
            Assert.That(open.Find("input").HasAttribute("maxlength"), Is.False);
        });
    }

    [Test]
    public void FocusOnShow_FocusesTheInputOnFirstRenderOnly()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;

        Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search").Add(x => x.FocusOnShow, true));
        var calls = FocusCalls();
        Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search"));

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(FocusCalls(), Is.EqualTo(1));
        });
    }

    [Test]
    public void FocusOnShow_DoesNotFocusAgainOnLaterRenders()
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search").Add(x => x.FocusOnShow, true));

        cut.Render(p => p.Add(x => x.AriaLabel, "Find").Add(x => x.FocusOnShow, true));

        Assert.That(FocusCalls(), Is.EqualTo(1));
    }

    [Test]
    public void BoxAndResultLine_ShareOneRoot()
    {
        var cut = Context.Render<SearchField>(p => p.Add(x => x.AriaLabel, "Search").Add(x => x.ResultText, "3 results"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Nodes.Length, Is.EqualTo(1));
            Assert.That(cut.Find(".search-field > .search"), Is.Not.Null);
            Assert.That(cut.Find(".search-field > .results").TextContent, Is.EqualTo("3 results"));
        });
    }

    private int FocusCalls() => Context.JSInterop.Invocations.Count(i => i.Identifier == "Blazor._internal.domWrapper.focus");
}
