---
$attribute: CustomPage("Foundations/Introduction")
---

# Shields UI

The shared UI kit for the Calorie Tracker and Workout Log apps: design tokens, base styles, general components and this stories site.

## Where it lives
| Item | Location |
|---|---|
| Tokens | `src/Shields.UI/wwwroot/css/tokens.css` (CSS custom properties) |
| Components | `src/Shields.UI/Components/<Group>/` (Razor components with scoped `.razor.css`) |
| Stories and these pages | `src/Shields.UI.Stories/Stories/`: `Foundations/` and `Components/<Group>/` |

## Sections
- **Foundations**: this page and [Tokens](./?path=/story/foundations-tokens--all-tokens).
- **Components**: one folder per group, with one stories file per component. Each story is one visual state.

## Rules
- Light and dark themes. `data-theme="light"` or `"dark"` forces a theme on an element; component stories have a System / Light / Dark switch.
- Components use tokens only; no hard-coded colours or sizes. Story-only chrome (phone frame, theme switch) is exempt.
- A component or token that only one app needs stays in that app's repo.

## Checks
Tests in `Shields.UI.Tests` fail when a public component in `Shields.UI` has no stories file.
