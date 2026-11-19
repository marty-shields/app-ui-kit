using System.Text.RegularExpressions;
using BlazingStory.Types;
using Microsoft.AspNetCore.Components;
using Shields.UI.Components.Actions;
using Shields.UI.Stories.Shared;

namespace Shields.UI.Tests.Stories;

public partial class StoryCoverageTests
{
    private const string ComponentsNamespace = "Shields.UI.Components.";

    private static readonly Type[] ExportedComponents =
        typeof(AppButton).Assembly.GetExportedTypes()
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

    private static IEnumerable<TestCaseData> Components() =>
        ExportedComponents
            .Where(t => IsInComponents(t))
            .Select(t => (Group: t.Namespace![ComponentsNamespace.Length..].Replace('.', '/'), Name: t.Name.Split('`')[0]))
            .Select(c => new TestCaseData(c.Group, c.Name).SetArgDisplayNames(c.Name));

    private static bool IsInComponents(Type t) => t.Namespace!.StartsWith(ComponentsNamespace);

    [Test]
    public void Components_AreAllInAComponentsGroup()
    {
        Assert.That(ExportedComponents.Where(t => !IsInComponents(t)).Select(t => t.FullName), Is.Empty);
    }

    [TestCaseSource(nameof(Components))]
    public void Component_HasOneStoriesFileInItsGroup(string group, string name)
    {
        var titles = StoriesFiles("Components").Where(f => f.Component == name).Select(f => f.Title);

        Assert.That(titles, Is.EqualTo(new[] { $"Components/{group}/{name}" }));
    }

    [Test]
    public void ComponentStories_ShowOnlyTheirComponent()
    {
        var shown = StoriesFiles("Components").SelectMany(f => f.Shown.Where(s => s != f.Component).Select(s => $"{f.Title}: {s}"));

        Assert.That(shown, Is.Empty);
    }

    [Test]
    public void ComponentStories_AreForComponentsOnly()
    {
        var names = Components().Select(c => (string)c.Arguments[1]!).ToHashSet();

        Assert.That(StoriesFiles("Components").Select(f => f.Component).Where(c => !names.Contains(c)), Is.Empty);
    }

    [Test]
    public void Stories_AreInFoundationsOrComponents()
    {
        var titles = StoriesFiles("").Select(f => f.Title.Split('/')[0]).Distinct();

        Assert.That(titles, Is.SubsetOf(new[] { "Foundations", "Components" }));
    }

    [Test]
    public void Foundations_HaveAnIntroductionPageAndATokensStory()
    {
        var titles = StoriesFiles("Foundations").Select(f => f.Title);

        Assert.That(titles, Is.EqualTo(new[] { "Foundations/Tokens" }));

        var pages = typeof(TokenSheet).Assembly.GetTypes()
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes(typeof(CustomPageAttribute), false).Cast<CustomPageAttribute>())
            .Select(a => a.Title);
        Assert.That(pages, Has.Member("Foundations/Introduction"));
    }

    [Test]
    public void TokensStory_ShowsATokenSheet()
    {
        var tokens = StoriesFiles("Foundations").Single(f => f.Title == "Foundations/Tokens");

        Assert.That(tokens.Shown, Is.EqualTo(new[] { "TokenSheet" }));
    }

    /// <summary>The stories files in a folder under Stories: title, TComponent and the component each story's template starts with.</summary>
    private static IEnumerable<(string Title, string Component, IReadOnlyList<string> Shown)> StoriesFiles(string folder) =>
        RepoFiles.Find("*.stories.razor", "src", "Shields.UI.Stories", "Stories", folder)
            .Select(File.ReadAllText)
            .Select(source => (
                StoriesTitle().Match(source).Groups[1].Value,
                TComponent().Match(source).Groups[1].Value,
                (IReadOnlyList<string>)Shown().Matches(source).Select(m => m.Groups[1].Value).ToList()));

    [GeneratedRegex("""\[Stories\("([^"]+)"\)\]""")]
    private static partial Regex StoriesTitle();

    [GeneratedRegex(@"<Stories\b[^>]*\bTComponent=""([A-Za-z_]\w*)")]
    private static partial Regex TComponent();

    [GeneratedRegex(@"<Template>\s*(?:@\{[^\n]*\}\s*)?<([A-Za-z_]\w*)")]
    private static partial Regex Shown();
}
