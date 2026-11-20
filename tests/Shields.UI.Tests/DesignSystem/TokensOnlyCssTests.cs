namespace Shields.UI.Tests.DesignSystem;

public class TokensOnlyCssTests
{
    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "Shields.UI")
            .Where(f => Path.GetFileName(f) != "tokens.css")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    private static IEnumerable<TestCaseData> MarkupFiles() =>
        RepoFiles.Find("*.razor", "src", "Shields.UI")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_HasNoLiteralColoursOrSizes(string file)
    {
        var literals = TokensOnlyCheck.Literals(File.ReadAllText(file), Path.GetFileName(file));

        Assert.That(literals, Is.Empty);
    }

    [TestCaseSource(nameof(MarkupFiles))]
    public void InlineStyles_HaveNoLiteralColoursOrSizes(string file)
    {
        var literals = TokensOnlyCheck.InlineStyleLiterals(File.ReadAllText(file));

        Assert.That(literals, Is.Empty);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_UsesOnlyDeclaredTokens(string file)
    {
        var unknown = TokensOnlyCheck.UnknownTokens(File.ReadAllText(file));

        Assert.That(unknown, Is.Empty);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_FillsWithBackgroundColor_SoThePressedLayerShows(string file)
    {
        var shorthands = TokensOnlyCheck.BackgroundShorthands(File.ReadAllText(file));

        Assert.That(shorthands, Is.Empty, "base.css's pressed layer is a background-image, which the background shorthand resets");
    }

    [Test]
    public void Kit_HasNoLiteralColoursOrSizes_OverTheWholeFolder()
    {
        var kit = RepoFiles.PathOf("src", "Shields.UI");

        Assert.Multiple(() =>
        {
            Assert.That(TokensOnlyCheck.LiteralsInFolder(kit), Is.Empty);
            Assert.That(TokensOnlyCheck.InlineStyleLiteralsInFolder(kit), Is.Empty);
        });
    }
}
