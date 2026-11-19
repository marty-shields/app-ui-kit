using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Components.Feedback;
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
    public void AddShieldsUi_KeepsTheHostsClock()
    {
        var clock = new ManualClock();

        using var provider = new ServiceCollection().AddSingleton<TimeProvider>(clock).AddShieldsUi().BuildServiceProvider();

        Assert.That(provider.GetRequiredService<TimeProvider>(), Is.SameAs(clock));
    }
}
