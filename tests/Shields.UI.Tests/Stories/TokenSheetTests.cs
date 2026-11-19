using System.Text.RegularExpressions;
using Shields.UI.Stories.Shared;

namespace Shields.UI.Tests.Stories;

public partial class TokenSheetTests : ComponentTestBase
{
    private const string Module = "./_content/Shields.UI.Stories/Shared/TokenSheet.razor.js";

    [Test]
    public void Render_ShowsEveryTokenInTokensCss()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true).SetResult([]);

        var cut = Context.Render<TokenSheet>();

        var shown = cut.FindAll("[data-token]").Select(e => e.GetAttribute("data-token"));
        Assert.That(shown, Is.EquivalentTo(TokensInCss()));
    }

    [Test]
    public void Colours_ShowLightAndDarkValues()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true)
            .SetResult(new() { ["--color-bar light"] = "#305252", ["--color-bar dark"] = "#305252" });

        var cut = Context.Render<TokenSheet>();

        var cells = cut.Find("[data-token=\"--color-bar\"]").QuerySelectorAll("td[data-theme]");
        Assert.Multiple(() =>
        {
            Assert.That(cells.Select(c => c.GetAttribute("data-theme")), Is.EqualTo(new[] { "light", "dark" }));
            Assert.That(cells.Select(c => c.QuerySelector(".value")!.TextContent), Is.EqualTo(new[] { "#305252", "#305252" }));
        });
    }

    [Test]
    public void ExtraTokens_AreShownAfterTheKitsTokens()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true)
            .SetResult(new() { ["--color-protein light"] = "#112233", ["--color-protein dark"] = "#445566", ["--ring-size"] = "10rem" });

        var cut = Context.Render<TokenSheet>(p => p
            .Add(x => x.ExtraColours, ["--color-protein"])
            .Add(x => x.ExtraSizes, ["--ring-size"]));

        var shown = cut.FindAll("[data-token]").Select(e => e.GetAttribute("data-token")).ToList();
        var colourCells = cut.Find("[data-token=\"--color-protein\"]").QuerySelectorAll("td[data-theme] .value");
        Assert.Multiple(() =>
        {
            Assert.That(shown, Is.EquivalentTo(TokensInCss().Concat(["--color-protein", "--ring-size"])));
            Assert.That(colourCells.Select(c => c.TextContent), Is.EqualTo(new[] { "#112233", "#445566" }));
            Assert.That(cut.Find("li[data-token=\"--ring-size\"] .value").TextContent, Is.EqualTo("10rem"));
        });
    }

    private static IEnumerable<string> TokensInCss()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");
        return TokenName().Matches(css).Select(m => m.Groups[1].Value);
    }

    [GeneratedRegex(@"^\s*(--[a-z0-9-]+):", RegexOptions.Multiline)]
    private static partial Regex TokenName();
}
