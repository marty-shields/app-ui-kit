namespace Shields.UI.Components.Charts;

/// <summary>How a <see cref="TrendBarChart"/> colours its bars.</summary>
public enum TrendBarStyle
{
    /// <summary>Each bar is coloured by how it compares with the target, with a marker above a bar that is over.</summary>
    Status,

    /// <summary>A bar of the current period is in <c>--color-accent</c> and a bar of an earlier period in <c>--color-chart-past</c>: without <see cref="TrendBarPoint.Earlier"/> values the bar with <see cref="TrendBarPoint.Current"/> is the accent one and the others are past; in a paired chart every bar's own value is accent and every earlier column past, and only the bold axis label marks <see cref="TrendBarPoint.Current"/>. There is no target, band or marker.</summary>
    PastAndCurrent,
}
