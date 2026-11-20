using System.Globalization;

namespace Shields.UI.Testing;

/// <summary>One foreground and background token pair measured in one theme.</summary>
public readonly record struct ContrastResult(string Foreground, string Background, ColourScheme Scheme, Rgb ForegroundColour, Rgb BackgroundColour, double Ratio, double Minimum)
{
    public bool Passes => Ratio >= Minimum;

    public override string ToString() =>
        $"{Foreground} {ForegroundColour} on {Background} {BackgroundColour} ({Scheme}): {Ratio.ToString("0.00", CultureInfo.InvariantCulture)}, needs {Minimum.ToString(CultureInfo.InvariantCulture)}";
}

/// <summary>WCAG contrast of a pair of colour tokens.</summary>
public static class ContrastCheck
{
    /// <summary>WCAG AA minimum for text.</summary>
    public const double Text = 4.5;

    /// <summary>WCAG AA minimum for large text and controls.</summary>
    public const double LargeTextOrControl = 3.0;

    /// <summary>Measures a pair of the kit's colour tokens.</summary>
    public static ContrastResult Measure(string foreground, string background, double minimum, ColourScheme scheme) =>
        Measure(KitCss.Tokens, foreground, background, minimum, scheme);

    /// <summary>Measures a pair of colour tokens declared in a piece of CSS, such as an app's own tokens.css. A token that CSS does not declare is read from the kit's tokens.css.</summary>
    public static ContrastResult Measure(string tokensCss, string foreground, string background, double minimum, ColourScheme scheme)
    {
        var fg = Colour(tokensCss, foreground).In(scheme);
        var bg = Colour(tokensCss, background).In(scheme);
        return new(foreground, background, scheme, fg, bg, Rgb.Contrast(fg, bg), minimum);
    }

    private static ThemedColour Colour(string css, string name) =>
        DesignTokens.Declares(css).Contains(name) ? DesignTokens.Colour(css, name) : DesignTokens.Colour(name);
}
