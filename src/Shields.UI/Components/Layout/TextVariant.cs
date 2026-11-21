namespace Shields.UI.Components.Layout;

/// <summary>Look of a <see cref="Typography"/>.</summary>
public enum TextVariant
{
    /// <summary>Normal text in the normal text colour at the surrounding size.</summary>
    Body,

    /// <summary>Section label: muted, small, bold, upper case, with letter spacing.</summary>
    Label,

    /// <summary>Muted text at the surrounding size.</summary>
    Muted,

    /// <summary>Muted text in the small size.</summary>
    MutedSmall,

    /// <summary>Bold text in the normal text colour, also inside <see cref="Muted"/> text.</summary>
    Strong,

    /// <summary>Large bold figure.</summary>
    Figure,

    /// <summary>Text in the error colour.</summary>
    Error,
}
