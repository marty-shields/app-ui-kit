namespace Shields.UI.Components.Charts;

public enum LegendSwatch
{
    /// <summary>A filled dot in <c>--color-accent</c>.</summary>
    Accent,

    /// <summary>A filled dot in <c>--color-chip</c>.</summary>
    Chip,

    /// <summary>A filled dot in <c>--color-error</c>.</summary>
    Error,

    /// <summary>A dashed ring in <c>--color-border-strong</c>, for something absent.</summary>
    Outline,

    /// <summary>A short dashed line in <c>--color-text-muted</c>, for a reference line.</summary>
    Line,

    /// <summary>
    /// A see-through filled rectangle in <c>--color-chart-band</c>, for a target band. Apps draw their band with the same
    /// token so the legend matches the chart.
    /// </summary>
    Band,
}
