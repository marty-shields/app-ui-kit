namespace Shields.UI.Components.Charts;

/// <summary>One entry of a <see cref="ChartLegend"/>.</summary>
/// <param name="Swatch">Picks the dot.</param>
/// <param name="Label">The words next to the dot.</param>
/// <param name="Marker">A short glyph beside the dot, such as ▲, so colour is not the only cue. Null shows none.</param>
public sealed record LegendItem(LegendSwatch Swatch, string Label, string? Marker = null);
