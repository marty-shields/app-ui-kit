namespace Shields.UI.Testing;

/// <summary>Names an app's tests compare its own components and tokens against, to stop local copies of what the kit already has.</summary>
public static class KitNames
{
    /// <summary>Type names of the kit's components, without generic arity; <c>SegmentedControl</c> stands for <c>SegmentedControl&lt;T&gt;</c>.</summary>
    public static IReadOnlySet<string> Components { get; } = new HashSet<string>
    {
        "AppButton", "AppErrorBoundary", "AppScreen", "BackHandler", "BottomNav", "Card", "ChartLegend", "ConfirmDialog",
        "DateStrip", "EmptyState", "ErrorView", "FieldInput", "FilterChips", "Icon", "IconButton", "ListRow", "LoadFailed",
        "NumberField", "PeriodStepper", "RadioGroup", "SegmentedControl", "SelectField", "Snackbar", "Spinner", "StartupGate",
        "StartupView", "StatTile", "TextField",
    };

    /// <summary>Every custom property the kit declares in tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Tokens => DesignTokens.Names;
}
