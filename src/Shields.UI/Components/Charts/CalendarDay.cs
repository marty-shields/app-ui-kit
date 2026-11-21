namespace Shields.UI.Components.Charts;

/// <summary>One day of a <see cref="MonthCalendar"/> that has, or may have, a recorded value.</summary>
/// <param name="Date">Matched against each day of the month; a date outside the month is ignored.</param>
/// <param name="Value">What the day shows. Null, NaN, infinity, or zero or less, means nothing was recorded.</param>
/// <param name="Target">The target that day had, when it differs from the calendar's.</param>
public sealed record CalendarDay(DateOnly Date, double? Value, double? Target = null);
