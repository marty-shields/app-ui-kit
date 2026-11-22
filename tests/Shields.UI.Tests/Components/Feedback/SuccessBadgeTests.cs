using Shields.UI.Components.Actions;
using Shields.UI.Components.Feedback;

namespace Shields.UI.Tests.Components.Feedback;

public class SuccessBadgeTests : ComponentTestBase
{
    [Test]
    public void ShowsASuccessIconAndTheTextInTheSuccessSmallVariant()
    {
        var cut = Context.Render<SuccessBadge>(p => p.Add(x => x.Text, "Record").Add(x => x.TestId, "badge"));

        var icon = cut.FindComponent<Icon>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That((icon.Name, icon.Tone, icon.Size), Is.EqualTo((IconName.Record, IconTone.Success, IconSize.Small)));
            Assert.That(cut.Find("[data-testid=badge]").TextContent.Trim(), Is.EqualTo("Record"));
            Assert.That(cut.Find("[data-testid=badge] .success-small").TextContent, Is.EqualTo("Record"));
        });
    }

    [Test]
    public void IconParameter_ChoosesTheIcon()
    {
        var cut = Context.Render<SuccessBadge>(p => p.Add(x => x.Text, "Done").Add(x => x.Icon, IconName.Check));

        Assert.That(cut.FindComponent<Icon>().Instance.Name, Is.EqualTo(IconName.Check));
    }
}
