namespace Shields.UI.Tests.Testing;

public class CheckTests
{
    [TestCase("a { color: #fff; }")]
    [TestCase("a { color: rgb(1, 2, 3); }")]
    [TestCase("a { color: red; }")]
    [TestCase("a { padding: 4px; }")]
    [TestCase("a { width: 50%; }")]
    public void Literals_FindsALiteralColourOrSize(string css)
    {
        Assert.That(TokensOnlyCheck.Literals(css, "x.css"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Literals_AcceptsTokensCommentsAndTheBaseAllowances()
    {
        var css = "/* color: #fff; */\na { color: var(--color-text); width: 100%; }\n";

        Assert.That(TokensOnlyCheck.Literals(css, "x.css"), Is.Empty);
        Assert.That(TokensOnlyCheck.Literals("width: 1px;", "base.css"), Is.Empty);
    }

    [Test]
    public void Literals_AcceptsLinesTheCallerAllows()
    {
        Assert.That(TokensOnlyCheck.Literals("width: 7px;", "x.css", ["x.css: width: 7px;"]), Is.Empty);
    }

    [Test]
    public void LiteralsInFolder_ReportsALiteralInAComponentCss_AndSkipsTokensCss()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(folder, "tokens.css"), ":root { --color-x: #123456; }");
            File.WriteAllText(Path.Combine(folder, "Button.razor.css"), ".b {\n  color: #123456;\n}\n");

            Assert.That(TokensOnlyCheck.LiteralsInFolder(folder), Is.EqualTo(new[] { "Button.razor.css: color: #123456;" }));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Test]
    public void InlineStyleLiterals_FindsALiteralInAStyleAttribute()
    {
        Assert.That(TokensOnlyCheck.InlineStyleLiterals("""<div style="color: #fff"></div><p style="color: var(--color-text)"></p>"""),
            Is.EqualTo(new[] { "color: #fff" }));
    }

    [Test]
    public void UnknownTokens_ListsATokenNeitherKnownNorDeclared()
    {
        var css = ":root {\n  --mine: 1;\n}\na { color: var(--color-text); b: var(--nope); c: var(--mine); }";

        Assert.That(TokensOnlyCheck.UnknownTokens(css), Is.EqualTo(new[] { "--nope" }));
    }

    [Test]
    public void BackgroundShorthands_FindsOnlyTheShorthand()
    {
        Assert.That(TokensOnlyCheck.BackgroundShorthands("a { background: var(--color-bg); }\nb { background-color: var(--color-bg); }"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Contrast_OfABlackOnWhitePair_IsTwentyOne_AndFailsAWeakPair()
    {
        var css = ":root {\n  --fg: #000000;\n  --bg: #ffffff;\n  --weak: #777777;\n}";

        Assert.Multiple(() =>
        {
            Assert.That(ContrastCheck.Measure(css, "--fg", "--bg", ContrastCheck.Text, ColourScheme.Dark).Ratio, Is.EqualTo(21).Within(0.001));
            Assert.That(ContrastCheck.Measure(css, "--weak", "--bg", ContrastCheck.Text, ColourScheme.Light).Passes, Is.False);
        });
    }

    [Test]
    public void TouchTarget_FlagsASmallControl_AndAnIconButtonWithoutMinWidth()
    {
        var controls = TouchTargetCheck.Controls("""<button class="a">x</button><button class="icon" aria-label="Close">x</button><input>""");
        var css = ".a { min-height: var(--touch-target); } .icon { min-height: var(--touch-target); }";

        Assert.Multiple(() =>
        {
            Assert.That(controls, Has.Count.EqualTo(3));
            Assert.That(TouchTargetCheck.Problems(controls[0], css), Is.Empty);
            Assert.That(TouchTargetCheck.Problems(controls[1], css), Has.Count.EqualTo(1).And.Some.Contain("min-width"));
            Assert.That(TouchTargetCheck.Problems(controls[2], css), Has.Count.EqualTo(1).And.Some.Contain("no class"));
        });
    }

    [Test]
    public void TouchTarget_FlagsAControlWhoseMinHeightIsMissing()
    {
        var control = TouchTargetCheck.Controls("""<button class="a">x</button>""").Single();

        Assert.That(TouchTargetCheck.Problems(control, ".a { padding: var(--space-2); }"), Has.Count.EqualTo(1));
    }

    [Test]
    public void ProblemsInFolder_NamesTheFileWithAControlThatIsTooSmall()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(folder, "Go.razor"), """<button class="go">Go</button>""");
            File.WriteAllText(Path.Combine(folder, "Go.razor.css"), ".go { padding: var(--space-2); }");

            Assert.That(TouchTargetCheck.ProblemsInFolder(folder), Is.EqualTo(new[] { "Go.razor: button.go needs min-height: var(--touch-target)" }));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
