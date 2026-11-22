using System.Globalization;

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
    }

    [Test]
    public void Literals_AcceptsTheBaseAllowancesOnlyInTheKitsBaseCss()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TokensOnlyCheck.Literals(KitCss.Base, "base.css"), Is.Empty);
            Assert.That(TokensOnlyCheck.Literals("a { width: 1px; }\n", "base.css"), Has.Count.EqualTo(1));
            Assert.That(TokensOnlyCheck.Literals("width: 1px;", "other/base.css"), Has.Count.EqualTo(1));
        });
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
    public void LiteralsInFolder_AllowsByPathInsideTheFolder_SoAnotherFileWithTheSameNameStillReports()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(folder, "a"));
            Directory.CreateDirectory(Path.Combine(folder, "b"));
            File.WriteAllText(Path.Combine(folder, "a", "base.css"), "width: 1px;");
            File.WriteAllText(Path.Combine(folder, "b", "base.css"), "width: 1px;");

            Assert.That(TokensOnlyCheck.LiteralsInFolder(folder, ["a/base.css: width: 1px;"]), Is.EqualTo(new[] { "b/base.css: width: 1px;" }));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Test]
    public void LiteralsInFolder_IgnoresBinAndObjInsideTheFolder_ButNotAboveIt()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var inside = Path.Combine(root, "obj", "app");
            Directory.CreateDirectory(Path.Combine(inside, "obj"));
            File.WriteAllText(Path.Combine(inside, "obj", "x.css"), "a { color: #fff; }");
            File.WriteAllText(Path.Combine(inside, "y.css"), "a { color: #fff; }");

            Assert.That(TokensOnlyCheck.LiteralsInFolder(inside), Is.EqualTo(new[] { "y.css: a { color: #fff; }" }));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public void Declarations_ReadSingleLineAndMinifiedCss()
    {
        var css = ":root { --a: #fff; --b:#000;--c: 1 }";

        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Declares(css), Is.EqualTo(new[] { "--a", "--b", "--c" }));
            Assert.That(DesignTokens.Value(css, "--a"), Is.EqualTo("#fff"));
            Assert.That(DesignTokens.Value(css, "--c"), Is.EqualTo("1"));
            Assert.That(DesignTokens.Colour(":root { --x: #ffffff; }", "--x").Light, Is.EqualTo(new Rgb(255, 255, 255)));
            Assert.That(TokensOnlyCheck.UnknownTokens(":root { --a: 1; } b { c: var(--a); d: var(--nope); }"), Is.EqualTo(new[] { "--nope" }));
        });
    }

    [Test]
    public void Value_TakesTheLastDeclaration_AndNamesAMissingToken()
    {
        var css = ":root { --a: 1; }\n@media (x) { :root { --a: 2; } }";

        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Value(css, "--a"), Is.EqualTo("2"));
            Assert.That(() => DesignTokens.Value(css, "--missing"), Throws.InvalidOperationException.With.Message.Contain("--missing"));
        });
    }

    [Test]
    public void UnknownTokens_AcceptsTheKitsTokensAndTheCallersKnownOnes()
    {
        var css = "a { color: var(--color-text); b: var(--app-x); c: var(--nope); }";

        Assert.Multiple(() =>
        {
            Assert.That(TokensOnlyCheck.UnknownTokens(css, ["--app-x"]), Is.EqualTo(new[] { "--nope" }));
            Assert.That(TokensOnlyCheck.UnknownTokens(css), Is.EqualTo(new[] { "--app-x", "--nope" }));
        });
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
    public void Contrast_OfATokenOnlyTheKitDeclares_ThrowsNamingIt()
    {
        Assert.That(() => ContrastCheck.Measure(":root { --app-fg: #000000; }", "--app-fg", "--color-bg", ContrastCheck.Text, ColourScheme.Light),
            Throws.InvalidOperationException.With.Message.Contain("--color-bg"));
    }

    [Test]
    public void Contrast_OfAForegroundTokenOnlyTheKitDeclares_ThrowsNamingIt()
    {
        Assert.That(() => ContrastCheck.Measure(":root { --app-bg: #ffffff; }", "--color-text", "--app-bg", ContrastCheck.Text, ColourScheme.Light),
            Throws.InvalidOperationException.With.Message.Contain("--color-text"));
    }

    [Test]
    public void Contrast_OfAMissingToken_NamesIt()
    {
        Assert.That(() => ContrastCheck.Measure(":root { --a: #000000; }", "--a", "--nope", 1, ColourScheme.Light),
            Throws.InvalidOperationException.With.Message.Contain("--nope"));
    }

    [Test]
    public void Contrast_ReadsEachThemeFromALightDarkPair()
    {
        var css = ":root { --fg: light-dark(#000000, #777777); --bg: #ffffff; }";

        var light = ContrastCheck.Measure(css, "--fg", "--bg", ContrastCheck.Text, ColourScheme.Light);
        var dark = ContrastCheck.Measure(css, "--fg", "--bg", ContrastCheck.Text, ColourScheme.Dark);

        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Colour(css, "--fg"), Is.EqualTo(new ThemedColour(new Rgb(0, 0, 0), new Rgb(0x77, 0x77, 0x77))));
            Assert.That(light.ForegroundColour, Is.EqualTo(new Rgb(0, 0, 0)));
            Assert.That(dark.ForegroundColour, Is.EqualTo(new Rgb(0x77, 0x77, 0x77)));
            Assert.That(light.Ratio, Is.EqualTo(21).Within(0.001));
            Assert.That(dark.Ratio, Is.EqualTo(4.478089).Within(0.0001));
        });
    }

    [Test]
    public void Contrast_OfAVeryDarkColour_UsesTheLinearLuminanceBranch()
    {
        var ratio = Rgb.Contrast(new Rgb(3, 3, 3), new Rgb(255, 255, 255));

        Assert.That(ratio, Is.EqualTo(20.624396).Within(0.0001));
    }

    [Test]
    public void Distance_OfBlackAndWhite_IsOne() =>
        Assert.That(Rgb.Distance(new Rgb(0, 0, 0), new Rgb(255, 255, 255)), Is.EqualTo(1.0).Within(0.001));

    [Test]
    public void Distance_OfAColourToItself_IsZero() =>
        Assert.That(Rgb.Distance(new Rgb(47, 127, 134), new Rgb(47, 127, 134)), Is.Zero.Within(1e-9));

    [Test]
    public void ContrastResult_PassesAtExactlyTheMinimum()
    {
        var result = new ContrastResult("--a", "--b", ColourScheme.Light, new Rgb(0, 0, 0), new Rgb(255, 255, 255), 4.5, 4.5);

        Assert.That(result.Passes, Is.True);
    }

    [Test]
    public void ContrastResult_ToString_IgnoresTheCurrentCulture()
    {
        var before = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var text = new ContrastResult("--a", "--b", ColourScheme.Light, new Rgb(0, 0, 0), new Rgb(255, 255, 255), 4.5, 3.5).ToString();

            Assert.That(text, Does.EndWith(": 4.50, needs 3.5"));
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
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
    public void Controls_SkipsARazorExpressionClass()
    {
        var control = TouchTargetCheck.Controls("""<button class="@Cls go">x</button>""").Single();

        Assert.That(control.CssClass, Is.EqualTo("go"));
    }

    [Test]
    public void ProblemsInFolder_NamesAControlInARazorFileWithoutScopedCss()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(folder, "Go.razor"), """<button class="go">Go</button>""");

            Assert.That(TouchTargetCheck.ProblemsInFolder(folder), Is.EqualTo(new[] { "Go.razor: button has no Go.razor.css" }));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
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
