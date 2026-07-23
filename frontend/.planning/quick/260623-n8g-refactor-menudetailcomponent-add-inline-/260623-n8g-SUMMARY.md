---
phase: quick
id: 260623-n8g
status: complete
completed: 2026-06-23
---

# Quick Task 260623-n8g: Refactor MenuDetailComponent for inline selection and ZPL batch printing

## Changes

1. **menu-detail.component.ts** — Added `SelectionModel` for row selection, `FormsModule`/`MatCheckboxModule`/`MatFormFieldModule`/`MatInputModule` imports, `_print` config init on items, `printSelectedBatch()` method, `isAllSelected`/`isPartiallySelected`/`toggleAllSelection` helpers; removed `downloadClassicLabel`, `downloadCustomLabel`, and `openBatchPrintDialog` methods
2. **menu-detail.component.html** — Added `select` (checkbox) column with header select-all, `copies`/`pauseAfter`/`lotNumber` inline inputs (shown when row selected), replaced "Stampa Intero Menu (ZPL)" with "Stampa Selezionati (ZPL)" bound to `printSelectedBatch()` and disabled when selection empty, removed old "Classica"/"Personalizzata" PDF buttons, disabled "Stampa ZPL" when row selected
3. **menu-detail.component.scss** — Updated column widths for new columns, added `mat-mdc-form-field` margin reset for compact inline inputs

## Files Modified
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.html`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss`

## Verification
- `ng build` compiles successfully (only pre-existing budget warning)
