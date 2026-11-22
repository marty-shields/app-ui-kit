# AGENTS.md — App UI Kit

The shared UI kit (.NET 11 Blazor) for the Calorie Tracker and Workout Log apps: design tokens, base styles, general components and the stories shell. Overview: [README.md](README.md). Trial notes: [docs/trial/README.md](docs/trial/README.md). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build Shields.UI.slnx` (warnings fail the build)
- Code style: `dotnet format Shields.UI.slnx --verify-no-changes` (CI runs it; drop `--verify-no-changes` to apply the fixes)
- Test: `dotnet test tests/Shields.UI.Tests`
- CI script tests (need `bash`, `git`, `python3`, `node`): `.github/scripts/tests/run.sh`
- CI lint (needs `shellcheck`): `.github/scripts/lint-ci.sh` runs shellcheck over the scripts and actionlint over the workflows; CI runs it in `Build and test`.
- Filler comments: `python3 -I .github/scripts/check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name; CI runs it in `Build and test`.
- Docs: `python3 -I .github/scripts/check-docs.py` fails on a relative link or `#anchor` in a tracked `*.md` that doesn't resolve; CI runs it in `Build and test`.
- Pack the test helpers: `dotnet pack src/Shields.UI.Testing -o <folder>` (embeds the kit's `tokens.css`, `base.css`, scoped CSS bundle and `theme.js`).
- Kit stories site: `dotnet run --project src/Shields.UI.Stories.Host` → http://localhost:5056 (the kit's stories only; the story layout check runs against it).
- Combined stories site (the kit and both apps): clone [calorie-tracker-app](https://github.com/marty-shields/calorie-tracker-app) and [workout-log-app](https://github.com/marty-shields/workout-log-app) beside this repo (`../calorie-tracker-app`, `../workout-log-app`; `-p:CalorieTrackerPath=` and `-p:WorkoutLogPath=` point elsewhere), then `dotnet run --project src/Shields.UI.Stories.All -p:UseKitSource=true -p:KitPath=$PWD -p:RestoreLockedMode=false` → http://localhost:5066. Both apps then build against this checkout's kit source. `src/Shields.UI.Stories.All` is not in `Shields.UI.slnx` because it needs the app repos, and it has no lock file. The `Stories site` workflow publishes it to https://marty-shields.github.io/app-ui-kit/ on every push to `main`, once a day at 05:00 UTC (to pick up a change merged to an app's `main`), and on demand. Its Foundations/Built from page names the three commits.
- Apps check: `.github/scripts/check-apps.sh <kit> <calorie-tracker-app> <workout-log-app>` builds each app's UI, stories library and UI tests against the kit's source and runs the UI tests, and names the app and the first error when one fails. An app whose kit references still resolve to packages (no `UseKitSource` switch of its own) fails, because it would test nothing. CI runs it on every PR to `main` as the required `apps-check`, with both apps at `main`. A PR labelled `semver:major` still runs it but passes with warnings, because breaking the apps is expected. A PR that changes only `docs/**` and `*.md` files skips the build.
- Story layout: with the stories site running, `node .github/scripts/check-story-layout.mjs http://localhost:5056 <path to Chrome>` fails when a story clips, overflows or wraps at the narrowest card; CI runs it in `Story layout`.

Releases and beta packages: [docs/release.md](docs/release.md).

## How the code fits
- Projects: `Shields.UI` (components, tokens, base styles), `Shields.UI.Stories` (the stories shell), `Shields.UI.Stories.Host` (the kit's site), `Shields.UI.Stories.All` (the combined site with both apps, outside the slnx), `Shields.UI.Testing` (package of design checks and the kit's tokens for the apps' tests; no test-framework dependency) and `tests/Shields.UI.Tests`.
- Styling: tokens only, no literal colours or sizes.
- The apps keep no tokens or general components (owner decision 2026-11-21): a general component or token an app needs belongs here. Only screens and app-specific logic stay in the app.

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- XML comments only when they add something the name and type don't (a unit, a range, what null means, an order, a why). Component parameters are the exception. Never restate the name. CI's `check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name (a real exception gets `/// <!-- filler-ok: why -->`).
- Interfaces only where something else implements them, such as a test fake or the stories site. No interface for a single implementation nothing swaps.
- Commit on a branch, push it and open a ready (non-draft) PR when the work is done, so all CI runs (a draft skips the build). Merge only when the user says to.
- Epics: a task of an epic issue gets a `task/<epic>-<id>` branch and a PR into the epic's `epic/<name>` branch, not `main`. You may squash-merge that PR yourself once its checks are green, a verifier other than the author has run every acceptance criterion on the issue and every review finding is fixed. Put open questions in the PR, then add them to the epic PR description when you merge. Never merge an epic PR ([how](docs/epics.md)).
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).
- A PR that changes the UI includes screenshots of the changed components, posted as a PR comment or in the body, before it is marked ready.
