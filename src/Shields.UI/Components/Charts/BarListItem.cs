namespace Shields.UI.Components.Charts;

/// <summary>One row of a <see cref="BarList"/>.</summary>
/// <param name="Label">The row's name.</param>
/// <param name="Value">Sets the bar's length against the list's largest value. Negative and non-finite values fill no bar.</param>
/// <param name="ValueText">What to show at the end of the row, when not the value with thousands separators and no decimals (zero if not finite).</param>
public sealed record BarListItem(string Label, double Value, string? ValueText = null);
