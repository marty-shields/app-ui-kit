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

    // Tokens only one app has that a kit component needs, and the decided names for the raised button and legend.
    private static readonly string[] KitComponents =
    [
        "--color-chip", "--color-on-chip", "--color-chip-soft", "--color-on-chip-soft", "--letter-spacing-label",
        "--dialog-max-width", "--add-button-size", "--add-button-lift", "--legend-swatch-size",
    ];

    // Calorie Tracker's ring, macro, target band, share bar and sheet tokens; Workout Log's bar-past, set, calendar and meter
    // tokens; and the old name of the raised button.
    private static readonly string[] AppOnlyPrefixes =
    [
        "--ring-", "--color-ring-", "--color-protein", "--color-carbs", "--color-fat", "--color-target-band",
        "--share-bar-", "--sheet-", "--color-bar-past", "--set-", "--calendar-", "--meter-", "--nav-new-",
    ];

    [Test]
    public void Has_every_shared_token()
    {
        Assert.That(DesignTokens.Names, Is.SupersetOf(Shared.Concat(KitComponents)));
    }

    [Test]
    public void Has_the_decided_values()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("--color-accent: light-dark(#29494c, #acd5d2);"));
            Assert.That(css, Does.Contain("--color-error: light-dark(#841c16, #f4c1be);"));
            Assert.That(css, Does.Contain("--add-button-size: 3.5rem;"));
            Assert.That(css, Does.Contain("--add-button-lift: 1.5rem;"));
            Assert.That(css, Does.Contain("--legend-swatch-size: 0.75rem;"));
        });
    }

    [Test]
    public void Has_no_app_only_token()
    {
        var forbidden = DesignTokens.Names.Where(n => AppOnlyPrefixes.Any(n.StartsWith));

        Assert.That(forbidden, Is.Empty);
    }
}
