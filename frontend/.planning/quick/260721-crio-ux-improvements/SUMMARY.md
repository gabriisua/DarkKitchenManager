---
slug: crio-ux-improvements
status: complete
date: 2026-07-21
---

# Summary: Crio UX Improvements

Improved the Crio print format UX with dedicated controls layout, dynamic expiry preview, and table badges.

## Changes

### 1. TypeScript (`menu-detail.component.ts`)
- Added `predictedCrioExpiryDate` getter: computes Crio expiry in real-time (thawed → +15d, frozen → +7mo)

### 2. Template (`menu-detail.component.html`)
- Moved Crio controls (checkbox + datepicker) from inside the format selector bar to a dedicated sub-row below it
- Replaced static helper text with dynamic preview: "Scadenza prevista sull'etichetta: **dd/MM/yyyy**"
- Updated table Scadenza column: Crio mode shows colored badges instead of date input
  - Blue badge: "dd/MM/yyyy — Surgelato (+7 mesi)"
  - Orange badge: "dd/MM/yyyy — Decongelato"

### 3. Styles (`menu-detail.component.scss`)
- Added `.crio-sub-row` — distinct sub-header with light gray background
- Added `.crio-controls-group` and `.crio-predicted-date` for layout
- Added `.crio-badge`, `.crio-badge-frozen`, `.crio-badge-thawed` for table badges

Build: ✅ successful (budget warnings pre-existing)
