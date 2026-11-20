using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shields.UI.Components.Layout;
using Shields.UI.Logging;
using Shields.UI.Stories.Shared;
namespace Shields.UI.Tests.Components.Layout;

public class AppErrorBoundaryTests : ComponentTestBase
{
    private RecordingLogger<AppErrorBoundary> logger = null!;
    private bool broken;

    [SetUp]
    public void AddLogger()
    {
        broken = false;
        logger = new RecordingLogger<AppErrorBoundary>();
        Context.Services.AddSingleton<ILogger<AppErrorBoundary>>(logger);
    }

    [Test]
    public void NoError_ShowsContentAndLogsNothing()
    {
        var cut = Render(() => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=error-message]"), Is.Empty);
            Assert.That(logger.Records, Is.Empty);
        });
    }

    [Test]
    public void ContentThrows_ShowsTheErrorScreenAndLogsTheException()
    {
        broken = true;

        var cut = Render(() => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.ScreenError, LogLevel.Error, "Unhandled error in a screen") }));
            Assert.That(logger.LastException, Is.TypeOf<InvalidOperationException>());
        });
    }

    [Test]
    public void ContentThrows_ShowsTheTitleOnTheErrorScreen()
    {
        broken = true;

        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.Title, "My App")
            .AddChildContent(Content));

        Assert.That(cut.Find(".title").TextContent, Is.EqualTo("My App"));
    }

    [Test]
    public void Restart_RaisesOnRestartThenShowsTheContentAgain()
    {
        broken = true;
        var restarted = 0;
        var cut = Render(() =>
        {
            restarted++;
            broken = false;
        });

        cut.Find("[data-testid=error-restart]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(restarted, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=error-message]"), Is.Empty);
        });
    }

    private IRenderedComponent<AppErrorBoundary> Render(Action onRestart) =>
        Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, onRestart)
            .AddChildContent(Content));

    private void Content(RenderTreeBuilder builder)
    {
        if (broken)
        {
            builder.OpenComponent<ThrowOnRender>(0);
            builder.CloseComponent();
        }
        else
        {
            builder.AddMarkupContent(1, "<p data-testid=\"content\">App</p>");
        }
    }
}
