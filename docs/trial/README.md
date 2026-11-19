# Trial: one stories site with the kit and both apps

Issue #3. The kit already carries the solution-wide build rules (`Directory.Build.props` and `.editorconfig`, copied from Workout Log); the build-rules issue (#4) adds CI and `AGENTS.md` and finishes them. The trial host loaded each app's `*.UI.ScreenStories` library (one screen story each) through MSBuild properties; that wiring is gone, so the kit's stories site builds alone. The app-side changes were throwaway: they are kept here as patches and never committed to the app repos.

## Reproduce

```
SCRATCH=<an empty folder>
KIT=<this repo's checkout>
git -C <calorie-tracker-app checkout> worktree add --detach $SCRATCH/calorie-tracker-app origin/main
git --git-dir=<workout-log-app>/.bare worktree add --detach $SCRATCH/workout-log-app origin/main
git -C $SCRATCH/calorie-tracker-app apply $KIT/docs/trial/calorie-tracker-app.patch
git -C $SCRATCH/workout-log-app apply $KIT/docs/trial/workout-log-app.patch
```

Patches were made on calorie-tracker-app `01bba0a` and workout-log-app `58bd59f` (their `origin/main` on 2026-11-19).

Build an app's UI against the kit's source (the `UseKitSource` switch from the design):

```
dotnet build src/CalorieTracker.UI -p:UseKitSource=true -p:KitPath=$KIT -p:RestoreLockedMode=false
dotnet build src/WorkoutLog.UI     -p:UseKitSource=true -p:KitPath=$KIT -p:RestoreLockedMode=false
```

Without `-p:RestoreLockedMode=false` the restore stops with `NU1004: A new project reference to Shields.UI was found ... The packages lock file is inconsistent with the project dependencies`.

The trial also ran one site with both apps' screen stories. The host no longer has that opt-in (`CalorieTrackerPath`, `WorkoutLogPath`, the native SQLite removal and the separate lock file); the patches stay here as the starting point for the app-side work. Each app's `*.UI.ScreenStories` library can be added to a host's `Assemblies` list.

## What the patches contain

- `UseKitSource` switch in each app's UI project: a `ProjectReference` to `$(KitPath)/src/Shields.UI/Shields.UI.csproj`. The `PackageReference` branch is left out because the package is not published yet.
- A new `*.UI.ScreenStories` Razor library per app (BlazingStory, the UI project and the kit's `Shields.UI.Stories`): one screen story with a four-level title (`Calorie Tracker/Screens/Dashboard`, `Workout Log/Screens/This week`), its sample data, a phone layout that also loads the app's own tokens, and an `Add...Stories` method for the host's services (Calorie Tracker's calls `AddUi()`; Workout Log's registers only the back button, see below).
- `app-tokens.css` per app: the app's `tokens.css` without the tokens the kit now ships. The kit's `tokens.css` wins for shared tokens, so Workout Log shows the decided accent and error colours.

Lock files are not in the patches; the restore regenerates them.

## Not done in the trial

- The apps' own stories projects are untouched (not split into a library plus host), their story titles are not prefixed, and the apps' views still use their own `AppButton`.
- Workout Log's seven sample services are not moved into the library; the trial screen is a pure view and needs none of them. Its `AddWorkoutLogStories()` therefore registers only `IBackButton` (the screen's back handler needs it) and does not call `AddUi()`: that registers services that need Core's (`IWorkoutService`, `IExerciseCatalog` and others), and the host's container validation fails at start-up without them. The full split registers the sample services next to `AddUi()`.
