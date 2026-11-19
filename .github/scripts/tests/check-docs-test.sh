#!/usr/bin/env bash
# shellcheck disable=SC2016 # the backticks in the sample Markdown are not command substitutions
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../check-docs.py"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

repo() {
  rm -rf "${work:?}/$1" && mkdir -p "$work/$1/docs" && cd "$work/$1" || exit 1
  git init -q
  printf 'data\n' > docs/data.txt
  printf '%s\n' "$2" > README.md
  printf '# Guide\n\n## Setup & run\n\n## Setup & run\n\n### `Code` heading\n\n```\n[not a link](missing.md)\n```\n' > docs/guide.md
  git add -A
}

run() {
  check "$1" "$2" "$(python3 -I "$script" "$work/$3" 2>&1; echo "exit=$?")"
}

repo ok '# Title

See [guide](docs/guide.md#setup--run), [again](docs/guide.md#setup--run-1), [code](docs/guide.md#code-heading),
[below](#title), [site](https://example.com/missing), [mail](mailto:a@b.c) and [dir](docs/).
<a id="custom"></a> [custom](#custom) and [story](./?path=/story/x).
<img src="docs/guide.md" /> `[inline](missing.md)`
<a id="Custom"></a> [mixed case](#Custom), [root](/docs/guide.md), [text](docs/data.txt#frag).'
run "links and anchors that resolve" 'exit=0' ok

repo missing '[gone](docs/nope.md)
<img src="docs/nope.png" />
[root](/docs/nope.md)'
run "a missing file in a link or an image" 'README.md:1: link to a missing path: docs/nope.md
README.md:2: link to a missing path: docs/nope.png
README.md:3: link to a missing path: /docs/nope.md
exit=1' missing

repo anchor '[a](docs/guide.md#nope) and [b](#nope)'
run "a missing anchor in another file or the same file" 'README.md:1: no heading or anchor "#nope" in docs/guide.md
README.md:1: no heading or anchor "#nope" in README.md
exit=1' anchor

repo ref '[ref]: docs/nope.md'
run "a reference-style link" 'README.md:1: link to a missing path: docs/nope.md
exit=1' ref

repo escape '[out](../outside.md)'
touch "$work/outside.md"
run "a link out of the repository" 'README.md:1: link to a missing path: ../outside.md
exit=1' escape

repo title "[gone](docs/nope.md 'a title') and [fine](docs/guide.md \"a title\")"
run "a single-quoted link title" 'README.md:1: link to a missing path: docs/nope.md
exit=1' title

repo fence '```foo``` and [one](docs/nope.md)

    ```
[two](docs/nope.md)

~~~
```
[hidden](docs/nope.md)
~~~
[three](docs/nope.md)'
run "a fence opens and closes as in GFM" 'README.md:1: link to a missing path: docs/nope.md
README.md:4: link to a missing path: docs/nope.md
README.md:10: link to a missing path: docs/nope.md
exit=1' fence

repo long '````
```
[hidden](docs/nope.md)
````
~~~ has `tick`
[hidden](docs/nope.md)
~~~
[seen](docs/nope.md)'
run "a shorter fence does not close a longer one and a tilde fence takes a backtick" 'README.md:8: link to a missing path: docs/nope.md
exit=1' long

finish
