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
        ("--color-primary", "--color-bg", LargeTextOrControl),
        ("--color-primary", "--color-surface", LargeTextOrControl),
        ("--color-on-secondary", "--color-secondary", Text),
        ("--color-on-danger", "--color-danger", Text),
        ("--color-on-primary", "--color-accent", Text),
        ("--color-on-danger", "--color-error", Text),
        ("--color-on-inverse-surface", "--color-inverse-surface", Text),
        ("--color-inverse-primary", "--color-inverse-surface", Text),
        ("--color-accent", "--color-surface", LargeTextOrControl),
        ("--color-border-strong", "--color-bg", LargeTextOrControl),
        ("--color-border-strong", "--color-surface", LargeTextOrControl),
        ("--color-accent", "--color-bg", LargeTextOrControl),
        ("--color-accent", "--color-selected", LargeTextOrControl),
        ("--color-on-chip", "--color-chip", Text),
        ("--color-on-chip-soft", "--color-chip-soft", Text),
        ("--color-chip", "--color-bg", LargeTextOrControl),
        ("--color-chip", "--color-surface", LargeTextOrControl),
        // The Soft icon button's outline (--color-chip) sits on the sheet's surface and the page. The icon sits on --color-chip-soft; the icon pairs
        // below guard it if that fill is ever translucent or removed.
        ("--color-on-chip-soft", "--color-surface", LargeTextOrControl),
        ("--color-on-chip-soft", "--color-bg", LargeTextOrControl),
        ("--color-focus", "--color-bg", LargeTextOrControl),
        ("--color-focus", "--color-surface", LargeTextOrControl),
        ("--color-ring-track", "--color-surface", LargeTextOrControl),
        ("--color-ring-track", "--color-bg", LargeTextOrControl),
        ("--color-ring-arc", "--color-ring-track", LargeTextOrControl),
        ("--color-ring-arc-over", "--color-ring-track", LargeTextOrControl),
        ("--color-ring-arc", "--color-bg", LargeTextOrControl),
        ("--color-ring-arc", "--color-surface", LargeTextOrControl),
        ("--color-ring-arc-over", "--color-bg", Text),
        ("--color-ring-arc-over", "--color-surface", Text),
        ("--color-chart-band", "--color-bg", LargeTextOrControl),
        ("--color-chart-band", "--color-surface", LargeTextOrControl),
        ("--color-data-1", "--color-bg", LargeTextOrControl),
        ("--color-data-1", "--color-surface", LargeTextOrControl),
        ("--color-data-2", "--color-bg", LargeTextOrControl),
        ("--color-data-2", "--color-surface", LargeTextOrControl),
        ("--color-data-3", "--color-bg", LargeTextOrControl),
        ("--color-data-3", "--color-surface", LargeTextOrControl),
        ("--color-data-4", "--color-bg", LargeTextOrControl),
        ("--color-data-4", "--color-surface", LargeTextOrControl),
        ("--color-data-5", "--color-bg", LargeTextOrControl),
        ("--color-data-5", "--color-surface", LargeTextOrControl),
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

    // Read from each component's CSS, so a variant that gains a new colour is measured without a row here.
    private static readonly (string Css, string Selector, string Property, string[] Backgrounds, double Minimum)[] ComponentColours =
    [
        .. new[] { ".text", ".label", ".muted", ".muted-small", ".strong", ".figure", ".error" }
            .Select(selector => ("Layout/Typography.razor.css", selector, "color", new[] { "--color-bg", "--color-surface" }, Text)),
        ("Feedback/InlineAlert.razor.css", ".error", "color", ["--color-surface"], Text),
        ("Feedback/InlineAlert.razor.css", ".error", "border-color", ["--color-bg", "--color-surface"], LargeTextOrControl),
        ("Layout/Card.razor.css", ".card.error", "border-color", ["--color-bg", "--color-surface"], LargeTextOrControl),
        ("Feedback/Banner.razor.css", ".banner", "color", ["--color-selected"], Text),
    ];

    private static IEnumerable<TestCaseData> ComponentCases() =>
        ComponentColours.SelectMany(c => c.Backgrounds.SelectMany(background => Enum.GetValues<ColourScheme>()
            .Select(scheme => new TestCaseData(c.Css, c.Selector, c.Property, background, c.Minimum, scheme)
                .SetArgDisplayNames($"{Path.GetFileNameWithoutExtension(c.Css)} {c.Selector} {c.Property} on {background} ({scheme})"))));

    [TestCaseSource(nameof(ComponentCases))]
    public void ComponentColour_MeetsWcagAA(string cssFile, string selector, string property, string background, double minimum, ColourScheme scheme)
    {
        var css = RepoFiles.Read(["src", "Shields.UI", "Components", .. cssFile.Split('/')]);
        string? Declared(string rule) => TouchTargetCheck.Declarations(css, rule).SingleOrDefault(d => d.StartsWith(property + ":", StringComparison.Ordinal));

        // A Text variant that sets no colour keeps the one of the base .text rule.
        var declaration = Declared(selector) ?? Declared(".text");
        Assert.That(declaration, Is.Not.Null, $"{selector} {property}");
        var foreground = declaration![(declaration.IndexOf("var(", StringComparison.Ordinal) + 4)..declaration.IndexOf(')')];

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
