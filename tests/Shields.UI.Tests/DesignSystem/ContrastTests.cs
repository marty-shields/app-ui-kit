namespace Shields.UI.Tests.DesignSystem;

public class ContrastTests
{
    private const double Text = ContrastCheck.Text;
    private const double LargeTextOrControl = ContrastCheck.LargeTextOrControl;

    // Minimums are WCAG AA contrast ratios.
    private static readonly (string Foreground, string Background, double Minimum)[] Pairs =
    [
        ("--color-text", "--color-bg", Text),
        ("--color-text", "--color-surface", Text),
        ("--color-text", "--color-selected", Text),
        ("--color-text-muted", "--color-bg", Text),
        ("--color-text-muted", "--color-surface", Text),
        ("--color-error", "--color-bg", Text),
        ("--color-error", "--color-surface", Text),
        ("--color-success", "--color-bg", Text),
        ("--color-success", "--color-surface", Text),
        ("--color-on-bar", "--color-bar", Text),
        ("--color-on-bar-muted", "--color-bar", Text),
        ("--color-on-primary", "--color-primary", Text),
        ("--color-on-secondary", "--color-secondary", Text),
        ("--color-on-danger", "--color-danger", Text),
        ("--color-on-inverse-surface", "--color-inverse-surface", Text),
        ("--color-inverse-primary", "--color-inverse-surface", Text),
        ("--color-accent", "--color-surface", LargeTextOrControl),
        ("--color-border-strong", "--color-bg", LargeTextOrControl),
        ("--color-border-strong", "--color-surface", LargeTextOrControl),
        ("--color-accent", "--color-bg", LargeTextOrControl),
        ("--color-on-chip", "--color-chip", Text),
        ("--color-on-chip-soft", "--color-chip-soft", Text),
        ("--color-chip", "--color-bg", LargeTextOrControl),
        ("--color-chip", "--color-surface", LargeTextOrControl),
        ("--color-focus", "--color-bg", LargeTextOrControl),
        ("--color-focus", "--color-surface", LargeTextOrControl),
    ];

    private static IEnumerable<TestCaseData> Cases() =>
        Pairs.SelectMany(p => Enum.GetValues<ColourScheme>().Select(scheme =>
            new TestCaseData(p.Foreground, p.Background, p.Minimum, scheme)
                .SetArgDisplayNames($"{p.Foreground} on {p.Background} ({scheme})")));

    [TestCaseSource(nameof(Cases))]
    public void Pair_MeetsWcagAA(string foreground, string background, double minimum, ColourScheme scheme)
    {
        var result = ContrastCheck.Measure(foreground, background, minimum, scheme);

        Assert.That(result.Passes, Is.True, result.ToString());
    }

    // The Soft chip's border is --color-chip (already checked above on bg and surface); its fill must read as different from the Filled chip.
    [TestCase(ColourScheme.Light)]
    [TestCase(ColourScheme.Dark)]
    public void SoftChipFill_DiffersFromFilledChip(ColourScheme scheme)
    {
        var soft = DesignTokens.Colour("--color-chip-soft").In(scheme);
        var filled = DesignTokens.Colour("--color-chip").In(scheme);

        Assert.That(Rgb.Contrast(soft, filled), Is.GreaterThanOrEqualTo(1.5), $"{soft} vs {filled}");
    }

    // Both apps keep the status bar's icons light in both themes, and Android draws them white over the top bar.
    [TestCase(ColourScheme.Light)]
    [TestCase(ColourScheme.Dark)]
    public void StatusBarIcons_OnTheTopBar_MeetWcagAA(ColourScheme scheme)
    {
        var bar = DesignTokens.Colour("--color-bar").In(scheme);

        Assert.That(Rgb.Contrast(Rgb.Parse("#ffffff"), bar), Is.GreaterThanOrEqualTo(LargeTextOrControl), $"#ffffff on {bar}");
    }
}
