using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Layout;

public partial class StartupGate
{
    private static partial class Log
    {
        [LoggerMessage((int)KitEvent.StartupFinished, LogLevel.Information, "Startup finished in {Milliseconds} ms")]
        public static partial void Started(ILogger logger, long milliseconds);

        [LoggerMessage((int)KitEvent.StartupFailed, LogLevel.Error, "Startup failed")]
        public static partial void Failed(ILogger logger, Exception exception);
    }
}
