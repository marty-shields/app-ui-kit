using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Components.Inputs;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Inputs;

public class DateRangeFieldTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);
    private static readonly DateOnly First = new(2025, 3, 12);
    private static readonly IReadOnlyDictionary<DateOnly, int> PerDay = new Dictionary<DateOnly, int> { [First] = 1 };

    private IBackButton BackButton => Context.Services.GetRequiredService<IBackButton>();

    [SetUp]
    public void AddKitServices()
    {
        Context.Services.AddShieldsUi();
        Context.Services.AddSingleton<TimeProvider>(new ManualClock());
    }

    [TestCase(2026, 8, 6, 2026, 11, 6, "6 Aug 2026 – 6 Nov 2026")]
    [TestCase(2025, 3, 12, 2025, 3, 12, "12 Mar 2025")]
    public void Field_ShowsTheDatesOnceForOneDay_AndNamesThemWithTheCount(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay, string text)
    {
        var cut = Render(new DateRange(new(fromYear, fromMonth, fromDay), new(toYear, toMonth, toDay)), count: 1);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dates-dates]").TextContent, Is.EqualTo(text));
            Assert.That(cut.Find("[data-testid=dates-count]").TextContent, Is.EqualTo("1"));
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-label"), Is.EqualTo($"Dates shown: {text}. 1"));
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-expanded"), Is.EqualTo("false"));
            Assert.That(cut.FindAll("[data-testid=dates-calendar]"), Is.Empty);
        });
    }

    [Test]
    public void CountText_WordsTheCountInTheAccessibleName()
    {
        var cut = Render(new DateRange(First, Today), count: 38, more: p => p.Add(x => x.CountText, n => $"{n} entries").Add(x => x.Label, "Range"));

        Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-label"), Is.EqualTo("Range: 12 Mar 2025 – 6 Nov 2026. 38 entries"));
    }

    [Test]
    public void WithoutACount_ShowsNoCountAndTheNameHasNoneEither()
    {
        var cut = Render(new DateRange(First, Today), count: null);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=dates-count]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-label"), Is.EqualTo("Dates shown: 12 Mar 2025 – 6 Nov 2026"));
        });
    }

    [Test]
    public void DateFormat_ShapesEachDate()
    {
        var cut = Render(new DateRange(new(2026, 8, 6), Today), more: p => p.Add(x => x.DateFormat, "d MMM"));

        Assert.That(cut.Find("[data-testid=dates-dates]").TextContent, Is.EqualTo("6 Aug – 6 Nov"));
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void TappingTheField_RaisesExpandedChangedWithTheOtherState(bool expanded, bool raised)
    {
        var changes = new List<bool>();
        var cut = Render(new DateRange(Today, Today), expanded: expanded, expandedChanged: changes.Add);

        cut.Find("[data-testid=dates]").Click();

        Assert.That(changes, Is.EqualTo(new[] { raised }));
    }

    [Test]
    public void Expanded_ShowsTheCalendarWithTheCurrentDatesAndTheMarkedDays()
    {
        var cut = Render(new DateRange(new(2026, 8, 6), Today), expanded: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-expanded"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid=dates-calendar]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=dates-calendar-from-value]").TextContent, Is.EqualTo("Thu 6 Aug 26"));
            Assert.That(cut.Find("[data-testid=dates-calendar-to-value]").TextContent, Is.EqualTo("Fri 6 Nov 26"));
        });
    }

    [Test]
    public void Calendar_UsesTheMinMaxAndTheDayCounts()
    {
        var cut = Render(new DateRange(new(2025, 3, 1), new(2025, 3, 31)), expanded: true, more: p => p.Add(x => x.CountText, n => $"{n} entries"));

        cut.Find("[data-testid=dates-calendar-day-2025-03-11]");
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dates-calendar-day-2025-03-11]").HasAttribute("disabled"), Is.True, "before Min");
            Assert.That(cut.Find("[data-testid=dates-calendar-day-2025-03-12]").HasAttribute("disabled"), Is.False, "Min itself");
            Assert.That(cut.Find("[data-testid=dates-calendar-day-2025-03-12] .visually-hidden").TextContent, Does.Contain("entry logged"));
        });
    }

    [Test]
    public void Apply_ClosesThenRaisesDatesChanged()
    {
        var events = new List<string>();
        var cut = Render(new DateRange(new(2026, 8, 6), Today), expanded: true, expandedChanged: e => events.Add($"expanded {e}"),
            datesChanged: d => events.Add($"dates {d.From:yyyy-MM-dd} {d.To:yyyy-MM-dd}"));

        cut.Find("[data-testid=dates-calendar-day-2026-11-02]").Click();
        cut.Find("[data-testid=dates-calendar-apply]").Click();

        Assert.That(events, Is.EqualTo(new[] { "expanded False", "dates 2026-11-02 2026-11-02" }));
    }

    [Test]
    public void Cancel_ClosesWithoutChangingTheDates()
    {
        var events = new List<string>();
        var cut = Render(new DateRange(new(2026, 8, 6), Today), expanded: true, expandedChanged: e => events.Add($"expanded {e}"), datesChanged: _ => events.Add("dates"));

        cut.Find("[data-testid=dates-calendar-day-2026-11-02]").Click();
        cut.Find("[data-testid=dates-calendar-cancel]").Click();

        Assert.That(events, Is.EqualTo(new[] { "expanded False" }));
    }

    [Test]
    public async Task AndroidBack_WhileExpanded_Closes()
    {
        var changes = new List<bool>();
        var cut = Render(new DateRange(Today, Today), expanded: true, expandedChanged: changes.Add);

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(changes, Is.EqualTo(new[] { false }));
    }

    [Test]
    public void Closed_DoesNotHandleAndroidBack()
    {
        Render(new DateRange(Today, Today));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    private IRenderedComponent<DateRangeField> Render(
        DateRange dates, int? count = 0, bool expanded = false, Action<bool>? expandedChanged = null, Action<DateRange>? datesChanged = null,
        Action<ComponentParameterCollectionBuilder<DateRangeField>>? more = null) =>
        Context.Render<DateRangeField>(p =>
        {
            p.Add(x => x.Dates, dates)
                .Add(x => x.Count, count)
                .Add(x => x.Min, First)
                .Add(x => x.Max, Today)
                .Add(x => x.Today, Today)
                .Add(x => x.DayCounts, PerDay)
                .Add(x => x.MarkedDayText, "entry logged")
                .Add(x => x.Expanded, expanded)
                .Add(x => x.ExpandedChanged, expandedChanged ?? (_ => { }))
                .Add(x => x.DatesChanged, datesChanged ?? (_ => { }))
                .Add(x => x.TestId, "dates");
            more?.Invoke(p);
        });
}
