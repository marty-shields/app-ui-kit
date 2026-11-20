namespace Shields.UI.Components.Lists;

/// <summary>How a <see cref="StatTile"/>'s value moved since the period before.</summary>
/// <param name="Direction">Picks the arrow and its colour; screen readers hear "up", "down" or "no change".</param>
/// <param name="Amount">The size of the change, always positive, e.g. 35m or 12%. Not shown for <see cref="ChangeDirection.None"/>.</param>
public sealed record StatChange(ChangeDirection Direction, string Amount = "");
