using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class SnackbarCssTests
{
    private static readonly string Css = RepoFiles.Read("src", "Shields.UI", "Components", "Feedback", "Snackbar.razor.css");

    [Test]
    public void Snackbar_HasMdCornersAndTheRaisedShadow()
    {
        var rule = Rule(".snackbar");

        Assert.Multiple(() =>
        {
            Assert.That(rule, Does.Contain("border-radius: var(--radius-md);"));
            Assert.That(rule, Does.Contain("box-shadow: var(--shadow-raised);"));
        });
    }

    [Test]
    public void Snackbar_RisesInOverTheEnterDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Rule(".snackbar"), Does.Contain("animation: rise var(--snackbar-enter-duration) ease-out;"));
            Assert.That(Css, Does.Contain("@keyframes rise"));
        });
    }

    [Test]
    public void ReducedMotion_TurnsTheEntryAnimationOff()
    {
        var reduced = ReducedMotion().Match(Css);

        Assert.That(reduced.Success, Is.True);
        Assert.That(Regex.Match(reduced.Groups["body"].Value, @"\.snackbar\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value,
            Does.Contain("animation: none;"));
    }

    private static string Rule(string selector) =>
        Regex.Match(Css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

    [GeneratedRegex(@"@media \(prefers-reduced-motion: reduce\)\s*\{(?<body>(?:[^{}]|\{[^}]*\})*)\}")]
    private static partial Regex ReducedMotion();
}
