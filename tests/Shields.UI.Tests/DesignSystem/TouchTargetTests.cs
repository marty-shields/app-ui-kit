using System.Text.RegularExpressions;

namespace Shields.UI.Tests.DesignSystem;

public partial class TouchTargetTests
{
    private const string TouchTarget = "var(--touch-target)";

    private static IEnumerable<TestCaseData> Controls() =>
        RepoFiles.Find("*.razor", "src", "Shields.UI")
            .SelectMany(file => Element().Matches(File.ReadAllText(file)).Select(m => (File: file, Tag: m.Groups[1].Value, Attributes: m.Groups[2].Value)))
            .Select(c => (c.File, c.Tag, Class: FirstClass(c.Attributes), Icon: c.Tag == "button" && c.Attributes.Contains("aria-label=")))
            .Distinct()
            .Select(c => new TestCaseData(c.File, c.Class, c.Icon).SetArgDisplayNames($"{Path.GetFileName(c.File)} {c.Tag}.{c.Class}"));

    [TestCaseSource(nameof(Controls))]
    public void Control_IsAtLeastATouchTarget(string razorFile, string? cssClass, bool icon)
    {
        Assert.That(cssClass, Is.Not.Null, "Buttons and inputs need a class that sizes them");
        var declarations = Rules(File.ReadAllText(razorFile + ".css"))
            .Where(r => r.Selectors.Contains($".{cssClass}"))
            .SelectMany(r => r.Declarations)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(declarations, Has.Member($"min-height: {TouchTarget}"));
            if (icon)
            {
                Assert.That(declarations, Has.Member($"min-width: {TouchTarget}"));
            }
        });
    }

    [Test]
    public void TouchTargetToken_Is48px() =>
        Assert.That(RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css"), Does.Contain("--touch-target: 48px;"));

    [Test]
    public void IconButton_IsATouchTarget_AndCompactStaysAtTheCompactTarget()
    {
        var css = Rules(RepoFiles.Read("src", "Shields.UI", "Components", "Actions", "IconButton.razor.css")).ToList();
        string[] Declarations(string selector) => css.Where(r => r.Selectors.Contains(selector)).SelectMany(r => r.Declarations).ToArray();

        var tokens = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");

        Assert.Multiple(() =>
        {
            Assert.That(tokens, Does.Contain("--touch-target-compact: 40px;"));
            Assert.That(Declarations(".icon-btn"), Has.Member($"min-width: {TouchTarget}").And.Member($"min-height: {TouchTarget}"));
            Assert.That(Declarations(".compact"), Has.Member("min-width: var(--touch-target-compact)").And.Member("min-height: var(--touch-target-compact)"));
        });
    }

    private static string? FirstClass(string attributes) =>
        Class().Match(attributes) is { Success: true } m
            ? m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(c => !c.StartsWith('@'))
            : null;

    private static IEnumerable<(string[] Selectors, string[] Declarations)> Rules(string css) =>
        Rule().Matches(Comment().Replace(css, "")).Select(m => (
            m.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries),
            m.Groups[2].Value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)));

    // Opening tag of a button, input or select; quoted attribute values may contain '>'.
    [GeneratedRegex("""<(button|input|select)\b((?:[^>"]|"[^"]*")*)>""")]
    private static partial Regex Element();

    [GeneratedRegex("\\bclass=\"([^\"]*)\"")]
    private static partial Regex Class();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();
}
