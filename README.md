# App UI Kit

The shared UI kit for the [Calorie Tracker](https://github.com/marty-shields/calorie-tracker-app) and [Workout Log](https://github.com/marty-shields/workout-log-app) Android apps (.NET MAUI Blazor Hybrid).

It will hold what the two apps should always have in common, so a change is made once and both apps pick it up:

- **Design tokens and base styles:** colours, type, spacing, corners and touch targets.
- **General components:** buttons, icons, inputs, list rows, cards, tiles, the screen frame and the bottom bar.
- **The stories shell:** the phone frame, theme switch and token sheet used by every stories site.
- **Design-system tests:** tokens-only CSS, contrast and touch-target checks that the apps reuse.
- **One stories site** showing the kit and both apps' screens.

Screens stay in each app's repo; the apps keep no tokens or general components of their own. The apps install the kit as NuGet packages: `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing`.

A stories host (its `index.html` and `iframe.html`) must link `_content/Shields.UI.Stories/css/tokens.css` after the kit's `_content/Shields.UI/css/tokens.css`; `index.html` needs it for `blazor-ui.css`, and the phone frame and sheets take their sizes from it. Without it they lose their size.

## Status

A trial is in place: `Shields.UI.Testing` (the kit's tokens and the tokens-only, contrast and touch-target checks, for an app's design tests), `Shields.UI` (with `AppButton`, `IconButton`, `Icon`, `Spinner`, `LoadFailed`, `Snackbar`, `SegmentedControl`, `FilterChips`, `FieldInput`, `NumberField`, `RadioGroup`, `TextField`, `SearchField`, `SelectField`, `DateStrip`, `ListRow`, `EmptyState`, `Typography`, `Divider`, `InlineAlert`, `Banner`, `Card`, `SectionHeader`, `StatTile`, `ChartLegend`, `PeriodStepper`, `ProgressRing`, `BarList`, `StackedBar`, `TrendBarChart`, `MonthCalendar`, `RangeCalendar`, `DurationField`, `ValueField`, `Sheet`, `SheetAction`, the tokens and base styles), `Shields.UI.Stories` and a stories site, `Shields.UI.Stories.Host`. The trial code is the start of the repo. It already sets the solution-wide build rules (`Directory.Build.props` and `.editorconfig`, copied from Workout Log); CI and [AGENTS.md](https://github.com/marty-shields/app-ui-kit/blob/main/AGENTS.md) are in place; the finished rules come with the later epics. The plan and the decisions behind it are in [docs/design/shared-ui-kit.html](https://github.com/marty-shields/app-ui-kit/blob/main/docs/design/shared-ui-kit.html) (download it and open it in a browser). The work is tracked in this repo's epic issues.

## Build and run

```
dotnet build Shields.UI.slnx
dotnet test tests/Shields.UI.Tests
dotnet run --project src/Shields.UI.Stories.Host     # http://localhost:5056
```

The stories site is published at <https://marty-shields.github.io/app-ui-kit/>; the command above runs it locally. It shows the kit's Foundations (Introduction, Tokens) and its components. The trial that also showed the apps' screens is in [docs/trial/README.md](https://github.com/marty-shields/app-ui-kit/blob/main/docs/trial/README.md).

## Licence

MIT. See [LICENSE](https://github.com/marty-shields/app-ui-kit/blob/main/LICENSE) and [NOTICE](https://github.com/marty-shields/app-ui-kit/blob/main/NOTICE).
