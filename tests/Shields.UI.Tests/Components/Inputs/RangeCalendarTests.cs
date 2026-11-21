using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Components.Inputs;

namespace Shields.UI.Tests.Components.Inputs;

public class RangeCalendarTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);
    private static readonly DateOnly Earliest = new(2025, 3, 3);

    private static readonly IReadOnlyDictionary<DateOnly, int> Counts = new Dictionary<DateOnly, int>
    {
        [new DateOnly(2026, 11, 4)] = 4,
        [new DateOnly(2025, 3, 12)] = 3,
        [new DateOnly(2025, 3, 3)] = 1,
        [new DateOnly(2026, 11, 5)] = 0,
    };

    private static readonly DateRange Autumn = new(new(2026, 8, 6), Today);

    [SetUp]
    public void FixTheClock() =>
        Context.Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero)));

    [Test]
    public void Opens_OnTheMonthOfTheLastDayWithBothEndsSet()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=range-from-value]").TextContent, Is.EqualTo("Thu 6 Aug 26"));
            Assert.That(cut.Find("[data-testid=range-to-value]").TextContent, Is.EqualTo("Fri 6 Nov 26"));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Tap the first day"));
        });
    }

    [Test]
    public void Grid_StartsTheWeekOnMonday()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".weekday").Select(d => d.TextContent), Is.EqualTo(new[] { "M", "T", "W", "T", "F", "S", "S" }));
            Assert.That(cut.Find(".days").Children.TakeWhile(c => c.TagName == "SPAN").Count(), Is.EqualTo(7 + 6), "1 Nov 2026 is a Sunday");
        });
    }

    [Test]
    public void Days_AreAGroupNamedAfterTheMonth()
    {
        var days = Render(Autumn).Find(".days");

        Assert.Multiple(() =>
        {
            Assert.That(days.GetAttribute("role"), Is.EqualTo("group"));
            Assert.That(days.GetAttribute("aria-label"), Is.EqualTo("November 2026"));
        });
    }

    [Test]
    public void FirstTap_SetsFromClearsToAndMovesOnToTo()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-day-2026-11-02]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-from-value]").TextContent, Is.EqualTo("Mon 2 Nov 26"));
            Assert.That(cut.Find("[data-testid=range-to-value]").TextContent, Is.EqualTo("Pick a day"));
            Assert.That(cut.Find("[data-testid=range-to]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Tap the last day, or the same day again for one day"));
        });
    }

    [Test]
    public void SecondTap_SetsTo_AndTheNextTapStartsOver()
    {
        var applied = new List<DateRange>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-day-2026-11-05]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(applied, Is.EqualTo(new[] { new DateRange(new(2026, 11, 2), new(2026, 11, 5)) }));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void SecondTapBeforeFrom_SwapsTheEnds()
    {
        var applied = new List<DateRange>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-05]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateRange(new(2026, 11, 2), new(2026, 11, 5)) }));
    }

    [Test]
    public void SameDayTwice_PicksOneDay()
    {
        var applied = new List<DateRange>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateRange(new(2026, 11, 4), new(2026, 11, 4)) }));
    }

    [Test]
    public void Apply_AfterOneTap_PicksThatDay()
    {
        var applied = new List<DateRange>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateRange(new(2026, 11, 4), new(2026, 11, 4)) }));
    }

    [Test]
    public void Days_FillTheRangeAndMarkBothEndsAndToday()
    {
        var cut = Render(new DateRange(new(2026, 11, 2), new(2026, 11, 4)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".day.in").Select(d => d.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "range-day-2026-11-02", "range-day-2026-11-03", "range-day-2026-11-04" }));
            Assert.That(cut.Find(".day.first").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-02"));
            Assert.That(cut.Find(".day.last").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-04"));
            Assert.That(cut.Find(".day.today").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-06"));
            Assert.That(cut.Find(".day.today").GetAttribute("aria-current"), Is.EqualTo("date"));
            Assert.That(cut.FindAll("[aria-current=date]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Days_NameTheirDateTheRangeEndsAndAMark()
    {
        var cut = Render(new DateRange(new(2026, 11, 2), new(2026, 11, 4)), markedDayText: "entry logged");

        Assert.Multiple(() =>
        {
            Assert.That(Hidden(cut, "2026-11-02"), Is.EqualTo("Mon 2 Nov, start"));
            Assert.That(Hidden(cut, "2026-11-03"), Is.EqualTo("Tue 3 Nov"));
            Assert.That(Hidden(cut, "2026-11-04"), Is.EqualTo("Wed 4 Nov, end, entry logged"));
            Assert.That(cut.Find("[data-testid=range-day-2026-11-04] .number").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Days_WithItems_HaveADot_AndOthersNone()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=range-day-2026-11-04] .dot"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=range-day-2026-11-03] .dot"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=range-day-2026-11-05] .dot"), Is.Empty, "a zero count is no mark");
            Assert.That(Hidden(cut, "2026-11-04"), Does.EndWith(", marked"));
        });
    }

    [Test]
    public void WithoutDayCounts_NoDotsAndNoMonthCounts()
    {
        var cut = Context.Render<RangeCalendar>(p => p.Add(x => x.Min, Earliest).Add(x => x.Max, Today).Add(x => x.Dates, Autumn));

        Assert.That(cut.FindAll(".dot"), Is.Empty);

        cut.Find("[data-testid=range-month-name]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".month-count"), Is.Empty);
            Assert.That(cut.Find("[data-testid=range-month-2026-11]").GetAttribute("aria-label"), Is.EqualTo("November 2026"));
        });
    }

    [Test]
    public void Days_AfterMaxOrBeforeMin_AreDisabled()
    {
        var march = Render(new DateRange(new(2025, 3, 3), new(2025, 3, 12)));
        var november = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(march.Find("[data-testid=range-day-2025-03-02]").HasAttribute("disabled"), Is.True);
            Assert.That(march.Find("[data-testid=range-day-2025-03-03]").HasAttribute("disabled"), Is.False);
            Assert.That(november.Find("[data-testid=range-day-2026-11-06]").HasAttribute("disabled"), Is.False);
            Assert.That(november.Find("[data-testid=range-day-2026-11-07]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void MonthArrows_StopAtTheMonthsOfMinAndMax()
    {
        var cut = Render(Autumn);
        var march = Render(new DateRange(new(2025, 3, 3), new(2025, 3, 12)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-next-month]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=range-prev-month]").HasAttribute("disabled"), Is.False);
            Assert.That(march.Find("[data-testid=range-prev-month]").HasAttribute("disabled"), Is.True);
            Assert.That(march.Find("[data-testid=range-next-month]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void PreviousAndNextMonth_ShowThem()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-prev-month]").Click();
        var previous = cut.Find("[data-testid=range-month-name]").TextContent.Trim();
        cut.Find("[data-testid=range-next-month]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(previous, Is.EqualTo("October 2026"));
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("November 2026"));
        });
    }

    [Test]
    public void TappingAnEndBox_ShowsItsMonthAndSetsItNext()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-from]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("August 2026"));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });

        cut.Find("[data-testid=range-to]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=range-to]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void ToBoxThenAnEarlierDay_SwapsTheEnds()
    {
        var applied = new List<DateRange>();
        var cut = Render(new DateRange(new(2026, 11, 4), new(2026, 11, 5)), applied.Add);

        cut.Find("[data-testid=range-to]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateRange(new(2026, 11, 2), new(2026, 11, 4)) }));
    }

    [Test]
    public void MonthName_OpensTheYearsMonthsWithTheirTotals()
    {
        var cut = Render(new DateRange(new(2025, 3, 3), new(2025, 3, 12)));

        cut.Find("[data-testid=range-month-name]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-year]").TextContent, Is.EqualTo("2025"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").GetAttribute("aria-label"), Is.EqualTo("March 2025, 4 entries"));
            Assert.That(cut.Find("[data-testid=range-month-count-2025-03]").TextContent, Is.EqualTo("4"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").GetAttribute("aria-current"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-month-count-2025-04]").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Choose a month"));
            Assert.That(cut.FindAll(".days"), Is.Empty);
        });
    }

    [Test]
    public void CountText_Unset_SaysTheBareNumberInTheMonthsName()
    {
        var cut = Context.Render<RangeCalendar>(p => p
            .Add(x => x.Min, Earliest).Add(x => x.Max, Today).Add(x => x.Dates, Autumn)
            .Add(x => x.DayCounts, Counts).Add(x => x.OpenOnMonths, true));

        Assert.That(cut.Find("[data-testid=range-month-2026-11]").GetAttribute("aria-label"), Is.EqualTo("November 2026, 4"));
    }

    [Test]
    public void Months_OutsideMinAndMax_AreDisabledAndShowNoCount()
    {
        var cut = Render(new DateRange(new(2025, 3, 3), new(2025, 3, 12)), openOnMonths: true);
        var thisYear = Render(Autumn, openOnMonths: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-2025-02]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=range-month-count-2025-02]").TextContent, Is.EqualTo("–"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=range-prev-year]").HasAttribute("disabled"), Is.True);
            Assert.That(thisYear.Find("[data-testid=range-month-2026-11]").HasAttribute("disabled"), Is.False);
            Assert.That(thisYear.Find("[data-testid=range-month-2026-12]").HasAttribute("disabled"), Is.True);
            Assert.That(thisYear.Find("[data-testid=range-next-year]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void ChoosingAMonth_ShowsItsDays()
    {
        var cut = Render(Autumn, openOnMonths: true);

        cut.Find("[data-testid=range-prev-year]").Click();
        cut.Find("[data-testid=range-month-2025-03]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("March 2025"));
            Assert.That(cut.FindAll("[data-testid=range-day-2025-03-12]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Cancel_RaisesOnCancelOnly()
    {
        var applied = new List<DateRange>();
        var cancels = 0;
        var cut = Context.Render<RangeCalendar>(p => p
            .Add(x => x.Min, Earliest).Add(x => x.Max, Today).Add(x => x.Dates, Autumn)
            .Add(x => x.OnApply, applied.Add)
            .Add(x => x.OnCancel, () => cancels++));

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cancels, Is.EqualTo(1));
            Assert.That(applied, Is.Empty);
        });
    }

    [Test]
    public void ApplyLabel_ReplacesTheDefaultApply()
    {
        var defaulted = Render(Autumn);
        var named = Render(Autumn, applyLabel: "Show entries");

        Assert.Multiple(() =>
        {
            Assert.That(defaulted.Find("[data-testid=range-apply]").TextContent.Trim(), Is.EqualTo("Apply"));
            Assert.That(named.Find("[data-testid=range-apply]").TextContent.Trim(), Is.EqualTo("Show entries"));
        });
    }

    [Test]
    public void TestId_PrefixesEveryId()
    {
        var cut = Context.Render<RangeCalendar>(p => p
            .Add(x => x.Min, Earliest).Add(x => x.Max, Today).Add(x => x.Dates, Autumn).Add(x => x.TestId, "trip"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trip]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trip-from],[data-testid=trip-day-2026-11-04],[data-testid=trip-apply]"), Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid^=range]"), Is.Empty);
        });
    }

    [Test]
    public void Today_Unset_UsesTheClock()
    {
        var cut = Context.Render<RangeCalendar>(p => p
            .Add(x => x.Min, Earliest).Add(x => x.Max, Today).Add(x => x.Dates, Autumn));

        Assert.That(cut.Find(".day.today").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-06"));
    }

    private static string Hidden(IRenderedComponent<RangeCalendar> cut, string date) =>
        cut.Find($"[data-testid=range-day-{date}] .visually-hidden").TextContent;

    private IRenderedComponent<RangeCalendar> Render(DateRange dates, Action<DateRange>? apply = null, bool openOnMonths = false, string? markedDayText = null, string? applyLabel = null) =>
        Context.Render<RangeCalendar>(p =>
        {
            p.Add(x => x.Min, Earliest)
                .Add(x => x.Max, Today)
                .Add(x => x.Today, Today)
                .Add(x => x.Dates, dates)
                .Add(x => x.DayCounts, Counts)
                .Add(x => x.CountText, n => $"{n} entries")
                .Add(x => x.OpenOnMonths, openOnMonths)
                .Add(x => x.OnApply, apply ?? (_ => { }));
            if (markedDayText is not null)
            {
                p.Add(x => x.MarkedDayText, markedDayText);
            }

            if (applyLabel is not null)
            {
                p.Add(x => x.ApplyLabel, applyLabel);
            }
        });
}
