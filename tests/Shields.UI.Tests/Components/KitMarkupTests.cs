using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Layout;

namespace Shields.UI.Tests.Components;

/// <summary>Guards against attribute markup that bUnit's parser accepts but a real browser's setAttribute rejects with InvalidCharacterError.</summary>
public partial class KitMarkupTests : ComponentTestBase
{
    private static readonly System.Buffers.SearchValues<char> InvalidNameChars = System.Buffers.SearchValues.Create("\"'<>=/ \t\r\n");

    [Test]
    public void RenderedAttributeNames_AreValidInTheDom_ForEveryComponentThatRendersWithDefaults()
    {
        var bad = new List<string>();
        var rendered = 0;
        foreach (var type in KitComponents.Exported.Where(t => !t.IsGenericTypeDefinition))
        {
            try
            {
                var cut = Context.Render(Fragment(type));
                rendered++;
                bad.AddRange(InvalidAttributeNames(cut).Select(n => $"{type.Name}: [{n}]"));
            }
            catch (Exception ex) when (ex is not AssertionException)
            {
                // Needs required parameters or services; its source is covered by the source scan.
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(rendered, Is.GreaterThan(10), "too few components rendered with defaults for the guard to mean anything");
            Assert.That(bad, Is.Empty);
        });
    }

    [TestCase("Records")]
    [TestCase(null)]
    public void Card_RendersValidAttributeNames_WithAndWithoutATitle(string? title)
    {
        var cut = Context.Render<Card>(p => p.Add(x => x.Title, title).Add(x => x.TestId, "card"));

        Assert.That(InvalidAttributeNames(cut), Is.Empty);
    }

    [Test]
    public void RazorSource_HasNoDoubledClosingQuoteAfterAnAttributeExpression()
    {
        var offenders = RepoFiles.Find("*.razor", "src")
            .SelectMany(file => File.ReadAllLines(file).Select((line, i) => (file, line, number: i + 1)))
            .Where(x => DoubledQuote().IsMatch(x.line))
            .Select(x => $"{Path.GetRelativePath(RepoFiles.Root, x.file)}:{x.number}: {x.line.Trim()}");

        Assert.That(offenders, Is.Empty);
    }

    private static RenderFragment Fragment(Type type) => builder =>
    {
        builder.OpenComponent(0, type);
        builder.CloseComponent();
    };

    private static IEnumerable<string> InvalidAttributeNames(IRenderedComponent<IComponent> cut) =>
        cut.FindAll("*")
            .SelectMany(e => e.Attributes.Select(a => a.Name))
            .Where(name => name.Length == 0 || name.AsSpan().ContainsAny(InvalidNameChars));

    [GeneratedRegex("=\"[^\"]*@(\\w[\\w.]*|\\([^\"]*\\))\"\"")]
    private static partial Regex DoubledQuote();
}
