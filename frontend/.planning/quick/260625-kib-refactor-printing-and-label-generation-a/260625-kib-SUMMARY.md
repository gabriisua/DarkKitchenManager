---
status: complete
---

# Summary: Refactor Printing and Label Generation

**Quick ID:** 260625-kib
**Date:** 2026-06-25

## Changes Made

1. **api.models.ts** — Added `customExpiryDate?: string | null` to `PrintLabelRequest` and `PrintBatchItem` interfaces
2. **menu-detail.component.ts** — Fixed `printSingle` and `printBatch` to pass `customExpiryDate` as raw string instead of ISO conversion, preventing 400 Bad Request on .NET API binding

## Frontend Already Implemented (pre-existing)

- `PrintService` already exists with all 6 methods (standard/cortilia/foorban × single/batch)
- `PlateService` already has no print-related methods
- `menu-detail.component.ts/html` already has `PrintService` injection, `customExpiryDate` in groupedMenuItems, foorban handling in switch statements, "Scadenza" column with date input, and foorban menu buttons

## Backend Note

The .NET backend (.cs files) is in a separate repository — the requested backend changes (DTOs, PrintController, PrinterService, FoorbanLabelGenerator) need to be applied there separately.
