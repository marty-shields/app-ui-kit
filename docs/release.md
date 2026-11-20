# Releases

The kit publishes `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing` to nuget.org, always at one version. No API key is stored anywhere: the workflows use nuget.org Trusted Publishing.

## How a release happens
1. A PR merges into `main`. Every PR has exactly one `semver:major|minor|patch` label (the `PR checks` workflow enforces it).
2. [`release.yml`](../.github/workflows/release.yml) runs on the push. [`next-version.sh`](../.github/scripts/next-version.sh) takes the highest label among the PRs merged since the last `v*` tag and bumps that tag's version by it. With no `v*` tag yet the version is 1.0.0.
3. The three projects are packed with `-p:Version=<version>`, so `Shields.UI.Stories` depends on `Shields.UI` at that same version.
4. `NuGet/login` exchanges the job's GitHub token for a short-lived key and the packages are pushed. The job then creates the `v<version>` tag and a GitHub release with generated notes.
5. The apps' Dependabot opens an update PR for the new version.

A push that changes only `docs/**`, `*.md` or `.github/**` does not run the release workflow. Its PR's label still counts at the next release.

If a run fails after the push, run it again from the Actions tab: the pushed packages are skipped and the rest of the job finishes.

## Beta packages
Label a PR into `main` with `publish-beta` to publish prerelease packages of it, so an app can try a kit change before it merges. The label is not a `semver:` label and `PR checks` ignores it.

- [`beta.yml`](../.github/workflows/beta.yml) runs when the PR is opened, reopened, pushed to or labelled `publish-beta`. A PR without the label, and a PR from a fork, publish nothing. The workflow uses `pull_request`, never `pull_request_target`.
- The version is `<next release version>-pr.<PR number>.<run number>`, for example `1.1.0-pr.42.7`. The next release version counts the PR's own `semver:` label. The run number is the workflow's `github.run_number`: it never repeats and only grows, so each push to the PR is a higher version than the one before.
- To use one in an app, take the version from the workflow run's log (`Beta version ...`) and set it on the `Shields.UI`, `Shields.UI.Stories` and `Shields.UI.Testing` package versions in the app's `Directory.Packages.props`. Put the app back on a stable version when the PR has merged.

## Hide a bad version
A published version cannot be deleted. On nuget.org open the package, then Manage package, and unlist the version: it stops appearing in search and new restores, and projects that already use it keep working. Fix the problem in a new PR; the next merge releases a higher version.

## Owner set-up
Needed once before the first publish:
1. A nuget.org account that will own the three packages.
2. On nuget.org, a Trusted Publishing policy (user menu, Trusted Publishing) for the repository owner `marty-shields`, repository `app-ui-kit` and workflow file `release.yml`, and a second policy for the workflow file `beta.yml`. Leave the environment empty.
3. The repo variable `NUGET_USER` (Settings, Secrets and variables, Actions, Variables) set to the nuget.org user name, not the email address. It is a variable, not a secret. The workflows fail early when it is empty.
