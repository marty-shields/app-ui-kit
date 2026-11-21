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

    /// <summary>
    /// Distance between two colours in the OKLab space, where equal steps look about equally different: about 0.02 is a
    /// just-noticeable difference and 0.12 reads as a clearly different colour. Black to white is 1.
    /// </summary>
    public static double Distance(Rgb a, Rgb b)
    {
        var (l1, a1, b1) = a.OkLab();
        var (l2, a2, b2) = b.OkLab();
        return Math.Sqrt(Math.Pow(l1 - l2, 2) + Math.Pow(a1 - a2, 2) + Math.Pow(b1 - b2, 2));
    }

    public override string ToString() => $"#{R:x2}{G:x2}{B:x2}";

    private (double L, double A, double B) OkLab()
    {
        var r = Channel(R);
        var g = Channel(G);
        var b = Channel(B);
        var l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
        return (
            0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s);
    }

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
