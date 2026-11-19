#!/usr/bin/env bash
# check-filler-comments.py on small C# and Razor files in a temp folder.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../check-filler-comments.py"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# run <name> <expected exit and findings> <file contents>
run() {
  local name="$1" expected="$2" out
  printf '%s\n' "$3" > "$work/Sample.cs"
  out=$(GITHUB_ACTIONS='' python3 -I "$script" "$work/Sample.cs" 2>/dev/null; echo "exit=$?")
  check "$name" "$expected" "${out//"$work/"/}"
}

run "a summary that restates the property name" 'Sample.cs:3: <summary> only restates the name: "Exercise name."
exit=1' 'public sealed class ExerciseState
{
    /// <summary>Exercise name.</summary>
    public string Name { get; }
}'

run "a param that restates the parameter name" 'Sample.cs:3: <param name="services"> only restates the name: "Service collection."
exit=1' 'public static class Extensions
{
    /// <param name="services">Service collection.</param>
    public static void AddCore(this IServiceCollection services) { }
}'

run "words of the enclosing type count as the name" 'Sample.cs:4: <param name="exerciseId"> only restates the name: "Exercise id."
exit=1' 'public sealed class ExerciseState
{
    /// <summary>An exercise in a workout while it is edited.</summary>
    /// <param name="exerciseId">Exercise id.</param>
    public ExerciseState(int exerciseId) { }
}'

run "a multi-line summary is read whole" 'Sample.cs:3: <summary> only restates the name: "Closes the list."
exit=1' 'public sealed class Editor
{
    /// <summary>
    /// Closes the list.
    /// </summary>
    public void CloseList() { }
}'

run "a unit is not filler" 'exit=0' 'public sealed class Row
{
    /// <summary>Incline, percent.</summary>
    public decimal? Incline { get; set; }
}'

run "a why is not filler" 'exit=0' 'public sealed class Row
{
    /// <summary>Start time, UTC.</summary>
    public DateTimeOffset StartedAt { get; }
}'

run "a summary with a link is not filler" 'exit=0' 'public sealed class Row
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    public int Records(int[] sessions) => 0;
}'

run "an inherited doc is left alone" 'exit=0' 'public sealed class Row
{
    /// <inheritdoc />
    public string Name { get; }
}'

run "a filler-ok marker skips the block" 'exit=0' 'public sealed class ExerciseState
{
    /// <summary>Exercise name.</summary>
    /// <!-- filler-ok: shown on the stories Docs page -->
    public string Name { get; }
}'

run "the same block without the marker is filler" 'Sample.cs:3: <summary> only restates the name: "Exercise name."
exit=1' 'public sealed class ExerciseState
{
    /// <summary>Exercise name.</summary>
    public string Name { get; }
}'

run "a comment of seven name words is not filler" 'exit=0' 'public sealed class Row
{
    /// <summary>Exercise name exercise name exercise name exercise.</summary>
    public string ExerciseName { get; }
}'

run "an attribute on the member line is stepped over" 'Sample.cs:3: <summary> only restates the name: "The name."
exit=1' 'public sealed class Row
{
    /// <summary>The name.</summary>
    [Obsolete] public string Name { get; }
}'

run "a component parameter with its attribute on the member line is left alone" 'exit=0' 'public partial class Icon
{
    /// <summary>Size of the icon.</summary>
    [Parameter] public IconSize Size { get; set; }
}'

run "a component parameter is left alone" 'exit=0' 'public partial class Icon
{
    /// <summary>Size of the icon.</summary>
    [Parameter]
    public IconSize Size { get; set; }
}'

run "an attribute between the comment and the member is stepped over" 'Sample.cs:3: <summary> only restates the name: "The file name."
exit=1' 'public sealed class FileLogWriter
{
    /// <summary>The file name.</summary>
    [Obsolete]
    public string FileName { get; }
}'

run "an enum member next to another is judged alone" 'Sample.cs:3: <summary> only restates the name: "Barbell."
exit=1' 'public enum Equipment
{
    /// <summary>Barbell.</summary>
    Barbell,

    /// <summary>Strength and cardio.</summary>
    Duration,
}'

mkdir -p "$work/tree/obj"
printf '%s\n' 'public sealed class ExerciseState
{
    /// <summary>Exercise name.</summary>
    public string Name { get; }
}' > "$work/tree/obj/Generated.cs"
out=$(python3 -I "$script" "$work/tree" 2>&1; echo "exit=$?")
check "a folder argument skips obj" 'exit=0' "$out"

tree=$(cd "$here/../../.." && pwd)
out=$(cd "$tree" && python3 -I "$script" 2>&1; echo "exit=$?")
check "the repository has no filler doc comments" 'exit=0' "$out"

finish
