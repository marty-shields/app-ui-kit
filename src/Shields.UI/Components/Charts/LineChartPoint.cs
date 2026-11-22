namespace Shields.UI.Components.Charts;

/// <summary>One point on a <see cref="LineChart"/>.</summary>
/// <param name="XLabel">Text under the x-axis for the first and last point, such as 20 Oct, and the point's lead in the text alternative.</param>
/// <param name="Value">Value plotted; null skips the point and the line joins across.</param>
/// <param name="Label">Text over the point when it is the best or the latest, such as ×6; the formatted value when null.</param>
public sealed record LineChartPoint(string XLabel, decimal? Value, string? Label = null);
