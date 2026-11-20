namespace Shields.UI.Testing;

/// <summary>The kit's tokens.css and base.css, embedded from <c>src/Shields.UI/wwwroot/css</c> when the package is built.</summary>
public static class KitCss
{
    public static string Tokens { get; } = Read("tokens.css");

    public static string Base { get; } = Read("base.css");

    private static string Read(string name)
    {
        using var stream = typeof(KitCss).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource {name} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
