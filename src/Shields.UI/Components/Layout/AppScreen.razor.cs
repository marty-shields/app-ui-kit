using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Layout;

public partial class AppScreen
{
    private static partial class Log
    {
        [LoggerMessage((int)KitEvent.PullRefreshFailed, LogLevel.Warning, "Setting up pull to refresh failed")]
        public static partial void PullRefreshFailed(ILogger logger, Exception exception);
    }
}
