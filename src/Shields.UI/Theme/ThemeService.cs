using Microsoft.JSInterop;

namespace Shields.UI.Theme;

/// <summary>
/// Keeps the chosen <see cref="ThemePreference"/> in the web view's local storage and applies it to the document as <c>data-theme</c>.
/// <c>theme.js</c> (its script tag carries <c>data-storage-key</c>), loaded in the page head, applies the saved choice before the first paint.
/// </summary>
public sealed class ThemeService(IJSRuntime js)
{
    /// <summary>The saved choice. <see cref="ThemePreference.System"/> when nothing is saved or storage is unavailable.</summary>
    public async Task<ThemePreference> GetAsync() =>
        Enum.TryParse<ThemePreference>(await js.InvokeAsync<string>("shieldsTheme.get"), ignoreCase: true, out var preference)
            ? preference
            : ThemePreference.System;

    /// <summary>Applies the choice to the document and saves it. False when nothing was saved, because storage refused the save or the <c>theme.js</c> script tag has no <c>data-storage-key</c>: the choice still applies until the app closes.</summary>
    public async Task<bool> SetAsync(ThemePreference preference) =>
        await js.InvokeAsync<bool>("shieldsTheme.set", preference.ToString().ToLowerInvariant());
}
