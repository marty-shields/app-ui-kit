namespace Shields.UI.Tests.DesignSystem;

public class DataPaletteTests
{
    // OKLab distance: about 0.02 is a just-noticeable difference, so 0.12 reads as a clearly different colour.
    private const double SeriesDistance = 0.12;
    private const double OtherColourDistance = 0.10;

    private static readonly string[] Series = ["--color-data-1", "--color-data-2", "--color-data-3", "--color-data-4", "--color-data-5"];
    private static readonly string[] Others = ["--color-error", "--color-success", "--color-chart-band", "--color-accent"];

    private static IEnumerable<TestCaseData> Pairs() =>
        Enum.GetValues<ColourScheme>().SelectMany(scheme => Series.SelectMany((a, i) => Series.Skip(i + 1).Select(b =>
            new TestCaseData(a, b, scheme).SetArgDisplayNames($"{a} and {b} ({scheme})"))));

    private static IEnumerable<TestCaseData> AgainstOthers() =>
        Enum.GetValues<ColourScheme>().SelectMany(scheme => Series.SelectMany(s => Others.Select(o =>
            new TestCaseData(s, o, scheme).SetArgDisplayNames($"{s} and {o} ({scheme})"))));

    [TestCaseSource(nameof(Pairs))]
    public void EveryPairOfSeries_IsClearlyDifferent(string first, string second, ColourScheme scheme)
    {
        var distance = Rgb.Distance(DesignTokens.Colour(first).In(scheme), DesignTokens.Colour(second).In(scheme));

        Assert.That(distance, Is.GreaterThanOrEqualTo(SeriesDistance), $"{first} and {second} ({scheme})");
    }

    [TestCaseSource(nameof(AgainstOthers))]
    public void Series_NeverReadsAsAStatusBandOrAccent(string series, string other, ColourScheme scheme)
    {
        var distance = Rgb.Distance(DesignTokens.Colour(series).In(scheme), DesignTokens.Colour(other).In(scheme));

        Assert.That(distance, Is.GreaterThanOrEqualTo(OtherColourDistance), $"{series} and {other} ({scheme})");
    }
}
