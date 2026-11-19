using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Theme;

namespace Shields.UI.Tests;

public class ShieldsUiServiceCollectionExtensionsTests
{
    [Test]
    public void AddShieldsUi_RegistersTheThemeServices()
    {
        using var context = new BunitContext();
        var services = new ServiceCollection().AddSingleton(context.JSInterop.JSRuntime).AddShieldsUi();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.Multiple(() =>
        {
            Assert.That(scope.ServiceProvider.GetRequiredService<ThemeService>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<SystemBarInsetWriter>(), Is.Not.Null);
        });
    }
}
