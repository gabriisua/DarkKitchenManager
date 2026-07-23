---
quick_id: 260610-eyj
status: complete
type: ui-refactor
date: 2026-06-10
---

# Quick Task 260610-eyj: Allergen Page UI Refactoring

## What was done

### Task 1: Standardize allergen page toolbar
- **allergen-page.component.html**: Replaced custom `header-actions` class with global `toolbar-row`, `toolbar-field`, `search-field`, `spacer`, `toolbar-cta` CSS classes
- **allergen-page.component.html**: Added Reset Filters button with `mat-stroked-button`, `clear_all` icon, tooltip, and `*ngIf="searchTerm"` visibility
- **allergen-page.component.html**: Removed inline `style="margin-left: 10px"` from CTA button
- **allergen-page.component.ts**: Removed inline `styles` array (global CSS takes over)
- **allergen-page.component.ts**: Added `clearFilters()` method

### Task 2: Fix allergen dialog
- **allergen-dialog.component.html**: Added `full-width` class to all form fields
- **allergen-dialog.component.ts**: Converted constructor injection to `inject()` for all dependencies

## Files modified
- `src/app/features/dashboard/allergen/allergen-page.component.html`
- `src/app/features/dashboard/allergen/allergen-page.component.ts`
- `src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.html`
- `src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.ts`

## Verification
- No `header-actions` or inline toolbar styles remain
- Global `toolbar-row`, `toolbar-cta`, `spacer` classes applied correctly
- Reset Filters button visible only when searchTerm has value
- Dialog form fields use `full-width` class
- All dependencies use `inject()` pattern
