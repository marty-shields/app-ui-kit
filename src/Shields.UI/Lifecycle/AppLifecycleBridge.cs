namespace Shields.UI.Lifecycle;

/// <summary>
/// Joins the host's current Android activity to the scoped <see cref="IAppLifecycle"/> the pages use. The activity lives outside the BlazorWebView's scope, so it cannot resolve that instance.
/// </summary>
/// <remarks>
/// Registered as a singleton by <c>AddShieldsUi</c>. The host calls <see cref="Attach"/> when an activity is created, <see cref="StartedAsync"/> when it starts and <see cref="Detach"/> when it is destroyed.
/// <c>HostBridges</c> connects the scoped lifecycle before any page registers. State is guarded by a lock and the scoped instance is called outside it.
/// </remarks>
public sealed class AppLifecycleBridge
{
    private readonly Lock gate = new();
    private object? current;
    private IAppLifecycle? lifecycle;

    /// <summary>Makes a created activity current, which removes the earlier activity's handlers.</summary>
    public void Attach(object activity)
    {
        IAppLifecycle? scoped;
        lock (gate)
        {
            current = activity;
            scoped = lifecycle;
        }

        scoped?.Attach(activity);
    }

    /// <summary>Follows the lifecycle of the scope the pages use, and makes the current activity current there. Connecting the same instance again does nothing.</summary>
    public void Connect(IAppLifecycle scoped)
    {
        object? activity;
        lock (gate)
        {
            if (ReferenceEquals(lifecycle, scoped))
            {
                return;
            }

            lifecycle = scoped;
            activity = current;
        }

        if (activity is not null)
        {
            scoped.Attach(activity);
        }
    }

    /// <summary>Stops following the connected lifecycle; ignored for any other instance.</summary>
    public void Disconnect(IAppLifecycle scoped)
    {
        lock (gate)
        {
            if (ReferenceEquals(lifecycle, scoped))
            {
                lifecycle = null;
            }
        }
    }

    /// <summary>Runs the pages' start handlers. Does nothing before the pages exist: they run their own first load.</summary>
    public Task StartedAsync()
    {
        IAppLifecycle? scoped;
        lock (gate)
        {
            scoped = lifecycle;
        }

        return scoped?.NotifyStartedAsync() ?? Task.CompletedTask;
    }

    /// <summary>Removes the handlers and stops following the lifecycle when the current activity is destroyed: its pages are not disposed with it. An earlier activity is ignored.</summary>
    public void Detach(object activity)
    {
        IAppLifecycle? scoped;
        lock (gate)
        {
            if (!ReferenceEquals(activity, current))
            {
                return;
            }

            current = null;
            scoped = lifecycle;
            lifecycle = null;
        }

        scoped?.Detach(activity);
    }
}
