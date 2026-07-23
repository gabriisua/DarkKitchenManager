---
phase: quick
id: 260624-out
type: execute
completed: true
tasks: 2/2
commits:
  - hash: e4f1209
    message: "style(quick-260624-out): increase table row height, fix cell padding, migrate inline styles to SCSS"
    files:
      - src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss
      - src/app/features/dashboard/menu/menu-detail/menu-detail.component.html
  - hash: 6711350
    message: "style(quick-260624-out): add global mat-menu styling for Print CTAs"
    files:
      - src/styles.css
key-files:
  modified:
    - src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss
    - src/app/features/dashboard/menu/menu-detail/menu-detail.component.html
    - src/styles.css
decisions: []
deviations: []
---

# Quick Task 260624-out: Fix UI layout and styling of the plates table in MenuDetailComponent

## One-liner

Increased plate table row height and cell padding for outline form-field clearance, migrated all 12 inline `style="..."` attributes from the HTML template to component SCSS classes, and added global mat-menu overlay styles for the ZPL print CTAs.

## Changes Made

### Task 1 — Table row height, cell padding, inline style migration

**Files:** `menu-detail.component.scss`, `menu-detail.component.html`

**SCSS additions:**
- `.plates-table .mat-mdc-row`: `height: auto; min-height: 68px` — provides enough vertical room for `appearance="outline"` form fields inside table cells
- `.plates-table .mat-mdc-cell`: `padding-top/bottom: 8px; vertical-align: middle` — breathing room inside cells
- `.mat-column-copies`, `.mat-column-pauseAfter`: width increased from 100px → 120px
- `.mat-column-lotNumber`: width increased from 140px → 160px
- New class `.category-block` with `margin-bottom: 32px` and `h3` heading styling (gold uppercase)
- New class `.price-badge` — bold gold text (replaces inline style on price spans)
- New class `.plate-name-label` — block display, 14px (replaces inline style on plate name span)
- New class `.plate-description` — 12px gray text (replaces inline style on description span)
- New classes `.form-field-narrow` (80px) and `.form-field-wide` (120px) — replace inline width on mat-form-fields
- `.plate-name-cell` — added `padding: 8px 0`

**HTML changes — 12 inline styles removed:**
1. `.page-header` `style="margin-bottom: 16px"` → removed (inherits `.page-header` base style)
2. `.category-block` `style="margin-bottom: 32px"` → moved to SCSS class
3. `<h3>` full heading inline → `.category-block h3` class
4. `<th>` `style="width: 48px"` → removed (already handled by `.mat-column-select`)
5. `.plate-name-cell` `style="padding: 8px 0"` → moved to SCSS class
6. `<span>` `style="display: block; font-size: 14px"` → `.plate-name-label` class
7. `<span>` `style="font-size: 12px; color: #666"` → `.plate-description` class
8. `.price-badge` `style="font-weight: bold; color: #D4A20A"` → `.price-badge` class
9. `<mat-form-field>` (copies) `style="width: 80px"` → `.form-field-narrow` class
10. `<mat-form-field>` (pause) `style="width: 80px"` → `.form-field-narrow` class
11. `<mat-form-field>` (lot) `style="width: 120px"` → `.form-field-wide` class
12. `.action-buttons-container` `style="justify-content: flex-end"` → removed (`.text-right` on parent td)

### Task 2 — Global mat-menu styling for Print CTAs

**File:** `src/styles.css`

Added Material Menu overlay styles after existing component overrides and before the `@font-face` section:
- `.mat-mdc-menu-panel`: white background, 8px border-radius, elevated shadow, 1px border, 4px padding, min-width 200px
- `.mat-mdc-menu-item`: 40px min-height, 14px font, gray text, hover → light gray, active → gold-tinted
- `.mat-mdc-menu-item .mat-icon`: gold color, 20px square, no right margin (gap handles spacing)
- `.mat-mdc-menu-item .mat-icon + span`: `line-height: normal` for baseline alignment

## Verification

```bash
ng build 2>&1 | tail -5
# Application bundle generation complete. [2.107 seconds] (Task 1)
# Application bundle generation complete. [seconds] (Task 2)
# Only pre-existing budget warning — no errors.
```

## Deviations from Plan

None — plan executed exactly as written.

## Stubs

None — all styling is complete and wired.
