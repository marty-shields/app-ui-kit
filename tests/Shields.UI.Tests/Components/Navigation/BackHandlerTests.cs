using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Navigation;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Navigation;

public class BackHandlerTests : ComponentTestBase
{
    private BackButton backButton = null!;

    [SetUp]
    public void RegisterBackButton()
    {
        backButton = new BackButton(NullLogger<BackButton>.Instance);
        Context.Services.AddSingleton<IBackButton>(backButton);
    }

    [Test]
    public void NoOnBack_DoesNotRegister()
    {
        Context.Render<BackHandler>();

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public async Task OnBackSetLater_Registers()
    {
        var back = 0;
        var cut = Context.Render<BackHandler>();

        cut.Render(p => p.Add(x => x.OnBack, () => back++));
        await backButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void OnBackCleared_Unregisters()
    {
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => { }));

        cut.Render(p => p.Add(x => x.OnBack, default(EventCallback)));

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public void OnBackReplacedThenCleared_LeavesNoHandler()
    {
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => { }));
        cut.Render(p => p.Add(x => x.OnBack, () => { }));

        cut.Render(p => p.Add(x => x.OnBack, default(EventCallback)));

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public void Removed_Unregisters()
    {
        RenderFragment handler = b =>
        {
            b.OpenComponent<BackHandler>(0);
            b.AddComponentParameter(1, nameof(BackHandler.OnBack), EventCallback.Factory.Create(this, () => { }));
            b.CloseComponent();
        };
        var cut = Context.Render<ErrorBoundary>(p => p.AddChildContent(handler));

        cut.Render(p => p.Add(x => x.ChildContent, (RenderFragment?)null));

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public async Task OnBackThrows_PassesTheFailureToTheNearestErrorBoundary()
    {
        RenderFragment handler = b =>
        {
            b.OpenComponent<BackHandler>(0);
            b.AddComponentParameter(1, nameof(BackHandler.OnBack), EventCallback.Factory.Create(this, () => throw new InvalidOperationException()));
            b.CloseComponent();
        };
        var cut = Context.Render<ErrorBoundary>(p => p
            .AddChildContent(handler)
            .Add(x => x.ErrorContent, _ => b => b.AddMarkupContent(0, "<p data-testid=\"error-message\">failed</p>")));

        await backButton.HandleAsync();

        Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AttachedAgain_TheHandlerOnScreenGetsTheBackPress()
    {
        var back = 0;
        var activity = new object();
        backButton.Attach(activity);
        Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => back++));

        backButton.Detach(activity);
        backButton.Attach(activity);
        await WaitForRendererAsync();
        await backButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public async Task AttachedToAnotherActivity_TheHandlerRegistersOnce()
    {
        var back = 0;
        Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => back++));

        backButton.Attach(new object());
        backButton.Attach(new object());
        await WaitForRendererAsync();
        await backButton.HandleAsync();
        await backButton.HandleAsync();

        Assert.That(back, Is.EqualTo(2));
    }

    [Test]
    public async Task AttachedTwiceWhileTheRendererIsBusy_LeavesNoHandlerOnceDisposed()
    {
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => { }));
        var busy = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var blocker = Task.Run(() => Context.Renderer.Dispatcher.InvokeAsync(() =>
        {
            busy.SetResult();
            release.Task.Wait();
        }));
        await busy.Task;

        backButton.Attach(new object());
        backButton.Attach(new object());
        release.SetResult();
        await blocker;
        await WaitForRendererAsync();
        cut.Instance.Dispose();

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public async Task AttachedAfterDispose_DoesNotRegister()
    {
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => { }));
        cut.Instance.Dispose();

        backButton.Attach(new object());
        await WaitForRendererAsync();

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public async Task RemovedWhileThePressWaitsForTheRenderer_DoesNotRun()
    {
        var back = 0;
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => back++));
        var busy = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var blocker = Task.Run(() => Context.Renderer.Dispatcher.InvokeAsync(() =>
        {
            busy.SetResult();
            release.Task.Wait();
        }));
        await busy.Task;

        var press = backButton.HandleAsync();
        cut.Instance.Dispose();
        release.SetResult();
        await blocker;
        await press;

        Assert.That(back, Is.Zero);
    }

    [Test]
    public async Task SecondPressWhileTheFirstRuns_DoesNotRunOnBackAgain()
    {
        var back = 0;
        var release = new TaskCompletionSource();
        Context.Render<BackHandler>(p => p.Add(x => x.OnBack, async () =>
        {
            back++;
            await release.Task;
        }));

        var first = backButton.HandleAsync();
        var second = backButton.HandleAsync();
        release.SetResult();
        await Task.WhenAll(first, second);

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public async Task PressAfterTheFirstFinished_RunsOnBackAgain()
    {
        var back = 0;
        Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => back++));

        await backButton.HandleAsync();
        await backButton.HandleAsync();

        Assert.That(back, Is.EqualTo(2));
    }
}
