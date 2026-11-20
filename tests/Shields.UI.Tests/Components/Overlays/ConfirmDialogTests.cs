using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Overlays;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Overlays;

public class ConfirmDialogTests : ComponentTestBase
{
    private BackButton backButton = null!;

    [SetUp]
    public void RegisterBackButton()
    {
        backButton = new BackButton(NullLogger<BackButton>.Instance);
        Context.Services.AddSingleton<IBackButton>(backButton);
    }

    [Test]
    public void Render_ShowsTitleMessageAndButtons()
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Delete this item?")
            .Add(x => x.Message, "This can't be undone.")
            .Add(x => x.ConfirmText, "Delete"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("Delete this item?"));
            Assert.That(cut.Find(".message").TextContent, Is.EqualTo("This can't be undone."));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").TextContent.Trim(), Is.EqualTo("Delete"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-cancel]").TextContent.Trim(), Is.EqualTo("Cancel"));
        });
    }

    [Test]
    public void Open_FocusesCancel()
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Delete this item?")
            .Add(x => x.ConfirmText, "Delete"));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=confirm-dialog-cancel]"));
    }

    [TestCase("This can't be undone.", true)]
    [TestCase(null, false)]
    public void Message_DescribesTheDialog(string? message, bool described)
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Delete this item?")
            .Add(x => x.Message, message)
            .Add(x => x.ConfirmText, "Delete"));

        var describedBy = cut.Find("[role=alertdialog]").GetAttribute("aria-describedby");

        Assert.That(describedBy, described ? Is.EqualTo(cut.Find(".message").Id) : Is.Null);
    }

    [Test]
    public void Title_LabelsTheDialog()
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Discard changes?")
            .Add(x => x.ConfirmText, "Discard"));

        var labelledBy = cut.Find("[role=alertdialog]").GetAttribute("aria-labelledby");

        Assert.That(cut.Find($"#{labelledBy}").TextContent, Is.EqualTo("Discard changes?"));
    }

    [Test]
    public void Confirm_RaisesOnConfirm()
    {
        var confirmed = false;
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Discard changes?")
            .Add(x => x.ConfirmText, "Discard")
            .Add(x => x.OnConfirm, () => confirmed = true));

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.That(confirmed, Is.True);
    }

    [Test]
    public void Cancel_RaisesOnCancel()
    {
        var cancelled = false;
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Discard changes?")
            .Add(x => x.ConfirmText, "Discard")
            .Add(x => x.OnCancel, () => cancelled = true));

        cut.Find("[data-testid=confirm-dialog-cancel]").Click();

        Assert.That(cancelled, Is.True);
    }

    [Test]
    public async Task AndroidBack_RaisesOnCancel()
    {
        var cancelled = 0;
        var confirmed = 0;
        Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Discard changes?")
            .Add(x => x.ConfirmText, "Discard")
            .Add(x => x.OnConfirm, () => confirmed++)
            .Add(x => x.OnCancel, () => cancelled++));

        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cancelled, Is.EqualTo(1));
            Assert.That(confirmed, Is.Zero);
        });
    }

    [Test]
    public void Danger_StylesConfirmAsDanger()
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Delete this item?")
            .Add(x => x.ConfirmText, "Delete")
            .Add(x => x.Danger, true));

        Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").ClassList, Does.Contain("danger"));
    }

    [Test]
    public void Buttons_AreFullWidthWithSecondaryCancel()
    {
        var cut = Context.Render<ConfirmDialog>(p => p
            .Add(x => x.Title, "Discard changes?")
            .Add(x => x.ConfirmText, "Discard"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=confirm-dialog-cancel]").ClassList, Does.Contain("secondary").And.Contain("full"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").ClassList, Does.Contain("full"));
        });
    }
}
