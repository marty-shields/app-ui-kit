namespace Shields.UI.Tests.DesignSystem;

public class TouchTargetTests
{
    private const string TouchTarget = TouchTargetCheck.TouchTarget;

    private static IEnumerable<TestCaseData> Controls() =>
        RepoFiles.Find("*.razor", "src", "Shields.UI")
            .SelectMany(file => TouchTargetCheck.Controls(File.ReadAllText(file)).Select(c => (File: file, Control: c)))
            .Select(c => new TestCaseData(c.File, c.Control).SetArgDisplayNames($"{Path.GetFileName(c.File)} {c.Control.Tag}.{c.Control.CssClass}"));

    [TestCaseSource(nameof(Controls))]
    public void Control_IsAtLeastATouchTarget(string razorFile, TouchTargetControl control) =>
        Assert.That(TouchTargetCheck.Problems(control, File.ReadAllText(razorFile + ".css")), Is.Empty);

    [Test]
    public void Kit_ControlsAreTouchTargets_OverTheWholeFolder() =>
        Assert.That(TouchTargetCheck.ProblemsInFolder(RepoFiles.PathOf("src", "Shields.UI")), Is.Empty);

    [Test]
    public void TouchTargetToken_Is48px() =>
        Assert.That(RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css"), Does.Contain("--touch-target: 48px;"));

    [Test]
    public void IconButton_IsATouchTarget_AndCompactStaysAtTheCompactTarget()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Actions", "IconButton.razor.css");
        IReadOnlyList<string> Declarations(string selector) => TouchTargetCheck.Declarations(css, selector);

        var tokens = RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css");

        Assert.Multiple(() =>
        {
            Assert.That(tokens, Does.Contain("--touch-target-compact: 40px;"));
            Assert.That(Declarations(".icon-btn"), Has.Member($"min-width: {TouchTarget}").And.Member($"min-height: {TouchTarget}"));
            Assert.That(Declarations(".compact"), Has.Member("min-width: var(--touch-target-compact)").And.Member("min-height: var(--touch-target-compact)"));
        });
    }

    [Test]
    public void BottomNav_TabsAndRaisedButtonAreTouchTargets_AndTheRaisedButtonIsWideEnough()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Layout", "BottomNav.razor.css");
        IReadOnlyList<string> Declarations(string selector) => TouchTargetCheck.Declarations(css, selector);

        Assert.Multiple(() =>
        {
            Assert.That(Declarations(".item"), Has.Member($"min-height: {TouchTarget}"));
            Assert.That(Declarations(".add"), Has.Member($"min-height: {TouchTarget}"));
            Assert.That(Declarations(".fab"), Has.Member("width: var(--add-button-size)").And.Member("height: var(--add-button-size)"));
            Assert.That(RepoFiles.Read("src", "Shields.UI", "wwwroot", "css", "tokens.css"), Does.Contain("--add-button-size: 3.5rem;"));
        });
    }
}
