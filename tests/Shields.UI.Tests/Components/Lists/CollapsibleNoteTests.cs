using Shields.UI.Components.Actions;
using Shields.UI.Components.Lists;

namespace Shields.UI.Tests.Components.Lists;

public class CollapsibleNoteTests : ComponentTestBase
{
    private IRenderedComponent<CollapsibleNote> Render(string text, bool expanded = false, Action<bool>? expandedChanged = null) =>
        Context.Render<CollapsibleNote>(p => p
            .Add(x => x.Text, text)
            .Add(x => x.Label, "Last note")
            .Add(x => x.Expanded, expanded)
            .Add(x => x.ExpandedChanged, expandedChanged ?? (_ => { }))
            .Add(x => x.TestId, "note"));

    [Test]
    public void ShowsTheNoteIconTheTextAndAHiddenLabel()
    {
        var cut = Render("Belt on from set 3");

        var note = cut.Find("[data-testid=note]");
        var icon = cut.FindComponent<Icon>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That((icon.Name, icon.Tone, icon.Size), Is.EqualTo((IconName.Note, IconTone.Muted, IconSize.Small)));
            Assert.That(note.QuerySelector(".text")!.TextContent, Is.EqualTo("Belt on from set 3"));
            Assert.That(note.QuerySelector(".visually-hidden")!.TextContent, Is.EqualTo("Last note: "));
        });
    }

    [Test]
    public void IconParameter_ChoosesTheIcon()
    {
        var cut = Context.Render<CollapsibleNote>(p => p.Add(x => x.Text, "t").Add(x => x.Label, "l").Add(x => x.Icon, IconName.Calendar));

        Assert.That(cut.FindComponent<Icon>().Instance.Name, Is.EqualTo(IconName.Calendar));
    }

    [TestCase(false, "false", false)]
    [TestCase(true, "true", true)]
    public void Expanded_SetsAriaExpandedAndTheOpenClass(bool expanded, string aria, bool open)
    {
        var note = Render("Belt on", expanded).Find("[data-testid=note]");

        Assert.Multiple(() =>
        {
            Assert.That(note.GetAttribute("aria-expanded"), Is.EqualTo(aria));
            Assert.That(note.ClassList.Contains("open"), Is.EqualTo(open));
        });
    }

    [Test]
    public void Tap_TogglesAndRaisesExpandedChanged()
    {
        var raised = new List<bool>();
        var cut = Render("Belt on", expandedChanged: raised.Add);

        cut.Find("[data-testid=note]").Click();
        var open = cut.Find("[data-testid=note]").ClassList.Contains("open");
        cut.Find("[data-testid=note]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(open, Is.True);
            Assert.That(cut.Find("[data-testid=note]").ClassList, Does.Not.Contain("open"));
            Assert.That(raised, Is.EqualTo(new[] { true, false }));
        });
    }

    [Test]
    public void LongNote_KeepsTheWholeTextInTheMarkup()
    {
        var text = new string('a', 400);

        Assert.That(Render(text).Find(".text").TextContent, Is.EqualTo(text));
    }
}
