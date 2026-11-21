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
            Assert.That(sheet.FirstElementChild!.GetAttribute("data-testid"), Is.EqualTo("sheet-handle"));
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
        var module = SetupModule();

        var cut = Render(open: true);

        module.Invocations["open"].Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=sheet]"));
    }

    [Test]
    public void Closed_ReleasesTheFocusTrapOfThatSheet()
    {
        var module = SetupModule(trap: 7);
        var cut = Render(open: true);

        cut.Render(p => p.Add(x => x.Open, false));

        Assert.That(module.Invocations["close"].Single().Arguments, Is.EqualTo(new object?[] { 7 }));
    }

    [Test]
    public void ReRenderedWhileOpen_DoesNotOpenOrCloseTheTrapAgain()
    {
        var module = SetupModule();
        var cut = Render(open: true);

        cut.Render(p => p.Add(x => x.Title, "Other title").AddChildContent("<p>New body</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(module.Invocations["open"], Has.Count.EqualTo(1));
            Assert.That(module.Invocations["close"], Is.Empty);
        });
    }

    [Test]
    public async Task Escape_FromTheScriptWhileFocusIsOutsideTheSheet_RaisesOnClose()
    {
        var module = SetupModule();
        var closed = 0;
        Render(open: true, p => p.Add(x => x.OnClose, () => closed++));
        var reference = (DotNetObjectReference<Sheet>)module.Invocations["open"].Single().Arguments[1]!;

        await reference.Value.EscapeOutsideSheet();

        Assert.That(closed, Is.EqualTo(1));
    }

    [Test]
    public async Task DisposedWhileOpen_ReleasesTheFocusTrapAndTheModule()
    {
        var module = new FakeModule { Trap = 3 };
        Context.Services.AddSingleton<IJSRuntime>(new FakeRuntime(module));
        Render(open: true);

        await Context.DisposeComponentsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(module.Calls, Is.EqualTo(new[] { "open", "close:3" }));
            Assert.That(module.Disposals, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task DisposedAfterClosing_ReleasesTheFocusTrapOnlyOnce()
    {
        var module = new FakeModule { Trap = 3 };
        Context.Services.AddSingleton<IJSRuntime>(new FakeRuntime(module));
        var cut = Render(open: true);
        cut.Render(p => p.Add(x => x.Open, false));

        await Context.DisposeComponentsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(module.Calls, Is.EqualTo(new[] { "open", "close:3" }));
            Assert.That(module.Disposals, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ClosedBeforeTheScriptLoads_NeverOpensTheTrapAndLoadsItOnce()
    {
        var runtime = new DelayedImportRuntime();
        Context.Services.AddSingleton<IJSRuntime>(runtime);
        var module = new FakeModule();
        var cut = Render(open: true);
        cut.Render(p => p.Add(x => x.Open, false));

        runtime.Import.SetResult(module);
        await cut.InvokeAsync(() => { });
        await Context.DisposeComponentsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(runtime.Imports, Is.EqualTo(1));
            Assert.That(module.Calls, Is.Empty);
        });
    }

    [Test]
    public async Task DisposedBeforeTheScriptLoads_NeverOpensTheTrap()
    {
        var runtime = new DelayedImportRuntime();
        Context.Services.AddSingleton<IJSRuntime>(runtime);
        var module = new FakeModule();
        var cut = Render(open: true);

        await Context.DisposeComponentsAsync();
        runtime.Import.SetResult(module);
        await cut.InvokeAsync(() => { });

        Assert.That(module.Calls, Is.Empty);
    }

    [Test]
    public void PageGoneWhileOpening_IsSwallowedAndNotLogged()
    {
        var logger = new RecordingLogger<Sheet>();
        Context.Services.AddSingleton<ILogger<Sheet>>(logger);
        Context.JSInterop.SetupModule(Module).Setup<int>("open", _ => true).SetException(new JSDisconnectedException("Gone"));

        var cut = Render(open: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=sheet]"), Has.Count.EqualTo(1));
            Assert.That(logger.Records, Is.Empty);
        });
    }

    [Test]
    public async Task PageGoneWhileDisposing_IsSwallowedAndNotLogged()
    {
        var logger = new RecordingLogger<Sheet>();
        Context.Services.AddSingleton<ILogger<Sheet>>(logger);
        var module = new FakeModule { Trap = 3, Throws = true };
        Context.Services.AddSingleton<IJSRuntime>(new FakeRuntime(module));
        Render(open: true);

        await Context.DisposeComponentsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(module.Disposals, Is.EqualTo(1));
            Assert.That(logger.Records, Is.Empty);
        });
    }

    [Test]
    public void FocusFails_LogsItAndStillShowsTheSheet()
    {
        var logger = new RecordingLogger<Sheet>();
        Context.Services.AddSingleton<ILogger<Sheet>>(logger);
        Context.JSInterop.SetupModule(Module).Setup<int>("open", _ => true).SetException(new JSException("Open failed"));

        var cut = Render(open: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=sheet]"), Has.Count.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.SheetFocusFailed, LogLevel.Warning, "Loading or running the sheet focus script failed") }));
        });
    }

    private BunitJSModuleInterop SetupModule(int trap = 1)
    {
        var module = Context.JSInterop.SetupModule(Module);
        module.Setup<int>("open", _ => true).SetResult(trap);
        module.SetupVoid("close", _ => true).SetVoidResult();
        return module;
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

#pragma warning disable BL0016 // Fakes implement the interop interfaces; nothing here calls into JS
    private sealed class FakeModule : IJSObjectReference
    {
        public List<string> Calls { get; } = [];

        public int Trap { get; init; } = 1;

        public bool Throws { get; init; }

        public int Disposals { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add(identifier == "close" ? $"close:{args![0]}" : identifier);
            return ValueTask.FromResult(typeof(TValue) == typeof(int) ? (TValue)(object)Trap : default!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);

        public ValueTask DisposeAsync()
        {
            Disposals++;
            return Throws ? ValueTask.FromException(new JSDisconnectedException("Gone")) : ValueTask.CompletedTask;
        }
    }

    private sealed class FakeRuntime(FakeModule module) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult((TValue)(object)module);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }

    private sealed class DelayedImportRuntime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new();

        public int Imports { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Imports++;
            return new((Task<TValue>)(object)Import.Task);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }
#pragma warning restore BL0016
}
