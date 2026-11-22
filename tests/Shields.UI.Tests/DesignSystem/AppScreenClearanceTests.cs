namespace Shields.UI.Tests.DesignSystem;

public class AppScreenClearanceTests
{
    [Test]
    public void NavClearance_IsATokenOfTwelvePixels()
    {
        var value = KitCss.Tokens.Split("--app-screen-nav-clearance:")[1].Split(';')[0].Trim();

        Assert.That(value, Is.EqualTo("0.75rem"));
    }

    [TestCase(".with-nav .content")]
    [TestCase(".with-nav:has(.status-slot > *) .content")]
    public void ContentUnderTheRaisedButton_AddsTheClearance(string selector) =>
        Assert.That(CssRules.Declarations("AppScreen", selector)["padding-bottom"], Does.Contain("var(--raised)").And.Contain("var(--app-screen-nav-clearance)"));
}
