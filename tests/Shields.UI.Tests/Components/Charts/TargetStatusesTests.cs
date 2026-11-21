using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class TargetStatusesTests
{
    [TestCase(1799, TargetStatus.Under)]
    [TestCase(1800, TargetStatus.On)]
    [TestCase(2200, TargetStatus.On)]
    [TestCase(2201, TargetStatus.Over)]
    public void Of_JudgesAValueWithinTenPercentOfTheTargetAsOn(double value, TargetStatus expected) =>
        Assert.That(TargetStatuses.Of(value, 2000), Is.EqualTo(expected));

    [TestCase(null)]
    [TestCase(0.0)]
    [TestCase(-1.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Of_WithoutAValidTarget_IsNone(double? target) =>
        Assert.That(TargetStatuses.Of(1000, target), Is.EqualTo(TargetStatus.None));

    [Test]
    public void WordAndClass_NameEachStatus()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Enum.GetValues<TargetStatus>().Select(TargetStatuses.Word), Is.EqualTo(new[] { "", "under target", "on target", "over target" }));
            Assert.That(Enum.GetValues<TargetStatus>().Select(TargetStatuses.Class), Is.EqualTo(new[] { "plain", "under", "on", "over" }));
        });
    }

    [TestCase(1234.5, "1,234.5")]
    [TestCase(7.5, "7.5")]
    [TestCase(999.4, "999.4")]
    [TestCase(1234.567, "1,234.57")]
    [TestCase(2000.0, "2,000")]
    public void Number_GroupsAndKeepsUpToTwoDecimalsTheSameInEveryCulture(double value, string expected) =>
        Assert.That(TargetStatuses.Number(value), Is.EqualTo(expected));

    [TestCase(null, false)]
    [TestCase(0.0, false)]
    [TestCase(double.NaN, false)]
    [TestCase(double.NegativeInfinity, false)]
    [TestCase(1.0, true)]
    public void IsRecorded_IsTrueOnlyForAFiniteValueAboveZero(double? value, bool expected) =>
        Assert.That(TargetStatuses.IsRecorded(value), Is.EqualTo(expected));
}
