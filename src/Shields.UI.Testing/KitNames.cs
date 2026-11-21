namespace Shields.UI.Testing;

/// <summary>Names an app's tests compare its own components and tokens against, to stop local copies of what the kit already has.</summary>
public static class KitNames
{
    /// <summary>Type names of the kit's components, without generic arity; <c>SegmentedControl</c> stands for <c>SegmentedControl&lt;T&gt;</c>.</summary>
    public static IReadOnlySet<string> Components { get; } = new HashSet<string>
    {
        "AppButton", "AppErrorBoundary", "AppScreen", "BackHandler", "Banner", "BarList", "BottomNav", "Card", "ChartLegend", "ConfirmDialog",
        "DateStrip", "Divider", "DurationField", "EmptyState", "ErrorView", "FieldInput", "FilterChips", "HostBridges", "Icon", "IconButton", "InlineAlert", "ListRow", "LoadFailed",
        "MonthCalendar", "NumberField", "PeriodStepper", "ProgressRing", "RadioGroup", "RangeCalendar", "SearchField", "SectionHeader", "SegmentedControl", "SelectField", "Sheet", "SheetAction", "Snackbar", "Spinner", "StackedBar", "StartupGate",
        "StartupView", "StatTile", "Text", "TextField", "TrendBarChart", "ValueField",
    };

    /// <summary>
    /// Names of the kit's public types that are not components (services, interfaces, enums, records, the bridges, the DI extensions), without generic arity.
    /// An app's tests compare their own type names against it, so a local copy of a kit type fails the build.
    /// </summary>
    public static IReadOnlySet<string> Types { get; } = new HashSet<string>
    {
        "AlertTone", "AppLifecycle", "AppLifecycleBridge", "BackButton", "BackButtonBridge", "BannerTone", "BarListItem", "BarListLabelWidth", "BarListTestIdBy", "BarListValueWidth", "BottomNavItem",
        "ButtonVariant", "CalendarDay", "CalendarStyle", "ChangeDirection", "DataSeries", "DataSeriesExtensions", "DateRange", "DayLabel", "IAppLifecycle", "IAppStart", "IBackButton", "IconButtonStyle",
        "IconName", "IconSize", "IconTone", "KitEvent", "LegendItem", "LegendSwatch", "PeriodKind", "RadioOption",
        "ShieldsUiServiceCollectionExtensions", "SnackbarMessage", "SnackbarService", "StackedBarSegment", "StatChange",
        "SystemBarInsetWriter", "TargetStatus", "TargetStatuses", "TextElement", "TextVariant", "ThemePreference", "ThemeService", "TrendBarPoint", "TrendBarStyle", "ValueKind"
    };

    /// <summary>Every custom property the kit declares in tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Tokens => DesignTokens.Names;
}
