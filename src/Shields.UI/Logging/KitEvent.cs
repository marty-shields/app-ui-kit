namespace Shields.UI.Logging;

/// <summary>
/// Event ids of the kit's log messages. Each keeps the number the apps logged it under, so ids sit in the apps' 2000–4999 range
/// at their old block. Values never change once logged; ids are never reused.
/// </summary>
internal enum KitEvent
{
    /// <summary><see cref="Navigation.BackButton"/>: back handled by the page.</summary>
    BackHandled = 2201,

    /// <summary><see cref="Navigation.BackButton"/>: back pressed with no handler.</summary>
    BackNotHandled = 2202,

    /// <summary><see cref="Navigation.BackButton"/>: a back handler failed.</summary>
    BackHandlerFailed = 2203,

    /// <summary><see cref="Lifecycle.AppLifecycle"/>: an app start handler failed.</summary>
    AppStartHandlerFailed = 3201,

    /// <summary><see cref="Components.Feedback.LoadFailed"/>: focusing Try again failed.</summary>
    LoadFailedFocusFailed = 3301,

    /// <summary><see cref="Components.Feedback.Snackbar"/>: focusing the action failed. Workout Log's id; Calorie Tracker uses 3901 for something else, in its own app.</summary>
    UndoFocusFailed = 3901,

    /// <summary><see cref="Components.Feedback.Snackbar"/>: checking the action for keyboard focus failed, so the timer runs on. Workout Log's id; Calorie Tracker uses 3902 for something else, in its own app.</summary>
    UndoFocusCheckFailed = 3902,

    /// <summary><see cref="Components.Inputs.NumberField"/>: focusing the input on show failed. Calorie Tracker's id; Workout Log uses 3701 for something else, in its own app.</summary>
    NumberFieldFocusFailed = 3701,
}
