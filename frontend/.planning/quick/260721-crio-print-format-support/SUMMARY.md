---
slug: crio-print-format-support
status: complete
date: 2026-07-21
---

# Summary: Crio Print Format Support

Added full Crio (Cryogenic) label printing support to the Angular frontend by updating existing files.

## Changes

### 1. DTOs (`api.models.ts`)
- Added `customWeight`, `isThawed`, `thawingDate`, `targetLanguage` to both `PrintLabelRequest` and `PrintBatchItem`

### 2. Service (`print.service.ts`)
- Added `printCrioSingleLabel()` → `POST /api/Print/crio/{id}/single`
- Added `printCrioBatchLabels()` → `POST /api/Print/crio/batch`

### 3. Component Logic (`menu-detail.component.ts`)
- Extended `selectedPrintFormat` signal type to include `'crio'`
- Added `isCrioThawed`, `crioThawingDate`, `targetLanguage` class variables
- Crio expiry logic: thawed → +15 days from thawing date; not thawed → 7 months from today
- Crio-specific fields mapped into single and batch payloads only when format is `'crio'`

### 4. Template (`menu-detail.component.html`)
- Added "Crio" toggle button in format selector
- Added conditional Crio settings panel with:
  - "Prodotto Decongelato" checkbox
  - Date picker (when thawed) + info text
  - Info text (when not thawed)

### 5. Styles (`menu-detail.component.scss`)
- Added `.crio-settings-panel` and `.crio-info-text` classes

Build: ✅ successful (budget warnings pre-existing)
