using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Feedback;

public partial class LoadFailed
{
    private static partial class Log
    {
        /// <summary>Logs a failed focus on Try again; the error stays as it is.</summary>
        [LoggerMessage((int)KitEvent.LoadFailedFocusFailed, LogLevel.Warning, "Focusing Try again failed")]
        public static partial void RetryFocusFailed(ILogger logger, Exception exception);
    }
}
