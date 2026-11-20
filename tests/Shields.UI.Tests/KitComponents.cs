using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Actions;

namespace Shields.UI.Tests;

/// <summary>The kit's public component types, found by reflection.</summary>
internal static class KitComponents
{
    public static IReadOnlyList<Type> Exported { get; } =
        typeof(AppButton).Assembly.GetExportedTypes()
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();
}
