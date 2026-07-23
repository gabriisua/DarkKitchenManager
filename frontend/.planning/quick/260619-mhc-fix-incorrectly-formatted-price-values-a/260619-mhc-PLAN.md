---
id: 260619-mhc
phase: quick
type: execute
wave: 1
depends_on: []
files_modified:
  - src/app/shared/data-grid/data-grid.models.ts
  - src/app/shared/data-grid/data-grid.component.html
  - src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts
autonomous: true
requirements: []
user_setup: []

must_haves:
  truths:
    - "Invoice history grid displays totalGrossCents as proper EUR amounts (e.g., 1500 cents → €15.00)"
    - "Future currency columns on cents-denominated fields can safely use a divisor property"
    - "Existing currency columns (plate-page basePrice, ingredient costPer1000g) remain unaffected"
  artifacts:
    - path: "src/app/shared/data-grid/data-grid.models.ts"
      provides: "ColumnDef<T> interface with optional divisor property"
      contains: "divisor"
    - path: "src/app/shared/data-grid/data-grid.component.html"
      provides: "Currency cell rendering that applies division when divisor > 1"
      pattern: "\\/ col\\.divisor"
    - path: "src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts"
      provides: "totalGrossCents column with divisor: 100"
      pattern: "divisor: 100"
  key_links:
    - from: "ColumnDef.divisor"
      to: "data-grid.component.html currency case"
      via: "template expression dividing row[col.field] by col.divisor"
      pattern: "divisor"
    - from: "invoice-history.component.ts columns definition"
      to: "ColumnDef interface"
      via: "totalGrossCents column sets divisor: 100"
      pattern: "divisor: 100"
---

<objective>
Fix incorrectly formatted price values in the invoice-history data grid by adding a `divisor` property to the shared `ColumnDef<T>` interface and applying division in the data-grid's currency cell renderer. Set `divisor: 100` on the `totalGrossCents` column to correctly convert cents to euros.

**Purpose:** The invoice-history grid displays `totalGrossCents` values (e.g., 1500 cents = €15.00) as €1,500.00 because the shared data-grid's `cellType: 'currency'` handler applies `| currency:'EUR'` without dividing by 100. Fixing the shared component prevents this bug from recurring on any future `cellType: 'currency'` column with cents-denominated values.
**Output:** Modified `ColumnDef` interface, updated data-grid template, corrected invoice-history column definition.
</objective>

<execution_context>
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/workflows/execute-plan.md
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/templates/summary.md
</execution_context>

<context>

## Current State

The shared `ColumnDef<T>` interface (data-grid.models.ts) has no `divisor` property. The data-grid template's `@case ('currency')` block applies `| currency:'EUR':'symbol':'1.2-2'` directly to `row[col.field]` without dividing:

```html
@case ('currency') {
  {{ row[col.field] | currency:'EUR':'symbol':'1.2-2' }}
}
```

The invoice-history component defines `totalGrossCents` with `cellType: 'currency'` but the value comes from the API in cents (see `InvoiceHistoryItem.totalGrossCents: number` in api.models.ts line 444).

**Other components handle cents correctly by pre-dividing:**
- `plate-page.component.ts` — divides `basePrice / 100` in data mapper before grid receives it
- `sale.component.html` — uses `(row.overridePrice / 100) | currency:'EUR'` inline
- `order-details.component.html` — uses `formatCents()` TS helper
- `plate-detail.component.html` — uses TS helpers that divide by 100

**Ingredient `costPer1000g`** is in euros (not cents), so it does NOT need a divisor.

## Pattern Being Established

Adding `divisor` to `ColumnDef<T>` is the correct architectural fix because it:
1. Fixes the invoice-history display immediately
2. Makes the currency cellType work correctly for ALL cents-valued columns going forward
3. Is backward-compatible (default `divisor: 1` = no change)
4. Is the same approach used in other enterprise Angular grid libraries

<interfaces>

From `src/app/shared/data-grid/data-grid.models.ts` (current `ColumnDef<T>`):
```typescript
export interface ColumnDef<T> {
  field: keyof T & string;
  header: string;
  cellType?: CellType;            // Default: 'text'
  sortable?: boolean;             // Default: true
  width?: string;
  sticky?: 'start' | 'end';
  hidden?: boolean;
  customTemplateRef?: string;
  // → ADD: divisor?: number;     // Divide value by this number before display (e.g., 100 for cents → EUR)
}
```

From `src/app/shared/models/api.models.ts`:
```typescript
export interface InvoiceHistoryItem {
  ficDocumentId: number;
  invoiceNumber: string;
  customerName: string;
  ordersCount: number;
  totalGrossCents: number;        // ← Value is in cents
  maxDeliveryDate: string;
}
```
</interfaces>

</context>

<tasks>

<task type="auto">
  <name>Task 1: Add divisor property to ColumnDef and update data-grid currency renderer</name>
  <files>
    src/app/shared/data-grid/data-grid.models.ts
    src/app/shared/data-grid/data-grid.component.html
  </files>
  <action>

  1. **In `src/app/shared/data-grid/data-grid.models.ts`:** Add optional `divisor?: number` property to the `ColumnDef<T>` interface after `customTemplateRef`. Document it with a JSDoc comment: "Divide the cell value by this number before display. Used for cents → currency conversion (e.g., divisor: 100 for price fields stored as cents). Default: 1 (no division)."

  2. **In `src/app/shared/data-grid/data-grid.component.html`:** Update the `@case ('currency')` block (currently line 60-62) to divide `row[col.field]` by `col.divisor ?? 1` before the currency pipe:
  ```html
  @case ('currency') {
    {{ (row[col.field] / (col.divisor ?? 1)) | currency:'EUR':'symbol':'1.2-2' }}
  }
  ```
  This is backward-compatible — columns without a `divisor` will divide by 1 (no change).
  </action>
  <verify>
    <automated>grep -n "divisor" src/app/shared/data-grid/data-grid.models.ts && grep -n "divisor" src/app/shared/data-grid/data-grid.component.html</automated>
  </verify>
  <done>
    - ColumnDef interface has `divisor?: number` property
    - Data-grid template divides by `col.divisor ?? 1` before currency pipe
    - No regressions in existing currency columns (they lack divisor → no-op)
  </done>
</task>

<task type="auto">
  <name>Task 2: Apply divisor: 100 to invoice-history totalGrossCents column</name>
  <files>
    src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts
  </files>
  <action>
  In `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts`, add `divisor: 100` to the `totalGrossCents` column definition (currently lines 81-85). The column should become:

  ```typescript
  {
    field: 'totalGrossCents',
    header: 'Totale Lordo',
    cellType: 'currency',
    divisor: 100
  },
  ```

  Do NOT change any other column definitions. Do NOT change the component logic, template, or service calls. This is a single-property addition.
  </action>
  <verify>
    <automated>grep -c "divisor: 100" src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts</automated>
  </verify>
  <done>
    - `totalGrossCents` column has `divisor: 100`
    - Invoice history grid displays € amounts correctly (e.g., 1500 cents → €15.00 instead of €1,500.00)
  </done>
</task>

</tasks>

<verification>

## Verification Steps

1. **Build check:** `ng build` or `ng serve` compiles without errors
2. **Grep verification:**
   - `ColumnDef` interface has `divisor?: number` — confirmed by `grep -n "divisor" src/app/shared/data-grid/data-grid.models.ts`
   - Template divides by `col.divisor` — confirmed by `grep -n "divisor" src/app/shared/data-grid/data-grid.component.html`
   - Invoice history uses `divisor: 100` — confirmed by `grep -c "divisor: 100" src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts`
3. **No regressions check:** Run `grep "cellType: 'currency'" src/**/*.ts` and verify that:
   - `plate-page.component.ts` `basePrice` column — no divisor needed (pre-divided in data mapper)
   - `ingredient-page.component.ts` `costPer1000g` column — no divisor needed (value is in euros)
   - `invoice-history.component.ts` `totalGrossCents` column — has `divisor: 100`

</verification>

<success_criteria>

- [ ] `ColumnDef<T>` has `divisor?: number` property with JSDoc
- [ ] Data-grid template divides by `col.divisor ?? 1` before currency pipe
- [ ] `totalGrossCents` column in invoice-history has `divisor: 100`
- [ ] `ng build` compiles successfully
- [ ] `cellType: 'currency'` on cents fields is now safe by default for all future columns

</success_criteria>

<output>
After completion, create `.planning/phases/quick/260619-mhc-fix-incorrectly-formatted-price-values-a/260619-mhc-SUMMARY.md`
</output>
