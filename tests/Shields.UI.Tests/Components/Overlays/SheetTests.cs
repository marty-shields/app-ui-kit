using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Shields.UI.Components.Overlays;
using Shields.UI.Logging;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Overlays;

public class SheetTests : ComponentTestBase
{
    private const string Module = "./_content/Shields.UI/Components/Overlays/Sheet.razor.js";

    private BackButton backButton = null!;

    [SetUp]
    public void RegisterBackButton()
    {
        backButton = new BackButton(NullLogger<BackButton>.Instance);
        Context.Services.AddSingleton<IBackButton>(backButton);
    }

    [Test]
    public void Closed_RendersNothingAndLoadsNoScript()
    {
        var cut = Render(open: false);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Markup.Trim(), Is.Empty);
            Assert.That(Context.JSInterop.Invocations, Is.Empty);
        });
    }

    [Test]
    public void Open_RendersADialogLabelledByItsTitleWithTheBody()
    {
        var cut = Render(open: true, p => p.AddChildContent("<p data-testid=\"body\">Body</p>"));

        var sheet = cut.Find("[data-testid=sheet]");
        Assert.Multiple(() =>
        {
            Assert.That(sheet.GetAttribute("role"), Is.EqualTo("dialog"));
            Assert.That(sheet.GetAttribute("aria-modal"), Is.EqualTo("true"));
            Assert.That(sheet.GetAttribute("tabindex"), Is.EqualTo("-1"));
            Assert.That(cut.Find($"#{sheet.GetAttribute("aria-labelledby")}").TextContent, Is.EqualTo("Add item"));
            Assert.That(sheet.QuerySelector("[data-testid=body]")!.TextContent, Is.EqualTo("Body"));
            Assert.That(cut.Find("[data-testid=sheet-handle]").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=sheet-scrim]").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void OpenedAndClosedByTheOwner_RendersOnlyWhileOpen()
    {
        var cut = Render(open: true);

        cut.Render(p => p.Add(x => x.Open, false));

        Assert.That(cut.FindAll("[data-testid=sheet]"), Is.Empty);
    }

    [Test]
    public void Scrim_RaisesOnCloseAndStaysOpenUntilTheOwnerClears()
    {
        var closed = 0;
        var cut = Render(open: true, p => p.Add(x => x.OnClose, () => closed++));

        cut.Find("[data-testid=sheet-scrim]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(closed, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=sheet]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Escape_RaisesOnClose_AndOtherKeysDoNot()
    {
        var closed = 0;
        var cut = Render(open: true, p => p.Add(x => x.OnClose, () => closed++));

        cut.Find("[data-testid=sheet]").KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.That(closed, Is.Zero);

        cut.Find("[data-testid=sheet]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.That(closed, Is.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_RaisesOnClose()
    {
        var closed = 0;
        Render(open: true, p => p.Add(x => x.OnClose, () => closed++));

        await backButton.HandleAsync();

        Assert.That(closed, Is.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_IsNotHandledWhileClosed()
    {
        var closed = 0;
        Render(open: false, p => p.Add(x => x.OnClose, () => closed++));

        await backButton.HandleAsync();

        Assert.That(closed, Is.Zero);
    }

    [Test]
    public void TestId_NamesTheSheetScrimAndHandle()
    {
        var cut = Render(open: true, p => p.Add(x => x.TestId, "add"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=add]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=add-scrim]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=add-handle]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Open_TrapsFocusInTheSheet()
    {
        var module = Context.JSInterop.SetupModule(Module);
        module.SetupVoid("open", _ => true).SetVoidResult();

        var cut = Render(open: true);

        module.Invocations["open"].Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=sheet]"));
    }

    [Test]
    public void Closed_ReleasesTheFocusTrap()
    {
        var module = Context.JSInterop.SetupModule(Module);
        module.SetupVoid("open", _ => true).SetVoidResult();
        module.SetupVoid("close").SetVoidResult();
        var cut = Render(open: true);

        cut.Render(p => p.Add(x => x.Open, false));

        Assert.That(module.Invocations["close"], Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DisposedWhileOpen_ReleasesTheFocusTrapAndTheModule()
    {
        var module = Context.JSInterop.SetupModule(Module);
        module.SetupVoid("open", _ => true).SetVoidResult();
        module.SetupVoid("close").SetVoidResult();
        Render(open: true);

        await Context.DisposeComponentsAsync();

        Assert.That(module.Invocations["close"], Has.Count.EqualTo(1));
    }

    [Test]
    public void FocusFails_LogsItAndStillShowsTheSheet()
    {
        var logger = new RecordingLogger<Sheet>();
        Context.Services.AddSingleton<ILogger<Sheet>>(logger);
        Context.JSInterop.SetupModule(Module).SetupVoid("open", _ => true).SetException(new JSException("Open failed"));

        var cut = Render(open: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=sheet]"), Has.Count.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.SheetFocusFailed, LogLevel.Warning, "Moving focus for the sheet failed") }));
        });
    }

    private IRenderedComponent<Sheet> Render(bool open, Action<ComponentParameterCollectionBuilder<Sheet>>? configure = null)
    {
        Context.JSInterop.Mode = JSRuntimeMode.Loose;
        return Context.Render<Sheet>(p =>
        {
            p.Add(x => x.Open, open).Add(x => x.Title, "Add item");
            configure?.Invoke(p);
        });
    }
}
