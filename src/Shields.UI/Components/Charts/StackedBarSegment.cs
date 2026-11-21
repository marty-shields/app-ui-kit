namespace Shields.UI.Components.Charts;

/// <summary>One segment of a <see cref="StackedBar"/>.</summary>
/// <param name="Label">The segment's name, shown in the labels and in the text alternative.</param>
/// <param name="Value">Sets the segment's share of the bar. Negative and non-finite values count as zero and draw no segment.</param>
/// <param name="Series">The data palette colour of the segment and its label swatch. Give neighbouring segments different series.</param>
public sealed record StackedBarSegment(string Label, double Value, DataSeries Series);
