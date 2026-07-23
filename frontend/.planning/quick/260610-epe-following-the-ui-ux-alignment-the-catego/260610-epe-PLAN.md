---
quick_id: 260610-epe
type: ui-refactor
phase: quick
plan: 1
wave: 1
depends_on: []
files_modified:
  - src/app/features/dashboard/category/category.component.html
  - src/app/features/dashboard/category/category.component.ts
  - src/app/features/dashboard/category/category-form/category-form.component.html
autonomous: true
requirements: []
user_setup: []
must_haves:
  truths:
    - "Category page toolbar uses consistent global CSS classes (toolbar-row, toolbar-field, search-field, spacer)"
    - "Main CTA button uses standard mat-raised-button with toolbar-cta class instead of custom gold-btn"
    - "Reset Filters button appears when filters are active, clears them on click"
    - "Dialog save button uses standard mat-raised-button instead of custom gold-btn"
  artifacts:
    - path: "src/app/features/dashboard/category/category.component.html"
      provides: "Standardized toolbar with reset button, proper classes"
    - path: "src/app/features/dashboard/category/category.component.ts"
      provides: "clearFilters() and hasActiveFilters() methods"
    - path: "src/app/features/dashboard/category/category-form/category-form.component.html"
      provides: "Standard Material save button"
  key_links:
    - from: "category.component.html"
      to: "styles.css"
      via: "CSS classes toolbar-row, toolbar-field, search-field, spacer, toolbar-cta"
    - from: "category.component.ts"
      to: "category.component.html"
      via: "clearFilters() called by reset button click"
---

<objective>
Refactor the category page toolbar to use global CSS classes, add Reset Filters functionality, and replace custom `gold-btn` styling with standard Material buttons throughout.

Purpose: Align category page with the established design system (ingredient page pattern) — consistent toolbar layout, standard Material CTAs, and complete filter management (add + reset).
Output: Updated category.component.html, category.component.ts, and category-form.component.html.
</objective>

<execution_context>
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/workflows/execute-plan.md
</execution_context>

<context>
## Current State

**Category page toolbar (category.component.html):**
- Uses inline `style="display: flex; gap: 16px; ..."` instead of `.toolbar-row`
- Search field uses inline `style="width: 300px; margin-bottom: -1.34375em;"` instead of `.toolbar-field.search-field`
- Status filter uses inline `style="margin-bottom: -1.34375em;"` without `toolbar-field` class
- Spacer uses inline `style="flex: 1 1 auto;"` instead of `<div class="spacer"></div>`
- No Reset Filters button exists
- CTA uses `mat-flat-button color="primary" class="gold-btn"` — breaks design system

**Category form dialog (category-form.component.html):**
- Save button uses `mat-flat-button class="gold-btn"` — breaks design system
- Dialog already has `align="end"` on actions and `full-width` on inputs (correct)

**Reference pattern (ingredient page):**
- `<div gridSearch class="toolbar-row">` wraps all toolbar children
- `<mat-form-field ... class="toolbar-field search-field">` on search
- `<div class="spacer"></div>` for flex spacing
- Reset button: `mat-stroked-button` with `[disabled]` + tooltip + `clear_all` icon
- CTA: `mat-raised-button color="primary" class="toolbar-cta"`

**Global CSS available in styles.css:**
- `.toolbar-row` — flex row with gap 16px, wrap
- `.toolbar-field` — responsive margin (media query only)
- `.search-field` — `min-width: 280px`
- `.spacer` — `flex: 1 1 auto`
- `.toolbar-cta` — `height: 48px`

## Files to Modify

1. **category.component.html** — Rewrite toolbar section to use global classes, add Reset button, fix CTA button type
2. **category.component.ts** — Add `hasActiveFilters()` and `clearFilters()` methods
3. **category-form.component.html** — Replace `gold-btn` on save with `mat-raised-button color="primary"`
</context>

<tasks>

<task type="auto" tdd="false">
  <name>Task 1: Standardize category page toolbar (+ Reset Filters)</name>
  <files>
    src/app/features/dashboard/category/category.component.html
    src/app/features/dashboard/category/category.component.ts
  </files>
  <action>
    **category.component.html changes:**

    1. Replace the `gridSearch` div's inline styles with `class="toolbar-row"`:
       - From: `<div gridSearch style="display: flex; gap: 16px; align-items: center; margin-bottom: 16px; width: 100%; flex-wrap: wrap;">`
       - To: `<div gridSearch class="toolbar-row">`

    2. Replace the search mat-form-field inline styles with classes:
       - From: `<mat-form-field appearance="outline" style="width: 300px; margin-bottom: -1.34375em;">`
       - To: `<mat-form-field appearance="outline" class="toolbar-field search-field">`

    3. Replace the status mat-form-field inline styles with `toolbar-field` class:
       - From: `<mat-form-field appearance="outline" style="margin-bottom: -1.34375em;">`
       - To: `<mat-form-field appearance="outline" class="toolbar-field">`

    4. Replace the spacer div:
       - From: `<div style="flex: 1 1 auto;"></div>`
       - To: `<div class="spacer"></div>`

    5. **Add Reset Filters button** between the spacer and the CTA button, following the ingredient page pattern:
       ```html
       <button
         mat-stroked-button
         (click)="clearFilters()"
         matTooltip="Resetta tutti i filtri"
         [disabled]="!hasActiveFilters()">
         <mat-icon>clear_all</mat-icon>
         Reset
       </button>
       ```
       Place it before the `.spacer` div (i.e., after the status mat-form-field, before the spacer), matching ingredient page ordering where Reset comes before spacer.

    6. Replace CTA button:
       - From: `<button mat-flat-button color="primary" class="gold-btn" (click)="openModal()">`
       - To: `<button mat-raised-button color="primary" class="toolbar-cta" (click)="openModal()">`

    **category.component.ts changes:**

    7. Add `hasActiveFilters()` method:
       ```typescript
       hasActiveFilters(): boolean {
         return !!this.searchTerm || this.isActiveFilter !== null;
       }
       ```
       Place after `onFilterChange()` method.

    8. Add `clearFilters()` method:
       ```typescript
       clearFilters(): void {
         this.searchTerm = '';
         this.isActiveFilter = null;
         this.query.next({
           page: 1,
           pageSize: this.query.value.pageSize,
           search: '',
           sortColumn: this.query.value.sortColumn,
           sortDirection: this.query.value.sortDirection,
           isActive: undefined,
         });
       }
       ```
       Place after `hasActiveFilters()`.

    Do NOT change: the `actionsTemplate`, `page-header`, `page-container`, or `app-data-grid` wrapper elements. Do NOT add/remove module imports — MatTooltipModule, MatButtonModule are already imported in the component.
  </action>
  <verify>
    <automated>grep -c 'toolbar-row' src/app/features/dashboard/category/category.component.html && grep -c 'spacer' src/app/features/dashboard/category/category.component.html && grep -c 'toolbar-cta' src/app/features/dashboard/category/category.component.html && grep -c 'clearFilters' src/app/features/dashboard/category/category.component.ts && grep -c 'hasActiveFilters' src/app/features/dashboard/category/category.component.ts && grep -c 'mat-raised-button' src/app/features/dashboard/category/category.component.html && grep -c 'mat-stroked-button' src/app/features/dashboard/category/category.component.html</automated>
  </verify>
  <done>
    - category.component.html toolbar uses `.toolbar-row`, `.toolbar-field`, `.search-field`, `.spacer`
    - CTA button is `mat-raised-button color="primary" class="toolbar-cta"`
    - Reset Filters button present with `mat-stroked-button`, `clear_all` icon, tooltip
    - `hasActiveFilters()` returns true when searchTerm or isActiveFilter is set
    - `clearFilters()` resets both filters and re-triggers the query
    - No `gold-btn` class remains in category.component.html
  </done>
</task>

<task type="auto" tdd="false">
  <name>Task 2: Fix category form dialog save button styling</name>
  <files>
    src/app/features/dashboard/category/category-form/category-form.component.html
  </files>
  <action>
    Replace the save button in the dialog actions:

    - From: `<button mat-flat-button class="gold-btn" (click)="onSubmit()">`
    - To: `<button mat-raised-button color="primary" (click)="onSubmit()">`

    Keep everything else unchanged — the dialog already uses `class="full-width"` on inputs and `align="end"` on `mat-dialog-actions`, which is the correct pattern.
  </action>
  <verify>
    <automated>grep -c 'mat-raised-button' src/app/features/dashboard/category/category-form/category-form.component.html && grep -v 'gold-btn' src/app/features/dashboard/category/category-form/category-form.component.html > /dev/null</automated>
  </verify>
  <done>
    - Dialog save button uses `mat-raised-button color="primary"` instead of `gold-btn`
    - No `gold-btn` class remains in category-form.component.html
    - Dialog layout (full-width inputs, align="end" actions) is unchanged
  </done>
</task>

</tasks>

<verification>
1. `grep -c 'style=' src/app/features/dashboard/category/category.component.html` — should return 0 lines with style attributes in the toolbar section (inline style on the h1 in `page-header` is acceptable, it was already there)
2. `grep -c 'gold-btn' .planning/quick/260610-epe-following-the-ui-ux-alignment-the-catego/` — should return 0
3. `grep -c 'toolbar-row' src/app/features/dashboard/category/category.component.html` — should return 1
4. `grep -c 'clearFilters\|hasActiveFilters' src/app/features/dashboard/category/category.component.ts` — should return 2
</verification>

<success_criteria>
- Category page toolbar matches ingredient page pattern: `.toolbar-row` wrapper, `.toolbar-field.search-field` on search, `.toolbar-field` on status select, `.spacer` div
- "Nuova Categoria" CTA is a standard `mat-raised-button color="primary"` with `.toolbar-cta` class
- Reset Filters button visible when filters are active, clears all filters on click
- Dialog "Crea Categoria" / "Salva Modifiche" uses standard `mat-raised-button color="primary"`
- Zero references to `gold-btn` class remain in category feature files
</success_criteria>

<output>
After completion, this plan is self-contained. No SUMMARY file needed for a quick task.
</output>
