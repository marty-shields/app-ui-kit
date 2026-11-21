namespace Shields.UI.Navigation;

/// <summary>Passes the Android back button to the page that registered for it.</summary>
/// <remarks>Registered scoped. The root provider holds a separate instance with no handlers, and throws under scope validation, so the host's activity uses <see cref="BackButtonBridge"/>, which <c>HostBridges</c> connects to the instance the pages use.</remarks>
public interface IBackButton
{
    /// <summary>True when a handler is registered, so back stays in the app.</summary>
    bool HasHandler { get; }

    /// <summary>Raised when a handler is registered or removed.</summary>
    event Action? Changed;

    /// <summary>
    /// Raised after every <see cref="Attach"/>, once the earlier handlers are removed, so pages still on screen can register again.
    /// They register through the renderer, so <see cref="HasHandler"/> can be briefly false after Attach, and they register in the order they subscribed.
    /// The default does nothing, so a host's own implementation keeps compiling.
    /// </summary>
    event Action? Attached
    {
        add { }
        remove { }
    }

    /// <summary>Registers a handler for back. The newest handler runs; disposing the result removes it.</summary>
    IDisposable Register(Func<Task> handler);

    /// <summary>Runs the newest handler. Does nothing when none is registered.</summary>
    Task HandleAsync();

    /// <summary>Removes any earlier activity's handlers and makes this activity current. Called when an activity is created, before its pages.</summary>
    void Attach(object activity);

    /// <summary>Removes every handler if the activity is current. Called when an activity, and the pages in it, are destroyed.</summary>
    void Detach(object activity);
}
