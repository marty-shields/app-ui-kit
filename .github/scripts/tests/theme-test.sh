#!/usr/bin/env bash
# Tests src/Shields.UI/wwwroot/theme.js with theme-test.js, which runs it against a stub page (needs node).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"

actual=$(node "$here/theme-test.js" "$here/../../../src/Shields.UI/wwwroot/theme.js" 2>&1)

expected='key a applies dark
key a touches a
key b applies light
key b touches b
stored light -> get light, data-theme light
stored dark -> get dark, data-theme dark
stored system -> get system, data-theme none
stored sepia -> get system, data-theme none
stored null -> get system, data-theme none
set dark returns true, stores dark, data-theme dark
set system stores system, data-theme none
storage denied -> get system, set returns false, data-theme light
no key -> get system, set returns false, storage touched 0
no key warns 1 time, with a key warns 0'

check "theme.js reads and writes only its own key, applies and saves the choice" "$expected" "$actual"
finish
