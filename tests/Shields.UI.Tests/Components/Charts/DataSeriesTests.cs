using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class DataSeriesTests
{
    [TestCase(DataSeries.One, "--color-data-1", "series-1")]
    [TestCase(DataSeries.Two, "--color-data-2", "series-2")]
    [TestCase(DataSeries.Three, "--color-data-3", "series-3")]
    [TestCase(DataSeries.Four, "--color-data-4", "series-4")]
    [TestCase(DataSeries.Five, "--color-data-5", "series-5")]
    public void Series_MapsToItsTokenAndClass(DataSeries series, string token, string cssClass)
    {
        Assert.Multiple(() =>
        {
            Assert.That(series.Token(), Is.EqualTo(token));
            Assert.That(series.CssClass(), Is.EqualTo(cssClass));
            Assert.That(DesignTokens.Names, Does.Contain(series.Token()));
        });
    }

    [Test]
    public void Series_CoverEveryPaletteToken() =>
        Assert.That(Enum.GetValues<DataSeries>().Select(s => s.Token()),
            Is.EqualTo(DesignTokens.Names.Where(n => n.StartsWith("--color-data-")).Order()));

    [Test]
    public void UnknownSeries_IsRejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ((DataSeries)9).Token());
}
