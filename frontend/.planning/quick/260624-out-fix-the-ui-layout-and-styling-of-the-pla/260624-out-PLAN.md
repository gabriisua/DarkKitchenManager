---
phase: quick
id: 260624-out
type: execute
wave: 1
autonomous: true
---

# Quick Task 260624-out: Fix UI layout and styling of the plates table in MenuDetailComponent

## Tasks

### Task 1: Increase table row height, fix cell padding, and migrate inline styles to component SCSS

**Files modified:**
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.html`

**Action:**

**A) Increase table row height and cell padding (menu-detail.component.scss)**

The `.plates-table` uses Angular Material MDC-based table. The default `.mat-mdc-row` has a fixed height that is too short for `mat-form-field appearance="outline"` with `subscriptSizing="dynamic"`. Fix by adding scoped overrides:

```scss
// Inside the .plates-table { ... } block, add:
.mat-mdc-row {
  height: auto;
  min-height: 68px;       // Enough vertical room for outline form fields
}

.mat-mdc-cell {
  padding-top: 8px;
  padding-bottom: 8px;
  vertical-align: middle;
}

// Increase column widths for copies/pause so the outline fields aren't squeezed
.mat-column-copies,
.mat-column-pauseAfter {
  width: 120px;           // Was 100px — too narrow for outline + number input
  text-align: center;
}

.mat-column-lotNumber {
  width: 160px;           // Was 140px
  text-align: center;
}
```

**B) Add CSS classes for all currently inline-styled elements (menu-detail.component.scss)**

Add these new classes inside the component SCSS — never use inline `style="..."`:

```scss
/* Category heading — replaces inline style on h3 inside .category-block */
.category-block {
  h3 {
    color: #D4A20A;
    text-transform: uppercase;
    font-weight: bold;
    border-bottom: 2px solid #f0f0f0;
    padding-bottom: 8px;
    margin-bottom: 16px;
  }
}

/* Price badge — replaces inline style on .price-badge */
.price-badge {
  font-weight: bold;
  color: #D4A20A;
}

/* Plate name label — replaces inline styles on the span inside .plate-name-cell */
.plate-name-label {
  display: block;
  font-size: 14px;
}

/* Plate description — replaces inline styles on the description span */
.plate-description {
  font-size: 12px;
  color: #666;
}

/* Inline form-field width variants (replace inline style="width: 80/120px") */
.form-field-narrow {
  width: 80px;
}

.form-field-wide {
  width: 120px;
}

/* Keep .action-buttons-container; the flex-end is already implied by .text-right parent.
   No justify-content override needed on the container — it inherits from the cell.
*/
```

**C) Remove inline `style` attributes from the template (menu-detail.component.html)**

Strip all 12 inline `style="..."` attributes and replace with proper CSS classes:

| Location | Inline style | Replacement |
|----------|-------------|-------------|
| Line 3 `.page-header` | `style="margin-bottom: 16px;"` | Remove — `mb-16` class already exists in styles.css as `.mb-16` |
| Line 78 `.category-block` | `style="margin-bottom: 32px;"` | Add class `mb-32` or just use existing `.mt-16` pattern — add `.category-block { margin-bottom: 32px; }` in SCSS |
| Line 80 `<h3>` | Full heading inline | Handled by `.category-block h3` class |
| Line 88 `<th>` | `style="width: 48px;"` | Remove — already handled by `.mat-column-select { width: 48px; }` |
| Line 106 `.plate-name-cell` | `style="padding: 8px 0;"` | Add to `.plate-name-cell` in SCSS |
| Line 107 `<span>` | `style="display: block; font-size: 14px;"` | Use `.plate-name-label` class |
| Line 109 `<span>` | `style="font-size: 12px; color: #666;"` | Use `.plate-description` class |
| Line 119 `.price-badge` | `style="font-weight: bold; color: #D4A20A;"` | Use `.price-badge` class |
| Line 133 `<mat-form-field>` | `style="width: 80px;"` | Use `form-field-narrow` class |
| Line 142 `<mat-form-field>` | `style="width: 80px;"` | Use `form-field-narrow` class |
| Line 151 `<mat-form-field>` | `style="width: 120px;"` | Use `form-field-wide` class |
| Line 160 `.action-buttons-container` | `style="justify-content: flex-end;"` | Remove — `.text-right` on the parent td already provides alignment |

**Verify:**
- `ng build` compiles without errors (run from project root)
- Visually inspect: table rows are taller, outline form-fields have breathing room, no inline `style` attributes remain in the HTML template

**Done:**
- `.plates-table .mat-mdc-row` has `min-height: 68px` and `height: auto`
- `.plates-table .mat-mdc-cell` has top/bottom padding and vertical-align middle
- All inline `style="..."` attributes removed from template and replaced with CSS classes
- Build succeeds

### Task 2: Add global mat-menu styling for Print CTAs

**Files modified:**
- `src/styles.css`

**Action:**

Add mat-menu global overrides to `src/styles.css`. mat-menu panels are rendered by Angular Material in an overlay outside the component's view encapsulation, so they must be styled globally.

Append these rules to the end of `styles.css` (before the `@font-face` section or after existing Material overrides):

```css
/* ==========================================================================
   MATERIAL MENU (mat-menu)
   Used by: MenuDetailComponent Print CTAs (single + batch)
   Note: mat-menu renders outside component view encapsulation → global styles
   ========================================================================== */

.mat-mdc-menu-panel {
  background-color: #ffffff !important;
  border-radius: 8px !important;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12), 0 2px 8px rgba(0, 0, 0, 0.08) !important;
  border: 1px solid #e0e0e0 !important;
  padding: 4px 0 !important;
  min-width: 200px !important;
}

.mat-mdc-menu-item {
  min-height: 40px !important;
  padding: 0 16px !important;
  font-family: var(--font-primary);
  font-size: 14px !important;
  color: #333333 !important;
  transition: background-color 0.15s ease !important;
  gap: 12px;                     /* Space between icon and text */
}

.mat-mdc-menu-item .mat-icon {
  margin-right: 0;               /* Remove default margin — gap handles spacing */
  color: var(--gold-primary);
  font-size: 20px;
  height: 20px;
  width: 20px;
  line-height: 20px;
}

.mat-mdc-menu-item .mat-icon + span {
  line-height: normal;           /* Ensure text baseline aligns with icon */
}

.mat-mdc-menu-item:hover:not([disabled]) {
  background-color: #f5f5f5 !important;
}

.mat-mdc-menu-item:active {
  background-color: var(--gold-primary-lightest) !important;
}
```

> **Why global (not component-scoped):** Angular Material's overlay system renders `mat-menu-panel` as a sibling of the application root, not inside the component's DOM subtree. Component-scoped styles (via `styleUrls`) cannot penetrate the overlay — only global styles in `styles.css` can style `.mat-mdc-menu-panel` and `.mat-mdc-menu-item`.

**Verify:**
- `ng build` compiles without errors
- Visually: Open both the single-row "Stampa..." menu and the batch "Stampa Selezionati..." menu
  - Menu panel has clean white background, subtle shadow, rounded corners
  - Icon (label/receipt_long) is vertically aligned with the label text
  - Hover state shows light gray background
  - Active/click state shows gold-tinted background

**Done:**
- `.mat-mdc-menu-panel` styled with white bg, border-radius, shadow, border
- `.mat-mdc-menu-item` has proper padding, icon-text alignment via `gap`, hover styles
- Build succeeds
- Both menus (single row + batch) are visually consistent

## Verification

```bash
ng build 2>&1 | tail -5
```
Expected: no errors (pre-existing budget warnings are acceptable).

## Summary

After completion, create `260624-out-SUMMARY.md` in this directory documenting:
1. Files modified
2. Key CSS changes (row height, cell padding, menu styles, inline style migration)
3. Verification result
