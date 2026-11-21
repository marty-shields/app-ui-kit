namespace Shields.UI.Components.Charts;

/// <summary>How a <see cref="MonthCalendar"/> draws its days.</summary>
public enum CalendarStyle
{
    /// <summary>Each recorded day is a cell filled by how it compares with the target, with a marker for on target and over.</summary>
    Status,

    /// <summary>Each recorded day is a solid circle in <c>--color-primary</c>, a day not recorded is a dashed circle, and the target is ignored.</summary>
    Marks,
}
