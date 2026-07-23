---
phase: quick
id: 260623-me1
status: complete
completed: 2026-06-23
---

# Quick Task 260623-me1: Add ZPL Print Buttons to Menu Detail UI

## Completed Changes

1. **api.models.ts** — Added `PrintLabelRequest` and `PrintBatchItem` interfaces
2. **plate.service.ts** — Added `printSingleLabel(plateId, payload)` and `printBatchLabels(payload)` methods (existing methods preserved)
3. **PrintSettingsDialogComponent** — New standalone dialog with form for copies (default 1, min 1), pauseAfter (default 0, min 0), and lotNumber (optional)
4. **menu-detail.component.ts** — Added `PlateService` and `MatDialog` injections, `openSinglePrintDialog(plateId)` and `openBatchPrintDialog()` handler methods
5. **menu-detail.component.html** — Added "Stampa Intero Menu (ZPL)" button in hero CTA area and "Stampa ZPL" button per plate in actions column
6. **menu-detail.component.scss** — Updated `.hero-cta-container` with `flex-wrap: wrap; gap: 12px` and `.mat-column-actions` width from 40% to 50%

## Files Modified
- `src/app/shared/models/api.models.ts`
- `src/app/core/services/plate.service.ts`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.html`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.scss`

## Files Created
- `src/app/features/dashboard/menu/print-settings-dialog/print-settings-dialog.component.ts`
- `src/app/features/dashboard/menu/print-settings-dialog/print-settings-dialog.component.html`
- `src/app/features/dashboard/menu/print-settings-dialog/print-settings-dialog.component.css`

## Verification
- `ng build` compiles successfully (only pre-existing budget warning)
- All existing functionality preserved (PDF downloads, navigation)
