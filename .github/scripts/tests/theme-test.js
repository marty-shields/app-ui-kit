// Loads wwwroot/theme.js into a stub page and prints one line per behaviour. Usage: node theme-test.js <path to theme.js>
const fs = require("fs");
const vm = require("vm");

const source = fs.readFileSync(process.argv[2], "utf8");

// A page whose script tag carries the storage key (or none), over the given storage contents.
function load(storageKey, stored, storageThrows = false) {
  const store = { ...stored };
  const reads = [];
  const writes = [];
  const attributes = {};
  const window = {};
  const warnings = [];
  vm.runInNewContext(source, {
    window,
    console: { warn: (message) => warnings.push(message) },
    document: {
      currentScript: { getAttribute: (name) => (name === "data-storage-key" ? storageKey : null) },
      documentElement: {
        setAttribute: (name, value) => { attributes[name] = value; },
        removeAttribute: (name) => { delete attributes[name]; },
      },
    },
    localStorage: {
      getItem: (k) => { if (storageThrows) throw new Error("denied"); reads.push(k); return k in store ? store[k] : null; },
      setItem: (k, v) => { if (storageThrows) throw new Error("denied"); writes.push(k); store[k] = v; },
    },
  });
  return { api: window.shieldsTheme, attributes, reads, writes, store, warnings };
}

const out = [];

for (const key of ["a", "b"]) {
  const page = load(key, { a: "dark", b: "light" });
  out.push(`key ${key} applies ${page.attributes["data-theme"]}`);
  page.api.set("system");
  page.api.set("dark");
  out.push(`key ${key} touches ${[...new Set([...page.reads, ...page.writes])].join(",")}`);
}

for (const value of ["light", "dark", "system", "sepia", null]) {
  const page = load("k", value === null ? {} : { k: value });
  out.push(`stored ${value} -> get ${page.api.get()}, data-theme ${page.attributes["data-theme"] ?? "none"}`);
}

const set = load("k", {});
out.push(`set dark returns ${set.api.set("dark")}, stores ${set.store.k}, data-theme ${set.attributes["data-theme"]}`);
set.api.set("system");
out.push(`set system stores ${set.store.k}, data-theme ${set.attributes["data-theme"] ?? "none"}`);

const blocked = load("k", {}, true);
out.push(`storage denied -> get ${blocked.api.get()}, set returns ${blocked.api.set("light")}, data-theme ${blocked.attributes["data-theme"]}`);

const keyless = load(null, { null: "dark" });
out.push(`no key -> get ${keyless.api.get()}, set returns ${keyless.api.set("dark")}, storage touched ${keyless.reads.length + keyless.writes.length}`);
out.push(`no key warns ${keyless.warnings.length} time, with a key warns ${set.warnings.length}`);

console.log(out.join("\n"));
