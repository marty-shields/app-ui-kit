namespace Shields.UI.Lifecycle;

/// <summary>Runs the handlers registered through <see cref="IAppStart"/> when the activity starts, and removes them when it's destroyed or replaced. Used by the host.</summary>
/// <remarks>Registered scoped. The host must resolve it from the BlazorWebView's (or circuit's) scope, the one the pages use. The root provider holds a separate instance with no handlers, and throws under scope validation.</remarks>
public interface IAppLifecycle
{
    /// <summary>Removes any earlier activity's handlers and makes this activity current. Called when an activity is created, before its pages.</summary>
    void Attach(object activity);

    /// <summary>Runs every registered handler. Called when the activity starts.</summary>
    Task NotifyStartedAsync();

    /// <summary>Removes every handler if the activity is current. Called when an activity, and the pages in it, are destroyed.</summary>
    void Detach(object activity);
}
