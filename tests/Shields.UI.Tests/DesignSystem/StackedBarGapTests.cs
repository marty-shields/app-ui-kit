using System.Globalization;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.DesignSystem;

public class StackedBarGapTests
{
    private static readonly DataSeries[] All = Enum.GetValues<DataSeries>();

    private static IEnumerable<TestCaseData> Pairs() =>
        Enum.GetValues<ColourScheme>().SelectMany(scheme => All.SelectMany((a, i) => All.Skip(i + 1).Select(b =>
            new TestCaseData(a, b, scheme).SetArgDisplayNames($"{a} and {b} ({scheme})"))));

    // Any two series can sit side by side, and none of the pairs reaches 3:1, so StackedBar separates segments with a gap.
    [TestCaseSource(nameof(Pairs))]
    public void NeighbouringSeries_AreUnderThreeToOne_SoSegmentsNeedAGap(DataSeries first, DataSeries second, ColourScheme scheme)
    {
        var result = ContrastCheck.Measure(first.Token(), second.Token(), ContrastCheck.LargeTextOrControl, scheme);

        Assert.That(result.Passes, Is.False, result.ToString());
    }

    [Test]
    public void Gap_IsDeclaredAndNotZero()
    {
        var gap = KitCss.Tokens.Split("--stacked-bar-gap:")[1].Split(';')[0].Trim();

        Assert.That(double.Parse(gap.Replace("rem", "").Replace("px", ""), CultureInfo.InvariantCulture), Is.GreaterThan(0), gap);
    }
}
