namespace Shields.UI.Components.Charts;

/// <summary>One row of a <see cref="BarList"/>.</summary>
/// <param name="Label">The row's name.</param>
/// <param name="Value">Sets the bar's length against the list's largest value. Negative and non-finite values count as zero.</param>
/// <param name="ValueText">What to show at the end of the row, when not the number with thousands separators and no decimals.</param>
public sealed record BarListItem(string Label, double Value, string? ValueText = null);
