namespace Shields.UI.Components.Inputs;

/// <summary>A span of days, both ends included. A single day has <see cref="From"/> equal to <see cref="To"/>.</summary>
public readonly record struct DateRange(DateOnly From, DateOnly To);
