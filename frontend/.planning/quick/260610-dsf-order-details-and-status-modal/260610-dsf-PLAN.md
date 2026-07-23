---
quick_id: 260610-dsf
slug: order-details-and-status-modal
date: 2026-06-10
must_haves:
  truths:
    - OrderPageComponent uses signal(), inject(), takeUntilDestroyed() pattern
    - OrderCreateComponent uses CSS Grid layout with .page-container, .form-grid, .header-section
    - MatDialog pattern uses MatDialogRef<Component>, MAT_DIALOG_DATA, standalone imports
    - OrderService.getById(id) returns Observable<OrderResponseDto>
    - OrderService.updateStatus(id, status) returns Observable<boolean>
    - UiService.showToast/message, showLoader/hideLoader, showAlert available
    - App routing uses loadComponent with standalone lazy imports
    - No AGENTS.md exists
    - OrderStatus is numeric enum (1-6)
  artifacts:
    - OrderDetailsComponent (standalone)
    - OrderStatusDialogComponent (standalone, MatDialog)
  key_links:
    - src/app/features/dashboard/order/order-page.component.ts
    - src/app/features/dashboard/order/order-page.component.html
    - src/app/features/dashboard/order/order-create.component/order-create.component.ts
    - src/app/core/services/order.service.ts
    - src/app/core/services/ui.service.ts
    - src/app/shared/models/api.models.ts
    - src/app/app.routes.ts
    - src/app/features/dashboard/customer/hub-dialog/hub-dialog.component.ts (dialog pattern)
---

# Quick Task 260610-dsf: Order Details & Status Modal

## Task 1: Create OrderDetailsComponent

**files:**
  - `src/app/features/dashboard/order/order-details/order-details.component.ts`
  - `src/app/features/dashboard/order/order-details/order-details.component.html`
  - `src/app/features/dashboard/order/order-details/order-details.component.css`

**action:**
  1. Create standalone component with `inject()`, `signal()`, `takeUntilDestroyed()`
  2. On init, get order ID from ActivatedRoute params
  3. Call `orderService.getById(id)` to fetch order data
  4. Display metadata: Customer, Delivery Hub, Dates (orderDate, requestedDeliveryDate, calculatedDeliveryDate), Totals (netAmountCents, vatAmountCents, totalGrossCents, shippingCostCents formatted as €), deliveryNotes
  5. Display items grid (table with plateNameSnapshot, quantity, unitPriceNetCents formatted as €)
  6. Add "Scarica Bolla" button that calls `uiService.showToast('Download in partenza...')`
  7. Use CSS Grid layout matching OrderCreateComponent styling (.page-container, .form-grid pattern)

**verify:**
  - Component compiles without errors
  - Route resolves and data loads
  - Metadata and items display correctly
  - Button shows toast

**done:** Component working at /orders/:id

## Task 2: Create OrderStatusDialogComponent

**files:**
  - `src/app/features/dashboard/order/order-status-dialog/order-status-dialog.component.ts`
  - `src/app/features/dashboard/order/order-status-dialog/order-status-dialog.component.html`
  - `src/app/features/dashboard/order/order-status-dialog/order-status-dialog.component.css`

**action:**
  1. Create standalone component following HubDialogComponent pattern
  2. Use `inject()` for `MatDialogRef<OrderStatusDialogComponent>` and `MAT_DIALOG_DATA`
  3. Data interface: `{ orderId: string, currentStatus: OrderStatus }`
  4. Use `signal<OrderStatus>(data.currentStatus)` for selected status
  5. Display current status label and a mat-select with all OrderStatus options
  6. Cancel button closes dialog without result
  7. Save button closes dialog returning `{ orderId: string, newStatus: OrderStatus }`

**verify:**
  - Dialog opens correctly
  - Status options are selectable
  - Save returns correct data
  - Cancel returns nothing

**done:** Dialog component created and testable

## Task 3: Integrate OrderDetails route and OrderStatusDialog in OrderPageComponent

**files:**
  - `src/app/app.routes.ts`
  - `src/app/features/dashboard/order/order-page.component.ts`
  - `src/app/features/dashboard/order/order-page.component.html`

**action:**
  1. Add route `/orders/:id` pointing to OrderDetailsComponent (before `/orders/:create` to prevent conflict)
  2. Update `viewDetails(row)` to navigate to `['/orders', row.id]`
  3. Update `changeStatus(row)` to open MatDialog with OrderStatusDialogComponent
  4. Subscribe to dialog result, if status returned call `orderService.updateStatus()`
  5. Handle loading state (showLoader/hideLoader), success/error toasts
  6. On success, call `refreshTable()`
  7. Remove mat-menu for status change from HTML template
  8. Add `MatDialogModule` to OrderPageComponent imports

**verify:**
  - Route /orders/:id works
  - viewDetails navigates correctly
  - changeStatus opens dialog
  - Status update works end-to-end
  - Toast shown on success/error

**done:** Full integration working
