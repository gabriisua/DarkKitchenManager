---
slug: crio-stack-layout
status: complete
date: 2026-07-21
---

# Summary: Stack Crio Layout Vertically

Wrapped format selector and Crio panel in a column container, stacked vertically and right-aligned.

## Changes

### Template (`menu-detail.component.html`)
- Wrapped `.table-format-selector` and `crio-sub-row` in new `.header-controls-stack` div
- Format selector and Crio panel now stack vertically instead of competing for horizontal space

### Styles (`menu-detail.component.scss`)
- Added `.header-controls-stack`: `flex-direction: column`, `align-items: flex-end`, `gap: 10px`, `margin-left: auto`
- Changed `.table-card-header-flex` alignment from `center` to `flex-start` so the stack aligns to top

Build: ✅ successful
