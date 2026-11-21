namespace Shields.UI.Navigation;

/// <summary>
/// Joins the host's current Android activity to the scoped <see cref="IBackButton"/> the pages use. The activity lives outside the BlazorWebView's scope, so it cannot resolve that instance.
/// </summary>
/// <remarks>
/// Registered as a singleton by <c>AddShieldsUi</c>. The host calls <see cref="Attach"/> and <see cref="Detach"/> from its activity and turns its back callback on and off with <see cref="Enabled"/>:
/// without a handler Android handles back and the app closes. <c>HostBridges</c> connects the scoped back button before any page registers.
/// Android calls arrive on the UI thread and Blazor runs on its own dispatcher, so state is guarded by a lock that is also held while the scoped instance is told about the activity, so the bridge and the scope never disagree; <see cref="EnabledChanged"/> is raised outside it.
/// It follows the scope that connected last, so it serves a single BlazorWebView, not Blazor Server with many circuits.
/// </remarks>
public sealed class BackButtonBridge
{
    private readonly Lock gate = new();
    private object? current;
    private IBackButton? backButton;

    /// <summary>Raised when <see cref="Enabled"/> may have changed; on any thread.</summary>
    public event Action? EnabledChanged;

    /// <summary>True when an activity is current and a page registered a back handler.</summary>
    public bool Enabled
    {
        get
        {
            bool hasActivity;
            IBackButton? scoped;
            lock (gate)
            {
                hasActivity = current is not null;
                scoped = backButton;
            }

            return hasActivity && scoped?.HasHandler == true;
        }
    }

    /// <summary>Makes a created activity current, which replaces any earlier one and removes its handlers.</summary>
    public void Attach(object activity)
    {
        lock (gate)
        {
            current = activity;
            if (backButton is { } scoped)
            {
                scoped.Changed -= Update;
                scoped.Attach(activity);
                Follow(scoped);
            }
        }

        Update();
    }

    /// <summary>Follows the back button of the scope the pages use, and makes the current activity current there. Connecting the same instance again does nothing.</summary>
    public void Connect(IBackButton scoped)
    {
        lock (gate)
        {
            if (ReferenceEquals(backButton, scoped))
            {
                return;
            }

            if (backButton is not null)
            {
                backButton.Changed -= Update;
            }

            backButton = scoped;
            if (current is not null)
            {
                scoped.Attach(current);
            }

            Follow(scoped);
        }

        Update();
    }

    /// <summary>Stops following the connected back button; ignored for any other instance.</summary>
    public void Disconnect(IBackButton scoped)
    {
        lock (gate)
        {
            if (!ReferenceEquals(backButton, scoped))
            {
                return;
            }

            backButton = null;
            scoped.Changed -= Update;
        }

        Update();
    }

    /// <summary>
    /// Removes the handlers and stops following the back button when the current activity is destroyed: its pages are not disposed with it,
    /// and a later activity must not reach them. An earlier activity is ignored.
    /// </summary>
    public void Detach(object activity)
    {
        lock (gate)
        {
            if (!ReferenceEquals(activity, current))
            {
                return;
            }

            current = null;
            if (backButton is { } scoped)
            {
                backButton = null;
                scoped.Changed -= Update;
                scoped.Detach(activity);
            }
        }

        Update();
    }

    /// <summary>Runs the newest handler. Null when there is none, so the host lets Android handle back: the handler can be removed between the gesture starting and back being pressed.</summary>
    public Task? HandleAsync()
    {
        IBackButton? scoped;
        lock (gate)
        {
            scoped = backButton;
        }

        return scoped is { HasHandler: true } ? scoped.HandleAsync() : null;
    }

    // Called with the lock held, after the scoped call that may raise Changed, so that event does not raise EnabledChanged under the lock.
    // A re-entrant Detach or Connect during that call has already replaced the scope, which must then stay unsubscribed.
    private void Follow(IBackButton scoped)
    {
        if (ReferenceEquals(backButton, scoped))
        {
            scoped.Changed += Update;
        }
    }

    private void Update() => EnabledChanged?.Invoke();
}
