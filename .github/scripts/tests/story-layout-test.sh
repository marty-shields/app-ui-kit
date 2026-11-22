#!/usr/bin/env bash
# Tests that check-story-layout.mjs fails a story whose page throws or logs a console error, against a stub page (needs node and Chrome; skips without Chrome).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"

chrome="${CHROME:-}"
for candidate in google-chrome chromium chromium-browser; do
  [ -n "$chrome" ] || chrome="$(command -v "$candidate" || true)"
done
[ -n "$chrome" ] || chrome="$(compgen -G "$HOME/.cache/ms-playwright/chromium-*/chrome-linux*/chrome" | head -1)"
if [ -z "$chrome" ]; then
  echo "skip story-layout: no Chrome found (set CHROME)"
  exit 0
fi

log="$(mktemp)"
node "$here/story-layout-test.js" > "$log" &
server=$!
trap 'kill "$server" 2>/dev/null; rm -f "$log"' EXIT
for _ in $(seq 50); do [ -s "$log" ] && break; sleep 0.1; done
port="$(cat "$log")"

output=$(node "$here/../check-story-layout.mjs" "http://127.0.0.1:$port" "$chrome" Components/Layout/Card Components/Layout/AppErrorBoundary 2>&1)
# The stub has no preview box, so every story also gets a layout finding; keep each story heading only above its error and attribute lines.
actual=$(printf '%s\n' "$output" | awk '/^Components\// { heading = $0 } /^  (page exception|console error|.* has an invalid attribute name)/ { if (heading) print heading; heading = ""; print }')

expected='Components/Layout/Card / Compact
  console error: Unhandled exception rendering component: bad attribute
Components/Layout/Card / Dark
  console error: fail: Shields.UI.Components.Layout.AppErrorBoundary[9101]
Components/Layout/Card / Light
  div.x has an invalid attribute name: "\""
Components/Layout/Card / Title
  page exception: Error: boom from the page'

check "a page exception and a console error fail their story with the first error line; an allowed error is skipped only for its own story; an attribute named a double quote fails" "$expected" "$actual"
finish
