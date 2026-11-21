using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Lifecycle;

namespace Shields.UI.Tests.Lifecycle;

public class AppLifecycleBridgeTests
{
    private readonly object activity = new();
    private AppLifecycleBridge bridge = null!;
    private AppLifecycle scoped = null!;
    private int started;

    [SetUp]
    public void Create()
    {
        bridge = new AppLifecycleBridge();
        scoped = NewLifecycle();
        started = 0;
    }

    private static AppLifecycle NewLifecycle() => new(NullLogger<AppLifecycle>.Instance);

    private void RegisterStart(AppLifecycle lifecycle) => lifecycle.Register(() =>
    {
        started++;
        return Task.CompletedTask;
    });

    [Test]
    public async Task Connect_AfterAttach_MakesTheCurrentActivityCurrentInTheScope()
    {
        bridge.Attach(activity);
        RegisterStart(scoped);
        bridge.Connect(scoped);
        await scoped.NotifyStartedAsync();
        var afterConnect = started;

        RegisterStart(scoped);
        bridge.Detach(activity);
        await scoped.NotifyStartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(afterConnect, Is.Zero, "connect attached the activity, so it cleared the earlier handler");
            Assert.That(started, Is.Zero, "detach reached the scope with the same activity, so it cleared the handler");
        });
    }

    [Test]
    public async Task Connect_BeforeAttach_HandsTheActivityOnWhenItIsAttached()
    {
        bridge.Connect(scoped);
        RegisterStart(scoped);

        bridge.Attach(activity);
        await scoped.NotifyStartedAsync();

        Assert.That(started, Is.Zero, "attach reached the connected scope and cleared its handler");
    }

    [Test]
    public async Task Connect_AgainAfterDisconnect_KeepsTheHandlersOfAScopeAlreadyAttached()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.Disconnect(scoped);
        RegisterStart(scoped);

        bridge.Connect(scoped);
        await scoped.NotifyStartedAsync();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public void Connect_WhenAnActivityIsDetachedWhileAttaching_DoesNotFollowTheScope()
    {
        var reentrant = new ReentrantLifecycle(bridge);
        bridge.Attach(activity);

        bridge.Connect(reentrant);
        bridge.Attach(new object());

        Assert.That(reentrant.Attached, Is.EqualTo(1), "the detach during connect dropped the scope, so the later attach never reached it");
    }

    private sealed class ReentrantLifecycle(AppLifecycleBridge bridge) : IAppLifecycle
    {
        public int Attached { get; private set; }

        public void Attach(object activity)
        {
            Attached++;
            bridge.Detach(activity);
        }

        public Task NotifyStartedAsync() => Task.CompletedTask;

        public void Detach(object activity)
        {
        }
    }

    [Test]
    public async Task StartedAsync_RunsTheScopesHandlers()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        RegisterStart(scoped);

        await bridge.StartedAsync();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public async Task StartedAsync_BeforeConnect_DoesNothing()
    {
        bridge.Attach(activity);

        await bridge.StartedAsync();

        Assert.That(started, Is.Zero);
    }

    [Test]
    public async Task Attach_RemovesTheEarlierHandlers()
    {
        bridge.Connect(scoped);
        RegisterStart(scoped);

        bridge.Attach(activity);
        await bridge.StartedAsync();

        Assert.That(started, Is.Zero);
    }

    [Test]
    public async Task Disconnect_OfAStaleScope_KeepsTheNewOne()
    {
        var current = NewLifecycle();
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.Connect(current);
        RegisterStart(current);

        bridge.Disconnect(scoped);
        await bridge.StartedAsync();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public async Task Disconnect_OfTheConnectedScope_StopsStarts()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        RegisterStart(scoped);

        bridge.Disconnect(scoped);
        await bridge.StartedAsync();

        Assert.That(started, Is.Zero);
    }

    [Test]
    public async Task Detach_OfAnEarlierActivity_IsIgnored()
    {
        bridge.Attach(new object());
        bridge.Attach(activity);
        bridge.Connect(scoped);
        RegisterStart(scoped);

        bridge.Detach(new object());
        await bridge.StartedAsync();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public async Task Detach_OfTheCurrentActivity_DisconnectsTheScopeSoALaterActivityNeverReachesIt()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.Detach(activity);

        bridge.Attach(new object());
        RegisterStart(scoped);
        await bridge.StartedAsync();

        Assert.That(started, Is.Zero, "the old scope is no longer followed, so it is neither attached nor started");
    }

    [Test]
    public async Task Connect_TheSameInstanceAgain_KeepsItsHandlers()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        RegisterStart(scoped);

        bridge.Connect(scoped);
        await bridge.StartedAsync();

        Assert.That(started, Is.EqualTo(1), "a second Connect must not attach again and clear the handler");
    }
}
