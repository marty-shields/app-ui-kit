#!/usr/bin/env bash
# Usage: check-apps.sh [--advisory] <kit> <calorie-tracker-app> <workout-log-app>
# Builds each app's UI, stories library and UI tests against the kit's source at <kit> (-p:UseKitSource=true), then runs
# the app's UI tests. An app whose kit references still resolve to packages (no UseKitSource switch) fails before its
# tests, since it would test nothing. Both apps are checked even when the first fails. For each failure it prints an annotation that
# names the app, the stage and the first error. With GITHUB_STEP_SUMMARY set, it also adds a table to the run summary.
# Exits 1 when an app fails. With --advisory (a semver:major PR, which is expected to break the apps), failures are
# warnings and it exits 0. DOTNET overrides the dotnet command, for the tests.
set -uo pipefail

advisory=false
if [[ "${1:-}" == "--advisory" ]]; then
  advisory=true
  shift
fi
if (( $# != 3 )); then
  echo "Usage: check-apps.sh [--advisory] <kit> <calorie-tracker-app> <workout-log-app>" >&2
  exit 2
fi
kit="$(cd "$1" && pwd)" || exit 2
dotnet="${DOTNET:-dotnet}"
props=(-p:UseKitSource=true "-p:KitPath=$kit" -p:RestoreLockedMode=false)

# first_error <log>: the first compiler, NuGet or MSBuild error, or the first failed test and its message.
first_error() {
  local line
  line=$(grep -m1 -E ': error [A-Z]+[0-9]+' "$1" | sed -E 's/ \[[^]]*\.csproj\]$//; s/^[[:space:]]+//')
  if [[ -z "$line" ]]; then
    # dotnet test prints "  Failed <test> [<time>]", then "  Error Message:" and the message on the next line.
    line=$(awk '
      !name && /^[[:space:]]*Failed [^ ]/ { sub(/^[[:space:]]*Failed /, ""); sub(/ \[[^]]*\]$/, ""); name = $0; next }
      name && /Error Message:/ { grab = 1; next }
      grab && NF { sub(/^[[:space:]]+/, ""); print name ": " $0; exit }
      END { if (name && !grab) print name }
    ' "$1")
  fi
  [[ -n "$line" ]] || line=$(grep -v '^[[:space:]]*$' "$1" | tail -n 1)
  printf '%s' "$line"
}

# Escapes a value for a workflow command's message.
escape() {
  local s="${1//%/%25}"
  s="${s//$'\r'/%0D}"
  printf '%s' "${s//$'\n'/%0A}"
}

rows=()
failed=0

# check_app <name> <folder> <project prefix>
check_app() {
  local name="$1" folder="$2" prefix="$3" log stage="" error="" project
  log=$(mktemp)
  if [[ ! -d "$folder" ]]; then
    stage=checkout
    error="$folder does not exist"
  else
    for project in "src/$prefix.UI" "src/$prefix.UI.Stories" "tests/$prefix.UI.Tests"; do
      echo "::group::$name: build $project"
      if ! "$dotnet" build "$folder/$project" "${props[@]}" 2>&1 | tee "$log"; then
        stage="build"
        error=$(first_error "$log")
      fi
      echo "::endgroup::"
      [[ -z "$stage" ]] || break
    done
    # An app without its own UseKitSource switch still builds, from the kit's packages, which would test nothing here.
    if [[ -z "$stage" ]]; then
      local packages
      packages=$(for project in "src/$prefix.UI" "src/$prefix.UI.Stories" "tests/$prefix.UI.Tests"; do
        jq -r '.libraries | to_entries[] | select((.key | startswith("Shields.UI")) and .value.type != "project") | .key' \
          "$folder/$project/obj/project.assets.json"
      done | sort -u | paste -sd ' ' -)
      if [[ -n "$packages" ]]; then
        stage="kit source"
        error="$name builds $packages from the package, not from the kit's source: it needs a UseKitSource switch for each kit reference."
      fi
    fi
    if [[ -z "$stage" ]]; then
      echo "::group::$name: test tests/$prefix.UI.Tests"
      if ! "$dotnet" test "$folder/tests/$prefix.UI.Tests" --no-build "${props[@]}" 2>&1 | tee "$log"; then
        stage="test"
        error=$(first_error "$log")
      fi
      echo "::endgroup::"
    fi
  fi
  rm -f "$log"

  if [[ -z "$stage" ]]; then
    rows+=("| $name | build and test | passed | |")
    return
  fi
  failed=$((failed + 1))
  local level=error
  [[ "$advisory" == true ]] && level=warning
  echo "::$level title=$name $stage failed::$(escape "$error")"
  rows+=("| $name | $stage | failed | \`${error//|/\\|}\` |")
}

check_app "Calorie Tracker" "$2" CalorieTracker
check_app "Workout Log" "$3" WorkoutLog

if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
  {
    echo "## Apps against this kit change"
    echo
    echo "| App | Stage | Result | First error |"
    echo "| --- | --- | --- | --- |"
    printf '%s\n' "${rows[@]}"
    if (( failed > 0 )) && [[ "$advisory" == true ]]; then
      echo
      echo "This PR is labelled \`semver:major\`, so breaking an app is expected: the check reports it and does not block the merge."
    fi
  } >> "$GITHUB_STEP_SUMMARY"
fi

if (( failed > 0 )) && [[ "$advisory" != true ]]; then
  exit 1
fi
exit 0
