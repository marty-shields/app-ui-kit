using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Theme;

namespace Shields.UI;

public static class ShieldsUiServiceCollectionExtensions
{
    /// <summary>Registers the kit's services. The host must also load <c>_content/Shields.UI/theme.js</c> with a <c>data-storage-key</c>.</summary>
    public static IServiceCollection AddShieldsUi(this IServiceCollection services) =>
        services
            .AddScoped<ThemeService>()
            .AddScoped<SystemBarInsetWriter>();
}
