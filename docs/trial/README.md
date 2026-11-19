# Trial: one stories site with the kit and both apps

Issue #3. The kit already carries the solution-wide build rules (`Directory.Build.props` and `.editorconfig`, copied from Workout Log); the build-rules issue (#4) adds CI and `AGENTS.md` and finishes them. The kit builds alone by default. The apps are opt-in, through two MSBuild properties that point at copies of the apps carrying a `*.UI.ScreenStories` library (one screen story each). The app-side changes are throwaway: they are kept here as patches and never committed to the app repos.

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

Build and run the combined site (http://localhost:5056):

```
dotnet run --project $KIT/src/Shields.UI.Stories.Host -p:UseKitSource=true -p:KitPath=$KIT -p:RestoreLockedMode=false -p:CalorieTrackerPath=$SCRATCH/calorie-tracker-app -p:WorkoutLogPath=$SCRATCH/workout-log-app
```

The host adds each app's story assembly to `BlazingStoryApp`, drops the native SQLite reference, and keeps its own lock file under `obj/`, so the kit's own lock files stay unchanged.

These properties are real global properties on purpose. Given only the two path properties, restore works, but the apps and the kit projects are then built twice with different global properties, in parallel, and a clean build fails on a file the two builds both write (`rpswa.dswa.cache.json`). A clean build with the command above passes.

## What the patches contain

- `UseKitSource` switch in each app's UI project: a `ProjectReference` to `$(KitPath)/src/Shields.UI/Shields.UI.csproj`. The `PackageReference` branch is left out because the package is not published yet.
- A new `*.UI.ScreenStories` Razor library per app (BlazingStory, the UI project and the kit's `Shields.UI.Stories`): one screen story with a four-level title (`Calorie Tracker/Screens/Dashboard`, `Workout Log/Screens/This week`), its sample data, a phone layout that also loads the app's own tokens, and an `Add...Stories` method for the host's services.
- `app-tokens.css` per app: the app's `tokens.css` without the tokens the kit now ships. The kit's `tokens.css` wins for shared tokens, so Workout Log shows the decided accent and error colours.

Lock files are not in the patches; the restore regenerates them.

## Not done in the trial

- The apps' own stories projects are untouched (not split into a library plus host), their story titles are not prefixed, and the apps' views still use their own `AppButton`.
- Workout Log's seven sample services are not moved into the library; the trial screen needs none of them.
