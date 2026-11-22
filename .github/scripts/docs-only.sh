#!/usr/bin/env bash
# Usage: docs-only.sh <base> <head>
# Prints true when every file changed between the merge base of <base> and <head> is documentation (under docs/ or a
# *.md file), else false. A change that touches no file prints false, so the caller still runs.
set -euo pipefail
files=$(git diff --name-only "$1...$2")
if [[ -z "$files" ]]; then
  echo false
  exit 0
fi
while IFS= read -r file; do
  if [[ "$file" != docs/* && "$file" != *.md ]]; then
    echo false
    exit 0
  fi
done <<<"$files"
echo true
