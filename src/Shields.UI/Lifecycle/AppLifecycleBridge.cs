namespace Shields.UI.Lifecycle;

/// <summary>
/// Joins the host's current Android activity to the scoped <see cref="IAppLifecycle"/> the pages use. The activity lives outside the BlazorWebView's scope, so it cannot resolve that instance.
/// </summary>
/// <remarks>
/// Registered as a singleton by <c>AddShieldsUi</c>. The host calls <see cref="Attach"/> when an activity is created, <see cref="StartedAsync"/> when it starts and <see cref="Detach"/> when it is destroyed.
/// <c>HostBridges</c> connects the scoped lifecycle before any page registers. State is guarded by a lock that is also held while the scoped instance is called, so the bridge and the scope never disagree.
/// Attaching clears the scope's handlers, so <c>HostBridges</c> must come first in the root component and exist once per scope. It follows the scope that connected last, so it serves a single BlazorWebView, not Blazor Server with many circuits.
/// </remarks>
public sealed class AppLifecycleBridge
{
    private readonly Lock gate = new();
    private object? current;
    private IAppLifecycle? lifecycle;
    private IAppLifecycle? attachedScope;
    private object? attachedActivity;

    /// <summary>Makes a created activity current, which removes the earlier activity's handlers.</summary>
    public void Attach(object activity)
    {
        lock (gate)
        {
            current = activity;
            if (lifecycle is { } scoped)
            {
                AttachScope(scoped, activity);
            }
        }
    }

    /// <summary>
    /// Follows the lifecycle of the scope the pages use, and makes the current activity current there unless it already is, which would clear handlers registered since.
    /// Connecting the same instance again does nothing.
    /// </summary>
    public void Connect(IAppLifecycle scoped)
    {
        lock (gate)
        {
            if (ReferenceEquals(lifecycle, scoped))
            {
                return;
            }

            lifecycle = scoped;
            if (current is not null && !(ReferenceEquals(attachedScope, scoped) && ReferenceEquals(attachedActivity, current)))
            {
                AttachScope(scoped, current);
            }
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
        lock (gate)
        {
            if (!ReferenceEquals(activity, current))
            {
                return;
            }

            current = null;
            var scoped = lifecycle;
            lifecycle = null;
            attachedScope = null;
            attachedActivity = null;
            scoped?.Detach(activity);
        }
    }

    private void AttachScope(IAppLifecycle scoped, object activity)
    {
        attachedScope = scoped;
        attachedActivity = activity;
        scoped.Attach(activity);
    }
}
