using System.Globalization;

namespace Shields.UI.Testing;

public enum ColourScheme
{
    Light,
    Dark,
}

/// <summary>An sRGB colour.</summary>
public readonly record struct Rgb(int R, int G, int B)
{
    /// <summary>Reads <c>#rrggbb</c>.</summary>
    public static Rgb Parse(string hex) => new(
        int.Parse(hex[1..3], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
        int.Parse(hex[3..5], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
        int.Parse(hex[5..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture));

    /// <summary>WCAG 2 relative luminance.</summary>
    public double Luminance => 0.2126 * Channel(R) + 0.7152 * Channel(G) + 0.0722 * Channel(B);

    /// <summary>WCAG 2 contrast ratio between two colours.</summary>
    public static double Contrast(Rgb a, Rgb b)
    {
        var (light, dark) = a.Luminance > b.Luminance ? (a.Luminance, b.Luminance) : (b.Luminance, a.Luminance);
        return (light + 0.05) / (dark + 0.05);
    }

    public override string ToString() => $"#{R:x2}{G:x2}{B:x2}";

    private static double Channel(int value)
    {
        var c = value / 255.0;
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}

/// <summary>A colour token's value in each theme.</summary>
public readonly record struct ThemedColour(Rgb Light, Rgb Dark)
{
    public Rgb In(ColourScheme scheme) => scheme == ColourScheme.Light ? Light : Dark;
}
