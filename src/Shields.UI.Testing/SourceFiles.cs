using System.Text.RegularExpressions;

namespace Shields.UI.Testing;

internal static partial class SourceFiles
{
    /// <summary>Files under a folder matching a pattern, excluding any bin or obj folder inside it. Folders above it do not count.</summary>
    public static IEnumerable<string> Find(string folder, string pattern) =>
        Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories)
            .Where(f => !Path.GetRelativePath(folder, f).Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"));

    /// <summary>Path of a file relative to a folder, with <c>/</c> separators on every platform.</summary>
    public static string Relative(string folder, string file) =>
        Path.GetRelativePath(folder, file).Replace(Path.DirectorySeparatorChar, '/');

    /// <summary>CSS with its <c>/* */</c> comments removed.</summary>
    public static string StripComments(string css) => Comment().Replace(css, "");

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();
}
