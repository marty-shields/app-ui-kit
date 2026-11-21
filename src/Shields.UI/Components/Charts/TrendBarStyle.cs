namespace Shields.UI.Components.Charts;

/// <summary>How a <see cref="TrendBarChart"/> colours its bars.</summary>
public enum TrendBarStyle
{
    /// <summary>Each bar is coloured by how it compares with the target, with a marker above a bar that is over.</summary>
    Status,

    /// <summary>The bar of the current period (<see cref="TrendBarPoint.Current"/>) is in <c>--color-accent</c> and the others in <c>--color-chart-past</c>. There is no target, band or marker.</summary>
    PastAndCurrent,
}
