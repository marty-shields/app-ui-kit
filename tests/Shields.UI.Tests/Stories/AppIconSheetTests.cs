using Shields.UI.Stories.Shared;

namespace Shields.UI.Tests.Stories;

public class AppIconSheetTests : ComponentTestBase
{
    private IRenderedComponent<AppIconSheet> RenderSheet() => Context.Render<AppIconSheet>(p => p
        .Add(x => x.Background, "bg.svg")
        .Add(x => x.Foreground, "fg.svg")
        .Add(x => x.Monochrome, "mono.svg")
        .Add(x => x.SplashLight, "splash-light.svg")
        .Add(x => x.SplashDark, "splash-dark.svg"));

    [Test]
    public void Render_ShowsTheImagesItIsGiven()
    {
        var cut = RenderSheet();

        var launcher = cut.FindAll(".mask img").Select(img => img.GetAttribute("src")).ToList();
        var themed = cut.FindAll(".silhouette").Select(e => e.GetAttribute("style")!).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(launcher, Has.Count.EqualTo(8));
            Assert.That(launcher.Where((_, i) => i % 2 == 0), Is.All.EqualTo("bg.svg"));
            Assert.That(launcher.Where((_, i) => i % 2 == 1), Is.All.EqualTo("fg.svg"));
            Assert.That(themed, Has.Count.EqualTo(4).And.All.Contains("mono.svg"));
            Assert.That(cut.Find(".splash.light .splash-image").GetAttribute("src"), Is.EqualTo("splash-light.svg"));
            Assert.That(cut.Find(".splash.dark .splash-image").GetAttribute("src"), Is.EqualTo("splash-dark.svg"));
        });
    }
}
