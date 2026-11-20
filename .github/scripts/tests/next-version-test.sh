#!/usr/bin/env bash
# next-version.sh in a throwaway repo, with a stub gh that answers from canned PR JSON.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../next-version.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir "$work/bin" "$work/prs"
cat > "$work/bin/gh" <<'STUB'
#!/usr/bin/env bash
# gh api repos/<repo>/commits/<sha>/pulls --jq <filter>
sha=$(sed 's#.*/commits/\(.*\)/pulls#\1#' <<<"$2")
file="$PRS/$sha.json"
[[ -f "$file" ]] || file="$PRS/none.json"
jq -r "$4" "$file"
STUB
chmod +x "$work/bin/gh"
echo '[]' > "$work/prs/none.json"

# pr <sha> <number> <label...>: the one merged PR the commit is the merge of.
pr() {
  local sha="$1" number="$2"; shift 2
  jq -n --argjson number "$number" '$ARGS.positional | map({name: .}) | [{number: $number, merged_at: "2099-01-01T00:00:00Z", base: {ref: "main"}, labels: .}]' \
    --args "$@" > "$work/prs/$sha.json"
}

# run <name> <expected output file contents>: runs the script in $work/repo for target HEAD.
run() {
  local name="$1" expected="$2" out
  out=$(cd "$work/repo" && PATH="$work/bin:$PATH" PRS="$work/prs" GITHUB_REPOSITORY=o/r GITHUB_OUTPUT="$work/out" bash "$script" v "$(git -C "$work/repo" rev-parse HEAD)" 2> "$work/err"; echo "exit=$?")
  check "$name" "$expected" "$(cat "$work/out" 2>/dev/null; echo "$out" | grep '^exit=')"
  rm -f "$work/out"
}

fresh() {
  rm -rf "$work/repo" "$work/prs"/*.json; echo '[]' > "$work/prs/none.json"
  git init -q -b main "$work/repo"
  git -C "$work/repo" config user.email t@t; git -C "$work/repo" config user.name t
  git -C "$work/repo" commit -q --allow-empty -m base
}
commit() { git -C "$work/repo" commit -q --allow-empty -m "$1"; git -C "$work/repo" rev-parse HEAD; }

fresh
run "no tags yet: 1.0.0" "release=true
version=1.0.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:patch"
run "patch bumps the patch" "release=true
version=1.2.4
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:minor" "enhancement"
run "minor bumps the minor and zeroes the patch; other labels are ignored" "release=true
version=1.3.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:major"
run "major bumps the major and zeroes the rest" "release=true
version=2.0.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
a=$(commit a); pr "$a" 10 "semver:patch"
b=$(commit b); pr "$b" 11 "semver:minor"
c=$(commit c); pr "$c" 12 "semver:patch"
run "the highest level among the PRs wins" "release=true
version=1.3.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3; git -C "$work/repo" tag v1.2.4 HEAD; git -C "$work/repo" tag v0.9.0; git -C "$work/repo" tag vnext
run "no PRs since the last tag: nothing to release" "release=false
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a)
run "a commit that is no PR's merge fails" "exit=1"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "enhancement"
run "a PR with no semver label fails" "exit=1"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:patch" "semver:minor"
run "a PR with two semver labels fails" "exit=1"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); echo '[{"number":10,"merged_at":null,"base":{"ref":"main"},"labels":[{"name":"semver:patch"}]}]' > "$work/prs/$sha.json"
run "an unmerged PR does not count" "exit=1"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:patch"; jq '.[0].base.ref = "dev"' "$work/prs/$sha.json" > "$work/x" && mv "$work/x" "$work/prs/$sha.json"
run "a PR into another branch does not count" "exit=1"

# A force-push can leave the last release tag off main. The PRs merged before that release's commit are already in it.
# rewritten_release <date>: tags v1.2.3 on a commit dated <date> that is not an ancestor of main.
rewritten_release() {
  git -C "$work/repo" checkout -q -b old
  GIT_COMMITTER_DATE="$1" git -C "$work/repo" commit -q --allow-empty -m released
  git -C "$work/repo" tag v1.2.3
  git -C "$work/repo" checkout -q main
}
pr_merged() {
  local sha="$1" number="$2" merged_at="$3"; shift 3
  pr "$sha" "$number" "$@"
  jq --arg at "$merged_at" '.[0].merged_at = $at' "$work/prs/$sha.json" > "$work/x" && mv "$work/x" "$work/prs/$sha.json"
}

fresh; rewritten_release "2026-01-02T00:00:00Z"
a=$(commit a); pr_merged "$a" 10 "2026-01-01T00:00:00Z"
b=$(commit b); pr_merged "$b" 11 "2026-01-03T00:00:00Z" "semver:minor"
run "a release tag off main: only the PRs merged after its commit count" "release=true
version=1.3.0
exit=0"

fresh; rewritten_release "2026-01-02T00:00:00Z"
a=$(commit a); pr_merged "$a" 10 "2026-01-01T00:00:00Z"
b=$(commit b); pr_merged "$b" 11 "2026-01-02T00:00:00Z"
run "a release tag off main: PRs merged by its commit, even with no label, are already released" "release=false
exit=0"

# PENDING_LABELS: the labels of the PR whose beta is being versioned.
fresh
PENDING_LABELS="semver:patch" run "beta with no tags yet: 1.0.0 whatever the label" "release=true
version=1.0.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
PENDING_LABELS="semver:minor" run "beta with nothing merged since the last tag: the pending PR's label bumps" "release=true
version=1.3.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
sha=$(commit a); pr "$sha" 10 "semver:major"
PENDING_LABELS="semver:patch" run "beta: the highest level among the merged PRs and the pending one wins" "release=true
version=2.0.0
exit=0"

fresh; git -C "$work/repo" tag v1.2.3
PENDING_LABELS="enhancement" run "beta whose PR has no semver label fails" "exit=1"

fresh; git -C "$work/repo" tag v1.2.3
PENDING_LABELS="semver:patch semver:minor" run "beta whose PR has two semver labels fails" "exit=1"

finish
