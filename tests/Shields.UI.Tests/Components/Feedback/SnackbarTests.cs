using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Shields.UI.Components.Feedback;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Components.Feedback;

public class SnackbarTests : ComponentTestBase
{
    private const string SnackbarModule = "./_content/Shields.UI/Components/Feedback/Snackbar.razor.js";
    private const string Action = "[data-testid=snackbar-action]";

    private ManualClock clock = null!;

    private SnackbarService Service => Context.Services.GetRequiredService<SnackbarService>();

    [SetUp]
    public void UseClock()
    {
        clock = new ManualClock();
        Context.Services.AddSingleton<TimeProvider>(clock).AddShieldsUi();
    }

    [Test]
    public void Message_ShowsTheTextAndTheAction()
    {
        var cut = RenderMessage();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=snackbar-message]").TextContent, Is.EqualTo("Set 2 deleted"));
            Assert.That(cut.Find(Action).TextContent, Is.EqualTo("Undo"));
        });
    }

    [Test]
    public void Message_PrefixesTheTestIdsWithTestId()
    {
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.ActionLabel, "Undo")
            .Add(x => x.OnAction, () => { })
            .Add(x => x.TestId, "undo-bar"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=undo-bar]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=undo-bar-message]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=undo-bar-action]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Action_RaisesOnAction()
    {
        var done = 0;
        var cut = RenderMessage(onAction: () => done++);

        cut.Find(Action).Click();

        Assert.That(done, Is.EqualTo(1));
    }

    [TestCase(null, true)]
    [TestCase("  ", true)]
    [TestCase("Undo", false)]
    public void ActionLabelOrHandlerMissing_ShowsNoAction(string? label, bool hasHandler)
    {
        var cut = Context.Render<Snackbar>(p =>
        {
            p.Add(x => x.Message, "Set 2 deleted").Add(x => x.ActionLabel, label);
            if (hasHandler)
            {
                p.Add(x => x.OnAction, () => { });
            }
        });

        Assert.That(cut.FindAll(Action), Is.Empty);
    }

    [Test]
    public void Action_IsANativeButtonSoTheKeyboardReachesIt()
    {
        var action = RenderMessage().Find(Action);

        Assert.Multiple(() =>
        {
            Assert.That(action.TagName, Is.EqualTo("BUTTON"));
            Assert.That(action.GetAttribute("tabindex"), Is.Null);
            Assert.That(action.HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public async Task Message_RaisesOnTimeoutOnceAfterTheDefaultDuration()
    {
        var timedOut = 0;
        RenderMessage(onTimeout: () => timedOut++);

        clock.Advance(Snackbar.DefaultDuration - TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        var early = timedOut;
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Zero);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public void DefaultDuration_IsWithinMaterialsFourToTenSeconds() =>
        Assert.That(Snackbar.DefaultDuration, Is.InRange(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(10)));

    [Test]
    public async Task Duration_ReplacesTheDefault()
    {
        var timedOut = 0;
        Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.Duration, TimeSpan.FromSeconds(2))
            .Add(x => x.OnTimeout, () => timedOut++));

        clock.Advance(TimeSpan.FromSeconds(2));
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public async Task InfiniteDuration_NeverTimesOut()
    {
        var timedOut = 0;
        Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.Duration, Timeout.InfiniteTimeSpan)
            .Add(x => x.OnTimeout, () => timedOut++));

        clock.Advance(TimeSpan.FromHours(1));
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task RemovedFirst_NeverTimesOut()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        await cut.Instance.DisposeAsync();
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task OnTimeoutThrows_ReachesTheErrorBoundary()
    {
        var cut = Context.Render<ErrorBoundary>(p => p
            .Add(x => x.ErrorContent, error => $"<p data-testid=\"boundary\">{error.Message}</p>")
            .AddChildContent<Snackbar>(s => s
                .Add(x => x.Message, "Set 2 deleted")
                .Add(x => x.OnTimeout, EventCallback.Factory.Create(this, () => throw new InvalidOperationException("Timed out badly")))));

        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(cut.Find("[data-testid=boundary]").TextContent, Is.EqualTo("Timed out badly"));
    }

    [Test]
    public void TakeFocus_FocusesTheActionWhenShown()
    {
        var cut = RenderMessage(takeFocus: true);

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find(Action));
    }

    [Test]
    public void TakeFocusFails_LogsAWarningWithEvent3901AndKeepsTheSnackbar()
    {
        var logger = new RecordingLogger<Snackbar>();
        Context.Services.AddSingleton<ILogger<Snackbar>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = RenderMessage(takeFocus: true);

        cut.WaitForAssertion(() => Assert.That(logger.Records.Select(r => (r.Id, r.Level)), Is.EqualTo(new[] { ((int)KitEvent.UndoFocusFailed, LogLevel.Warning) })));
        Assert.That(cut.FindAll(Action), Has.Count.EqualTo(1));
    }

    [Test]
    public void NoTakeFocus_LeavesFocusAlone()
    {
        RenderMessage();

        Context.JSInterop.VerifyNotInvoke("Blazor._internal.domWrapper.focus");
    }

    [Test]
    public async Task ActionKeyboardFocused_NeverTimesOut()
    {
        KeyboardFocus(true);
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration * 3);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task TakeFocusAfterATap_StillTimesOutOnTime()
    {
        KeyboardFocus(false);
        var timedOut = 0;
        var cut = RenderMessage(takeFocus: true, onTimeout: () => timedOut++);

        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public async Task FocusLeavesBeforeTheKeyboardCheckAnswers_KeepsTheTimer()
    {
        var check = Context.JSInterop.SetupModule(SnackbarModule).Setup<bool>("isFocusVisible", _ => true);
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);
        var action = cut.Find(Action);

        var focusing = action.FocusAsync(new FocusEventArgs());
        await action.BlurAsync(new FocusEventArgs());
        check.SetResult(true);
        await focusing;
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public async Task KeyboardCheckFails_LogsAWarningWithEvent3902AndStillTimesOut()
    {
        var logger = new RecordingLogger<Snackbar>();
        Context.Services.AddSingleton<ILogger<Snackbar>>(logger);
        Context.JSInterop.SetupModule(SnackbarModule).Setup<bool>("isFocusVisible", _ => true).SetException(new JSException("Check failed"));
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(logger.Records.Select(r => (r.Id, r.Level)), Is.EqualTo(new[] { ((int)KitEvent.UndoFocusCheckFailed, LogLevel.Warning) }));
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ActionLosesKeyboardFocus_TimesOutAFullDurationLater()
    {
        KeyboardFocus(true);
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);
        clock.Advance(Snackbar.DefaultDuration / 2);
        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration);

        await cut.Find(Action).BlurAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration - TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        var early = timedOut;
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Zero);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task FocusedAndLeftAfterTimingOut_DoesNotTimeOutAgain()
    {
        KeyboardFocus(true);
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        await cut.Find(Action).BlurAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public async Task FocusedTwice_ImportsTheModuleOnce()
    {
        KeyboardFocus(false);
        var cut = RenderMessage();
        var action = cut.Find(Action);
        await action.FocusAsync(new FocusEventArgs());
        await action.BlurAsync(new FocusEventArgs());

        await cut.Find(Action).FocusAsync(new FocusEventArgs());

        Assert.Multiple(() =>
        {
            Assert.That(Context.JSInterop.Invocations["import"], Has.Count.EqualTo(1));
            Assert.That(Context.JSInterop.Invocations["isFocusVisible"], Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task ImportFails_TriesAgainOnTheNextFocus()
    {
        var js = new FailingImports();
        Context.Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderMessage();
        var action = cut.Find(Action);
        await action.FocusAsync(new FocusEventArgs());
        await action.BlurAsync(new FocusEventArgs());

        await cut.Find(Action).FocusAsync(new FocusEventArgs());

        Assert.That(js.Imports, Is.EqualTo(2));
    }

    [Test]
    public async Task MessageChanges_RestartsTheTimerForAFullDuration()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);
        clock.Advance(Snackbar.DefaultDuration - TimeSpan.FromSeconds(1));

        cut.Render(p => p.Add(x => x.Message, "Set 3 deleted"));
        clock.Advance(Snackbar.DefaultDuration - TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();
        var early = timedOut;
        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Zero);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task MessageChangesAfterTimingOut_TimesOutAgain()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        cut.Render(p => p.Add(x => x.Message, "Set 3 deleted"));
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(2));
    }

    [Test]
    public async Task MessageAppearsOnALiveInstance_StartsTheTimer()
    {
        var timedOut = 0;
        var cut = Context.Render<Snackbar>(p => p.Add(x => x.OnTimeout, () => timedOut++));

        cut.Render(p => p.Add(x => x.Message, "Set 2 deleted"));
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public async Task MessageGoesNull_StopsTheTimer()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        cut.Render(p => p.Add(x => x.Message, null));
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public void MessageChanges_TakesFocusAgain()
    {
        var cut = RenderMessage(takeFocus: true);

        cut.Render(p => p.Add(x => x.Message, "Set 3 deleted"));

        Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Action_StopsTheTimerSoItNeverTimesOutAfterwards()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        cut.Find(Action).Click();
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task ActionAfterTimingOut_IsIgnored()
    {
        var done = 0;
        var cut = RenderMessage(onAction: () => done++);
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        cut.Find(Action).Click();

        Assert.That(done, Is.Zero);
    }

    [TestCase(-1)]
    [TestCase(-2)]
    public void NegativeDuration_Throws(int seconds) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.Duration, TimeSpan.FromSeconds(seconds))));

    [Test]
    public void DurationAboveWhatATimerAccepts_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.Duration, TimeSpan.FromDays(50))));

    [Test]
    public async Task StoppedAfterTheDelayEndsButBeforeTheTimeoutRuns_DoesNotTimeOut()
    {
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        // The clock moves off the renderer's thread while it is held, so the timeout waits in the renderer's queue until the dispose has run.
        await Context.Renderer.Dispatcher.InvokeAsync(() =>
        {
            Task.Run(() => clock.Advance(Snackbar.DefaultDuration)).Wait();
            return cut.Instance.DisposeAsync().AsTask();
        });
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task FocusCheckAfterDisconnect_LogsNothingAndStillTimesOut()
    {
        var logger = new RecordingLogger<Snackbar>();
        Context.Services.AddSingleton<ILogger<Snackbar>>(logger);
        Context.JSInterop.SetupModule(SnackbarModule).Setup<bool>("isFocusVisible", _ => true).SetException(new JSDisconnectedException("Gone"));
        var timedOut = 0;
        var cut = RenderMessage(onTimeout: () => timedOut++);

        await cut.Find(Action).FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.DefaultDuration);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(logger.Records, Is.Empty);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public void TakeFocusAfterDisconnect_LogsNothingAndKeepsTheSnackbar()
    {
        var logger = new RecordingLogger<Snackbar>();
        Context.Services.AddSingleton<ILogger<Snackbar>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSDisconnectedException("Gone"));

        var cut = RenderMessage(takeFocus: true);

        cut.WaitForAssertion(() => Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(logger.Records, Is.Empty);
            Assert.That(cut.FindAll(Action), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Disposed_DisposesTheImportedModule()
    {
        var js = new FakeModuleRuntime();
        Context.Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderMessage();
        await cut.Find(Action).FocusAsync(new FocusEventArgs());

        await cut.Instance.DisposeAsync();

        Assert.That(js.Module.Disposals, Is.EqualTo(1));
    }

    [Test]
    public async Task DisposedAfterDisconnect_SwallowsIt()
    {
        var js = new FakeModuleRuntime { DisposeThrows = true };
        Context.Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderMessage();
        await cut.Find(Action).FocusAsync(new FocusEventArgs());

        await cut.Instance.DisposeAsync();
        Assert.That(js.Module.Disposals, Is.EqualTo(1));
    }

    [Test]
    public async Task DisposedWithoutAnImport_DisposesNothing()
    {
        var js = new FakeModuleRuntime();
        Context.Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderMessage();

        await cut.Instance.DisposeAsync();

        Assert.That(js.Module.Disposals, Is.Zero);
    }

    [Test]
    public void NoMessageAndNothingPosted_RendersNothing()
    {
        var cut = Context.Render<Snackbar>();

        Assert.That(cut.Markup.Trim(), Is.Empty);
    }

    [Test]
    public async Task Posted_ShowsTheTextAndNoActionWhenThereIsNone()
    {
        var cut = Context.Render<Snackbar>();

        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("Entry added"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=snackbar-message]").TextContent, Is.EqualTo("Entry added"));
            Assert.That(cut.FindAll(Action), Is.Empty);
        });
    }

    [Test]
    public async Task PostedAction_FiresOnceThenDismisses()
    {
        var fired = 0;
        var cut = Context.Render<Snackbar>();
        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("Entry deleted", "Undo", () =>
        {
            fired++;
            return Task.CompletedTask;
        }));
        var shown = Service.Current!;
        var action = cut.Find(Action);

        Assert.That(action.TextContent, Is.EqualTo("Undo"));
        action.Click();
        await WaitForRendererAsync();
        await Service.InvokeActionAsync(shown);

        Assert.Multiple(() =>
        {
            Assert.That(fired, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=snackbar]"), Is.Empty);
            Assert.That(Service.Current, Is.Null);
        });
    }

    [Test]
    public async Task Posted_DismissesAfterTheServicesDuration()
    {
        var cut = Context.Render<Snackbar>();
        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("Entry added"));

        clock.Advance(SnackbarService.DefaultDuration - TimeSpan.FromMilliseconds(1));
        Assert.That(cut.FindAll("[data-testid=snackbar]"), Has.Count.EqualTo(1));

        clock.Advance(TimeSpan.FromMilliseconds(1));
        cut.WaitForState(() => cut.FindAll("[data-testid=snackbar]").Count == 0);

        Assert.That(Service.Current, Is.Null);
    }

    [Test]
    public async Task NewPost_ReplacesTheOldOneAndRestartsTheTimeout()
    {
        var cut = Context.Render<Snackbar>();
        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("First", "Undo", () => Task.CompletedTask));
        clock.Advance(TimeSpan.FromSeconds(4));

        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("Second"));
        clock.Advance(TimeSpan.FromSeconds(4));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=snackbar-message]").Select(e => e.TextContent), Is.EqualTo(new[] { "Second" }));
            Assert.That(cut.FindAll(Action), Is.Empty);
        });

        clock.Advance(TimeSpan.FromSeconds(1));
        cut.WaitForState(() => cut.FindAll("[data-testid=snackbar]").Count == 0);

        Assert.That(Service.Current, Is.Null);
    }

    [Test]
    public async Task PostedBeforeItRenders_ShowsWhenItDoes()
    {
        Service.Show("Entry deleted", "Undo", () => Task.CompletedTask);

        var cut = Context.Render<Snackbar>();
        await WaitForRendererAsync();

        Assert.That(cut.Find("[data-testid=snackbar-message]").TextContent, Is.EqualTo("Entry deleted"));
    }

    [Test]
    public async Task OwnService_ShowsInsteadOfTheHostsAndDoesNotTimeOutWhenInfinite()
    {
        using var own = new SnackbarService(clock);
        own.Show("Story", duration: Timeout.InfiniteTimeSpan);
        var cut = Context.Render<Snackbar>(p => p.Add(x => x.Service, own));
        await WaitForRendererAsync();

        clock.Advance(TimeSpan.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=snackbar-message]").TextContent, Is.EqualTo("Story"));
            Assert.That(Service.Current, Is.Null);
        });
    }

    [Test]
    public async Task Message_WinsOverAPostedMessage()
    {
        Service.Show("Posted");

        var cut = RenderMessage();
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=snackbar-message]").Select(e => e.TextContent), Is.EqualTo(new[] { "Set 2 deleted" }));
    }

    [Test]
    public async Task Disposed_StopsFollowingTheService()
    {
        var cut = Context.Render<Snackbar>();
        await cut.Instance.DisposeAsync();

        await Context.Renderer.Dispatcher.InvokeAsync(() => Service.Show("Entry added"));

        Assert.That(cut.Markup.Trim(), Is.Empty);
    }

    private IRenderedComponent<Snackbar> RenderMessage(Action? onAction = null, Action? onTimeout = null, bool takeFocus = false) =>
        Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.ActionLabel, "Undo")
            .Add(x => x.OnAction, onAction ?? (() => { }))
            .Add(x => x.OnTimeout, onTimeout ?? (() => { }))
            .Add(x => x.TakeFocus, takeFocus));

    /// <summary>The action answers whether it has keyboard focus (<c>:focus-visible</c>) with <paramref name="visible"/>.</summary>
    private void KeyboardFocus(bool visible) =>
        Context.JSInterop.SetupModule(SnackbarModule).Setup<bool>("isFocusVisible", _ => true).SetResult(visible);

    private sealed class FakeModuleRuntime : IJSRuntime
    {
        public FakeModule Module { get; } = new();

        public bool DisposeThrows { set => Module.Throws = value; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult((TValue)(object)Module);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromResult((TValue)(object)Module);
    }

    private sealed class FakeModule : IJSObjectReference
    {
        public int Disposals { get; private set; }

        public bool Throws { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult((TValue)(object)false);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult((TValue)(object)false);

        public ValueTask DisposeAsync()
        {
            Disposals++;
            return Throws ? ValueTask.FromException(new JSDisconnectedException("Gone")) : ValueTask.CompletedTask;
        }
    }

    private sealed class FailingImports : IJSRuntime
    {
        public int Imports { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Imports += identifier == "import" ? 1 : 0;
            return ValueTask.FromException<TValue>(new JSException("Import failed"));
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Imports += identifier == "import" ? 1 : 0;
            return ValueTask.FromException<TValue>(new JSException("Import failed"));
        }
    }
}
