using Microsoft.Extensions.Logging;
using Shields.UI.Lifecycle;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Lifecycle;

public class AppLifecycleTests
{
    private RecordingLogger<AppLifecycle> logger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        logger = new RecordingLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(logger);
    }

    [Test]
    public async Task NotifyStarted_RunsEveryHandlerInOrder()
    {
        var calls = new List<int>();
        lifecycle.Register(() => Run(() => calls.Add(1)));
        lifecycle.Register(() => Run(() => calls.Add(2)));

        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task NotifyStarted_WithNoHandler_LogsNothing()
    {
        await lifecycle.NotifyStartedAsync();

        Assert.That(logger.Records.Count, Is.Zero);
    }

    [Test]
    public async Task Disposed_HandlerNoLongerRuns()
    {
        var calls = 0;
        var registration = lifecycle.Register(() => Run(() => calls++));

        registration.Dispose();
        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.Zero);
    }

    [Test]
    public async Task Attach_RemovesTheEarlierActivitysHandlers()
    {
        var calls = 0;
        lifecycle.Attach(new object());
        lifecycle.Register(() => Run(() => calls++));
        lifecycle.Register(() => Run(() => calls++));

        lifecycle.Attach(new object());
        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.Zero);
    }

    [Test]
    public async Task Detach_CurrentActivity_RemovesEveryHandler()
    {
        var calls = 0;
        var activity = new object();
        lifecycle.Attach(activity);
        lifecycle.Register(() => Run(() => calls++));
        lifecycle.Register(() => Run(() => calls++));

        lifecycle.Detach(activity);
        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.Zero);
    }

    [Test]
    public async Task Detach_EarlierActivity_KeepsTheCurrentActivitysHandlers()
    {
        var calls = 0;
        var earlier = new object();
        lifecycle.Attach(earlier);
        lifecycle.Attach(new object());
        lifecycle.Register(() => Run(() => calls++));

        lifecycle.Detach(earlier);
        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public async Task HandlerThrows_LogsAnErrorAndRunsTheOthers()
    {
        var calls = 0;
        var failure = new InvalidOperationException("Reload failed");
        lifecycle.Register(() => throw failure);
        lifecycle.Register(() => Run(() => calls++));

        await lifecycle.NotifyStartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.AppStartHandlerFailed, LogLevel.Error, "App start handler failed") }));
            Assert.That(logger.LastException, Is.SameAs(failure));
        });
    }

    [Test]
    public async Task HandlerReturnsAFaultedTask_LogsAnErrorAndRunsTheOthers()
    {
        var calls = 0;
        var failure = new InvalidOperationException("Reload failed");
        lifecycle.Register(() => Task.FromException(failure));
        lifecycle.Register(() => Run(() => calls++));

        await lifecycle.NotifyStartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.AppStartHandlerFailed, LogLevel.Error, "App start handler failed") }));
            Assert.That(logger.LastException, Is.SameAs(failure));
        });
    }

    [Test]
    public async Task HandlerDisposedByAnEarlierOne_StillRunsThisTimeOnly()
    {
        var calls = 0;
        IDisposable? second = null;
        lifecycle.Register(() => Run(() => second!.Dispose()));
        second = lifecycle.Register(() => Run(() => calls++));

        await lifecycle.NotifyStartedAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(calls, Is.EqualTo(1));
    }

    private static Task Run(Action action)
    {
        action();
        return Task.CompletedTask;
    }
}
