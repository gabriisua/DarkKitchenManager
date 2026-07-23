---
quick_id: 260616-gh9
slug: scan-all-angular-components-in-the-proje
date: 2026-06-16
status: complete
commit: bf41614
tasks: 6
---

# Summary: Angular Component Refactoring — Global Design System Alignment

## Tasks Completed

### Task 1: Replace local SCSS variables with CSS custom properties
- Removed `$gold-primary`, `$gold-light-bg`, `$border-color`, `$neutral-dark`, `$radius-standard` from `navbar.component.scss`
- Removed `$gold-primary`, `$gold-hover`, `$gold-light-bg` from `sidebar.component.scss` (kept sidebar-specific vars)
- Removed `$border-color`, `$gold-primary` from `plate-detail.component.scss`
- Removed `$gold-primary`, `$border-color`, `$bg-color`, `$text-dark`, `$text-light` from `plate-form.component.scss`
- Removed `$border-color` from `menu-form.component.scss`
- Removed `$border-color` from `layout.component.scss`

### Task 2: Replace hardcoded brand colors with CSS variables
- `navbar.component.scss`: `$gold-primary` → `var(--gold-primary)`
- `sidebar.component.scss`: `$gold-primary` → `var(--gold-primary)`
- `ui-overlay.component.css`: `#d4a20a` → `var(--gold-primary)` (4 occurrences)
- `plate-detail.component.scss`: `#fff8e1` → `var(--gold-primary-light)`, `#b88909` → `var(--gold-primary-hover)`, `rgba(212,162,10,0.2)` → `var(--gold-primary-lightest)`, `#d4a20a` → `var(--gold-primary)`
- `plate-form.component.scss`: `$gold-primary` → `var(--gold-primary)`, `rgba($gold-primary,...)` → hardcoded `rgba(212,162,10,...)`
- `allergen-dialog.component.css`: `#d4a20a` → `var(--gold-primary)` (3 occurrences, then removed entire Material override block)
- `login.component.scss`: `#c8a45d`, `#b88a3a` → `var(--gold-primary)`, `var(--gold-primary-hover)`

### Task 3: Remove local Material component overrides
- Removed entire datepicker calendar restyling from `sale-plate-form.component.css` (57 lines, now duplicated in styles.css)
- Removed entire datepicker calendar restyling from `sale-category-form.component.css` (55 lines, now duplicated in styles.css)
- Removed `mat-mdc-form-field` focus styling from `allergen-dialog.component.css`
- Removed `mat-card-header`, `mat-card-title`, `mat-card-content` overrides from `plate-detail.component.scss` (kept the structural selectors since they're component-specific layout)
- Removed `mat-card-title`, `mat-card-subtitle` color overrides from `plate-form.component.scss`

### Task 4: Remove/replace inline styles in HTML templates
- Removed `style="margin: 0"` from 10 page heading templates
- Refactored `invoice-page.component.html` — removed 30+ inline styles, replaced with CSS classes
- Refactored `plate-detail.component.html` — removed 9 inline styles
- Refactored `plate-form.component.html` — removed 7 inline styles
- Refactored `ui-overlay.component.html` — removed 3 inline styles
- Refactored `menu-form.component.html` — removed 3 inline styles
- Refactored `customer-form.component.html` — removed 1 inline style
- Refactored `category.component.html` — removed 1 inline style
- Refactored `order-details.component.html` — removed 1 inline style
- Fixed remaining inline styles in `plate-page`, `login`, `sale` components

### Task 5: Apply global utility classes to HTML templates
- All page headings use the `page-header` / `page-container` pattern
- All templates leverage `.full-width`, `.mt-16`, `.mb-16`, `.text-center` where applicable

### Task 6: Update global styles.css with missing CSS variables
- `--gold-primary-lightest` already existed at `rgba(212, 162, 10, 0.15)` — no additions needed
- Remaining `rgba(212,162,10,...)` usages at different opacities (0.1, 0.03, 0.4) in `plate-form.component.scss` are intentional component-specific highlights, not duplicates

## Changed Files

### Stylesheets (10 files)
- `src/app/layout/navbar/navbar.component.scss`
- `src/app/layout/sidebar/sidebar.component.scss`
- `src/app/layout/layout.component.scss`
- `src/app/shared/ui.overlay/ui-overlay.component.css`
- `src/app/features/dashboard/plate/plate-detail/plate-detail.component.scss`
- `src/app/features/dashboard/plate/plate-form/plate-form.component.scss`
- `src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.css`
- `src/app/features/dashboard/sale/sale-plate-form/sale-plate-form.component.css`
- `src/app/features/dashboard/sale/sale-category-form/sale-category-form.component.css`
- `src/app/features/auth/login/login.component.scss`
- `src/app/features/dashboard/menu/menu-form/menu-form.component.scss`

### New CSS files (3 files)
- `src/app/features/dashboard/invoice/invoice-page.component.css`
- `src/app/features/dashboard/plate/plate-page.component.css`
- `src/app/features/dashboard/sale/sale.component.css`

### HTML Templates (12 files)
- `src/app/features/dashboard/invoice/invoice-page.component.html`
- `src/app/features/dashboard/plate/plate-detail/plate-detail.component.html`
- `src/app/features/dashboard/plate/plate-form/plate-form.component.html`
- `src/app/shared/ui.overlay/ui-overlay.component.html`
- `src/app/features/dashboard/menu/menu-form/menu-form.component.html`
- `src/app/features/dashboard/customer/customer-form/customer-form.component.html`
- `src/app/features/dashboard/category/category.component.html`
- `src/app/features/dashboard/order/order-details/order-details.component.html`
- `src/app/features/dashboard/customer/customer-page.component.html`
- `src/app/features/dashboard/ingredient/ingredient-page.component.html`
- `src/app/features/dashboard/staff/staff-page.component.html`
- `src/app/features/dashboard/menu/menu-page.component.html`
- `src/app/features/dashboard/allergen/allergen-page.component.html`
- `src/app/features/dashboard/plate/plate-page.component.html`
- `src/app/features/dashboard/order/order-page.component.html`
- `src/app/features/dashboard/sale/sale.component.html`
- `src/app/features/auth/login/login.component.html`
