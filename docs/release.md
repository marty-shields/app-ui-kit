# Releases

The kit publishes `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing` to nuget.org, always at one version. No API key is stored anywhere: the workflows use nuget.org Trusted Publishing.

## How a release happens
1. A PR merges into `main`. Every PR has exactly one `semver:major|minor|patch` label (the `PR checks` workflow enforces it).
2. [`release.yml`](../.github/workflows/release.yml) runs on the push. [`next-version.sh`](../.github/scripts/next-version.sh) takes the highest label among the PRs merged since the last `v*` tag and bumps that tag's version by it. With no `v*` tag yet the version is 1.0.0.
3. The `pack` job packs the three projects with `-p:Version=<version>`, so `Shields.UI.Stories` depends on `Shields.UI` at that same version, and uploads the `.nupkg` files as a workflow artifact. It has no `id-token` permission.
4. The `publish` job runs in the `release` environment. It runs no project code: it creates the `v<version>` tag and a GitHub release with generated notes, then `NuGet/login` exchanges the job's GitHub token for a short-lived key and the downloaded packages are pushed. The tag comes before the push on purpose: if the push fails, the tag is already there, so the next push to `main` works out a higher version instead of computing the same one again.
5. The apps' Dependabot opens an update PR for the new version.

A push that changes only `docs/**`, `tests/**`, `*.md` or `.github/**` does not run the release workflow, because it ships nothing. Its PR's label still counts at the next release.

If the `publish` job fails, use "Re-run failed jobs" on the run in the Actions tab: it keeps the version already worked out, finds the existing tag and release, skips the packages already pushed (`--skip-duplicate`) and pushes the rest. Do not use "Re-run all jobs": the tag now exists, so the version job finds nothing new to release. If you leave a failed run alone, that version stays unpublished (its tag exists) and the next merge releases a higher one.

## Beta packages
Label a PR into `main` with `publish-beta` to publish prerelease packages of it, so an app can try a kit change before it merges. The label is not a `semver:` label and `PR checks` ignores it.

- [`beta.yml`](../.github/workflows/beta.yml) runs when the PR is opened, reopened, pushed to or labelled `publish-beta`. A PR without the label, and a PR from a fork, publish nothing. The workflow uses `pull_request`, never `pull_request_target`.
- The version is `<next release version>-pr.<PR number>.<run number>`, for example `1.1.0-pr.42.7`. The next release version counts the PR's own `semver:` label. The run number is the workflow's `github.run_number`: it only grows, so each new run is a higher version than the one before. A re-run of the same run keeps its number, so it pushes the same version again and `--skip-duplicate` publishes nothing new; push a commit to get a new beta.
- Creating a PR with the label already on it fires `opened` and `labeled`, which can publish two betas. Both are valid; use the later one.
- The workflow's `pack` job runs the PR's own code and has no `id-token` permission. The `publish` job runs none of it and runs in the `beta` environment, which needs the reviewer's approval before it starts.
- To use one in an app, take the version from the workflow run's log (`Beta version ...`) and set it on the `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing` package versions in the app's `Directory.Packages.props`. Put the app back on a stable version when the PR has merged.

## Hide a bad version
A published version cannot be deleted. On nuget.org open the package, then Manage package, and unlist the version: it stops appearing in search and new restores, and projects that already use it keep working. Fix the problem in a new PR; the next merge releases a higher version.

## Owner set-up
Needed once before the first publish:
1. A nuget.org account that will own the three packages.
2. In the GitHub repository (Settings, Environments) create two environments:
   - `release`, with a deployment branch rule that allows only `main`.
   - `beta`, with yourself (or another trusted person) as a required reviewer. Approve a beta run only after looking at the PR.
3. On nuget.org, two Trusted Publishing policies (user menu, Trusted Publishing) for the repository owner `marty-shields` and repository `app-ui-kit`: workflow file `release.yml` with environment `release`, and workflow file `beta.yml` with environment `beta`. Do not leave the environment empty.
4. The repo variable `NUGET_USER` (Settings, Secrets and variables, Actions, Variables) set to the nuget.org user name, not the email address. It is a variable, not a secret. The workflows fail early when it is empty.

Why the environments matter: a PR can edit its own `beta.yml` and the `pack` job's build, so the workflow file alone proves nothing about the code that runs. A run that does not use the `beta` environment gets a token that the policy rejects, and a run that does use it waits for the reviewer. The `release` environment only accepts runs from `main`, so a branch cannot publish a release either.
