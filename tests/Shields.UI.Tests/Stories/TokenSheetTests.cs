using Shields.UI.Stories.Shared;

namespace Shields.UI.Tests.Stories;

public class TokenSheetTests : ComponentTestBase
{
    private const string Module = "./_content/Shields.UI.Stories/Shared/TokenSheet.razor.js";

    [Test]
    public void Render_ShowsEveryTokenInTokensCss()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true).SetResult([]);

        var cut = Context.Render<TokenSheet>();

        Assert.That(TokenNames(cut), Is.EquivalentTo(DesignTokens.TokensCssNames));
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

        var kitOnly = TokenNames(Context.Render<TokenSheet>());
        var cut = Context.Render<TokenSheet>(p => p
            .Add(x => x.ExtraColours, ["--color-protein"])
            .Add(x => x.ExtraSizes, ["--ring-size"]));

        var kitColours = cut.FindAll("tbody [data-token]").Count - 1;
        var colourCells = cut.Find("[data-token=\"--color-protein\"]").QuerySelectorAll("td[data-theme] .value");
        Assert.Multiple(() =>
        {
            Assert.That(TokenNames(cut), Is.EqualTo(kitOnly.Take(kitColours).Append("--color-protein")
                .Concat(kitOnly.Skip(kitColours)).Append("--ring-size")));
            Assert.That(colourCells.Select(c => c.TextContent), Is.EqualTo(new[] { "#112233", "#445566" }));
            Assert.That(cut.Find("li[data-token=\"--ring-size\"] .value").TextContent, Is.EqualTo("10rem"));
        });
    }

    private static List<string?> TokenNames(IRenderedComponent<TokenSheet> cut) =>
        cut.FindAll("[data-token]").Select(e => e.GetAttribute("data-token")).ToList();
}
