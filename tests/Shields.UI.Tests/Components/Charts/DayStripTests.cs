using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Components.Actions;
using Shields.UI.Components.Charts;

namespace Shields.UI.Tests.Components.Charts;

public class DayStripTests : ComponentTestBase
{
    private static readonly DateOnly Monday = new(2026, 11, 16);
    private static readonly DateOnly Wednesday = Monday.AddDays(2);
    private static readonly IReadOnlyList<DateOnly> Week = [.. Enumerable.Range(0, 7).Select(Monday.AddDays)];

    [SetUp]
    public void AddClock() => Context.Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero)));

    [Test]
    public void Strip_IsAListNamedByLabel_WithAColumnPerDay()
    {
        var cut = Render(Wednesday, [Monday], p => p.Add(x => x.Label, "Days this week"));

        var list = cut.Find("ul.days");
        Assert.Multiple(() =>
        {
            Assert.That(list.GetAttribute("aria-label"), Is.EqualTo("Days this week"));
            Assert.That(list.GetAttribute("data-testid"), Is.EqualTo("strip"));
            Assert.That(list.GetAttribute("style"), Is.EqualTo("grid-template-columns: repeat(7, minmax(0, 1fr))"));
            Assert.That(cut.FindAll("li.day"), Has.Count.EqualTo(7));
        });
    }

    [Test]
    public void Days_ShowTheirShortNamesInOrder()
    {
        var cut = Render(Wednesday, []);

        Assert.That(cut.FindAll(".name").Select(n => n.TextContent), Is.EqualTo(new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }));
    }

    [Test]
    public void Culture_SetsTheDayNames()
    {
        var cut = Render(Wednesday, [Monday], p => p.Add(x => x.Culture, CultureInfo.GetCultureInfo("fr-FR")));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".name")[0].TextContent, Is.EqualTo("lun."));
            Assert.That(Hidden(cut, Monday), Is.EqualTo("lundi 16 novembre, done"));
        });
    }

    [Test]
    public void DoneDays_ShowATickAndNotTheDate()
    {
        var cut = Render(Wednesday, [Monday, Wednesday]);

        Assert.Multiple(() =>
        {
            Assert.That(Day(cut, Monday).ClassList, Does.Contain("done"));
            Assert.That(cut.FindComponents<Icon>().Select(i => i.Instance.Name), Is.EqualTo(new[] { IconName.Check, IconName.Check }));
            Assert.That(Day(cut, Monday).QuerySelector(".dot")!.TextContent.Trim(), Is.Empty);
            Assert.That(Day(cut, Wednesday).ClassList, Does.Contain("done"));
        });
    }

    [Test]
    public void DaysNotDone_ShowTheirDate()
    {
        var cut = Render(Wednesday, [Monday]);

        Assert.Multiple(() =>
        {
            Assert.That(Day(cut, Monday.AddDays(1)).ClassList, Does.Not.Contain("done"));
            Assert.That(Day(cut, Monday.AddDays(1)).QuerySelector(".dot")!.TextContent.Trim(), Is.EqualTo("17"));
            Assert.That(Day(cut, Monday.AddDays(6)).QuerySelector(".dot")!.TextContent.Trim(), Is.EqualTo("22"));
        });
    }

    [Test]
    public void Today_IsTheOnlyCurrentDate_AndLaterDaysAreFuture()
    {
        var cut = Render(Wednesday, []);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".today").Select(d => d.GetAttribute("data-testid")), Is.EqualTo(new[] { "strip-2026-11-18" }));
            Assert.That(cut.FindAll("[aria-current=date]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".future").Select(d => d.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "strip-2026-11-19", "strip-2026-11-20", "strip-2026-11-21", "strip-2026-11-22" }));
        });
    }

    [Test]
    public void Today_Unset_IsTodayByTheClock()
    {
        var cut = Context.Render<DayStrip>(p => p.Add(x => x.Days, Week).Add(x => x.TestId, "strip"));

        Assert.That(cut.FindAll(".today").Select(d => d.GetAttribute("data-testid")), Is.EqualTo(new[] { "strip-2026-11-20" }));
    }

    [Test]
    public void EachDay_HasATextAlternativeForItsState_NotJustColour()
    {
        var cut = Render(Wednesday, [Monday, Wednesday]);

        Assert.Multiple(() =>
        {
            Assert.That(Hidden(cut, Monday), Is.EqualTo("Monday 16 November, done"));
            Assert.That(Hidden(cut, Monday.AddDays(1)), Is.EqualTo("Tuesday 17 November, not done"));
            Assert.That(Hidden(cut, Wednesday), Is.EqualTo("Wednesday 18 November, done, today"));
            Assert.That(Hidden(cut, Monday.AddDays(3)), Is.EqualTo("Thursday 19 November, coming up"));
            Assert.That(cut.FindAll(".name, .dot").All(e => e.GetAttribute("aria-hidden") == "true"), Is.True);
        });
    }

    [Test]
    public void StateWords_CanBeReplaced()
    {
        var cut = Render(Wednesday, [Monday], p => p
            .Add(x => x.DoneText, "trained").Add(x => x.EmptyText, "no workout").Add(x => x.FutureText, "later").Add(x => x.TodayText, "now"));

        Assert.Multiple(() =>
        {
            Assert.That(Hidden(cut, Monday), Is.EqualTo("Monday 16 November, trained"));
            Assert.That(Hidden(cut, Wednesday), Is.EqualTo("Wednesday 18 November, no workout, now"));
            Assert.That(Hidden(cut, Monday.AddDays(4)), Is.EqualTo("Friday 20 November, later"));
        });
    }

    [Test]
    public void AnyNumberOfDays_GetsAColumnEach_AndDoneDaysOutsideAreIgnored()
    {
        var cut = Context.Render<DayStrip>(p => p
            .Add(x => x.Days, [Monday, Monday.AddDays(1), Monday.AddDays(2)])
            .Add(x => x.Done, [Monday.AddDays(-1)])
            .Add(x => x.Today, Wednesday));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("ul").GetAttribute("style"), Is.EqualTo("grid-template-columns: repeat(3, minmax(0, 1fr))"));
            Assert.That(cut.FindAll(".done"), Is.Empty);
            Assert.That(cut.FindAll("li[data-testid]"), Is.Empty);
        });
    }

    [Test]
    public void Styles_UseTokensForTheCircleAndStates()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Charts", "DayStrip.razor.css");

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Contain("width: var(--calendar-day-size);"));
            Assert.That(css, Does.Contain("background-color: var(--color-primary);"));
            Assert.That(css, Does.Contain("outline: var(--border-width-error) solid var(--color-text);"));
            Assert.That(css, Does.Contain("opacity: var(--opacity-disabled);"));
        });
    }

    private IRenderedComponent<DayStrip> Render(DateOnly today, IReadOnlyCollection<DateOnly> done, Action<ComponentParameterCollectionBuilder<DayStrip>>? more = null) =>
        Context.Render<DayStrip>(p =>
        {
            p.Add(x => x.Days, Week).Add(x => x.Done, done).Add(x => x.Today, today).Add(x => x.TestId, "strip");
            more?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement Day(IRenderedComponent<DayStrip> cut, DateOnly date) =>
        cut.Find($"[data-testid=strip-{date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}]");

    private static string Hidden(IRenderedComponent<DayStrip> cut, DateOnly date) => Day(cut, date).QuerySelector(".visually-hidden")!.TextContent;
}
