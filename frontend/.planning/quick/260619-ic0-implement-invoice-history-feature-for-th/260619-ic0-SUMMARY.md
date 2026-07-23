---
phase: quick-260619-ic0
plan: 01
subsystem: ui
tags: [angular, invoice, data-grid, pdf-download, datepicker]
requires: []
provides:
  - Invoice history standalone page with paginated, sortable grid
  - Invoice download and delete actions with confirmation
  - Service methods for invoice history API, delete, and PDF URL
affects: invoice, sidebar

tech-stack:
  added: []
  patterns:
    - Standalone smart component using app-data-grid with gridSearch slot and actionsTemplate
    - HTTP Result<T> wrapper extraction via res.data mapping
    - Confirm-before-delete with UiService.askConfirm
    - Date filter toolbar with MatDatepicker

key-files:
  created:
    - src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts
    - src/app/features/dashboard/invoice/invoice-history/invoice-history.component.html
    - src/app/features/dashboard/invoice/invoice-history/invoice-history.component.css
  modified:
    - src/app/shared/models/api.models.ts
    - src/app/core/services/invoice.service.ts
    - src/app/app.routes.ts
    - src/app/features/dashboard/invoice/invoice-page.component.ts
    - src/app/features/dashboard/invoice/invoice-page.component.html
    - src/app/layout/sidebar/sidebar.component.ts

key-decisions:
  - "Used ficDocumentId (number) as delete/PDF identifier per API spec, not string id"
  - "Used corrected API model: customerName, ordersCount, totalGrossCents, maxDeliveryDate fields"
  - "All InvoiceService HTTP calls wrapped with res.data extraction (Result<T> pattern)"
  - "Back arrow button with routerLink=/invoices in history page header"
  - "Date filters in gridSearch slot using MatDatepickerModule"

patterns-established: []
requirements-completed: []

duration: 8min
completed: 2026-06-19
---

# Quick Task 260619-ic0: Implement Invoice History Feature Summary

**Standalone invoice history page with paginated/sortable data grid, PDF download, delete with confirmation, back navigation, and date filters**

## Performance

- **Duration:** 8 min
- **Started:** 2026-06-19T11:17:46Z
- **Completed:** 2026-06-19T11:20:33Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Added `InvoiceHistoryItem` and `InvoiceHistoryRequest` interfaces in `api.models.ts` with correct API fields (`ficDocumentId`, `invoiceNumber`, `customerName`, `ordersCount`, `totalGrossCents`, `maxDeliveryDate`)
- Added `getInvoiceHistory()`, `deleteInvoice()`, `getInvoicePdfUrl()` methods in `InvoiceService` with `res.data` Result<T> wrapper extraction
- Created standalone `InvoiceHistoryComponent` with `app-data-grid` integration showing 5 columns (N. Fattura, Cliente, Ordini Accorpati, Totale Lordo, Data Consegna)
- Registered `/invoices/history` lazy-loaded route in `app.routes.ts` before the catch-all `/invoices` route
- Added back-arrow navigation button (`routerLink="/invoices"`) with "Storico Fatture" title in page header
- Added date filter fields (`MatDatepicker`) for dateFrom/dateTo filtering in gridSearch slot
- Delete action with confirmation dialog via `UiService.askConfirm` + loader + error toast
- PDF download via `getInvoicePdfUrl(ficDocumentId)` + `window.open` in new tab
- Added "Storico Fatture" CTA button on InvoicePageComponent toolbar (click → navigateToHistory)
- Updated sidebar `ngOnInit` to detect `invoice-history` path for ORDINI/FATTURE panel auto-expand

## Task Commits

Each task was committed atomically:

1. **Task 1: Add InvoiceHistory models and InvoiceService endpoint methods** - `d8f64b9` (feat)
2. **Task 2: Create InvoiceHistoryComponent, register route, add CTA to InvoicePage** - `52e1175` (feat)

## Files Created/Modified

| File | Action | Description |
|------|--------|-------------|
| `src/app/shared/models/api.models.ts` | Modified | Added `InvoiceHistoryItem` and `InvoiceHistoryRequest` interfaces |
| `src/app/core/services/invoice.service.ts` | Modified | Added `getInvoiceHistory()`, `deleteInvoice()`, `getInvoicePdfUrl()` methods with `buildPagedParams` + `res.data` mapping |
| `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts` | Created | Standalone smart component with data-grid, search, sort, pagination, date filters, delete, PDF download |
| `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.html` | Created | Template with back-button header, app-data-grid, gridSearch slot, actionsTemplate |
| `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.css` | Created | Minimal component styles (inherits global styles) |
| `src/app/app.routes.ts` | Modified | Added `/invoices/history` lazy route before `/invoices` |
| `src/app/features/dashboard/invoice/invoice-page.component.ts` | Modified | Added `Router` injection, `RouterModule` import, `navigateToHistory()` method |
| `src/app/features/dashboard/invoice/invoice-page.component.html` | Modified | Added "Storico Fatture" CTA button in toolbar |
| `src/app/layout/sidebar/sidebar.component.ts` | Modified | Added `invoice-history` to ORDINI/FATTURE panel auto-detect |

## Deviations from Plan

### Corrected Model Fields
- **Found during:** Task 1
- **Issue:** Plan specified incorrect model fields (`id`, `businessName`, `invoiceDate`, `netAmountCents`, `vatAmountCents`, `status`, `pdfDownloadUrl`) that don't match the API specification
- **Fix:** Used API-corrected fields: `ficDocumentId` (number), `invoiceNumber`, `customerName`, `ordersCount`, `totalGrossCents`, `maxDeliveryDate`
- **Files modified:** api.models.ts, invoice-history.component.ts, invoice-history.component.html
- **Verification:** Build succeeds, types match API contract

### Corrected API Endpoints and Parameter Types
- **Found during:** Task 1
- **Issue:** Plan used incorrect API endpoints (e.g., `/pdf-url` returning `{ pdfDownloadUrl: string }`) and string `id` parameter for delete
- **Fix:** Used correct endpoints (`/pdf` returning `Result<string>`, `/` with `ficDocumentId` returning `Result<boolean>`), all wrapped with `res.data` extraction
- **Files modified:** invoice.service.ts, invoice-history.component.ts
- **Verification:** Build succeeds, method signatures match API contracts

### Column Definition Corrections
- **Found during:** Task 2
- **Issue:** Plan specified 7 columns with fields (`invoiceNumber`, `businessName`, `invoiceDate`, `netAmountCents`, `vatAmountCents`, `totalGrossCents`, `status`) that don't match the API model
- **Fix:** Changed to 5 correct columns: `invoiceNumber`, `customerName`, `ordersCount`, `totalGrossCents` (currency), `maxDeliveryDate` (date)
- **Files modified:** invoice-history.component.ts
- **Verification:** Build succeeds, column fields match InvoiceHistoryItem type

## Issues Encountered

- Import paths in `invoice-history.component.ts` needed one additional `../` level (file is nested under `invoice/invoice-history/`)
- TypeScript strict mode required explicit `err: any` type annotations in error callbacks
- `DatePipe` was initially imported but unused (date formatting is handled by data-grid's `cellType: 'date'`); removed the import

## Known Stubs

None - all features fully implemented and data-wired.

## Threat Flags

None - all security-relevant surface (delete, PDF URL) covered by existing JWT auth interceptor per threat model T-ic0-01 through T-ic0-04.

---

*Phase: quick-260619-ic0*
*Completed: 2026-06-19*
