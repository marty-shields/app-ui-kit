using System.Globalization;

namespace Shields.UI.Components.Charts;

/// <summary>Compares a value with its target, and the words, CSS classes and number format that go with the result.</summary>
public static class TargetStatuses
{
    /// <summary>A value is on target within this share either side of its target.</summary>
    public const double Tolerance = 0.1;

    /// <summary>True for a value that counts as recorded: finite and above zero. NaN and infinity do not.</summary>
    public static bool IsRecorded(double? value) => value is > 0 and < double.PositiveInfinity;

    /// <summary>The target when it is finite and above zero, otherwise null (no target).</summary>
    public static double? ValidTarget(double? target) => IsRecorded(target) ? target : null;

    /// <summary>The status of the value against the target, within <see cref="Tolerance"/>; None without a valid target.</summary>
    public static TargetStatus Of(double value, double? target)
    {
        target = ValidTarget(target);
        if (target is null)
        {
            return TargetStatus.None;
        }

        return value < target.Value * (1 - Tolerance) - 1e-9 ? TargetStatus.Under
            : value > target.Value * (1 + Tolerance) + 1e-9 ? TargetStatus.Over
            : TargetStatus.On;
    }

    /// <summary>The words a screen reader gets for a status; empty for None.</summary>
    public static string Word(TargetStatus status) => status switch
    {
        TargetStatus.Under => "under target",
        TargetStatus.On => "on target",
        TargetStatus.Over => "over target",
        _ => "",
    };

    /// <summary>The CSS class of a status; "plain" when there is none.</summary>
    public static string Class(TargetStatus status) => status switch
    {
        TargetStatus.Under => "under",
        TargetStatus.On => "on",
        TargetStatus.Over => "over",
        _ => "plain",
    };

    /// <summary>The value with thousands separators and up to two decimals (7.5 stays 7.5), the same in every culture.</summary>
    public static string Number(double value) => value.ToString("#,##0.##", CultureInfo.InvariantCulture);
}
