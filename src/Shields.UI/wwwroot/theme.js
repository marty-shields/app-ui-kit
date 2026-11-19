(function () {
    // The script tag names the storage key, so each app keeps the key it already uses.
    var key = document.currentScript && document.currentScript.getAttribute("data-storage-key");
    if (!key) {
        console.warn("shields theme: the script tag has no data-storage-key, so the choice will not be saved.");
    }

    function get() {
        try {
            var value = key ? localStorage.getItem(key) : null;
            return value === "light" || value === "dark" ? value : "system";
        } catch (e) {
            return "system";
        }
    }

    function apply(value) {
        if (value === "light" || value === "dark") {
            document.documentElement.setAttribute("data-theme", value);
        } else {
            document.documentElement.removeAttribute("data-theme");
        }
    }

    function set(value) {
        apply(value);
        if (!key) {
            return false;
        }
        try {
            localStorage.setItem(key, value);
            return true;
        } catch (e) {
            return false;
        }
    }

    apply(get());
    window.shieldsTheme = { get: get, set: set };
})();
