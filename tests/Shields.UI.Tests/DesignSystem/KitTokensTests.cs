namespace Shields.UI.Tests.DesignSystem;

public class KitTokensTests
{
    // Every token both apps declare today, plus the one-app tokens a kit component needs.
    private static readonly string[] Shared =
    [
        "--color-bg", "--color-surface", "--color-border", "--color-border-strong", "--color-text", "--color-text-muted",
        "--color-bar", "--color-on-bar", "--color-on-bar-muted", "--color-primary", "--color-on-primary",
        "--color-secondary", "--color-on-secondary", "--color-selected", "--color-accent", "--color-focus",
        "--color-danger", "--color-on-danger", "--color-error", "--color-success", "--color-inverse-surface",
        "--color-on-inverse-surface", "--color-inverse-primary", "--color-scrim", "--state-pressed", "--shadow-raised",
        "--font-family", "--font-size-sm", "--font-size-md", "--font-size-lg", "--font-size-xl", "--font-size-xxl",
        "--font-size-input", "--font-weight-regular", "--font-weight-bold", "--line-height", "--line-height-tight",
        "--space-1", "--space-2", "--space-3", "--space-4", "--space-6", "--space-8",
        "--radius-sm", "--radius-md", "--radius-lg", "--radius-round",
        "--touch-target", "--touch-target-compact", "--border-width", "--border-width-error", "--focus-width",
        "--focus-offset", "--opacity-disabled", "--spinner-size", "--spinner-width", "--spinner-size-small",
        "--spinner-width-small", "--spinner-duration", "--pull-refresh-threshold", "--pull-refresh-return-duration",
        "--snackbar-enter-duration", "--radio-size", "--radio-dot-size", "--nav-indicator-width", "--nav-indicator-height",
    ];

    // Tokens only one app has that a kit component needs, and the decided names for the raised button, legend, chart band and snackbar gap.
    private static readonly string[] KitComponents =
    [
        "--color-chip", "--color-on-chip", "--color-chip-soft", "--color-on-chip-soft", "--letter-spacing-label",
        "--dialog-max-width", "--add-button-size", "--add-button-lift", "--legend-swatch-size",
        "--color-ring-track", "--color-ring-arc", "--color-ring-arc-over", "--font-size-xs", "--ring-size", "--ring-width", "--ring-size-compact", "--ring-width-compact",
        "--color-chart-band", "--color-chart-past", "--snackbar-gap-bottom",
        "--opacity-thin", "--chart-dash", "--color-data-1", "--color-data-2", "--color-data-3", "--color-data-4", "--color-data-5",
        "--bar-list-bar-height", "--bar-list-value-max-width",
        "--stacked-bar-height", "--stacked-bar-gap", "--stacked-bar-segment-min-width",
        // AppScreen, StatTile and Card compact.
        "--app-screen-nav-clearance", "--stat-tile-compact-padding", "--stat-tile-compact-value-size", "--card-compact-padding", "--card-compact-title-gap",
        // Sheet and SheetAction.
        "--sheet-enter-duration", "--sheet-max-width", "--sheet-max-height", "--sheet-handle-width",
        "--sheet-handle-height", "--sheet-tile-min-height", "--sheet-tile-icon-size",
        // Input box.
        "--input-box-padding-inline", "--input-box-padding-inline-compact",
    ];

    // Calorie Tracker's macro, target band and share bar tokens, and the app's own sheet names (the kit's are allowed above); Workout Log's bar-past, set, calendar and meter
    // tokens; the old name of the raised button; and the ring names, of which only the kit's own are allowed.
    private static readonly string[] AppOnlyPrefixes =
    [
        "--color-protein", "--color-carbs", "--color-fat", "--color-target-band",
        "--share-bar-", "--sheet-", "--color-bar-past", "--set-", "--calendar-", "--meter-", "--nav-new-", "--ring-", "--color-ring-",
    ];

    [Test]
    public void Has_every_shared_token()
    {
        Assert.That(DesignTokens.Names, Is.SupersetOf(Shared.Concat(KitComponents)));
    }

    [Test]
    public void Has_the_decided_values()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Value("--color-accent"), Is.EqualTo("light-dark(#29494c, #acd5d2)"));
            Assert.That(DesignTokens.Value("--color-error"), Is.EqualTo("light-dark(#841c16, #f4c1be)"));
            Assert.That(DesignTokens.Value("--add-button-size"), Is.EqualTo("3.5rem"));
            Assert.That(DesignTokens.Value("--add-button-lift"), Is.EqualTo("1.5rem"));
            Assert.That(DesignTokens.Value("--legend-swatch-size"), Is.EqualTo("0.75rem"));
            Assert.That(DesignTokens.Value("--color-chart-band"), Is.EqualTo("light-dark(#3f7fa6, #7fb8de)"));
            Assert.That(DesignTokens.Value("--snackbar-gap-bottom"), Is.EqualTo("var(--space-2)"));
            Assert.That(DesignTokens.Value("--opacity-thin"), Is.EqualTo("0.55"));
            Assert.That(DesignTokens.Value("--chart-dash"), Is.EqualTo("calc(var(--space-1) * 1.5) var(--space-1)"));
            Assert.That(DesignTokens.Value("--sheet-enter-duration"), Is.EqualTo("200ms"));
            Assert.That(DesignTokens.Value("--sheet-max-width"), Is.EqualTo("32rem"));
            Assert.That(DesignTokens.Value("--sheet-max-height"), Is.EqualTo("85dvh"));
        });
    }

    [Test]
    public void Has_no_app_only_token()
    {
        var forbidden = DesignTokens.Names.Where(n => AppOnlyPrefixes.Any(n.StartsWith) && !KitComponents.Contains(n));

        Assert.That(forbidden, Is.Empty);
    }
}
