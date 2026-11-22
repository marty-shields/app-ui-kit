using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class EntryGridRowTests : ComponentTestBase
{
    private static RenderFragment Text(string text) => builder => builder.AddContent(0, text);

    [Test]
    public void Columns_AreTheIndexThenOneEqualColumnPerFieldThenTheActionTargetWidth()
    {
        var cut = Context.Render<EntryGridRow>(p => p
            .Add(x => x.Index, Text("1"))
            .Add(x => x.FieldCount, 2)
            .Add(x => x.Fields, "<input /><input />")
            .Add(x => x.Actions, Text("x")));

        Assert.That(cut.Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
    }

    [Test]
    public void Aside_AddsItsColumnBeforeTheFields()
    {
        var cut = Context.Render<EntryGridRow>(p => p
            .Add(x => x.Index, Text("1"))
            .Add(x => x.Aside, Text("60×8"))
            .Add(x => x.FieldCount, 2)
            .Add(x => x.Fields, "<input /><input />"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) var(--entry-grid-aside-width) minmax(0, 1fr) minmax(0, 1fr)"));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("60×8"));
        });
    }

    [Test]
    public void Cells_AreInOrder_IndexAsideFieldsActions()
    {
        var cut = Context.Render<EntryGridRow>(p => p
            .Add(x => x.Index, Text("2"))
            .Add(x => x.Aside, Text("a"))
            .Add(x => x.FieldCount, 1)
            .Add(x => x.Fields, "<input class=\"field\" />")
            .Add(x => x.Actions, Text("x")));

        Assert.That(cut.Find(".row").Children.Select(c => c.ClassName), Is.EqualTo(new[] { "index", "aside", "field", "actions" }));
    }

    [Test]
    public void NoFields_StillHasTheIndexColumn()
    {
        var cut = Context.Render<EntryGridRow>(p => p.Add(x => x.FieldCount, 0));

        Assert.That(cut.Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width)"));
    }

    [Test]
    public void Header_IsHiddenFromScreenReaders_AndARowIsNot()
    {
        var header = Context.Render<EntryGridRow>(p => p.Add(x => x.FieldCount, 1).Add(x => x.Header, true).Add(x => x.TestId, "header"));
        var row = Context.Render<EntryGridRow>(p => p.Add(x => x.FieldCount, 1).Add(x => x.TestId, "row"));

        Assert.Multiple(() =>
        {
            Assert.That(header.Find("[data-testid=header]").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(header.Find("[data-testid=header]").ClassList, Does.Contain("header"));
            Assert.That(row.Find("[data-testid=row]").HasAttribute("aria-hidden"), Is.False);
        });
    }

    [Test]
    public void AsideWidth_Wide_UsesTheWideAsideToken_AndNormalIsTheDefault()
    {
        IRenderedComponent<EntryGridRow> Row(EntryGridAsideWidth? width) => Context.Render<EntryGridRow>(p =>
        {
            p.Add(x => x.Aside, Text("57.25kg×12")).Add(x => x.FieldCount, 1);
            if (width is { } w)
            {
                p.Add(x => x.AsideWidth, w);
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(Row(EntryGridAsideWidth.Wide).Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) var(--entry-grid-aside-width-wide) minmax(0, 1fr)"));
            Assert.That(Row(null).Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) var(--entry-grid-aside-width) minmax(0, 1fr)"));
        });
    }

    [Test]
    public void AsideWidth_Wide_HeaderAndRowHaveTheSameColumns()
    {
        var header = Context.Render<EntryGridRow>(p => p.Add(x => x.Header, true).Add(x => x.Aside, Text("Last")).Add(x => x.FieldCount, 2).Add(x => x.Actions, Text("")).Add(x => x.AsideWidth, EntryGridAsideWidth.Wide));
        var row = Context.Render<EntryGridRow>(p => p.Add(x => x.Aside, Text("57.25kg×12")).Add(x => x.FieldCount, 2).Add(x => x.Actions, Text("x")).Add(x => x.AsideWidth, EntryGridAsideWidth.Wide));

        Assert.That(header.Find(".row").GetAttribute("style"), Is.EqualTo(row.Find(".row").GetAttribute("style")));
    }

    [Test]
    public void AsideWidth_Wide_HasNoEffectWithoutAnAside()
    {
        var cut = Context.Render<EntryGridRow>(p => p.Add(x => x.FieldCount, 1).Add(x => x.AsideWidth, EntryGridAsideWidth.Wide));

        Assert.That(cut.Find(".row").GetAttribute("style"), Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) minmax(0, 1fr)"));
    }

    [Test]
    public void WideAsideToken_IsWiderThanTheNormalOne()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Value("--entry-grid-aside-width"), Is.EqualTo("3.5rem"));
            Assert.That(DesignTokens.Value("--entry-grid-aside-width-wide"), Is.EqualTo("6rem"));
        });
    }
}
