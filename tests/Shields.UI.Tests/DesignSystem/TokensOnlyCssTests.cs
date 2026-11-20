namespace Shields.UI.Tests.DesignSystem;

public class TokensOnlyCssTests
{
    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "Shields.UI")
            .Where(f => Path.GetFileName(f) != "tokens.css")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    // blazor-ui.css is the Blazor template's loading and error UI, which the framework drives (--blazor-load-percentage), so it stays as shipped.
    private static IEnumerable<string> StoriesCss() =>
        RepoFiles.Find("*.css", "src", "Shields.UI.Stories")
            .Where(f => Path.GetFileName(f) is not ("tokens.css" or "blazor-ui.css"));

    private static IEnumerable<TestCaseData> StoriesStyleFiles() =>
        StoriesCss().Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    private static string StoriesTokens() => RepoFiles.Read("src", "Shields.UI.Stories", "wwwroot", "css", "tokens.css");

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

    [TestCaseSource(nameof(StoriesStyleFiles))]
    public void StoriesCss_HasNoLiteralColoursOrSizes(string file)
    {
        var literals = TokensOnlyCheck.Literals(File.ReadAllText(file), Path.GetFileName(file));

        Assert.That(literals, Is.Empty);
    }

    [TestCaseSource(nameof(StoriesStyleFiles))]
    public void StoriesCss_UsesOnlyDeclaredTokens(string file)
    {
        var unknown = TokensOnlyCheck.UnknownTokens(File.ReadAllText(file), DesignTokens.Declares(StoriesTokens()));

        Assert.That(unknown, Is.Empty);
    }

    [Test]
    public void StoriesCss_ScansTheStoriesLibrary()
    {
        var names = StoriesCss().Select(Path.GetFileName).ToList();

        Assert.That(names, Does.Contain("PhoneFrame.razor.css").And.Contain("AppIconSheet.razor.css").And.Contain("TokenSheet.razor.css"));
    }

    [Test]
    public void StoriesCss_ALiteralColour_IsCaught()
    {
        var css = File.ReadAllText(StoriesCss().First()) + "\n.x { color: #ff0000; }";

        Assert.That(TokensOnlyCheck.Literals(css, "mutated.razor.css"), Has.Some.Contain("color: #ff0000;"));
    }

    [Test]
    public void StoriesTokens_AreNamedForTheStories_NotTheKit()
    {
        Assert.That(DesignTokens.Declares(StoriesTokens()), Has.All.StartsWith("--stories-"));
    }
}
