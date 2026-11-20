using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Shields.UI.Components.Actions;
using Shields.UI.Components.Layout;
using Shields.UI.Logging;
using Shields.UI.Navigation;

namespace Shields.UI.Tests.Components.Layout;

public class AppScreenTests : ComponentTestBase
{
    private const string AppScreenModule = "./_content/Shields.UI/Components/Layout/AppScreen.razor.js";

    private static readonly BottomNavItem Today = new("Today", IconName.Today, "today-link");
    private static readonly BottomNavItem Trends = new("Trends", IconName.BarChart, "trends-link");
    private static readonly IReadOnlyList<BottomNavItem> Tabs = [Today, Trends];

    private IBackButton BackButton => Context.Services.GetRequiredService<IBackButton>();

    [SetUp]
    public void AddKitServices() => Context.Services.AddShieldsUi();

    private IRenderedComponent<AppScreen> Render(string title, Action<ComponentParameterCollectionBuilder<AppScreen>>? more = null) =>
        Context.Render<AppScreen>(p =>
        {
            p.Add(x => x.Title, title);
            more?.Invoke(p);
        });

    private IRenderedComponent<AppScreen> RenderTopLevel(BottomNavItem current, Action<ComponentParameterCollectionBuilder<AppScreen>>? more = null) =>
        Render(current.Label, p =>
        {
            p.Add(x => x.NavItems, Tabs).Add(x => x.Current, current).Add(x => x.AddLabel, "Add").Add(x => x.AddIcon, IconName.Plus);
            more?.Invoke(p);
        });

    [Test]
    public void Render_ShowsTitleAndSubtitle()
    {
        var cut = Render("Screen title", p => p.Add(x => x.Subtitle, "Elapsed 0:01:00"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Screen title"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 0:01:00"));
        });
    }

    [Test]
    public void NoSubtitle_ShowsNone()
    {
        var cut = Render("Screen title");

        Assert.That(cut.FindAll("[data-testid=screen-subtitle]"), Is.Empty);
    }

    [Test]
    public void NoOnBack_HidesBackArrow()
    {
        var cut = Render("My App");

        Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
    }

    [Test]
    public void BackArrow_RaisesOnBack()
    {
        var back = false;
        var cut = Render("Screen title", p => p.Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }

    [Test]
    public void BackArrow_IsTheBackIcon()
    {
        var cut = Render("Screen title", p => p.Add(x => x.OnBack, () => { }));

        var back = cut.FindComponent<IconButton>().Instance;

        Assert.That((back.TestId, back.Icon, back.Muted, back.Size), Is.EqualTo(("back", IconName.Back, false, IconSize.Default)));
    }

    [Test]
    public void BackTestId_NamesTheBackArrow()
    {
        var cut = Render("Screen title", p => p.Add(x => x.OnBack, () => { }).Add(x => x.BackTestId, "example-back"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=example-back]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
        });
    }

    [Test]
    public async Task AndroidBack_RaisesOnBack()
    {
        var back = 0;
        Render("Screen title", p => p.Add(x => x.OnBack, () => back++));

        await BackButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void NoOnBack_DoesNotTakeAndroidBack()
    {
        Render("My App");

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void Actions_ShowInTheTopBar()
    {
        var cut = Render("Screen title", p => p.Add(x => x.Actions, "<button data-testid=\"action\">Edit</button>"));

        Assert.That(cut.FindAll("header [data-testid=action]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Toolbar_RendersOnlyWhenSet()
    {
        var without = Render("Screen title");
        var with = Render("Screen title", p => p.Add(x => x.Toolbar, "<p data-testid=\"filters\">Filters</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(without.FindAll(".toolbar"), Is.Empty);
            Assert.That(with.FindAll(".toolbar [data-testid=filters]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void ChildContent_ShowsInTheMainRegion()
    {
        var cut = Render("Screen title", p => p.AddChildContent("<p data-testid=\"shown\">Rows</p>"));

        Assert.That(cut.FindAll("main [data-testid=shown]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void BottomBar_RendersOnlyWhenSet()
    {
        var without = Render("Screen title");
        var with = Render("My App", p => p.Add(x => x.BottomBar, (RenderFragment)(b => b.AddContent(0, "Main action"))));

        Assert.Multiple(() =>
        {
            Assert.That(without.FindAll("[data-testid=bottom-bar]"), Is.Empty);
            Assert.That(with.Find("[data-testid=bottom-bar]").TextContent, Does.Contain("Main action"));
        });
    }

    [Test]
    public void NoNavItems_HidesTheNavigationBar()
    {
        var cut = Render("Settings");

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Is.Empty);
            Assert.That(cut.Find(".screen").ClassList, Does.Not.Contain("with-nav"));
        });
    }

    [Test]
    public void EmptyNavItems_HidesTheNavigationBarAndFooter()
    {
        var cut = Render("Settings", p => p.Add(x => x.NavItems, []));

        Assert.That(cut.FindAll("footer"), Is.Empty);
    }

    [Test]
    public void NavItems_ShowTheNavigationBarUnderTheBottomBarWithTheCurrentItemMarked()
    {
        var cut = RenderTopLevel(Today, p => p.Add(x => x.BottomBar, (RenderFragment)(b => b.AddContent(0, "Main action"))));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("footer > *").Select(e => e.GetAttribute("data-testid")), Is.EqualTo(new[] { "bottom-bar", "bottom-nav" }));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("today-link"));
            Assert.That(cut.Find(".screen").ClassList, Does.Contain("with-nav"));
        });
    }

    [Test]
    public void NavItemsAlone_ShowTheFooter()
    {
        var cut = RenderTopLevel(Today);

        Assert.That(cut.FindAll("footer [data-testid=bottom-nav]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void TheRaisedButton_PassesItsLabelIconPopupAndAttributesToTheBar()
    {
        var cut = RenderTopLevel(Today, p => p
            .Add(x => x.AddHasPopup, "dialog")
            .Add(x => x.AddButtonAttributes, new Dictionary<string, object> { ["data-sheet-opener"] = "" }));

        var add = cut.Find("[data-testid=add-button]");
        Assert.Multiple(() =>
        {
            Assert.That(add.TextContent.Trim(), Is.EqualTo("Add"));
            Assert.That(add.GetAttribute("aria-haspopup"), Is.EqualTo("dialog"));
            Assert.That(add.HasAttribute("data-sheet-opener"), Is.True);
        });
    }

    [Test]
    public void TheRaisedButton_RaisesOnAdd()
    {
        var added = 0;
        var cut = RenderTopLevel(Today, p => p.Add(x => x.OnAdd, () => added++));

        cut.Find("[data-testid=add-button]").Click();

        Assert.That(added, Is.EqualTo(1));
    }

    [Test]
    public void TappingAnotherItem_RaisesOnNavigateWithIt()
    {
        var navigated = new List<BottomNavItem>();
        var cut = RenderTopLevel(Today, p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=trends-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { Trends }));
    }

    [Test]
    public async Task TopLevelScreenWithOnBack_AndroidBackRaisesOnBack()
    {
        var back = 0;
        RenderTopLevel(Today, p => p.Add(x => x.OnBack, () => back++));

        await BackButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void TheStartItem_DoesNotTakeAndroidBack()
    {
        RenderTopLevel(Today, p => p.Add(x => x.OnNavigate, (BottomNavItem _) => { }));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public async Task AnotherItem_AndroidBackGoesToTheFirstItem_WithNoBackArrow()
    {
        var navigated = new List<BottomNavItem>();
        var cut = RenderTopLevel(Trends, p => p.Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(navigated, Is.EqualTo(new[] { Today }));
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
        });
    }

    [Test]
    public async Task AnotherItem_WithItsOwnOnBack_AndroidBackRaisesThatInstead()
    {
        var navigated = new List<BottomNavItem>();
        var back = 0;
        RenderTopLevel(Trends, p => p.Add(x => x.OnBack, () => back++).Add(x => x.OnNavigate, navigated.Add));

        await BackButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(back, Is.EqualTo(1));
            Assert.That(navigated, Is.Empty);
        });
    }

    [Test]
    public void AnotherItem_WithNoOnNavigate_DoesNotTakeAndroidBack()
    {
        RenderTopLevel(Trends);

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void NoCurrentItem_DoesNotTakeAndroidBack()
    {
        Render("Details", p => p.Add(x => x.NavItems, Tabs).Add(x => x.AddLabel, "Add").Add(x => x.AddIcon, IconName.Plus));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void Status_SitsInAPoliteLiveRegionAboveTheBottomBar()
    {
        var cut = Render("Screen title", p => p
            .Add(x => x.BottomBar, "<button>Save</button>")
            .Add(x => x.Status, "<p data-testid=\"shown\">Set 2 deleted</p>"));

        var slot = cut.Find("footer > [role=status]");
        Assert.Multiple(() =>
        {
            Assert.That(slot.GetAttribute("aria-live"), Is.EqualTo("polite"));
            Assert.That(slot.QuerySelector("[data-testid=shown]"), Is.Not.Null);
            Assert.That(slot.NextElementSibling!.GetAttribute("data-testid"), Is.EqualTo("bottom-bar"));
        });
    }

    [Test]
    public void Status_WithTheNavigationBar_IsAboveItInTheFooter()
    {
        var cut = RenderTopLevel(Today, p => p.Add(x => x.Status, "<p>Saved</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".screen").ClassList, Does.Contain("with-nav"));
            Assert.That(cut.Find("footer > [role=status]").NextElementSibling!.GetAttribute("data-testid"), Is.EqualTo("bottom-nav"));
        });
    }

    [Test]
    public void Status_WithNeitherBottomBarNorNavigation_StillShows()
    {
        var cut = Render("Screen title", p => p.Add(x => x.Status, "<p data-testid=\"shown\">Oats added to Lunch</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("footer > [role=status] [data-testid=shown]"), Is.Not.Null);
            Assert.That(cut.FindAll("[data-testid=bottom-bar]"), Is.Empty);
        });
    }

    [Test]
    public void NoStatus_HasNoLiveRegion()
    {
        var cut = Render("Screen title", p => p.Add(x => x.BottomBar, "<button>Save</button>"));

        Assert.That(cut.FindAll("[role=status]"), Is.Empty);
    }

    [Test]
    public void NoOnRefresh_HasNoPullGesture()
    {
        var cut = Render("Settings");

        Assert.Multiple(() =>
        {
            Assert.That(Context.JSInterop.Invocations, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=pull-refresh]"), Is.Empty);
        });
    }

    [Test]
    public void OnRefresh_AttachesThePullGestureToTheContent()
    {
        var module = PullModule();

        var cut = Render("Screen title", p => p.Add(x => x.OnRefresh, () => Task.CompletedTask));

        var attach = module.Invocations["attach"].Single();
        Assert.Multiple(() =>
        {
            attach.Arguments[0].ShouldBeElementReferenceTo(cut.Find("main"));
            attach.Arguments[1].ShouldBeElementReferenceTo(cut.Find("[data-testid=pull-refresh]"));
        });
    }

    [Test]
    public async Task Refresh_RunsOnRefreshAndShowsNothingOfItsOwn()
    {
        PullModule();
        var done = new TaskCompletionSource();
        var cut = Render("Screen title", p => p.Add(x => x.OnRefresh, () => done.Task));
        var idle = cut.Markup;

        var refresh = cut.InvokeAsync(cut.Instance.RefreshAsync);

        Assert.Multiple(() =>
        {
            Assert.That(refresh.IsCompleted, Is.False);
            Assert.That(cut.Markup, Is.EqualTo(idle));
            Assert.That(cut.FindAll("[role=status]"), Is.Empty);
        });
        done.SetResult();
        await refresh;
    }

    [Test]
    public async Task Refresh_DuringARefresh_IsIgnored()
    {
        PullModule();
        var done = new TaskCompletionSource();
        var calls = 0;
        var cut = Render("Screen title", p => p.Add(x => x.OnRefresh, () =>
        {
            calls++;
            return done.Task;
        }));

        var first = cut.InvokeAsync(cut.Instance.RefreshAsync);
        var during = cut.InvokeAsync(cut.Instance.RefreshAsync);

        Assert.That(calls, Is.EqualTo(1));
        done.SetResult();
        await Task.WhenAll(first, during);
        await cut.InvokeAsync(cut.Instance.RefreshAsync);
        Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public async Task OnRefreshThrows_PassesTheFailureToTheNearestErrorBoundary()
    {
        PullModule();
        AppScreen? screen = null;
        RenderFragment content = b =>
        {
            b.OpenComponent<AppScreen>(0);
            b.AddComponentParameter(1, nameof(AppScreen.Title), "Screen title");
            b.AddComponentParameter(2, nameof(AppScreen.OnRefresh), EventCallback.Factory.Create(this, () => Task.FromException(new InvalidOperationException())));
            b.AddComponentReferenceCapture(3, instance => screen = (AppScreen)instance);
            b.CloseComponent();
        };
        var cut = Context.Render<ErrorBoundary>(p => p
            .AddChildContent(content)
            .Add(x => x.ErrorContent, _ => b => b.AddMarkupContent(0, "<p data-testid=\"error-message\">failed</p>")));

        await cut.InvokeAsync(screen!.RefreshAsync);

        Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DisposedWhileImporting_DisposesTheModuleAndLogsNothing()
    {
        var logger = new RecordingLogger<AppScreen>();
        Context.Services.AddSingleton<ILogger<AppScreen>>(logger);
        var module = new FakeModule();
        var runtime = new DelayedImportRuntime();
        Context.Services.AddSingleton<IJSRuntime>(runtime);
        var cut = Render("Screen title", p => p.Add(x => x.OnRefresh, () => Task.CompletedTask));

        await Context.DisposeComponentsAsync();
        runtime.Import.SetResult(module);
        await Task.Yield();

        Assert.Multiple(() =>
        {
            Assert.That(module.Disposed, Is.True);
            Assert.That(module.Calls, Is.Empty);
            Assert.That(logger.Records, Is.Empty);
        });
    }

    [Test]
    public void PullGestureFailsToAttach_LogsItAndShowsTheScreen()
    {
        var logger = new RecordingLogger<AppScreen>();
        Context.Services.AddSingleton<ILogger<AppScreen>>(logger);
        Context.JSInterop.SetupModule(AppScreenModule).SetupVoid("attach", _ => true).SetException(new JSException("Attach failed"));

        var cut = Render("Screen title", p => p
            .Add(x => x.OnRefresh, () => Task.CompletedTask)
            .AddChildContent("<p data-testid=\"shown\">Rows</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=shown]"), Has.Count.EqualTo(1));
            Assert.That(logger.Records, Is.EqualTo(new[] { ((int)KitEvent.PullRefreshFailed, LogLevel.Warning, "Setting up pull to refresh failed") }));
        });
    }

    [Test]
    public async Task Disposed_DetachesThePullGesture()
    {
        var module = PullModule();
        var cut = Render("Screen title", p => p.Add(x => x.OnRefresh, () => Task.CompletedTask));
        var main = cut.Find("main");

        await Context.DisposeComponentsAsync();

        module.Invocations["detach"].Single().Arguments[0].ShouldBeElementReferenceTo(main);
    }

    private BunitJSModuleInterop PullModule()
    {
        var module = Context.JSInterop.SetupModule(AppScreenModule);
        module.Mode = JSRuntimeMode.Loose;
        return module;
    }

#pragma warning disable BL0016 // Fakes implement the interop interfaces; nothing here calls into JS
    private sealed class FakeModule : IJSObjectReference
    {
        public List<string> Calls { get; } = [];

        public bool Disposed { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add(identifier);
            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class DelayedImportRuntime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            new((Task<TValue>)(object)Import.Task);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }
#pragma warning restore BL0016
}
