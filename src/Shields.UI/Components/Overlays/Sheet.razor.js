// Keeps Tab inside the topmost open sheet, and puts focus back on the control that opened it.
const traps = [];
let nextId = 1;

function visible(element) {
    return element.getClientRects().length > 0 && !element.closest("[inert], [hidden]");
}

function tabStops(sheet) {
    return [...sheet.querySelectorAll("button, input, select, textarea, a[href], [tabindex]")].filter((element) => {
        if (element.disabled || element.tabIndex < 0 || !visible(element)) {
            return false;
        }
        // A radio group is one tab stop: the checked radio, or the first when none is checked. An unnamed radio is its own group.
        if (element.type === "radio" && element.name) {
            const group = [...sheet.querySelectorAll(`input[type="radio"][name="${CSS.escape(element.name)}"]`)].filter(visible);
            return element === (group.find((radio) => radio.checked) ?? group[0]);
        }
        return true;
    });
}

// Focus is in a dialog stacked above the sheet, such as a confirmation opened from it.
function inOtherDialog(sheet, current) {
    const dialog = current?.closest('[role="dialog"], [role="alertdialog"]');
    return dialog !== null && dialog !== undefined && dialog !== sheet && !sheet.contains(dialog);
}

export function open(sheet, dotNet) {
    const trap = { id: nextId++, sheet, opener: document.activeElement };
    trap.onKeyDown = (event) => {
        if (traps[traps.length - 1] !== trap) {
            return;
        }
        const current = document.activeElement;
        if (event.key === "Escape") {
            if (!sheet.contains(current) && !inOtherDialog(sheet, current)) {
                dotNet.invokeMethodAsync("EscapeOutsideSheet");
            }
            return;
        }
        if (event.key !== "Tab" || inOtherDialog(sheet, current)) {
            return;
        }
        const stops = tabStops(sheet);
        if (stops.length === 0) {
            event.preventDefault();
            return;
        }
        const first = stops[0];
        const last = stops[stops.length - 1];
        if (!sheet.contains(current)) {
            event.preventDefault();
            (event.shiftKey ? last : first).focus();
        } else if (event.shiftKey && current === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && current === last) {
            event.preventDefault();
            first.focus();
        }
    };
    traps.push(trap);
    document.addEventListener("keydown", trap.onKeyDown, true);
    (tabStops(sheet)[0] ?? sheet).focus();
    return trap.id;
}

export function close(id) {
    const index = traps.findIndex((trap) => trap.id === id);
    if (index < 0) {
        return;
    }
    const [trap] = traps.splice(index, 1);
    document.removeEventListener("keydown", trap.onKeyDown, true);
    if (index !== traps.length) {
        return;
    }
    // The app marks its opener with data-sheet-opener; a tapped button may not hold focus in WebKit, so fall back to the recorded one.
    const target = document.querySelector("[data-sheet-opener]") ?? trap.opener;
    if (target?.isConnected && target !== document.body) {
        target.focus();
    }
}
