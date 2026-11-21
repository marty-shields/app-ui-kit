namespace Shields.UI.Testing;

/// <summary>Names an app's tests compare its own components and tokens against, to stop local copies of what the kit already has.</summary>
public static class KitNames
{
    /// <summary>Type names of the kit's components, without generic arity; <c>SegmentedControl</c> stands for <c>SegmentedControl&lt;T&gt;</c>.</summary>
    public static IReadOnlySet<string> Components { get; } = new HashSet<string>
    {
        "AppButton", "AppErrorBoundary", "AppScreen", "BackHandler", "BarList", "BottomNav", "Card", "ChartLegend", "ConfirmDialog",
        "DateStrip", "EmptyState", "ErrorView", "FieldInput", "FilterChips", "HostBridges", "Icon", "IconButton", "ListRow", "LoadFailed",
        "NumberField", "PeriodStepper", "ProgressRing", "RadioGroup", "SectionHeader", "SegmentedControl", "SelectField", "Snackbar", "Spinner", "StartupGate",
        "StartupView", "StatTile", "TextField", "TrendBarChart",
    };

    /// <summary>
    /// Names of the kit's public types that are not components (services, interfaces, enums, records, the bridges, the DI extensions), without generic arity.
    /// An app's tests compare their own type names against it, so a local copy of a kit type fails the build.
    /// </summary>
    public static IReadOnlySet<string> Types { get; } = new HashSet<string>
    {
        "AppLifecycle", "AppLifecycleBridge", "BackButton", "BackButtonBridge", "BarListItem", "BottomNavItem",
        "ButtonVariant", "ChangeDirection", "DataSeries", "DataSeriesExtensions", "DayLabel", "IAppLifecycle", "IAppStart", "IBackButton", "IconButtonStyle",
        "IconName", "IconSize", "IconTone", "LegendItem", "LegendSwatch", "PeriodKind", "RadioOption",
        "ShieldsUiServiceCollectionExtensions", "SnackbarMessage", "SnackbarService", "StatChange", "TargetStatus", "TargetStatuses", "TrendBar",
        "SystemBarInsetWriter", "ThemePreference", "ThemeService"
    };

    /// <summary>Every custom property the kit declares in tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Tokens => DesignTokens.Names;
}
