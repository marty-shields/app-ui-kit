#!/usr/bin/env bash
# Usage: next-version.sh <tag prefix> <target sha>. Needs GH_TOKEN and GITHUB_REPOSITORY.
# Writes release and version to $GITHUB_OUTPUT (or stdout).
# PENDING_LABELS (the label names of a PR not merged yet, space separated) counts as one more merged PR, so a PR's beta
# gets the version its merge would release.
set -euo pipefail
prefix="$1"
target="$2"
out="${GITHUB_OUTPUT:-/dev/stdout}"

# level_of <label names>: 3, 2 or 1 for exactly one semver:major, semver:minor or semver:patch among them, else 0.
level_of() {
  local found
  found=$(tr ' ' '\n' <<<"$1" | { grep -E '^semver:(major|minor|patch)$' || true; })
  case "$found" in
    semver:major) echo 3 ;;
    semver:minor) echo 2 ;;
    semver:patch) echo 1 ;;
    *) echo 0 ;;
  esac
}

# Release tags only: <prefix>X.Y.Z.
part="(0|[1-9][0-9]*)"
semver="^${prefix}${part}\.${part}\.${part}$"
last=$(git tag --list "${prefix}*" --sort=-v:refname | { grep -E "$semver" || true; } | head -n 1)

if [[ -z "$last" ]]; then
  version=1.0.0
  echo "No ${prefix} tag yet: first release."
else
  bump=0
  released_at=$(git log -1 --format=%ct "$last")
  base=$(git merge-base "$last" "$target")
  shas=$(git rev-list "$base..$target")
  for sha in $shas; do
    prs=$(gh api "repos/$GITHUB_REPOSITORY/commits/$sha/pulls" --jq '[.[] | select(.merged_at != null and .base.ref == "main")]')
    if [[ $(jq length <<<"$prs") -ne 1 ]]; then
      echo "::error title=Version::Commit $sha isn't the merge of exactly one PR into main."
      exit 1
    fi
    # A PR merged by the release commit is in the release already, which matters when the tag is off the target branch.
    if jq -e --argjson at "$released_at" '.[0].merged_at | fromdateiso8601 <= $at' <<<"$prs" > /dev/null; then continue; fi
    number=$(jq -r '.[0].number' <<<"$prs")
    labels=$(jq -r '[.[0].labels[].name] | join(" ")' <<<"$prs")
    level=$(level_of "$labels")
    if (( level == 0 )); then
      echo "::error title=Version::PR #$number needs exactly one semver label; it has: ${labels:-none}. Fix the labels and run the release again."
      exit 1
    fi
    echo "PR #$number: $labels"
    if (( level > bump )); then bump=$level; fi
  done

  if [[ -n "${PENDING_LABELS:-}" ]]; then
    level=$(level_of "$PENDING_LABELS")
    if (( level == 0 )); then
      echo "::error title=Version::The PR needs exactly one semver label; it has: $PENDING_LABELS."
      exit 1
    fi
    if (( level > bump )); then bump=$level; fi
  fi

  if (( bump == 0 )); then
    echo "::notice title=Version::No PRs merged since $last: nothing to release."
    echo "release=false" >> "$out"
    exit 0
  fi

  IFS=. read -r major minor patch <<<"${last#"$prefix"}"
  case $bump in
    3) version="$((major + 1)).0.0" ;;
    2) version="$major.$((minor + 1)).0" ;;
    1) version="$major.$minor.$((patch + 1))" ;;
  esac
  echo "Last release $last."
fi

echo "Next release ${prefix}${version}."
{
  echo "release=true"
  echo "version=$version"
} >> "$out"
