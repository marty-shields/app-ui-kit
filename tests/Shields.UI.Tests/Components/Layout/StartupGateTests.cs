using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shields.UI.Components.Layout;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Components.Layout;

public class StartupGateTests : ComponentTestBase
{
    private RecordingLogger<StartupGate> logger = null!;

    [SetUp]
    public void AddLogger()
    {
        logger = new RecordingLogger<StartupGate>();
        Context.Services.AddSingleton<ILogger<StartupGate>>(logger);
    }

    [Test]
    public void Pending_ShowsLoadingAndNotContent()
    {
        var init = new TaskCompletionSource();

        var cut = Render(() => init.Task);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public async Task Completed_ShowsContent()
    {
        var init = new TaskCompletionSource();
        var cut = Render(() => init.Task);

        await cut.InvokeAsync(init.SetResult);

        cut.WaitForAssertion(() =>
        {
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Is.Empty);
        });
    }

    [Test]
    public async Task FailsLater_ShowsErrorAndNotContent()
    {
        var init = new TaskCompletionSource();
        var cut = Render(() => init.Task);

        await cut.InvokeAsync(() => init.SetException(new InvalidOperationException()));

        cut.WaitForAssertion(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void Throws_ShowsErrorAndNotContent()
    {
        var cut = Render(() => Task.FromException(new InvalidOperationException()));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=startup-error]").TextContent, Is.EqualTo("Couldn't open the database"));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void Parameters_ReachTheStartupView()
    {
        var init = new TaskCompletionSource();

        var cut = Render(() => init.Task, p => p.Add(x => x.Title, "My App").Add(x => x.LoadingLabel, "Opening…"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("My App"));
            Assert.That(cut.Find("[data-testid=startup-loading]").TextContent.Trim(), Is.EqualTo("Opening…"));
        });
    }

    [Test]
    public void NotRetryable_FailureOffersNoTryAgain()
    {
        var calls = 0;
        var cut = Render(() =>
        {
            calls++;
            return Task.FromException(new InvalidOperationException());
        }, p => p.Add(x => x.Retryable, false));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-retry]"), Is.Empty);
            Assert.That(calls, Is.EqualTo(1));
        });
    }

    [Test]
    public void TryAgain_RunsInitializeAgainAndShowsContentWhenItSucceeds()
    {
        var calls = 0;
        var cut = Render(() => ++calls == 1 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask);

        cut.Find("[data-testid=startup-retry]").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_ShowsLoadingWhileItRuns()
    {
        var calls = 0;
        var again = new TaskCompletionSource();
        var cut = Render(() => ++calls == 1 ? Task.FromException(new InvalidOperationException()) : again.Task);

        cut.Find("[data-testid=startup-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_ShowsTheErrorAgainWhenItFailsAgain()
    {
        var calls = 0;
        var cut = Render(() =>
        {
            calls++;
            return Task.FromException(new InvalidOperationException());
        });

        cut.Find("[data-testid=startup-retry]").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void Completed_LogsTheDuration()
    {
        Render(() => Task.CompletedTask);

        var logged = logger.Records.Single();
        Assert.Multiple(() =>
        {
            Assert.That((logged.Id, logged.Level), Is.EqualTo(((int)KitEvent.StartupFinished, LogLevel.Information)));
            Assert.That(logged.Message, Does.Match(@"^Startup finished in \d+ ms$"));
        });
    }

    [Test]
    public void Throws_LogsTheException()
    {
        var exception = new InvalidOperationException();

        Render(() => Task.FromException(exception));

        Assert.Multiple(() =>
        {
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.StartupFailed, LogLevel.Error, "Startup failed") }));
            Assert.That(logger.LastException, Is.SameAs(exception));
        });
    }

    [Test]
    public void Rerender_RunsInitializeOnce()
    {
        var calls = 0;
        var cut = Render(() =>
        {
            calls++;
            return Task.CompletedTask;
        });

        cut.Render();

        Assert.That(calls, Is.EqualTo(1));
    }

    private IRenderedComponent<StartupGate> Render(Func<Task> initialize, Action<ComponentParameterCollectionBuilder<StartupGate>>? more = null) =>
        Context.Render<StartupGate>(p =>
        {
            p.Add(x => x.Initialize, initialize)
                .Add(x => x.FailedMessage, "Couldn't open the database")
                .AddChildContent("<p data-testid=\"content\">App</p>");
            more?.Invoke(p);
        });
}
