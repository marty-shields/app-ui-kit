using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class SheetCssTests
{
    private static readonly string Css = RepoFiles.Read("src", "Shields.UI", "Components", "Overlays", "Sheet.razor.css");
    private static readonly string TileCss = RepoFiles.Read("src", "Shields.UI", "Components", "Overlays", "SheetAction.razor.css");

    [Test]
    public void Sheet_RisesInOverTheEnterDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Rule(Css, ".sheet"), Does.Contain("animation: rise var(--sheet-enter-duration) ease-out;"));
            Assert.That(Css, Does.Contain("@keyframes rise"));
        });
    }

    [Test]
    public void Sheet_IsAtMostTheMaxWidthAndMaxHeightAndScrollsWhenTaller()
    {
        var rule = Rule(Css, ".sheet");

        Assert.Multiple(() =>
        {
            Assert.That(rule, Does.Contain("max-width: var(--sheet-max-width);"));
            Assert.That(rule, Does.Contain("max-height: var(--sheet-max-height);"));
            Assert.That(rule, Does.Contain("overflow-y: auto;"));
        });
    }

    [Test]
    public void ReducedMotion_TurnsTheEntryAnimationOff()
    {
        var reduced = ReducedMotion().Match(Css);

        Assert.That(reduced.Success, Is.True);
        Assert.That(Rule(reduced.Groups["body"].Value, ".sheet"), Does.Contain("animation: none;"));
    }

    [Test]
    public void Handle_TakesItsSizeFromTheTokens()
    {
        var rule = Rule(Css, ".handle");

        Assert.Multiple(() =>
        {
            Assert.That(rule, Does.Contain("width: var(--sheet-handle-width);"));
            Assert.That(rule, Does.Contain("height: var(--sheet-handle-height);"));
        });
    }

    [Test]
    public void Tile_HasTheIconSizeAndANeverBelowTouchTargetMinimumHeight()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Rule(TileCss, ".tile ::deep .icon"), Does.Contain("--icon-size: var(--sheet-tile-icon-size);"));
            Assert.That(TileCss, Does.Contain("min-height: max(var(--touch-target), var(--sheet-tile-min-height));"));
        });
    }

    private static string Rule(string css, string selector) =>
        Regex.Match(css, Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

    [GeneratedRegex(@"@media \(prefers-reduced-motion: reduce\)\s*\{(?<body>(?:[^{}]|\{[^}]*\})*)\}")]
    private static partial Regex ReducedMotion();
}
