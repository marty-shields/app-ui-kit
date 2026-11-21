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

    /// <summary>A short dashed line in <c>--color-text-muted</c> with the dash and gap of <c>--chart-dash</c>, the same as the chart's target line.</summary>
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

    /// <summary>
    /// A small <c>--color-accent</c> box with a ✓ in <c>--color-on-primary</c>: the on-target cell of <see cref="MonthCalendar"/> in miniature,
    /// so the mark has a legend entry.
    /// </summary>
    Check,

    /// <summary>A small <c>--color-error</c> box with a ▲ in <c>--color-on-danger</c>: the over cell of <see cref="MonthCalendar"/> in miniature.</summary>
    Triangle,
}
