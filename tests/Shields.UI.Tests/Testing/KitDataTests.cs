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
