namespace Shields.UI.Components.Charts;

/// <summary>One segment of a <see cref="StackedBar"/>.</summary>
/// <param name="Label">The segment's name, shown in the labels and in the text alternative.</param>
/// <param name="Value">Sets the segment's share of the bar. Negative and non-finite values count as zero and draw no segment.</param>
/// <param name="Series">The data palette colour of the segment and its label swatch. Give neighbouring segments different series.</param>
/// <param name="ValueText">Replaces the formatted <paramref name="Value"/> and <c>Unit</c> in the segment's visible value and in the text alternative, so a segment can be sized in one unit and labelled in another, such as "32 g". <paramref name="Value"/> still sizes the segment and sets its percentage. The formatted value when null.</param>
public sealed record StackedBarSegment(string Label, double Value, DataSeries Series, string? ValueText = null);
