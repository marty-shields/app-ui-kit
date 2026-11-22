using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Navigation;

public class BackButtonBridgeTests
{
    private readonly object activity = new();
    private BackButtonBridge bridge = null!;
    private BackButton scoped = null!;

    [SetUp]
    public void Create()
    {
        bridge = new BackButtonBridge();
        scoped = NewBackButton();
    }

    private static BackButton NewBackButton() => new(NullLogger<BackButton>.Instance);

    [Test]
    public void Connect_AfterAttach_MakesTheCurrentActivityCurrentInTheScope()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);
        scoped.Detach(activity);

        Assert.That(scoped.HasHandler, Is.False);
    }

    [Test]
    public void Connect_BeforeAttach_HandsTheActivityOnWhenItIsAttached()
    {
        var attached = 0;
        bridge.Connect(scoped);
        scoped.Attached += () => attached++;
        bridge.Attach(activity);

        Assert.That(attached, Is.EqualTo(1));
    }

    [Test]
    public void Attach_RemovesTheEarlierHandlers()
    {
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        bridge.Attach(activity);

        Assert.That(scoped.HasHandler, Is.False);
    }

    [Test]
    public void Enabled_IsFalseWithNoActivityEvenWithAHandler()
    {
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        Assert.That(bridge.Enabled, Is.False);
    }

    [Test]
    public void Enabled_FollowsTheHandlersAndRaisesEnabledChanged()
    {
        var raised = 0;
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.EnabledChanged += () => raised++;

        var registration = scoped.Register(() => Task.CompletedTask);
        var onWithHandler = bridge.Enabled;
        registration.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(onWithHandler, Is.True);
            Assert.That(bridge.Enabled, Is.False);
            Assert.That(raised, Is.EqualTo(2));
        });
    }

    [Test]
    public void Connect_UnsubscribesThePreviousScope()
    {
        var other = NewBackButton();
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.Connect(other);
        var raised = 0;
        bridge.EnabledChanged += () => raised++;

        scoped.Register(() => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(raised, Is.Zero);
            Assert.That(bridge.Enabled, Is.False);
        });
    }

    [Test]
    public void Disconnect_OfAStaleScope_KeepsTheNewOne()
    {
        var current = NewBackButton();
        bridge.Attach(activity);
        bridge.Connect(scoped);
        bridge.Connect(current);

        bridge.Disconnect(scoped);
        current.Register(() => Task.CompletedTask);

        Assert.That(bridge.Enabled, Is.True);
    }

    [Test]
    public void Disconnect_OfTheConnectedScope_TurnsBackOff()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        bridge.Disconnect(scoped);

        Assert.That(bridge.Enabled, Is.False);
    }

    [Test]
    public void Detach_OfAnEarlierActivity_IsIgnored()
    {
        bridge.Attach(new object());
        bridge.Attach(activity);
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        bridge.Detach(new object());

        Assert.That(bridge.Enabled, Is.True);
    }

    [Test]
    public void Detach_OfTheCurrentActivity_DisconnectsTheScopeSoALaterActivityNeverReachesIt()
    {
        var raised = 0;
        bridge.Attach(activity);
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        bridge.Detach(activity);
        bridge.EnabledChanged += () => raised++;
        bridge.Attach(new object());
        scoped.Register(() => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(scoped.HasHandler, Is.True, "the new activity must not clear the old scope");
            Assert.That(bridge.Enabled, Is.False);
            Assert.That(raised, Is.EqualTo(1), "only the attach raises; the old scope is no longer followed");
        });
    }

    [Test]
    public async Task HandleAsync_RunsTheNewestHandlerOrReturnsNullWithNone()
    {
        var ran = false;
        bridge.Attach(activity);
        bridge.Connect(scoped);
        var none = bridge.HandleAsync();
        scoped.Register(() =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        var handled = bridge.HandleAsync();
        await handled!;

        Assert.Multiple(() =>
        {
            Assert.That(none, Is.Null);
            Assert.That(ran, Is.True);
        });
    }

    [Test]
    public void Connect_TheSameInstanceAgain_KeepsItsHandlersAndSubscribesOnce()
    {
        var raised = 0;
        bridge.Attach(activity);
        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);
        bridge.EnabledChanged += () => raised++;

        bridge.Connect(scoped);
        scoped.Register(() => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(bridge.Enabled, Is.True, "a second Connect must not attach again and clear the handler");
            Assert.That(raised, Is.EqualTo(1));
        });
    }

    [Test]
    public void EnabledChanged_IsRaisedOutsideTheBridgesLock()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        var enabledSeenFromAnotherThread = false;
        var finished = false;
        bridge.EnabledChanged += () =>
        {
            var read = Task.Run(() => bridge.Enabled);
            finished = read.Wait(TimeSpan.FromSeconds(5));
            enabledSeenFromAnotherThread = finished && read.Result;
        };

        scoped.Register(() => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(finished, Is.True, "the read on another thread must not wait for the bridge's lock");
            Assert.That(enabledSeenFromAnotherThread, Is.True);
        });
    }

    [Test]
    public void Connect_RaisesEnabledChanged()
    {
        var raised = 0;
        bridge.EnabledChanged += () => raised++;

        bridge.Connect(scoped);

        Assert.That(raised, Is.EqualTo(1));
    }

    [Test]
    public void Disconnect_RaisesEnabledChangedAndStopsFollowingTheScope()
    {
        bridge.Attach(activity);
        bridge.Connect(scoped);
        var raised = 0;
        bridge.EnabledChanged += () => raised++;

        bridge.Disconnect(scoped);
        var afterDisconnect = raised;
        scoped.Register(() => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(afterDisconnect, Is.EqualTo(1));
            Assert.That(raised, Is.EqualTo(1), "a handler registered after disconnect must not reach the bridge");
        });
    }

    [Test]
    public void Connect_WhenAnActivityIsDetachedWhileAttaching_DoesNotFollowTheScope()
    {
        var reentrant = new ReentrantBackButton(bridge);
        bridge.Attach(activity);

        bridge.Connect(reentrant);

        Assert.That(reentrant.Subscribers, Is.Zero, "the detach during connect dropped the scope, so it must not stay subscribed");
    }

    private sealed class ReentrantBackButton(BackButtonBridge bridge) : IBackButton
    {
        private Action? changed;

        public int Subscribers => changed?.GetInvocationList().Length ?? 0;

        public bool HasHandler => false;

        public event Action? Changed
        {
            add => changed += value;
            remove => changed -= value;
        }

        public IDisposable Register(Func<Task> handler) => throw new NotSupportedException();

        public Task HandleAsync() => Task.CompletedTask;

        public void Attach(object activity) => bridge.Detach(activity);

        public void Detach(object activity)
        {
        }
    }
}
