using Microsoft.Extensions.Logging;
using Shields.UI.Logging;

namespace Shields.UI.Components.Inputs;

public partial class NumberField
{
    private static partial class Log
    {
        /// <summary>Logs a failed focus on show; the field works without it.</summary>
        [LoggerMessage((int)KitEvent.NumberFieldFocusFailed, LogLevel.Warning, "Focusing the number field failed")]
        public static partial void AutoFocusFailed(ILogger logger, Exception exception);
    }
}
