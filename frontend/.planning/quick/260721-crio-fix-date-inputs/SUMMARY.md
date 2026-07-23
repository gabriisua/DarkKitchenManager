---
slug: crio-fix-date-inputs
status: complete
date: 2026-07-21
---

# Summary: Fix Missing Crio Date Inputs

Added missing date picker inputs for Crio mode so users can set production/thawing dates.

## Changes

### TypeScript (`menu-detail.component.ts`)
- Added `crioProductionDate: Date = new Date()` variable
- Updated `predictedCrioExpiryDate` getter to use `crioProductionDate` (+7mo) when not thawed
- Updated `printSingle` and `printBatch` to use `crioProductionDate` for frozen Crio expiry calculation

### Template (`menu-detail.component.html`)
- Replaced Material datepicker with native `<input type="date">` for both states
- When NOT thawed: shows "Data di Produzione" date input bound to `crioProductionDate`
- When thawed: shows "Data di Scongelamento" date input bound to `crioThawingDate`
- Both inputs update the predicted expiry date in real-time

### Styles (`menu-detail.component.scss`)
- Added `.crio-date-input-group`, `.crio-date-label`, `.crio-date-input` for native date input styling

Build: ✅ successful
