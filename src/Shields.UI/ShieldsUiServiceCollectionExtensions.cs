using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shields.UI.Components.Feedback;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;
using Shields.UI.Theme;

namespace Shields.UI;

public static class ShieldsUiServiceCollectionExtensions
{
    /// <summary>Registers the kit's services (including the back button and app lifecycle singletons), and <see cref="TimeProvider.System"/> unless the host has set a clock. The <see cref="SnackbarService"/> is a singleton, unless the host registered its own: a message posted before a screen change must reach the next screen. One per app process suits MAUI Blazor Hybrid and WebAssembly, not Blazor Server, where every circuit would share it. The host must also load <c>_content/Shields.UI/theme.js</c> with a <c>data-storage-key</c>.</summary>
    public static IServiceCollection AddShieldsUi(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<SnackbarService>();
        services.TryAddSingleton<IBackButton, BackButton>();
        services.TryAddSingleton<AppLifecycle>();
        services.TryAddSingleton<IAppStart>(p => p.GetRequiredService<AppLifecycle>());
        services.TryAddSingleton<IAppLifecycle>(p => p.GetRequiredService<AppLifecycle>());
        return services
            .AddScoped<ThemeService>()
            .AddScoped<SystemBarInsetWriter>();
    }
}
