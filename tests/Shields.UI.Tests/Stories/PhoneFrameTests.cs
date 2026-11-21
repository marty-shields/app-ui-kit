using System.Text.RegularExpressions;
using Shields.UI.Stories.Shared;

namespace Shields.UI.Tests.Stories;

public class PhoneFrameTests : ComponentTestBase
{
    [Test]
    public void Render_FollowsSystemTheme()
    {
        var cut = Context.Render<PhoneFrame>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=phone]").HasAttribute("data-theme"), Is.False);
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [TestCase("light")]
    [TestCase("dark")]
    public void Theme_SetsThemeOnThePhone(string theme)
    {
        var cut = Context.Render<PhoneFrame>();

        cut.Find($"[data-testid=theme-{theme}]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=phone]").GetAttribute("data-theme"), Is.EqualTo(theme));
            Assert.That(cut.Find($"[data-testid=theme-{theme}]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("false"));
        });
    }

    [Test]
    public void Css_OuterWidth_FitsTheNarrowestCardTheLayoutCheckUses()
    {
        var tokens = RepoFiles.Read("src", "Shields.UI.Stories", "wwwroot", "css", "tokens.css");
        var content = Pixels(tokens, "--stories-phone-width");
        var bezel = Pixels(tokens, "--stories-bezel-width");
        var narrowest = int.Parse(Regex.Match(RepoFiles.Read(".github", "scripts", "check-story-layout.mjs"), @"const WIDTH = (\d+);").Groups[1].Value);

        Assert.Multiple(() =>
        {
            Assert.That(content + 2 * bezel, Is.LessThanOrEqualTo(narrowest));
            Assert.That(content, Is.EqualTo(412), "the screen's content box keeps the device width");
        });
    }

    private static int Pixels(string css, string token) =>
        int.Parse(Regex.Match(css, Regex.Escape(token) + @":\s*(\d+)px;").Groups[1].Value);

    [Test]
    public void System_ClearsTheme()
    {
        var cut = Context.Render<PhoneFrame>();
        cut.Find("[data-testid=theme-dark]").Click();

        cut.Find("[data-testid=theme-system]").Click();

        Assert.That(cut.Find("[data-testid=phone]").HasAttribute("data-theme"), Is.False);
    }
}
