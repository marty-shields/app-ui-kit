namespace Shields.UI.Tests.DesignSystem;

public class DataPaletteTests
{
    // OKLab distance: about 0.02 is a just-noticeable difference, so 0.12 reads as a clearly different colour.
    private const double NeighbourDistance = 0.12;
    private const double ErrorDistance = 0.10;

    private static readonly string[] Series = ["--color-data-1", "--color-data-2", "--color-data-3", "--color-data-4", "--color-data-5"];

    private static IEnumerable<TestCaseData> Neighbours() =>
        Enum.GetValues<ColourScheme>().SelectMany(scheme => Series.Zip(Series.Skip(1), (a, b) =>
            new TestCaseData(a, b, scheme).SetArgDisplayNames($"{a} and {b} ({scheme})")));

    private static IEnumerable<TestCaseData> AgainstError() =>
        Enum.GetValues<ColourScheme>().SelectMany(scheme => Series.Select(s =>
            new TestCaseData(s, scheme).SetArgDisplayNames($"{s} and --color-error ({scheme})")));

    [TestCaseSource(nameof(Neighbours))]
    public void NeighbouringSeries_AreClearlyDifferent(string first, string second, ColourScheme scheme)
    {
        var distance = OkLab.Distance(DesignTokens.Colour(first).In(scheme), DesignTokens.Colour(second).In(scheme));

        Assert.That(distance, Is.GreaterThanOrEqualTo(NeighbourDistance), $"{first} and {second} ({scheme})");
    }

    [TestCaseSource(nameof(AgainstError))]
    public void Series_NeverReadsAsAnError(string series, ColourScheme scheme)
    {
        var distance = OkLab.Distance(DesignTokens.Colour(series).In(scheme), DesignTokens.Colour("--color-error").In(scheme));

        Assert.That(distance, Is.GreaterThanOrEqualTo(ErrorDistance), $"{series} ({scheme})");
    }

    [Test]
    public void Distance_OfBlackAndWhite_IsOne() =>
        Assert.That(OkLab.Distance(new Rgb(0, 0, 0), new Rgb(255, 255, 255)), Is.EqualTo(1.0).Within(0.001));

    [Test]
    public void Distance_OfAColourToItself_IsZero() =>
        Assert.That(OkLab.Distance(new Rgb(47, 127, 134), new Rgb(47, 127, 134)), Is.Zero.Within(1e-9));
}

/// <summary>Distance between two colours in the OKLab space, where equal steps look about equally different.</summary>
internal static class OkLab
{
    public static double Distance(Rgb a, Rgb b)
    {
        var (l1, a1, b1) = From(a);
        var (l2, a2, b2) = From(b);
        return Math.Sqrt(Math.Pow(l1 - l2, 2) + Math.Pow(a1 - a2, 2) + Math.Pow(b1 - b2, 2));
    }

    private static (double L, double A, double B) From(Rgb colour)
    {
        var r = Linear(colour.R);
        var g = Linear(colour.G);
        var b = Linear(colour.B);
        var l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
        return (
            0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s);
    }

    private static double Linear(int value)
    {
        var c = value / 255.0;
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
