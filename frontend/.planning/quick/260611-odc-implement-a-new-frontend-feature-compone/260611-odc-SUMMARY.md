---
phase: quick
plan: 260611-odc
subsystem: invoice
tags:
  - invoice
  - bulk-invoice
  - pending-summary
  - feature
dependency-graph:
  requires: []
  provides:
    - route:/invoices
    - InvoicePageComponent
    - InvoiceService
  affects:
    - src/app/app.routes.ts
    - src/app/layout/sidebar/sidebar.component.html
    - src/app/shared/models/api.models.ts
tech-stack:
  added:
    - InvoiceService (HTTP client for `/api/invoice/*` endpoints)
  patterns:
    - Standalone component with inject() DI pattern
    - BehaviorSubject + switchMap + signal data flow
    - Custom MatTable with selection and expandable detail rows
    - forkJoin for batch-loading pending orders on bulk invoice
key-files:
  created:
    - src/app/core/services/invoice.service.ts
    - src/app/features/dashboard/invoice/invoice-page.component.ts
    - src/app/features/dashboard/invoice/invoice-page.component.html
    - src/app/features/dashboard/invoice/invoice-page.component.css
  modified:
    - src/app/shared/models/api.models.ts
    - src/app/app.routes.ts
    - src/app/layout/sidebar/sidebar.component.html
decisions:
  - InvoicePendingSummaryRequest is self-contained (does NOT extend PagedRequest because the endpoint does not use SortColumn/SortDirection)
  - Bulk invoice uses forkJoin to batch-load pending orders for all selected customers, then POSTs combined orderIds
  - sendToSdiImmediately defaults to false in bulkInvoice call
metrics:
  duration: ~8 min
  completed_date: "2026-06-11"
---

# Phase quick Plan 260611-odc: Invoice Feature — Models, Service, Page Component

**One-liner:** Created the Invoices feature page — paginated grid of pending customer invoice summaries with multi-select checkboxes, expandable order detail drawer, and bulk invoice CTA connected to the backend API.

## Task Results

### Task 1: Create Invoice models and API service ✅

**Commit:** `192f562`

**Files:**
- `src/app/shared/models/api.models.ts` — Added `PendingInvoiceSummary`, `PendingOrderItem`, `BulkInvoiceRequest`, `InvoicePendingSummaryRequest` interfaces
- `src/app/core/services/invoice.service.ts` — Created `InvoiceService` with three methods:
  - `getPendingSummary()` — GET `/api/invoice/pending-summary` with PascalCase query params
  - `getPendingOrders()` — GET `/api/invoice/pending-summary/{customerId}/orders`
  - `bulkInvoice()` — POST `/api/invoice/bulk-invoice` (returns 202 Accepted)

### Task 2: Create Invoice page component, route, and sidebar link ✅

**Commit:** `a29e21c`

**Files:**
- `src/app/features/dashboard/invoice/invoice-page.component.ts` — Standalone smart component with:
  - Signals for `data`, `totalItems`, `loading`, `error`, `selectedCustomerIds`, `expandedCustomerId`, `detailOrders`, `detailLoading`
  - `BehaviorSubject` + `switchMap` data flow (mirrors staff-page pattern)
  - Multi-select via `toggleAllRows()` and `toggleRow()`
  - Expandable detail rows via `toggleDetailRow()` with `getPendingOrders()` call
  - Bulk invoice CTA using `forkJoin` to batch-load orders, then POST combined `orderIds`
  - Search with debounce, pagination, error state, empty state
  - CurrencyPipe formatting (EUR) for all amount columns
- `src/app/features/dashboard/invoice/invoice-page.component.html` — Template with toolbar, MatTable, expandable detail, paginator
- `src/app/features/dashboard/invoice/invoice-page.component.css` — Component styles
- `src/app/app.routes.ts` — Added lazy-loaded route for `/invoices`
- `src/app/layout/sidebar/sidebar.component.html` — Added "Fatturazione" link with `receipt_long` icon

## Verification

- [x] TypeScript compilation: **PASS** (`npx tsc --noEmit --pretty` — no errors)
- [x] Invoice models added to api.models.ts without breaking existing imports
- [x] InvoiceService has all 3 methods with correct endpoint paths
- [x] InvoicePageComponent registers as a lazy-loaded route under auth guard
- [x] Sidebar link renders with correct routerLink="/invoices"
- [x] No existing feature or route modified — only additions to routes and sidebar

## Deviations from Plan

None — plan executed exactly as written.

## Threat Flags

None — all endpoints are behind the existing auth guard and JWT interceptor (mitigated per threat model).

## Known Stubs

None.

## Self-Check: PASSED

- `src/app/core/services/invoice.service.ts` — ✅ exists
- `src/app/features/dashboard/invoice/invoice-page.component.ts` — ✅ exists
- `src/app/features/dashboard/invoice/invoice-page.component.html` — ✅ exists
- `src/app/features/dashboard/invoice/invoice-page.component.css` — ✅ exists
- `192f562` — ✅ commit exists
- `a29e21c` — ✅ commit exists
- TypeScript — ✅ compiles with zero errors
