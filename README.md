# App UI Kit

The shared UI kit for the [Calorie Tracker](https://github.com/marty-shields/calorie-tracker-app) and [Workout Log](https://github.com/marty-shields/workout-log-app) Android apps (.NET MAUI Blazor Hybrid).

It will hold what the two apps should always have in common, so a change is made once and both apps pick it up:

- **Design tokens and base styles:** colours, type, spacing, corners and touch targets.
- **General components:** buttons, icons, inputs, list rows, cards, tiles, the screen frame and the bottom bar.
- **The stories shell:** the phone frame, theme switch and token sheet used by every stories site.
- **Design-system tests:** tokens-only CSS, contrast and touch-target checks that the apps reuse.
- **One stories site** showing the kit and both apps' screens.

Screens and app-only components stay in each app's repo. The apps install the kit as NuGet packages: `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing`.

## Status

A trial is in place: `Shields.UI` (with `AppButton`, the tokens and base styles), `Shields.UI.Stories` and a stories site, `Shields.UI.Stories.Host`. The trial code is the start of the repo; solution-wide build rules and CI come later. The plan and the decisions behind it are in [docs/design/shared-ui-kit.html](docs/design/shared-ui-kit.html) (download it and open it in a browser). The work is tracked in this repo's epic issues.

## Build and run

```
dotnet build Shields.UI.slnx
dotnet test tests/Shields.UI.Tests
dotnet run --project src/Shields.UI.Stories.Host     # http://localhost:5056
```

To show the apps' screens too, see [docs/trial/README.md](docs/trial/README.md).

## Licence

MIT. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
