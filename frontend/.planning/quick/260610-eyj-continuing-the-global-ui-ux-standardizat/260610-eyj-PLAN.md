---
quick_id: 260610-eyj
type: ui-refactor
phase: quick
plan: 1
wave: 1
depends_on: []
files_modified:
  - src/app/features/dashboard/allergen/allergen-page.component.html
  - src/app/features/dashboard/allergen/allergen-page.component.ts
  - src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.html
  - src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.ts
autonomous: true
---

<objective>
Refactor the allergen page toolbar to use global CSS classes, add Reset Filters functionality, replace custom inline styles with standard toolbar-row pattern, and modernize the dialog component.
</objective>

<context>
## Current State

**Allergen page toolbar (allergen-page.component.html):**
- Uses custom `header-actions` class instead of global `toolbar-row`
- Search field uses `search-field` class but missing `toolbar-field`
- CTA button has inline `style="margin-left: 10px"` with no `toolbar-cta` class
- No `.spacer` div to push CTA to the right
- No Reset Filters button

**Allergen dialog (allergen-dialog.component.html):**
- Form fields missing `full-width` class
- Already uses `align="end"` on actions (correct)
- Already uses `mat-raised-button color="primary"` (correct)

**Allergen dialog TS (allergen-dialog.component.ts):**
- Uses constructor injection instead of `inject()` pattern

**Reference pattern:**
- Category and ingredient pages use `.toolbar-row`, `.toolbar-field`, `.spacer`, `.toolbar-cta`
- Global CSS defined in styles.css
</context>

<tasks>

<task type="auto" tdd="false">
  <name>Task 1: Standardize allergen page toolbar (+ Reset Filters)</name>
  <files>
    src/app/features/dashboard/allergen/allergen-page.component.html
    src/app/features/dashboard/allergen/allergen-page.component.ts
  </files>
  <action>
    1. Replace `class="header-actions"` with `class="toolbar-row"` on gridSearch div
    2. Change search field class from `class="search-field"` to `class="toolbar-field search-field"`
    3. Remove inline `style="margin-left: 10px"` from the CTA button
    4. Add `class="toolbar-cta"` to the CTA button
    5. Add Reset Filters button between search field and spacer
    6. Add `<div class="spacer"></div>` before the CTA button
    7. Add `clearFilters()` method to component TS
    8. Remove inline `styles` from component decorator
  </action>
  <verify>
    grep -c 'toolbar-row' allergen-page.component.html
    grep -c 'toolbar-cta' allergen-page.component.html
    grep -c 'clearFilters' allergen-page.component.ts
    grep -c 'mat-stroked-button' allergen-page.component.html
  </verify>
  <done>
    - toolbar uses global toolbar-row/spacer/toolbar-cta classes
    - Reset Filters button present with *ngIf="searchTerm"
    - clearFilters() resets search and triggers reload
    - No inline styles in the toolbar section
  </done>
</task>

<task type="auto" tdd="false">
  <name>Task 2: Fix allergen dialog layout and code standard</name>
  <files>
    src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.html
    src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.ts
  </files>
  <action>
    1. Add `class="full-width"` to all three mat-form-fields in the dialog
    2. Convert constructor injection to `inject()` for all dependencies
  </action>
  <verify>
    grep -c 'full-width' allergen-dialog.component.html
    grep -c 'inject' allergen-dialog.component.ts
  </verify>
  <done>
    - Dialog form fields use full-width class
    - All dependencies use inject() pattern
    - Dialog actions already have align="end"
    - Save button already uses mat-raised-button color="primary"
  </done>
</task>

</tasks>

<success_criteria>
- Allergen page toolbar matches ingredient/category pattern
- Reset Filters button visible only when search has value
- Dialog uses modern inject() pattern
- No custom/inline styles remain in toolbar
</success_criteria>
