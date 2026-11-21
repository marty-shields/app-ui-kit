namespace Shields.UI.Logging;

/// <summary>
/// Event ids of the kit's log messages. The kit's own range is 9000–9999, grouped by component in steps of 100
/// (start-up 90xx, screen 91xx, back 92xx, and so on). Values never change once logged; ids are never reused.
/// </summary>
public enum KitEvent
{
    /// <summary><see cref="Components.Layout.StartupGate"/>: start-up work finished.</summary>
    StartupFinished = 9001,

    /// <summary><see cref="Components.Layout.StartupGate"/>: start-up work failed.</summary>
    StartupFailed = 9002,

    /// <summary><see cref="Components.Layout.AppErrorBoundary"/>: unhandled error in a screen.</summary>
    ScreenError = 9101,

    /// <summary><see cref="Navigation.BackButton"/>: back handled by the page.</summary>
    BackHandled = 9201,

    /// <summary><see cref="Navigation.BackButton"/>: back pressed with no handler.</summary>
    BackNotHandled = 9202,

    /// <summary><see cref="Navigation.BackButton"/>: a back handler failed.</summary>
    BackHandlerFailed = 9203,

    /// <summary><see cref="Lifecycle.AppLifecycle"/>: an app start handler failed.</summary>
    AppStartHandlerFailed = 9301,

    /// <summary><see cref="Components.Feedback.LoadFailed"/>: focusing Try again failed.</summary>
    LoadFailedFocusFailed = 9401,

    /// <summary><see cref="Components.Feedback.Snackbar"/>: focusing the action failed.</summary>
    UndoFocusFailed = 9501,

    /// <summary><see cref="Components.Feedback.Snackbar"/>: checking the action for keyboard focus failed, so the timer runs on.</summary>
    UndoFocusCheckFailed = 9502,

    /// <summary><see cref="Components.Inputs.NumberField"/>: focusing the input on show failed.</summary>
    NumberFieldFocusFailed = 9601,

    /// <summary><see cref="Components.Layout.AppScreen"/>: setting up pull to refresh failed.</summary>
    PullRefreshFailed = 9701,

    /// <summary><see cref="Components.Overlays.Sheet"/>: loading or running the script that moves focus into the sheet or back to its opener failed.</summary>
    SheetFocusFailed = 9801,
}
