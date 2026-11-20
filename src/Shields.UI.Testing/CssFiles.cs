namespace Shields.UI.Testing;

internal static class CssFiles
{
    /// <summary>Files under a folder matching a pattern, excluding bin and obj.</summary>
    public static IEnumerable<string> Find(string folder, string pattern) =>
        Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"));
}
