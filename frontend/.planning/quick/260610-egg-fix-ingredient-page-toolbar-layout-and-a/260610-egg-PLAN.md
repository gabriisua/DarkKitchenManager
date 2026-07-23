---
id: 260610-egg
type: quick
kind: fix
phase: quick
plan: 01
wave: 1
depends_on: []
files_modified:
  - src/app/features/dashboard/ingredient/ingredient-page.component.ts
  - src/app/features/dashboard/ingredient/ingredient-page.component.html
  - src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts
autonomous: true
must_haves:
  truths:
    - "User can reset all ingredient filters with one click"
    - "Reset button is visibly disabled when no filters are active"
    - "Ingredient dialog follows consistent Angular Material import patterns"
  artifacts:
    - path: "src/app/features/dashboard/ingredient/ingredient-page.component.ts"
      provides: "resetFilters() method"
      contains: "resetFilters"
    - path: "src/app/features/dashboard/ingredient/ingredient-page.component.html"
      provides: "Reset button in toolbar"
      contains: "resetFilters()"
    - path: "src/app/features/dashboard/ingredient/ingredient-dialog.component.ts"
      provides: "MatDividerModule import from canonical path"
      contains: "@angular/material/divider"
  key_links:
    - from: "ingredient-page.component.html"
      to: "ingredient-page.component.ts"
      via: "resetFilters() click handler"
      pattern: "click.*resetFilters"
    - from: "ingredient-page.component.html"
      to: "filters model"
      via: "[disabled] condition checking all filter fields are null/empty"
      pattern: "minEnergyKcal.*null"
---

<objective>
Add Reset Filters button to Ingredient page toolbar and fix MatDivider import in ingredient dialog.

Purpose: Consistency with Customer and Plate pages that already have Reset buttons. Fix incorrect MatDivider import path (`@angular/material/list` is legacy — `@angular/material/divider` is canonical in Angular Material 21).

Output: Updated Ingredient page with Reset Filters button matching established toolbar pattern; ingredient dialog with canonical MatDividerModule import.
</objective>

<execution_context>
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/workflows/execute-plan.md
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
## Established Reset Filters Pattern (Customer & Plate pages)

Both customer-page and plate-page follow this pattern:

```html
<button
  mat-stroked-button
  (click)="resetFilters()"
  matTooltip="Resetta tutti i filtri"
  [disabled]="!searchTerm && ... all filter fields === null ...">
  <mat-icon>clear_all</mat-icon>
  Reset
</button>
```

```typescript
resetFilters(): void {
  this.searchTerm = '';
  // reset all filter fields to null
  this.querySubject.next({
    page: 1,
    pageSize: this.querySubject.value.pageSize,
    search: '',
    // all filter fields as undefined
    sortColumn: 'name',    // or 'businessName'
    sortDirection: 'asc'
  });
}
```

## Global Styles (styles.css)
`.toolbar-row` uses `display: flex; align-items: center; gap: 16px; flex-wrap: wrap;`
`.spacer` uses `flex: 1 1 auto;`
`.toolbar-field` uses `margin-bottom: -1.25em;` (for standalone mat-form-fields in toolbar)

## Current Ingredient Page Details
- `filters` model has: `minEnergyKcal`, `maxEnergyKcal`, `minCost`, `maxCost` (all `number | null`)
- `searchTerm` is a plain string
- All needed Material modules already imported: `MatButtonModule`, `MatIconModule`, `MatTooltipModule`, `MatFormFieldModule`, `MatInputModule`
- Reset button sits between the cost `range-pair` and the `.spacer` div in toolbar order
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add Reset Filters button and method to Ingredient page</name>
  <files>
    src/app/features/dashboard/ingredient/ingredient-page.component.ts
    src/app/features/dashboard/ingredient/ingredient-page.component.html
  </files>
  <action>
    **A) Add `resetFilters()` method to the component class** (after `onFilterChange()`, before `refreshTable()`).

    ```typescript
    resetFilters(): void {
      this.searchTerm = '';
      this.filters.minEnergyKcal = null;
      this.filters.maxEnergyKcal = null;
      this.filters.minCost = null;
      this.filters.maxCost = null;
      this.querySubject.next({
        page: 1,
        pageSize: this.querySubject.value.pageSize,
        search: '',
        sortColumn: this.querySubject.value.sortColumn,
        sortDirection: this.querySubject.value.sortDirection,
        minEnergyKcal: undefined,
        maxEnergyKcal: undefined,
        minCost: undefined,
        maxCost: undefined,
      });
    }
    ```

    **Behavior:** Clears all filter fields, resets search term, emits a new query at page 1. Preserves current sort column/direction (consistent with plate-page approach).

    **B) Add Reset button to the toolbar HTML** between the cost `range-pair` and the `.spacer` div.

    Insert after the cost range-pair closing `</div>` (line ~48) and before the `.spacer` (line ~50). Do NOT use a wrapping container — the button sits directly in the flex `toolbar-row`.

    ```
      </div>

      <button
        mat-stroked-button
        (click)="resetFilters()"
        matTooltip="Resetta tutti i filtri"
        [disabled]="!searchTerm && filters.minEnergyKcal === null && filters.maxEnergyKcal === null && filters.minCost === null && filters.maxCost === null">
        <mat-icon>clear_all</mat-icon>
        Reset
      </button>

      <div class="spacer"></div>
    ```

    **Disabled condition logic:** Button is disabled when all 5 filter sources are empty/null — `!searchTerm` (falsy: empty string) AND all four `filters.*` values are `null`.

    **NO changes needed to imports** — `MatButtonModule`, `MatIconModule`, and `MatTooltipModule` are already imported.
  </action>
  <verify>
    <automated>
      grep -n 'resetFilters' src/app/features/dashboard/ingredient/ingredient-page.component.ts | head -5 && grep -n 'resetFilters' src/app/features/dashboard/ingredient/ingredient-page.component.html | head -3
    </automated>
  </verify>
  <done>
    - `resetFilters()` method exists in ingredient-page.component.ts following customer/plate patterns
    - Reset button renders in toolbar between cost range-pair and spacer
    - Button is disabled when all filters are empty, enabled when any filter has a value
    - Button uses `mat-stroked-button`, `mat-icon="clear_all"`, `matTooltip="Resetta tutti i filtri"`
  </done>
</task>

<task type="auto">
  <name>Task 2: Fix MatDivider import in ingredient dialog to canonical path</name>
  <files>
    src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts
  </files>
  <action>
    **Fix the MatDivider import** — currently imports from legacy re-export location (`@angular/material/list`). Change to canonical `@angular/material/divider` with module import, matching the pattern used in plate-detail, plate-form, order-page, and order-details components.

    Change line 13:
    ```typescript
    import {MatDivider} from '@angular/material/list';
    ```
    → 
    ```typescript
    import { MatDividerModule } from '@angular/material/divider';
    ```

    Change line 27 in the `imports` array:
    ```typescript
    MatDivider
    ```
    → 
    ```typescript
    MatDividerModule,
    ```

    **Review findings (no further changes needed):**
    1. **Angular Material dialog structure** ✅ — Uses `<mat-dialog-title>`, `<mat-dialog-content>`, `<mat-dialog-actions align="end">` per Material spec.
    2. **`inject()` pattern** ✅ — All dependencies (`FormBuilder`, `MatDialogRef`, `MAT_DIALOG_DATA`, `AllergenService`) use `inject()`.
    3. **Button alignment** ✅ — `<mat-dialog-actions align="end">` correctly right-aligns buttons.
    4. **No `MatListModule` needed** — The `MatDivider` was the only import from `@angular/material/list`; after this fix, the import can be fully removed.
  </action>
  <verify>
    <automated>
      grep -n '@angular/material/divider' src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts && ! grep -q '@angular/material/list' src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts
    </automated>
  </verify>
  <done>
    - `MatDividerModule` imported from `@angular/material/divider` (canonical)
    - `@angular/material/list` import removed
    - `MatDividerModule` in the component `imports` array
    - All three review criteria documented as passing (structure, inject(), alignment)
  </done>
</task>

</tasks>

<verification>
- [ ] `grep -n 'resetFilters' src/app/features/dashboard/ingredient/ingredient-page.component.ts` shows the method
- [ ] `grep -n 'resetFilters' src/app/features/dashboard/ingredient/ingredient-page.component.html` shows the click handler
- [ ] `grep -n 'MatDividerModule' src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts` shows import from `@angular/material/divider`
- [ ] `grep -c '@angular/material/list' src/app/features/dashboard/ingredient/ingredient-dialog/ingredient-dialog.component.ts` == 0
</verification>

<success_criteria>
- Ingredient page toolbar has a Reset Filters button matching customer/plate pattern
- Reset button is disabled when no filters active, enabled when any filter set
- `resetFilters()` clears all filter values and resets to page 1
- Ingredient dialog uses canonical `MatDividerModule` from `@angular/material/divider`
- `@angular/material/list` import fully removed from ingredient dialog
</success_criteria>

<output>
After completion, create `.planning/quick/260610-egg-fix-ingredient-page-toolbar-layout-and-a/260610-egg-SUMMARY.md`
</output>
