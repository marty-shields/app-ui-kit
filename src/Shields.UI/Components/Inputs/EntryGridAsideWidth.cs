namespace Shields.UI.Components.Inputs;

/// <summary>The width of the aside column of an <see cref="EntryGridRow"/>.</summary>
public enum EntryGridAsideWidth
{
    /// <summary>Fits a short text such as 62.5×6: <c>--entry-grid-aside-width</c>.</summary>
    Normal,

    /// <summary>Fits a longer text such as 57.25kg×12: <c>--entry-grid-aside-width-wide</c>. The fields share less room.</summary>
    Wide,
}
