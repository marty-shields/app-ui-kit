using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Shields.UI.Components.Feedback;
using Shields.UI.Logging;

namespace Shields.UI.Tests.Components.Feedback;

public class LoadFailedTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheMessageAsAnAlertAndTryAgain()
    {
        var cut = Render();

        var error = cut.Find("[data-testid=dashboard-error]");
        Assert.Multiple(() =>
        {
            Assert.That(error.TextContent, Is.EqualTo("Couldn't load the Dashboard"));
            Assert.That(error.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=dashboard-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
        });
    }

    [Test]
    public void TryAgain_IsAPrimaryButtonNotThePaleSecondaryOne()
    {
        var classes = Render().Find("[data-testid=dashboard-retry]").ClassList;

        Assert.Multiple(() =>
        {
            Assert.That(classes, Does.Contain("primary"));
            Assert.That(classes, Does.Not.Contain("secondary"));
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Render(() => retries++);

        cut.Find("[data-testid=dashboard-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void FirstRender_FocusesTryAgain()
    {
        var cut = Render();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=dashboard-retry]")));
    }

    [Test]
    public void RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Render();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Message, "Couldn't load the Dashboard"));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FocusFails_LogsAWarningWithEvent3301AndKeepsTheError()
    {
        var logger = new RecordingLogger();
        Context.Services.AddSingleton<ILogger<LoadFailed>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Render();

        cut.WaitForAssertion(() => Assert.That(logger.Records, Is.EqualTo(new[] { (3301, LogLevel.Warning, "Focusing Try again failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LastException, Is.TypeOf<JSException>());
            Assert.That(logger.Records.Single().Id, Is.EqualTo((int)KitEvent.LoadFailedFocusFailed));
            Assert.That(cut.FindAll("[data-testid=dashboard-retry]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void FocusAfterDisconnect_LogsNothingAndKeepsTheError()
    {
        var logger = new RecordingLogger();
        Context.Services.AddSingleton<ILogger<LoadFailed>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSDisconnectedException("Gone"));

        var cut = Render();

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(logger.Records, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=dashboard-retry]"), Has.Count.EqualTo(1));
        });
    }

    private IRenderedComponent<LoadFailed> Render(Action? onRetry = null) =>
        Context.Render<LoadFailed>(p => p
            .Add(x => x.Message, "Couldn't load the Dashboard")
            .Add(x => x.TestId, "dashboard")
            .Add(x => x.OnRetry, onRetry ?? (() => { })));

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    private sealed class RecordingLogger : ILogger<LoadFailed>
    {
        private readonly List<(int Id, LogLevel Level, string Message)> records = [];

        public IReadOnlyList<(int Id, LogLevel Level, string Message)> Records => records;

        public Exception? LastException { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            records.Add((eventId.Id, logLevel, formatter(state, exception)));
            LastException = exception;
        }
    }
}
