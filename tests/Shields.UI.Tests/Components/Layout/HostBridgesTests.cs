using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Layout;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Layout;

public class HostBridgesTests : ComponentTestBase
{
    private readonly object activity = new();
    private readonly BackButtonBridge backBridge = new();
    private readonly AppLifecycleBridge lifecycleBridge = new();
    private BackButton backButton = null!;
    private AppLifecycle lifecycle = null!;
    private int started;

    [SetUp]
    public void Register()
    {
        backButton = new BackButton(NullLogger<BackButton>.Instance);
        lifecycle = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        started = 0;
        Context.Services.AddSingleton<IBackButton>(backButton)
            .AddSingleton<IAppLifecycle>(lifecycle)
            .AddSingleton(backBridge)
            .AddSingleton(lifecycleBridge);
    }

    [Test]
    public async Task Render_ConnectsTheScopedServicesToTheBridges()
    {
        backBridge.Attach(activity);
        lifecycleBridge.Attach(activity);

        Context.Render<HostBridges>();
        backButton.Register(() => Task.CompletedTask);
        lifecycle.Register(() =>
        {
            started++;
            return Task.CompletedTask;
        });
        await lifecycleBridge.StartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backBridge.Enabled, Is.True);
            Assert.That(started, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Dispose_DisconnectsTheScopedServicesFromTheBridges()
    {
        backBridge.Attach(activity);
        lifecycleBridge.Attach(activity);
        var cut = Context.Render<HostBridges>();
        backButton.Register(() => Task.CompletedTask);
        lifecycle.Register(() =>
        {
            started++;
            return Task.CompletedTask;
        });

        cut.Instance.Dispose();
        await lifecycleBridge.StartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backBridge.Enabled, Is.False);
            Assert.That(backBridge.HandleAsync(), Is.Null);
            Assert.That(started, Is.Zero);
        });
    }
}
