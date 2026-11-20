namespace Shields.UI.Components.Actions;

/// <summary>Look of an <see cref="IconButton"/>.</summary>
public enum IconButtonStyle
{
    /// <summary>Icon only, on a transparent background.</summary>
    Plain,

    /// <summary>Icon on a round chip filled with <c>--color-chip</c>, for the main add action of a section.</summary>
    Filled,

    /// <summary>Icon on an outlined round chip: a <c>--color-chip-soft</c> fill (lighter than <see cref="Filled"/> in light, darker in dark) with a <c>--color-chip</c> border, for a secondary add action.</summary>
    Soft,
}
