namespace Shields.UI.Components.Lists;

/// <summary>How a <see cref="StatTile"/>'s value moved since the period before.</summary>
/// <param name="Direction">Picks the arrow and its colour; screen readers hear "up", "down" or "no change".</param>
/// <param name="Amount">The size of the change, always positive, e.g. 35m or 12%. Required for up and down; ignored for <see cref="ChangeDirection.None"/>.</param>
/// <exception cref="ArgumentException">Direction is up or down and Amount is empty or blank.</exception>
public sealed record StatChange(ChangeDirection Direction, string Amount)
{
    public string Amount { get; } = Direction != ChangeDirection.None && string.IsNullOrWhiteSpace(Amount)
        ? throw new ArgumentException("Amount is required for an up or down change.", nameof(Amount))
        : Amount;
}
