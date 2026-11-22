namespace Shields.UI.Testing;

internal static class EmbeddedText
{
    public static string Read(string name)
    {
        using var stream = typeof(EmbeddedText).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource {name} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
