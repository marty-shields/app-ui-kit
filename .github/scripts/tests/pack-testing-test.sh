#!/usr/bin/env bash
# Packs src/Shields.UI.Testing and checks the package: its dll embeds the kit's files, and it depends on no Shields.UI package (needs dotnet, python3).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

if ! dotnet pack "$here/../../../src/Shields.UI.Testing" -c Release -o "$work" > "$work/pack.log" 2>&1; then
  tail -20 "$work/pack.log"
  check "dotnet pack succeeds" "pass" "fail"
  finish
  exit
fi

# shellcheck disable=SC2016 # the Python is single-quoted on purpose
report=$(python3 -I -c '
import glob, sys, zipfile
package = zipfile.ZipFile(glob.glob(sys.argv[1] + "/Shields.UI.Testing.*.nupkg")[0])
dll = package.read("lib/net11.0/Shields.UI.Testing.dll")
for name in ("tokens.css", "base.css", "bundle.scp.css", "theme.js"):
    print(name, "embedded" if name.encode() in dll else "missing")
nuspec = package.read([n for n in package.namelist() if n.endswith(".nuspec")][0]).decode()
print("depends on Shields.UI" if "Shields.UI\"" in nuspec else "no Shields.UI dependency")
' "$work")

check "the package dll embeds the kit's files and has no Shields.UI dependency" "tokens.css embedded
base.css embedded
bundle.scp.css embedded
theme.js embedded
no Shields.UI dependency" "$report"
finish
