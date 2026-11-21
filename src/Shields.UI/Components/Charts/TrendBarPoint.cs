namespace Shields.UI.Components.Charts;

/// <summary>One bar of a <see cref="TrendBarChart"/>.</summary>
/// <param name="Date">What <c>OnSelect</c> returns. For a month bar, any day of that month.</param>
/// <param name="Label">The short text under the bar, such as "Mon".</param>
/// <param name="Value">What the bar shows. Null, NaN, infinity, or zero or less, means nothing was recorded and leaves an empty slot.</param>
/// <param name="Target">The target this bar is judged against, when it differs from the chart's. It sets the bar's status and the scale height; the band and line show the chart's target only.</param>
/// <param name="Note">Marks the bar as resting on little data, such as "based on 2 days": the bar is drawn faded and the note joins its text alternative. Null for a normal bar.</param>
/// <param name="Current">Marks the bar of the current period: its axis label is bold, and with <see cref="TrendBarStyle.PastAndCurrent"/> its text alternative ends with "current", and, unless the chart is paired (some bar has an <paramref name="Earlier"/> value), its bar is in the accent colour and the others are past. In a paired chart every bar is accent whatever this is.</param>
/// <param name="Labelled">With <see cref="TrendBarStyle.PastAndCurrent"/>, prints the value above the bar.</param>
/// <param name="Earlier">With <see cref="TrendBarStyle.PastAndCurrent"/>, the value from the period before, drawn as a second bar left of this one when any bar has one. Same rule as <paramref name="Value"/> for nothing recorded.</param>
public sealed record TrendBarPoint(DateOnly Date, string Label, double? Value, double? Target = null, string? Note = null, bool Current = false, bool Labelled = false, double? Earlier = null);
