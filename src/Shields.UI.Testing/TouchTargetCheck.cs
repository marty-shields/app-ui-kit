using System.Text.RegularExpressions;

namespace Shields.UI.Testing;

/// <summary>A button, input or select found in markup. A button with an aria-label is an icon button, which also needs a minimum width.</summary>
public readonly record struct TouchTargetControl(string Tag, string? CssClass, bool IsIconButton);

/// <summary>Finds buttons, inputs and selects in markup and checks that their scoped CSS sizes them to at least the touch target.</summary>
public static partial class TouchTargetCheck
{
    /// <summary>The value a control's <c>min-height</c> (and an icon button's <c>min-width</c>) must have.</summary>
    public const string TouchTarget = "var(--touch-target)";

    /// <summary>Distinct controls in a piece of markup.</summary>
    public static IReadOnlyList<TouchTargetControl> Controls(string markup) =>
        Element().Matches(markup)
            .Select(m => new TouchTargetControl(
                m.Groups[1].Value,
                FirstClass(m.Groups[2].Value),
                m.Groups[1].Value == "button" && m.Groups[2].Value.Contains("aria-label=", StringComparison.Ordinal)))
            .Distinct()
            .ToList();

    /// <summary>What is wrong with a control given its scoped CSS; empty when it is a touch target.</summary>
    public static IReadOnlyList<string> Problems(TouchTargetControl control, string css)
    {
        if (control.CssClass is null)
        {
            return [$"{control.Tag} has no class that sizes it"];
        }

        var declarations = Declarations(css, $".{control.CssClass}");
        var problems = new List<string>();
        if (!declarations.Contains($"min-height: {TouchTarget}"))
        {
            problems.Add($"{control.Tag}.{control.CssClass} needs min-height: {TouchTarget}");
        }

        if (control.IsIconButton && !declarations.Contains($"min-width: {TouchTarget}"))
        {
            problems.Add($"{control.Tag}.{control.CssClass} is an icon button and needs min-width: {TouchTarget}");
        }

        return problems;
    }

    /// <summary>Problems of every control in the <c>.razor</c> files under a folder, each checked against its <c>.razor.css</c>.</summary>
    public static IReadOnlyList<string> ProblemsInFolder(string folder) =>
        SourceFiles.Find(folder, "*.razor")
            .SelectMany(file => Controls(File.ReadAllText(file)).SelectMany(control =>
                File.Exists(file + ".css")
                    ? Problems(control, File.ReadAllText(file + ".css")).Select(p => $"{Path.GetFileName(file)}: {p}")
                    : [$"{Path.GetFileName(file)}: {control.Tag} has no {Path.GetFileName(file)}.css"]))
            .ToList();

    /// <summary>Trimmed declarations of every rule in a piece of CSS whose selector list contains the selector.</summary>
    public static IReadOnlyList<string> Declarations(string css, string selector) =>
        Rule().Matches(SourceFiles.StripComments(css))
            .Where(m => m.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries).Contains(selector))
            .SelectMany(m => m.Groups[2].Value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .ToList();

    private static string? FirstClass(string attributes) =>
        Class().Match(attributes) is { Success: true } m
            ? m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(c => !c.StartsWith('@'))
            : null;

    // Opening tag of a button, input or select; quoted attribute values may contain '>'.
    [GeneratedRegex("""<(button|input|select)\b((?:[^>"]|"[^"]*")*)>""")]
    private static partial Regex Element();

    [GeneratedRegex("\\bclass=\"([^\"]*)\"")]
    private static partial Regex Class();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex Rule();
}
