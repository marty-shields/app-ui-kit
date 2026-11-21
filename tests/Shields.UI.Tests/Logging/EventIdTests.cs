using System.Text.RegularExpressions;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Logging;

/// <summary>
/// Event ids are named values: <c>KitEvent</c> has its own range, 9000-9999. Values are unique and fixed.
/// No <c>[LoggerMessage]</c> in the source uses a bare number.
/// </summary>
public partial class EventIdTests
{
    private static IEnumerable<TestCaseData> SourceFiles() =>
        RepoFiles.Find("*.cs", "src", "Shields.UI").Concat(RepoFiles.Find("*.razor", "src", "Shields.UI"))
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetRelativePath(RepoFiles.Root, f)));

    [Test]
    public void KitEvent_ValuesAreUnique() =>
        Assert.That(Enum.GetValues<KitEvent>().Select(e => (int)e), Is.Unique);

    [Test]
    public void KitEvent_ValuesAreInTheKitsRange() =>
        Assert.That(Enum.GetValues<KitEvent>().Select(e => (int)e), Is.All.InRange(9000, 9999));

    [Test]
    public void KitEvent_ValuesAreFixed() =>
        Assert.That(Enum.GetValues<KitEvent>().ToDictionary(e => e, e => (int)e), Is.EqualTo(new Dictionary<KitEvent, int>
        {
            [KitEvent.StartupFinished] = 9001,
            [KitEvent.StartupFailed] = 9002,
            [KitEvent.ScreenError] = 9101,
            [KitEvent.BackHandled] = 9201,
            [KitEvent.BackNotHandled] = 9202,
            [KitEvent.BackHandlerFailed] = 9203,
            [KitEvent.AppStartHandlerFailed] = 9301,
            [KitEvent.LoadFailedFocusFailed] = 9401,
            [KitEvent.UndoFocusFailed] = 9501,
            [KitEvent.UndoFocusCheckFailed] = 9502,
            [KitEvent.NumberFieldFocusFailed] = 9601,
            [KitEvent.PullRefreshFailed] = 9701,
            [KitEvent.SheetFocusFailed] = 9801,
        }));

    [TestCaseSource(nameof(SourceFiles))]
    public void LoggerMessage_UsesANamedEventId(string file) =>
        Assert.That(BareNumber().IsMatch(File.ReadAllText(file)), Is.False);

    [GeneratedRegex(@"\[LoggerMessage\((\s*\d|[^\]]*?\b(EventId\s*=|eventId\s*:)\s*\d)")]
    private static partial Regex BareNumber();
}
