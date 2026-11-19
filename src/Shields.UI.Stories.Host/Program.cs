using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shields.UI.Stories.Host;
#if CALORIE_TRACKER
using CalorieTracker.UI.ScreenStories;
#endif
#if WORKOUT_LOG
using WorkoutLog.UI.ScreenStories;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(TimeProvider.System);
#if CALORIE_TRACKER
builder.Services.AddCalorieTrackerStories();
#endif
#if WORKOUT_LOG
builder.Services.AddWorkoutLogStories();
#endif

await builder.Build().RunAsync();
