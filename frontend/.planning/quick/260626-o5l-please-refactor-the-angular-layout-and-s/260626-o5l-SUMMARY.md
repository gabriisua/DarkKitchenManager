# Quick Task 260626-o5l: Refactor Menu Detail Layout & Theme Integration

**Phase:** quick/260626-o5l
**Plan:** 01
**Type:** execute
**Status:** Complete

**One-liner:** Move batch print button from hero section into right-aligned table header action group alongside format toggle, theme-integrate selector block with global palette colors.

## Task Results

| # | Task | Type | Status | Commit |
|---|------|------|--------|--------|
| 1 | Restructure table card header — move batch print button into right-aligned action group | auto | Done | `c28284c` |
| 2 | Theme-integrate format selector styles and add batch button layout | auto | Done | `d2ad39f` |

## Files Modified

- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.html`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss`

## Changes Summary

### Task 1 — HTML Restructure
- Removed "Stampa Selezionati" button from `.hero-cta-container` in the hero section
- Added the button inside `.table-format-selector` after the toggle group, with a `.action-group-spacer` to push it right
- Wrapped the format label + toggle group in a `.format-toggle-section` wrapper
- Button retains `[disabled]="selection.isEmpty()"` binding
- PDF download button remains unchanged in the hero section
- No HTML comments added

### Task 2 — SCSS Theme Integration
- Replaced `.table-format-selector` custom background `#faf6f0` with theme-matching `#fafafa`
- Replaced custom border `#eae1d8` with neutral `#e0e0e0`
- Added `.format-toggle-section` (flex row, 12px gap) to group label + toggle
- Added `.action-group-spacer` (`flex: 1 1 auto`) to push button to the right
- Added `.batch-print-header-btn` with compact pill shape (36px height, 18px border-radius)
- Preserved all existing styles (`.hero-cta-container`, `.hero-download-btn`, etc.)

## Deviations from Plan

None — plan executed exactly as written.

## Verification Results

| Check | Result |
|-------|--------|
| No custom colors (`#faf6f0`/`#eae1d8`) in SCSS | PASS |
| `Stampa Selezionati` appears exactly once (table header only) | PASS (1 occurrence) |
| `[disabled]="selection.isEmpty()"` on moved button | PASS |
| `Scarica PDF` button remains in hero section | PASS |
| No HTML comments (`<!--`) | PASS |
| No SCSS `//` comments | PASS |

## Key Decisions

- Used `#fafafa` (table header row bg) and `#e0e0e0` (dropdown border) per the global styles.css palette — consistent with the rest of the dashboard's table header styling
- `.batch-print-header-btn` uses 36px height (vs hero's 40px) for a more compact fit in the table header line, sharing the pill shape (`border-radius: 18px`) pattern with `.hero-download-btn`
