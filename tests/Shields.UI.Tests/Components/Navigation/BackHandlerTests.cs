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
}
