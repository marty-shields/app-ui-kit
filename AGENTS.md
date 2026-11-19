# AGENTS.md — App UI Kit

The shared UI kit (.NET 11 Blazor) for the Calorie Tracker and Workout Log apps: design tokens, base styles, general components and the stories shell. Overview: [README.md](README.md). Trial notes: [docs/trial/README.md](docs/trial/README.md). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build Shields.UI.slnx` (warnings fail the build)
- Code style: `dotnet format Shields.UI.slnx --verify-no-changes` (CI runs it; drop `--verify-no-changes` to apply the fixes)
- Test: `dotnet test tests/Shields.UI.Tests`
- CI script tests (need `bash`, `git`, `python3`, `shellcheck`): `.github/scripts/tests/run.sh`
- CI lint (needs `shellcheck`): `.github/scripts/lint-ci.sh` runs shellcheck over the scripts and actionlint over the workflows; CI runs it in `Build and test`.
- Filler comments: `python3 -I .github/scripts/check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name; CI runs it in `Build and test`.
- Docs: `python3 -I .github/scripts/check-docs.py` fails on a relative link or `#anchor` in a tracked `*.md` that doesn't resolve; CI runs it in `Build and test`.
- Stories site: `dotnet run --project src/Shields.UI.Stories.Host` → http://localhost:5056

The story-layout check, the stories Pages site and the release workflow are still to come ([epic issues](https://github.com/marty-shields/app-ui-kit/issues)).

## How the code fits
- Projects: `Shields.UI` (components, tokens, base styles), `Shields.UI.Stories` (the stories shell), `Shields.UI.Stories.Host` (the site) and `tests/Shields.UI.Tests`.
- Styling: tokens only, no literal colours or sizes.
- A component or token that only one app needs does not belong here. It stays in that app's repo.

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- XML comments only when they add something the name and type don't (a unit, a range, what null means, an order, a why). Component parameters are the exception. Never restate the name. CI's `check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name (a real exception gets `/// <!-- filler-ok: why -->`).
- Interfaces only where something else implements them, such as a test fake or the stories site. No interface for a single implementation nothing swaps.
- Commit on a branch, push it and open a ready (non-draft) PR when the work is done, so all CI runs (a draft skips the build). Merge only when the user says to.
- Epics: a task of an epic issue gets a `task/<epic>-<id>` branch and a PR into the epic's `epic/<name>` branch, not `main`. You may squash-merge that PR yourself once its checks are green, a verifier other than the author has run every acceptance criterion on the issue and every review finding is fixed. Put open questions in the PR, then add them to the epic PR description when you merge. Never merge an epic PR ([how](docs/epics.md)).
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).
- A PR that changes the UI includes screenshots of the changed components, posted as a PR comment or in the body, before it is marked ready.
