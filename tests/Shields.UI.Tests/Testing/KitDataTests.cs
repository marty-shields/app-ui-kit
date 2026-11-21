using Microsoft.AspNetCore.Components;

namespace Shields.UI.Tests.Testing;

public class KitDataTests
{
    [Test]
    public void EmbeddedCss_EqualsTheKitsFiles()
    {
        Assert.Multiple(() =>
        {
            Assert.That(KitCss.Tokens, Is.EqualTo(RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css")));
            Assert.That(KitCss.Base, Is.EqualTo(RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "base.css")));
        });
    }

    [Test]
    public void ThemeJs_EqualsTheKitsFile()
    {
        Assert.Multiple(() =>
        {
            Assert.That(KitAssets.ThemeJs, Is.Not.Empty);
            Assert.That(KitAssets.ThemeJs, Is.EqualTo(RepoFiles.Read("src", "Shields.UI", "wwwroot", "theme.js")));
        });
    }

    [Test]
    public void Bundle_IsTheScopedCssTheKitBuildsWithItsComponentsRules()
    {
        var built = RepoFiles.Find("Shields.UI.bundle.scp.css", "src", "Shields.UI", "obj").Select(File.ReadAllText).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(KitCss.Bundle, Does.Contain(".failed").And.Contain("[b-"));
            Assert.That(built, Has.Some.EqualTo(KitCss.Bundle));
        });
    }

    [Test]
    public void EmbeddedFiles_AreInTheTestingAssemblyThatThePackageCarries()
    {
        var names = typeof(KitCss).Assembly.GetManifestResourceNames();

        Assert.That(names, Is.SupersetOf(new[] { "tokens.css", "base.css", "theme.js", "bundle.scp.css" }));
    }

    [Test]
    public void Tokens_AreEveryCustomPropertyInTokensAndBaseCss()
    {
        var declared = RepoFiles.Find("*.css", "src", "Shields.UI", "wwwroot", "css")
            .SelectMany(f => DesignTokens.Declares(File.ReadAllText(f)));

        Assert.That(KitNames.Tokens, Is.EquivalentTo(declared));
    }

    [Test]
    public void Components_AreEveryExportedComponentOfTheKit()
    {
        var exported = KitComponents.Exported.Select(t => t.Name.Split('`')[0]);

        Assert.That(KitNames.Components, Is.EquivalentTo(exported));
    }

    [Test]
    public void Types_AreEveryExportedNonComponentTypeOfTheKit()
    {
        var exported = typeof(ShieldsUiServiceCollectionExtensions).Assembly.GetExportedTypes()
            .Where(t => !typeof(ComponentBase).IsAssignableFrom(t) && t.Name != "_Imports")
            .Select(t => t.Name.Split('`')[0]);

        Assert.That(KitNames.Types, Is.EquivalentTo(exported));
    }

    [Test]
    public void Colour_ReadsTheLightAndDarkValueOfAToken()
    {
        var accent = DesignTokens.Colour("--color-accent");

        Assert.Multiple(() =>
        {
            Assert.That(accent.Light, Is.EqualTo(Rgb.Parse("#29494c")));
            Assert.That(accent.Dark, Is.EqualTo(Rgb.Parse("#acd5d2")));
        });
    }

    [Test]
    public void Colour_OfAnAppsOwnCss_ReadsThatCss()
    {
        var colour = DesignTokens.Colour(":root {\n  --color-x: #102030;\n}", "--color-x");

        Assert.That(colour, Is.EqualTo(new ThemedColour(Rgb.Parse("#102030"), Rgb.Parse("#102030"))));
    }
}
