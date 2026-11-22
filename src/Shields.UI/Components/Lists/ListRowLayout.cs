namespace Shields.UI.Components.Lists;

/// <summary>How a <see cref="ListRow"/> arranges its text, detail and aside.</summary>
public enum ListRowLayout
{
    /// <summary>The text with the detail under it, and the muted aside at the right.</summary>
    Stacked,

    /// <summary>A bold text at the left and a bold aside at the right, both in the normal text colour on one line, with the muted detail on a full-width line under them.</summary>
    Headline,
}
