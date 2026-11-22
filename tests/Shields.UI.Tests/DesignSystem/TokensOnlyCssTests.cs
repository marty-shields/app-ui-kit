namespace Shields.UI.Tests.DesignSystem;

public class TokensOnlyCssTests
{
    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "Shields.UI")
            .Where(f => Path.GetFileName(f) != "tokens.css")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    private static IEnumerable<string> StoriesCss() =>
        RepoFiles.Find("*.css", "src", "Shields.UI.Stories")
            .Where(f => Path.GetFileName(f) != "tokens.css");

    // The framework sets --blazor-load-percentage and --blazor-load-percentage-text on the page at runtime, so blazor-ui.css reads it without declaring it.
    private static readonly string[] FrameworkTokens = ["--blazor-load-percentage", "--blazor-load-percentage-text"];

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
        var unknown = TokensOnlyCheck.UnknownTokens(File.ReadAllText(file), DesignTokens.Declares(StoriesTokens()).Concat(FrameworkTokens));

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

    [Test]
    public void StoriesRazor_HasNoLiteralInlineStyles()
    {
        Assert.That(TokensOnlyCheck.InlineStyleLiteralsInFolder(RepoFiles.PathOf("src", "Shields.UI.Stories")), Is.Empty);
    }

    [Test]
    public void StoriesRazor_ALiteralInlineStyle_IsCaught()
    {
        Assert.That(TokensOnlyCheck.InlineStyleLiterals("<div style=\"width: 10px\"></div>"), Has.Count.EqualTo(1));
    }

    [TestCase("iframe.html")]
    [TestCase("index.html")]
    public void HostPage_LinksTheStoriesTokens(string page)
    {
        var html = RepoFiles.Read("src", "Shields.UI.Stories.Host", "wwwroot", page);
        var stories = html.IndexOf("_content/Shields.UI.Stories/css/tokens.css", StringComparison.Ordinal);
        var kit = html.IndexOf("_content/Shields.UI/css/tokens.css", StringComparison.Ordinal);

        Assert.That(stories, Is.GreaterThanOrEqualTo(0));
        if (page == "iframe.html")
        {
            Assert.That(kit, Is.GreaterThanOrEqualTo(0).And.LessThan(stories), "the stories tokens load after the kit's");
        }
    }

    [Test]
    public void StoriesTokens_HoldOnlyCustomPropertyDeclarations()
    {
        var body = System.Text.RegularExpressions.Regex.Replace(StoriesTokens(), @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        var rules = System.Text.RegularExpressions.Regex.Matches(body, @"([^{}]*)\{");

        Assert.That(rules.Select(r => r.Groups[1].Value.Trim()), Is.EqualTo(new[] { ":root" }));
        var declarations = body[(body.IndexOf('{') + 1)..body.LastIndexOf('}')]
            .Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
        Assert.That(declarations, Has.All.StartWith("--stories-"));
    }
}
