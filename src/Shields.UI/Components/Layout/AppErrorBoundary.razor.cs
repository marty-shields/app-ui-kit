using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Layout;

public partial class AppErrorBoundary
{
    private static partial class Log
    {
        [LoggerMessage((int)KitEvent.ScreenError, LogLevel.Error, "Unhandled error in a screen")]
        public static partial void Unhandled(ILogger logger, Exception exception);
    }
}
