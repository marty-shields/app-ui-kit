namespace Shields.UI.Components.Inputs;

/// <summary>How a <see cref="DateRangeField"/> writes its range.</summary>
public enum DateRangeDisplay
{
    /// <summary>Each end in the field's <c>DateFormat</c>, joined by a dash, on one line that ellipsises when too long; one day shows once.</summary>
    EachEnd,

    /// <summary>
    /// A year both ends share shows once (6 Aug – 6 Nov 2026), across years each end has its own (6 Nov 2025 – 6 Nov 2026), and one day shows its weekday (Wed 12 Mar 2025).
    /// The field's <c>DateFormat</c> is not used. Each date is kept whole with no-break spaces, so a narrow field wraps at the dash and never mid-date.
    /// </summary>
    SharedYear,
}
