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
    /// An unfilled outlined box wider than tall, for a target band. Apps draw the band's top and bottom edges as strokes in
    /// <c>--color-chart-band</c> with no fill, so the legend matches the chart.
    /// </summary>
    Band,

    /// <summary>A filled dot in <c>--color-chart-past</c>, for bars of earlier periods next to the current one.</summary>
    Past,

    /// <summary>A filled dot in <c>--color-primary</c>.</summary>
    Primary,

    /// <summary>A solid ring in <c>--color-text</c>, the stroke of the month calendar's today ring.</summary>
    Today,
}
