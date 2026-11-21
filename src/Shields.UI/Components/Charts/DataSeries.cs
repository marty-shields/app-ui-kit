namespace Shields.UI.Components.Charts;

/// <summary>A series of the data palette, <c>--color-data-1</c> to <c>--color-data-5</c>. Use them in order, so a chart reads the same everywhere.</summary>
public enum DataSeries
{
    One,
    Two,
    Three,
    Four,
    Five,
}

public static class DataSeriesExtensions
{
    /// <summary>The colour token of the series, such as <c>--color-data-1</c>.</summary>
    public static string Token(this DataSeries series) => $"--color-data-{Number(series)}";

    /// <summary>The CSS class a component gives a mark of the series, such as <c>series-1</c>. The kit has no shared rule for it: each component writes its own, using <see cref="Token"/>.</summary>
    public static string CssClass(this DataSeries series) => $"series-{Number(series)}";

    private static int Number(DataSeries series) =>
        Enum.IsDefined(series) ? (int)series + 1 : throw new ArgumentOutOfRangeException(nameof(series), series, null);
}
