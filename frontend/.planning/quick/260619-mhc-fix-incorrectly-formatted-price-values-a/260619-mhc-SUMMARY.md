---
id: 260619-mhc
phase: quick
type: execute
subsystem: ui
tags: [data-grid, currency, cents, angular, template]
requires: []
provides:
  - ColumnDef.divisor property for cents-to-currency conversion
  - Backward-compatible currency cell renderer with divisor support
  - Corrected invoice-history totalGrossCents display
affects: []
tech-stack:
  added: []
  patterns:
    - "Cents-valued currency columns use divisor: 100 on ColumnDef"
    - "Data-grid currency renderer divides by col.divisor ?? 1 before formatting"
key-files:
  created: []
  modified:
    - src/app/shared/data-grid/data-grid.models.ts
    - src/app/shared/data-grid/data-grid.component.html
    - src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts
key-decisions:
  - "Added divisor property to shared ColumnDef interface rather than pre-dividing per-component"
  - "No JSDoc comment on divisor per no-comments constraint"
patterns-established:
  - "Cents-valued columns set divisor: 100; euro-valued columns omit divisor (defaults to 1)"
requirements-completed: []
duration: 5min
completed: 2026-06-19
---

# Quick Task 260619-mhc: Fix incorrectly formatted price values in invoice history

**Added `divisor` property to shared `ColumnDef` interface and applied `divisor: 100` to `totalGrossCents` column so cents values render as proper EUR amounts**

## Performance

- **Duration:** 5 min
- **Started:** 2026-06-19T14:14:00Z
- **Completed:** 2026-06-19T14:19:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- Added `divisor?: number` to `ColumnDef<T>` interface in data-grid.models.ts for cents-to-currency conversion
- Updated data-grid template's `@case ('currency')` to divide `row[col.field]` by `col.divisor ?? 1` before the currency pipe
- Set `divisor: 100` on the `totalGrossCents` column in invoice-history.component.ts so 1500 cents renders as €15.00 instead of €1,500.00
- Backward-compatible: existing currency columns (`basePrice`, `costPer1000g`) have no `divisor` → divide by 1 → no change

## Task Commits

Each task was committed atomically:

1. **Task 1: Add divisor property to ColumnDef and update data-grid currency renderer** - `2ba82f2` (feat)
2. **Task 2: Apply divisor: 100 to invoice-history totalGrossCents column** - `24287fe` (fix)

## Files Modified

- `src/app/shared/data-grid/data-grid.models.ts` - Added `divisor?: number` to `ColumnDef<T>` interface
- `src/app/shared/data-grid/data-grid.component.html` - Updated currency case to divide by `col.divisor ?? 1`
- `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts` - Added `divisor: 100` to `totalGrossCents` column

## Decisions Made

- **Architecture:** Adding `divisor` to the shared `ColumnDef` is the correct fix — it prevents the cents-vs-euros bug from recurring on any future `cellType: 'currency'` column, unlike per-component pre-division in data mappers
- **No JSDoc comment:** Per constraint, the `divisor` property was added without a JSDoc comment despite the plan's original specification

## Deviations from Plan

None — plan executed exactly as written.

## Verification

- `ng build` compiles successfully (only pre-existing budget warning)
- All three `cellType: 'currency'` columns verified:
  - `ingredient-page`: `costPer1000g` — no divisor (euros, correct)
  - `plate-page`: `basePrice` — no divisor (pre-divided in mapper, correct)
  - `invoice-history`: `totalGrossCents` — `divisor: 100` (cents, corrected)

---

*Task: 260619-mhc*
*Completed: 2026-06-19*
