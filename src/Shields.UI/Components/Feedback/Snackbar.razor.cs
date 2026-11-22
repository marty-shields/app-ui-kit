using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Feedback;

public partial class Snackbar
{
    private static partial class Log
    {
        /// <summary>Logs a failed focus on the action; the snackbar stays as it is.</summary>
        [LoggerMessage((int)KitEvent.UndoFocusFailed, LogLevel.Warning, "Focusing the snackbar action failed")]
        public static partial void ActionFocusFailed(ILogger logger, Exception exception);

        /// <summary>Logs a failed check for keyboard focus on the action; the timer runs on.</summary>
        [LoggerMessage((int)KitEvent.UndoFocusCheckFailed, LogLevel.Warning, "Checking the snackbar action for keyboard focus failed")]
        public static partial void FocusCheckFailed(ILogger logger, Exception exception);
    }
}
