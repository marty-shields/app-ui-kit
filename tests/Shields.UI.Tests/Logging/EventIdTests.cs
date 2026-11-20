using System.Text.RegularExpressions;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Logging;

/// <summary>
/// Event ids are named values: <c>KitEvent</c> keeps the numbers the apps logged. Values are unique and fixed.
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
    public void KitEvent_ValuesAreInTheAppsRange() =>
        Assert.That(Enum.GetValues<KitEvent>().Select(e => (int)e), Is.All.InRange(2000, 4999));

    [Test]
    public void KitEvent_ValuesAreFixed() =>
        Assert.That(Enum.GetValues<KitEvent>().ToDictionary(e => e, e => (int)e), Is.EqualTo(new Dictionary<KitEvent, int>
        {
            [KitEvent.BackHandled] = 2201,
            [KitEvent.BackNotHandled] = 2202,
            [KitEvent.BackHandlerFailed] = 2203,
            [KitEvent.AppStartHandlerFailed] = 3201,
            [KitEvent.LoadFailedFocusFailed] = 3301,
            [KitEvent.UndoFocusFailed] = 3901,
            [KitEvent.UndoFocusCheckFailed] = 3902,
            [KitEvent.NumberFieldFocusFailed] = 3701,
        }));

    [TestCaseSource(nameof(SourceFiles))]
    public void LoggerMessage_UsesANamedEventId(string file) =>
        Assert.That(BareNumber().IsMatch(File.ReadAllText(file)), Is.False);

    [GeneratedRegex(@"\[LoggerMessage\((\s*\d|[^\]]*?\b(EventId\s*=|eventId\s*:)\s*\d)")]
    private static partial Regex BareNumber();
}
