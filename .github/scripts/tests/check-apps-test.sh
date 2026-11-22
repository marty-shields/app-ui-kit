#!/usr/bin/env bash
# check-apps.sh with a stub dotnet that fails the project named in FAIL, and docs-only.sh in a throwaway repo.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../check-apps.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir -p "$work/kit" "$work/ct" "$work/wl"

# FAIL=<command>:<project folder name>:<output kind> makes that one call fail with a canned log.
cat > "$work/dotnet" <<'STUB'
#!/usr/bin/env bash
echo "$*" >> "$CALLS"
project=$(basename "$2")
if [[ "$1" == build ]]; then
  type=project
  [[ "$project" == "${PACKAGE:-}" ]] && type=package
  mkdir -p "$2/obj"
  echo "{\"libraries\": {\"Shields.UI/1.1.0\": {\"type\": \"$type\"}, \"bunit/2.0.0\": {\"type\": \"package\"}}}" > "$2/obj/project.assets.json"
fi
IFS=: read -r command target kind <<<"${FAIL:-}"
if [[ "$1" != "$command" || "$project" != "$target" ]]; then
  echo "  $project -> bin/$project.dll"
  exit 0
fi
case "$kind" in
  compile)
    echo "/w/ct/src/CalorieTracker.UI/Screens/DashboardView.razor(12,5): warning CS0618: old [/w/ct/src/CalorieTracker.UI/CalorieTracker.UI.csproj]"
    echo "/w/ct/src/CalorieTracker.UI/Screens/DashboardView.razor(14,9): error CS1061: 'ProgressRing' does not contain a definition for 'FigureUnit' [/w/ct/src/CalorieTracker.UI/CalorieTracker.UI.csproj]"
    echo "/w/ct/src/CalorieTracker.UI/Screens/DashboardView.razor(15,9): error CS0103: second [/w/ct/src/CalorieTracker.UI/CalorieTracker.UI.csproj]" ;;
  restore)
    echo "/w/wl/src/WorkoutLog.UI/WorkoutLog.UI.csproj : error NU1605: Detected package downgrade: BlazingStory [/w/wl/src/WorkoutLog.UI/WorkoutLog.UI.csproj]" ;;
  test)
    echo "A total of 1 test files matched the specified pattern."
    echo "  Failed ADateParameter_OpensThatDay [435 ms]"
    echo "  Error Message:"
    echo "   System.InvalidOperationException : Object of type 'ProgressRing' does not have a property matching the name 'FigureUnit'."
    echo "  Stack Trace:"
    echo "     at Somewhere()"
    echo "  Failed SecondTest [2 ms]" ;;
  *)
    echo "Something went wrong 100%" ;;
esac
exit 1
STUB
chmod +x "$work/dotnet"

# run <name> <expected exit> <expected annotations> <expected summary rows> [args...]
run() {
  local name="$1" exit_code="$2" annotations="$3" rows="$4" out code
  shift 4
  : > "$work/calls"
  : > "$work/summary"
  out=$(DOTNET="$work/dotnet" CALLS="$work/calls" GITHUB_STEP_SUMMARY="$work/summary" \
    bash "$script" "$@" "$work/kit" "$work/ct" "$work/wl" 2>&1)
  code=$?
  check "$name: exit" "$exit_code" "$code"
  check "$name: annotations" "$annotations" "$(grep -E '^::(error|warning)' <<<"$out")"
  check "$name: summary rows" "$rows" "$(grep -E '^\| (Calorie|Workout)' "$work/summary")"
}

FAIL="" run "both pass" 0 "" \
"| Calorie Tracker | build and test | passed | |
| Workout Log | build and test | passed | |"
check "both pass: builds UI, stories and tests, then tests, with the kit source switch" \
"build $work/ct/src/CalorieTracker.UI -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
build $work/ct/src/CalorieTracker.UI.Stories -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
build $work/ct/tests/CalorieTracker.UI.Tests -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
test $work/ct/tests/CalorieTracker.UI.Tests --no-build -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
build $work/wl/src/WorkoutLog.UI -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
build $work/wl/src/WorkoutLog.UI.Stories -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
build $work/wl/tests/WorkoutLog.UI.Tests -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false
test $work/wl/tests/WorkoutLog.UI.Tests --no-build -p:UseKitSource=true -p:KitPath=$work/kit -p:RestoreLockedMode=false" \
"$(cat "$work/calls")"
check "both pass: no major note" "0" "$(grep -c 'semver:major' "$work/summary")"

FAIL="build:CalorieTracker.UI:compile" run "compile error names the app and the first error" 1 \
"::error title=Calorie Tracker build failed::/w/ct/src/CalorieTracker.UI/Screens/DashboardView.razor(14,9): error CS1061: 'ProgressRing' does not contain a definition for 'FigureUnit'" \
"| Calorie Tracker | build | failed | \`/w/ct/src/CalorieTracker.UI/Screens/DashboardView.razor(14,9): error CS1061: 'ProgressRing' does not contain a definition for 'FigureUnit'\` |
| Workout Log | build and test | passed | |"
check "compile error: stops that app at the failed build, still checks the other" "0
4" "$(grep -c 'CalorieTracker.UI.Stories\|CalorieTracker.UI.Tests' "$work/calls"; grep -c WorkoutLog "$work/calls")"

FAIL="build:WorkoutLog.UI:restore" run "restore error in the second app" 1 \
"::error title=Workout Log build failed::/w/wl/src/WorkoutLog.UI/WorkoutLog.UI.csproj : error NU1605: Detected package downgrade: BlazingStory" \
"| Calorie Tracker | build and test | passed | |
| Workout Log | build | failed | \`/w/wl/src/WorkoutLog.UI/WorkoutLog.UI.csproj : error NU1605: Detected package downgrade: BlazingStory\` |"

FAIL="test:CalorieTracker.UI.Tests:test" run "failed test gives the first test and its message" 1 \
"::error title=Calorie Tracker test failed::ADateParameter_OpensThatDay: System.InvalidOperationException : Object of type 'ProgressRing' does not have a property matching the name 'FigureUnit'." \
"| Calorie Tracker | test | failed | \`ADateParameter_OpensThatDay: System.InvalidOperationException : Object of type 'ProgressRing' does not have a property matching the name 'FigureUnit'.\` |
| Workout Log | build and test | passed | |"

FAIL="build:WorkoutLog.UI.Stories:other" run "no error line falls back to the last line, escaped" 1 \
"::error title=Workout Log build failed::Something went wrong 100%25" \
"| Calorie Tracker | build and test | passed | |
| Workout Log | build | failed | \`Something went wrong 100%\` |"

FAIL="test:CalorieTracker.UI.Tests:test" run "semver:major reports a warning and passes" 0 \
"::warning title=Calorie Tracker test failed::ADateParameter_OpensThatDay: System.InvalidOperationException : Object of type 'ProgressRing' does not have a property matching the name 'FigureUnit'." \
"| Calorie Tracker | test | failed | \`ADateParameter_OpensThatDay: System.InvalidOperationException : Object of type 'ProgressRing' does not have a property matching the name 'FigureUnit'.\` |
| Workout Log | build and test | passed | |" --advisory
check "semver:major: the summary says breaking is expected" "1" "$(grep -c 'semver:major' "$work/summary")"

FAIL="" run "semver:major with nothing broken passes quietly" 0 "" \
"| Calorie Tracker | build and test | passed | |
| Workout Log | build and test | passed | |" --advisory
check "semver:major with nothing broken: no note" "0" "$(grep -c 'semver:major' "$work/summary")"

PACKAGE=CalorieTracker.UI.Stories FAIL="" run "an app that builds the kit from its package fails before its tests" 1 \
"::error title=Calorie Tracker kit source failed::Calorie Tracker builds Shields.UI/1.1.0 from the package, not from the kit's source: it needs a UseKitSource switch for each kit reference." \
"| Calorie Tracker | kit source | failed | \`Calorie Tracker builds Shields.UI/1.1.0 from the package, not from the kit's source: it needs a UseKitSource switch for each kit reference.\` |
| Workout Log | build and test | passed | |"
check "kit from package: no test run for that app" "0" "$(grep -c '^test .*CalorieTracker' "$work/calls")"

rm -rf "$work/wl"
FAIL="" run "a missing checkout fails that app" 1 \
"::error title=Workout Log checkout failed::$work/wl does not exist" \
"| Calorie Tracker | build and test | passed | |
| Workout Log | checkout | failed | \`$work/wl does not exist\` |"

# docs-only.sh
docs="$here/../docs-only.sh"
repo="$work/repo"
git init -q -b main "$repo"
git -C "$repo" -c user.name=t -c user.email=t@t commit -q --allow-empty -m base
base=$(git -C "$repo" rev-parse HEAD)
# change <name> <expected> <file...>: a branch from base that adds the files.
change() {
  local name="$1" expected="$2" file
  shift 2
  git -C "$repo" checkout -q -B "b$RANDOM" "$base"
  for file in "$@"; do
    mkdir -p "$repo/$(dirname "$file")"
    echo x > "$repo/$file"
  done
  git -C "$repo" add -A
  git -C "$repo" -c user.name=t -c user.email=t@t commit -q --allow-empty -m change
  check "docs-only: $name" "$expected" "$(cd "$repo" && bash "$docs" "$base" HEAD)"
}
change "docs folder and markdown files" true docs/release.md docs/design/a.html README.md src/Shields.UI/notes.md
change "a source file" false docs/release.md src/Shields.UI/Card.razor
change "a workflow" false .github/workflows/apps-check.yml
change "a docs-like name outside docs" false documents/a.txt
change "no files" false

finish
