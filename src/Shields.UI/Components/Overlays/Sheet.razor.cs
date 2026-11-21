using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Overlays;

public partial class Sheet
{
    private static partial class Log
    {
        /// <summary>Logs a failure to load or run the focus script, which moves focus into the sheet or back to the opener; the sheet stays as it is.</summary>
        [LoggerMessage((int)KitEvent.SheetFocusFailed, LogLevel.Warning, "Loading or running the sheet focus script failed")]
        public static partial void FocusFailed(ILogger logger, Exception exception);
    }
}
