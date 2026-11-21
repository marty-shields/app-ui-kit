namespace Shields.UI.Components.Inputs;

/// <summary>What a <see cref="ValueField"/> asks for, which decides its control.</summary>
public enum ValueKind
{
    /// <summary>A whole number, on the numeric keyboard.</summary>
    Whole,

    /// <summary>A number with a decimal part, on the decimal keyboard.</summary>
    Fractional,

    /// <summary>A length of time as hours, minutes and seconds boxes.</summary>
    Duration,
}
