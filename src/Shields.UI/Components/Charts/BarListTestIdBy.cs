namespace Shields.UI.Components.Charts;

/// <summary>What a <see cref="BarList"/> numbers its per-row test ids by.</summary>
public enum BarListTestIdBy
{
    /// <summary>The row's position in the list: <c>-row-0</c>.</summary>
    Position,

    /// <summary>The row's label as a slug (lowercase, runs of other characters as one <c>-</c>, trimmed): <c>-row-north-east</c>. A repeated slug gets <c>-2</c>, <c>-3</c> in list order, so ids only stay the same when rows are added or reordered if the labels are unique.</summary>
    Slug,
}
