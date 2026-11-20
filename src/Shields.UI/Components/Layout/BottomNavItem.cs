using Shields.UI.Components.Actions;

namespace Shields.UI.Components.Layout;

/// <summary>One tab of a <see cref="BottomNav"/>.</summary>
/// <param name="Label">The words under the icon.</param>
/// <param name="Icon">The icon in the tab's pill.</param>
/// <param name="TestId">Value of the tab's data-testid attribute.</param>
public sealed record BottomNavItem(string Label, IconName Icon, string TestId);
