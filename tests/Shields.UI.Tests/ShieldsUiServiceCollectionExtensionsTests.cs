using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Feedback;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;
using Shields.UI.Theme;

namespace Shields.UI.Tests;

public class ShieldsUiServiceCollectionExtensionsTests
{
    [Test]
    public void AddShieldsUi_RegistersTheThemeServices()
    {
        using var context = new BunitContext();
        var services = new ServiceCollection().AddScoped(_ => context.JSInterop.JSRuntime).AddShieldsUi();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.Multiple(() =>
        {
            Assert.That(scope.ServiceProvider.GetRequiredService<ThemeService>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<SystemBarInsetWriter>(), Is.Not.Null);
        });
    }

    [Test]
    public void AddShieldsUi_RegistersTheSnackbarServiceAsOneInstanceAcrossScopes()
    {
        using var provider = new ServiceCollection().AddShieldsUi().BuildServiceProvider(validateScopes: true);
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.That(first.ServiceProvider.GetRequiredService<SnackbarService>(),
            Is.SameAs(second.ServiceProvider.GetRequiredService<SnackbarService>()));
    }

    [Test]
    public void AddShieldsUi_KeepsTheHostsOwnSnackbarService()
    {
        using var own = new SnackbarService(TimeProvider.System);
        var services = new ServiceCollection().AddSingleton(own).AddShieldsUi();

        using var provider = services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<SnackbarService>(), Is.SameAs(own));
            Assert.That(services.Count(d => d.ServiceType == typeof(SnackbarService)), Is.EqualTo(1));
        });
    }

    [Test]
    public void AddShieldsUi_RegistersTheBackButtonAndLifecycleAsOneInstanceEach()
    {
        using var provider = new ServiceCollection().AddLogging().AddShieldsUi().BuildServiceProvider(validateScopes: true);
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.Multiple(() =>
        {
            Assert.That(first.ServiceProvider.GetRequiredService<IBackButton>(),
                Is.SameAs(second.ServiceProvider.GetRequiredService<IBackButton>()));
            Assert.That(first.ServiceProvider.GetRequiredService<IAppStart>(), Is.SameAs(first.ServiceProvider.GetRequiredService<AppLifecycle>()));
            Assert.That(first.ServiceProvider.GetRequiredService<IAppLifecycle>(), Is.SameAs(first.ServiceProvider.GetRequiredService<AppLifecycle>()));
        });
    }

    [Test]
    public void AddShieldsUi_KeepsTheHostsBackButton()
    {
        var own = new BackButton(NullLogger<BackButton>.Instance);
        var services = new ServiceCollection().AddSingleton<IBackButton>(own).AddShieldsUi();

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetRequiredService<IBackButton>(), Is.SameAs(own));
    }

    [Test]
    public void AddShieldsUi_KeepsTheHostsAppLifecycle()
    {
        var own = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        using var provider = new ServiceCollection().AddLogging().AddSingleton(own).AddShieldsUi().BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<AppLifecycle>(), Is.SameAs(own));
            Assert.That(provider.GetRequiredService<IAppStart>(), Is.SameAs(own));
            Assert.That(provider.GetRequiredService<IAppLifecycle>(), Is.SameAs(own));
        });
    }

    [Test]
    public void AddShieldsUi_WithAHostLifecycle_DoesNotSplitStartFromLifecycle()
    {
        IAppLifecycle own = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        var services = new ServiceCollection().AddLogging().AddSingleton(own).AddShieldsUi();

        using var provider = services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IAppLifecycle>(), Is.SameAs(own));
            Assert.That(provider.GetService<IAppStart>(), Is.Null);
        });
    }

    [Test]
    public void AddShieldsUi_WithAHostAppStart_DoesNotSplitStartFromLifecycle()
    {
        IAppStart own = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        var services = new ServiceCollection().AddLogging().AddSingleton(own).AddShieldsUi();

        using var provider = services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IAppStart>(), Is.SameAs(own));
            Assert.That(provider.GetService<IAppLifecycle>(), Is.Null);
        });
    }

    [Test]
    public void AddShieldsUi_KeepsTheHostsClock()
    {
        var clock = new ManualClock();

        using var provider = new ServiceCollection().AddSingleton<TimeProvider>(clock).AddShieldsUi().BuildServiceProvider();

        Assert.That(provider.GetRequiredService<TimeProvider>(), Is.SameAs(clock));
    }
}
