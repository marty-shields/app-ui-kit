using Shields.UI.Components.Actions;
using Shields.UI.Components.Overlays;

namespace Shields.UI.Tests.Components.Overlays;

public class SheetActionTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsAButtonWithTheLabelAndAnIcon()
    {
        var cut = Context.Render<SheetAction>(p => p
            .Add(x => x.Icon, IconName.Search)
            .Add(x => x.Label, "Search")
            .Add(x => x.TestId, "search"));

        var button = cut.Find("button[data-testid=search]");
        Assert.Multiple(() =>
        {
            Assert.That(button.GetAttribute("type"), Is.EqualTo("button"));
            Assert.That(button.TextContent.Trim(), Is.EqualTo("Search"));
            Assert.That(button.QuerySelectorAll("svg.icon"), Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void Click_RaisesOnClick()
    {
        var clicked = 0;
        var cut = Context.Render<SheetAction>(p => p
            .Add(x => x.Icon, IconName.Scan)
            .Add(x => x.Label, "Scan")
            .Add(x => x.OnClick, () => clicked++));

        cut.Find("button").Click();

        Assert.That(clicked, Is.EqualTo(1));
    }
}
