using CalorieTracker.UI.Stories;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shields.UI;
using Shields.UI.Stories.All;
using WorkoutLog.UI;
using WorkoutLog.UI.Stories.Samples;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddShieldsUi();
builder.Services.AddCalorieTrackerStories();
// Workout Log has no single stories method: its own host makes these two calls.
builder.Services.AddUi();
builder.Services.AddWorkoutLogStorySamples();

await builder.Build().RunAsync();
