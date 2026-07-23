---
phase: quick
plan: 260611-odc
type: execute
wave: 1
depends_on: []
files_modified:
  - src/app/shared/models/api.models.ts
  - src/app/core/services/invoice.service.ts
  - src/app/features/dashboard/invoice/invoice-page.component.ts
  - src/app/features/dashboard/invoice/invoice-page.component.html
  - src/app/features/dashboard/invoice/invoice-page.component.css
  - src/app/app.routes.ts
  - src/app/layout/sidebar/sidebar.component.html
autonomous: true
requirements: []
must_haves:
  truths:
    - "User can navigate to the Invoices page via sidebar link"
    - "Page shows a paginated grid of pending customer invoice summaries"
    - "Each row has a checkbox for multi-selection"
    - "Header checkbox selects/deselects all visible rows"
    - "'Generate Bulk Invoices' button is visible; when clicked with selected rows, it POSTs the relevant orderIds and shows a success toast"
    - "Clicking a customer row opens a collapsible detail section showing their pending orders (OrderNumber, OrderDate, CalculatedDeliveryDate, TotalGrossCents formatted in EUR, CustomerReference)"
    - "All currency amounts are displayed in Euros (cents / 100) formatted via CurrencyPipe"
  artifacts:
    - path: "src/app/shared/models/api.models.ts"
      provides: "Invoice API type definitions"
      contains: "PendingInvoiceSummary, PendingOrderItem, BulkInvoiceRequest"
    - path: "src/app/core/services/invoice.service.ts"
      provides: "Invoice API service"
      exports: ["InvoiceService"]
    - path: "src/app/features/dashboard/invoice/invoice-page.component.ts"
      provides: "Invoices smart page component with grid, selection, and expandable order detail"
      min_lines: 150
    - path: "src/app/features/dashboard/invoice/invoice-page.component.html"
      provides: "Invoices page template with toolbar, custom mat-table, drawer, and CTA"
      min_lines: 80
    - path: "src/app/app.routes.ts"
      provides: "Lazy-loaded route for /invoices"
      contains: "invoice-page"
    - path: "src/app/layout/sidebar/sidebar.component.html"
      provides: "Sidebar navigation link to invoices"
      contains: "invoices"
  key_links:
    - from: "invoice-page.component.ts"
      to: "invoice.service.ts"
      via: "inject(InvoiceService)"
      pattern: "InvoiceService"
    - from: "invoice-page.component.ts"
      to: "api.models.ts"
      via: "import PendingInvoiceSummary"
      pattern: "PendingInvoiceSummary"
    - from: "app.routes.ts"
      to: "invoice-page.component"
      via: "loadComponent for path 'invoices'"
      pattern: "loadComponent.*invoice"
    - from: "sidebar.component.html"
      to: "invoice-page component"
      via: "routerLink='/invoices'"
      pattern: "invoices"
---

<objective>
Create the Invoices feature page in the Angular app — a paginated grid of pending customer invoice summaries with multi-select, a "Generate Bulk Invoices" CTA, and an expandable drawer showing each customer's pending orders.

Purpose: Staff can review which customers have pending orders ready for invoicing, see each customer's pending order details, and trigger bulk invoice generation against the backend.
Output: Invoice service, models, page component, route, and sidebar link.
</objective>

<execution_context>
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/workflows/execute-plan.md
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/STATE.md

<interfaces>
## Existing patterns to mirror (extracted from codebase)

### Shared models location
File: `src/app/shared/models/api.models.ts`
- All DTOs and request/response interfaces live here
- Uses `PagedRequest` from `src/app/shared/data-grid/data-grid.models.ts` as base for paged query interfaces
- Uses `PagedResponse<T> { items: T[], totalCount, page, pageSize, totalPages }` for paginated responses

### Service pattern
File: `src/app/core/services/order.service.ts`
- `@Injectable({ providedIn: 'root' })` with `inject(HttpClient)`
- `private baseUrl = \`\${environment.apiUrl}/order\``
- Methods return `Observable<T>`, pipe through `.pipe(map(res => res.data))` for wrapper unwrapping
- Manual `HttpParams` building with PascalCase param names

### Page smart component pattern
File: `src/app/features/dashboard/staff/staff-page.component.ts`
- Standalone component with `inject()` for DI
- Signals: `data`, `totalItems`, `loading`, `error`
- `BehaviorSubject<RequestType>` for query state
- `switchMap` + `tap` observable chain in `ngOnInit`/constructor
- `updateQuery(partial)` method that resets page to 1 on filter changes
- UiService for toast/loader/confirm

### Grid toolbar pattern (via content projection)
File: `src/app/features/dashboard/order/order-page.component.html`
- `<div gridSearch class="toolbar-row">` for filters/search/CTA
- Buttons with `mat-raised-button color="primary"` with `<mat-icon>` + label

### Sidebar navigation
File: `src/app/layout/sidebar/sidebar.component.html`
- `<a mat-list-item routerLink="/{entity}" routerLinkActive="active">` with `<mat-icon>` + `<span>`

### Currency display
File: `src/app/features/dashboard/order/order-details/order-details.component.ts`
- Manual `formatCents(cents: number): string { return \`€ \${(cents / 100).toFixed(2)}\` }`
- The shared DataGrid also uses `currency` pipe: `{{ row[col.field] | currency:'EUR':'symbol':'1.2-2' }}`

### Invoice endpoints
- GET `/api/invoice/pending-summary` — query: Page, PageSize, Search, DateFrom, DateTo → returns paginated `PendingInvoiceSummary`
- GET `/api/invoice/pending-summary/{customerId}/orders` — query: DateFrom, DateTo → returns `PendingOrderItem[]`
- POST `/api/invoice/bulk-invoice` — payload: `{ orderIds: string[], sendToSdiImmediately: boolean }` → 202 Accepted
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create Invoice models and API service</name>
  <files>src/app/shared/models/api.models.ts, src/app/core/services/invoice.service.ts</files>
  <action>
    **Part A — Add interfaces to `src/app/shared/models/api.models.ts`**:

    Append these interfaces before the last line of the file (the import of PagedRequest is at line 206, add after the OrderResponseDto block ends at ~line 350):

    ```typescript
    // ── Invoice: Pending Summary ──

    export interface PendingInvoiceSummary {
      customerId: string;
      businessName: string;
      vatNumber: string;
      pendingOrdersCount: number;
      totalNetAmountCents: number;
      totalVatAmountCents: number;
      totalGrossAmountCents: number;
    }

    export interface PendingOrderItem {
      orderId: string;
      orderNumber: string;
      orderDate: string;
      calculatedDeliveryDate: string;
      totalGrossCents: number;
      customerReference: string;
    }

    export interface BulkInvoiceRequest {
      orderIds: string[];
      sendToSdiImmediately: boolean;
    }

    export interface InvoicePendingSummaryRequest {
      page: number;
      pageSize: number;
      search?: string;
      dateFrom?: string;
      dateTo?: string;
    }
    ```

    Do NOT import `PagedRequest` here — `InvoicePendingSummaryRequest` is self-contained and does not extend it (the endpoint does not use SortColumn/SortDirection).

    **Part B — Create `src/app/core/services/invoice.service.ts`**:

    Follow exact pattern from `order.service.ts`:
    - `@Injectable({ providedIn: 'root' })`, inject `HttpClient`
    - `private baseUrl = \`${environment.apiUrl}/invoice\``
    - Three methods:

    1. `getPendingSummary(params: InvoicePendingSummaryRequest): Observable<PagedResponse<PendingInvoiceSummary>>`
       - Build `HttpParams` with PascalCase keys: `Page`, `PageSize`, `Search`, `DateFrom`, `DateTo`
       - Call `this.http.get<any>(\`${this.baseUrl}/pending-summary\`, { params })`
       - Pipe through `.pipe(map(res => res.data))` to unwrap wrapper (same as OrderService.getPaged)

    2. `getPendingOrders(customerId: string, dateFrom?: string, dateTo?: string): Observable<PendingOrderItem[]>`
       - If both dateFrom and dateTo provided, set them as `DateFrom`, `DateTo` params
       - Call `this.http.get<any>(\`${this.baseUrl}/pending-summary/${customerId}/orders\`, { params })`
       - Pipe through `.pipe(map(res => res.data || res))` — the endpoint returns a list directly

    3. `bulkInvoice(payload: BulkInvoiceRequest): Observable<void>`
       - Call `this.http.post<void>(\`${this.baseUrl}/bulk-invoice\`, payload)`
       - No pipe needed, 202 Accepted means observable completes successfully

    Imports needed:
    - `Injectable, inject` from `@angular/core`
    - `HttpClient, HttpParams` from `@angular/common/http`
    - `Observable` from `rxjs`
    - `map` from `rxjs/operators`
    - `environment` from `../../../environments/environment`
    - `PagedResponse, PendingInvoiceSummary, PendingOrderItem, BulkInvoiceRequest, InvoicePendingSummaryRequest` from `../../shared/models/api.models`
  </action>
  <verify>
    <automated>cd /Users/gabrielesuardi/Desktop/fe-roscoff && npx tsc --noEmit --pretty 2>&1 | head -30</automated>
  </verify>
  <done>
    - TypeScript compiles without errors
    - Invoice models exist in api.models.ts
    - InvoiceService exists with all 3 methods, compiles cleanly
  </done>
</task>

<task type="auto">
  <name>Task 2: Create Invoice page component, route, and sidebar link</name>
  <files>src/app/features/dashboard/invoice/invoice-page.component.ts, src/app/features/dashboard/invoice/invoice-page.component.html, src/app/features/dashboard/invoice/invoice-page.component.css, src/app/app.routes.ts, src/app/layout/sidebar/sidebar.component.html</files>
  <action>
    **Step 1 — Create `invoice-page.component.ts`**:

    File: `src/app/features/dashboard/invoice/invoice-page.component.ts`

    Pattern: mirror `staff-page.component.ts` with signals, BehaviorSubject, switchMap. However, because this page needs multi-select + expandable rows (not supported by the shared DataGridComponent), build a custom MatTable directly.

    ```typescript
    import { Component, DestroyRef, inject, signal } from '@angular/core';
    import { CommonModule } from '@angular/common';
    import { FormsModule } from '@angular/forms';
    import { BehaviorSubject, switchMap, tap } from 'rxjs';
    import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
    import { InvoiceService } from '../../../core/services/invoice.service';
    import { UiService } from '../../../core/services/ui.service';
    import { PendingInvoiceSummary, PendingOrderItem, InvoicePendingSummaryRequest } from '../../../shared/models/api.models';
    import { MatTableModule } from '@angular/material/table';
    import { MatCheckboxModule } from '@angular/material/checkbox';
    import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
    import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
    import { MatIconModule } from '@angular/material/icon';
    import { MatButtonModule } from '@angular/material/button';
    import { MatFormFieldModule } from '@angular/material/form-field';
    import { MatInputModule } from '@angular/material/input';
    import { MatTooltipModule } from '@angular/material/tooltip';
    import { CurrencyPipe, DatePipe } from '@angular/common';
    ```

    Full component implementation:
    - `@Component({ standalone: true, selector: 'app-invoice-page', ... })`
    - Imports listed above + `CurrencyPipe, DatePipe`
    - Inject `InvoiceService`, `UiService`, `DestroyRef`
    - Signals: `data`, `totalItems`, `loading`, `error`, `selectedCustomerIds` (Set<string>), `expandedCustomerId` (string | null), `detailOrders` (PendingOrderItem[]), `detailLoading`
    - `searchTerm: string = ''`
    - `querySubject = new BehaviorSubject<InvoicePendingSummaryRequest>({ page: 1, pageSize: 10 })`
    - `displayedColumns: string[] = ['select', 'businessName', 'vatNumber', 'pendingOrdersCount', 'totalNetAmountCents', 'totalVatAmountCents', 'totalGrossAmountCents']`
    - In constructor or `ngOnInit`: subscribe to querySubject with switchMap → invoiceService.getPendingSummary(). Map response to data signal.
    - `isAllSelected()` — check if all visible rows are selected
    - `toggleAllRows()` — select/deselect all visible rows
    - `toggleRow(customerId: string)` — add/remove from selectedCustomerIds
    - `toggleDetailRow(row: PendingInvoiceSummary)` — toggle expandedCustomerId, call invoiceService.getPendingOrders() for the customer, populate detailOrders signal
    - `onSearch(value: string)` — updateQuery with search
    - `updateQuery(partial)` — standard pattern (reset page to 1 on filter changes)
    - `onPageChange(event: PageEvent)` — updateQuery with page/pageSize
    - `hasActiveFilters()` — return !!searchTerm
    - `clearFilters()` — reset searchTerm, updateQuery with empty search
    - `onGenerateBulkInvoices()` — collect all orderIds from selected customers (need to track which customers are selected, then gather orderIds). Per requirements: "gather all selected order IDs" — since orderIds come from the detail view, the approach should be:
      - Track selected customers
      - For each selected customer, fetch their pending orders (or we can use the data already loaded in expanded rows)
      - Simpler approach: When user clicks "Generate Bulk Invoices", iterate selectedCustomerIds, for each customer fetch pending orders (if not already loaded), then POST all orderIds.
      - Even simpler and more aligned with the UX description: The bulk invoice button collects ALL order IDs from ALL selected customers. Since orders are loaded per-customer in the detail drawer, we should either:
        a) Load orders for all selected customers on CTA click, then POST
        b) Cache orders from previously expanded rows
      - Best approach for this quick implementation: On CTA click, batch-load orders for all selected customers using forkJoin (if multiple), then POST the combined orderIds. Show loader during the process, show success toast on 202, handle errors.
      - Implementation: `this.uiService.showLoader('Generazione fatture in corso...')`, `forkJoin(selectedCustomerIds.map(id => this.invoiceService.getPendingOrders(id)))`, collect all `orderIds`, call `this.invoiceService.bulkInvoice({ orderIds, sendToSdiImmediately: false })`, on success `this.uiService.hideLoader(); this.uiService.showToast('Fatture generate con successo! Il processo è in esecuzione in background.'); this.selectedCustomerIds.set(new Set()); this.refreshTable();`
    - `refreshTable()` — updateQuery({})
    - Helper `formatCents(cents: number): string` — returns `(cents / 100).toFixed(2)` — actual formatting with CurrencyPipe happens in template

    **Step 2 — Create `invoice-page.component.html`**:

    Template structure following `staff-page.component.html` and `order-page.component.html` patterns:

    ```
    <div class="page-container">
      <div class="page-header">
        <h1 class="mat-headline-4" style="margin: 0;">Fatturazione</h1>
      </div>

      <!-- Toolbar row (mirrors staff/order page pattern) -->
      <div class="toolbar-row">
        <mat-form-field appearance="outline" class="toolbar-field search-field">
          <mat-label>Cerca cliente...</mat-label>
          <input matInput [(ngModel)]="searchTerm" (ngModelChange)="onSearch($event)" placeholder="Ragione sociale o P.IVA">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <button mat-stroked-button (click)="clearFilters()" matTooltip="Resetta i filtri" [disabled]="!hasActiveFilters()">
          <mat-icon>clear_all</mat-icon>
          Reset
        </button>

        <div class="spacer"></div>

        <button mat-raised-button color="primary" class="toolbar-cta" (click)="onGenerateBulkInvoices()" [disabled]="selectedCustomerIds().size === 0" matTooltip="Genera fatture per i clienti selezionati">
          <mat-icon>receipt_long</mat-icon>
          Genera Fatture Bulk
        </button>
      </div>

      <!-- Loading overlay -->
      @if (loading()) {
        <div class="loading-overlay"><mat-spinner diameter="40"></mat-spinner></div>
      }

      <!-- Error state -->
      @if (error(); as errMsg) {
        <div class="error-banner">
          <mat-icon class="error-icon">error_outline</mat-icon>
          <span class="error-text">{{ errMsg }}</span>
          <button mat-stroked-button color="warn" (click)="refreshTable()">
            <mat-icon>refresh</mat-icon> Riprova
          </button>
        </div>
      }

      <!-- Table (custom MatTable with selection + expandable rows) -->
      @if (!loading() && !error()) {
        <div class="table-container">
          <table mat-table [dataSource]="data()" class="full-width" multiTemplateDataRows>

            <!-- Selection column -->
            <ng-container matColumnDef="select">
              <th mat-header-cell *matHeaderCellDef style="width: 48px">
                <mat-checkbox
                  [checked]="isAllSelected()"
                  [indeterminate]="selectedCustomerIds().size > 0 && !isAllSelected()"
                  (change)="toggleAllRows()">
                </mat-checkbox>
              </th>
              <td mat-cell *matCellDef="let row" style="width: 48px">
                <mat-checkbox
                  [checked]="selectedCustomerIds().has(row.customerId)"
                  (change)="toggleRow(row.customerId)">
                </mat-checkbox>
              </td>
            </ng-container>

            <!-- BusinessName column (clickable to expand) -->
            <ng-container matColumnDef="businessName">
              <th mat-header-cell *matHeaderCellDef>Cliente</th>
              <td mat-cell *matCellDef="let row">
                <button mat-button class="expand-toggle" (click)="toggleDetailRow(row)">
                  <mat-icon>{{ expandedCustomerId() === row.customerId ? 'expand_less' : 'expand_more' }}</mat-icon>
                  {{ row.businessName }}
                </button>
              </td>
            </ng-container>

            <ng-container matColumnDef="vatNumber">
              <th mat-header-cell *matHeaderCellDef>P.IVA</th>
              <td mat-cell *matCellDef="let row">{{ row.vatNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="pendingOrdersCount">
              <th mat-header-cell *matHeaderCellDef>Ordini in Sospeso</th>
              <td mat-cell *matCellDef="let row">{{ row.pendingOrdersCount }}</td>
            </ng-container>

            <ng-container matColumnDef="totalNetAmountCents">
              <th mat-header-cell *matHeaderCellDef>Netto</th>
              <td mat-cell *matCellDef="let row">{{ row.totalNetAmountCents / 100 | currency:'EUR':'symbol':'1.2-2' }}</td>
            </ng-container>

            <ng-container matColumnDef="totalVatAmountCents">
              <th mat-header-cell *matHeaderCellDef>IVA</th>
              <td mat-cell *matCellDef="let row">{{ row.totalVatAmountCents / 100 | currency:'EUR':'symbol':'1.2-2' }}</td>
            </ng-container>

            <ng-container matColumnDef="totalGrossAmountCents">
              <th mat-header-cell *matHeaderCellDef>Totale</th>
              <td mat-cell *matCellDef="let row">{{ row.totalGrossAmountCents / 100 | currency:'EUR':'symbol':'1.2-2' }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns; sticky: true"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                class="detail-row"
                [class.expanded]="expandedCustomerId() === row.customerId">
            </tr>

            <!-- Expanded detail row: pending orders for this customer -->
            <tr mat-row *matRowDef="let row; columns: ['expandedDetail']; when: isExpandedRow"
                class="detail-expanded-row">
              <td mat-cell [attr.colspan]="displayedColumns.length">
                <div class="detail-container">
                  @if (detailLoading() && expandedCustomerId() === row.customerId) {
                    <mat-spinner diameter="24"></mat-spinner>
                  } @else {
                    <div class="detail-header">Ordini in sospeso per {{ row.businessName }}</div>
                    @if (detailOrders().length === 0) {
                      <span class="no-orders">Nessun ordine in sospeso.</span>
                    } @else {
                      <table class="detail-orders-table full-width">
                        <thead>
                          <tr>
                            <th>N. Ordine</th>
                            <th>Data Ordine</th>
                            <th>Data Consegna</th>
                            <th>Importo</th>
                            <th>Riferimento Cliente</th>
                          </tr>
                        </thead>
                        <tbody>
                          @for (order of detailOrders(); track order.orderId) {
                            <tr>
                              <td>{{ order.orderNumber }}</td>
                              <td>{{ order.orderDate | date:'shortDate' }}</td>
                              <td>{{ order.calculatedDeliveryDate | date:'shortDate' }}</td>
                              <td>{{ order.totalGrossCents / 100 | currency:'EUR':'symbol':'1.2-2' }}</td>
                              <td>{{ order.customerReference }}</td>
                            </tr>
                          }
                        </tbody>
                      </table>
                    }
                  }
                </div>
              </td>
            </tr>

            <!-- Empty state -->
            @if (data().length === 0) {
              <tr class="mat-row">
                <td class="mat-cell empty-cell" [attr.colspan]="displayedColumns.length">
                  <mat-icon>inbox</mat-icon>
                  <span>Nessun dato trovato</span>
                </td>
              </tr>
            }
          </table>
        </div>

        <!-- Paginator -->
        <mat-paginator
          [length]="totalItems()"
          [pageSize]="querySubject.value.pageSize"
          [pageSizeOptions]="[10, 25, 50, 100]"
          [showFirstLastButtons]="true"
          (page)="onPageChange($event)"
          aria-label="Seleziona pagina">
        </mat-paginator>
      }
    </div>
    ```

    **Step 3 — Create `invoice-page.component.css`**:

    ```css
    /* Reuses global .page-container, .page-header, .toolbar-row, .spacer, .toolbar-cta from styles.css */

    .loading-overlay {
      display: flex;
      justify-content: center;
      padding: 48px;
    }

    .error-banner {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      background: #fff3f3;
      border-radius: 8px;
      margin-bottom: 16px;
    }

    .error-icon {
      color: #d32f2f;
    }

    .error-text {
      flex: 1;
      color: #d32f2f;
    }

    .expand-toggle {
      cursor: pointer;
      text-align: left;
      font-weight: 500;
    }

    .detail-row.expanded {
      background-color: #fafafa;
    }

    .detail-expanded-row {
      background-color: #f5f5f5;
    }

    .detail-expanded-row td {
      padding: 0 !important;
    }

    .detail-container {
      padding: 16px 24px 16px 72px;
      border-bottom: 1px solid #e0e0e0;
    }

    .detail-header {
      font-weight: 600;
      margin-bottom: 12px;
      color: #333;
    }

    .detail-orders-table {
      border-collapse: collapse;
    }

    .detail-orders-table th {
      text-align: left;
      font-size: 13px;
      color: #666;
      padding: 8px 12px;
      border-bottom: 1px solid #e0e0e0;
      font-weight: 600;
    }

    .detail-orders-table td {
      padding: 8px 12px;
      font-size: 14px;
      border-bottom: 1px solid #f0f0f0;
    }

    .no-orders {
      color: #999;
      font-style: italic;
    }

    .empty-cell {
      text-align: center;
      padding: 48px 16px;
      color: #999;
    }

    .empty-cell mat-icon {
      vertical-align: middle;
      margin-right: 8px;
    }
    ```

    **Step 4 — Add route in `src/app/app.routes.ts`**:

    Add a new child route under the `children: [...]` array inside the `path: ''`, `component: LayoutComponent`, `canMatch: [authGuard]` block — after the `orders` routes (around line 123, after the `orders/:id` route closing):

    ```typescript
          {
            path: 'invoices',
            loadComponent: () =>
              import('./features/dashboard/invoice/invoice-page.component')
                .then(m => m.InvoicePageComponent)
          },
    ```

    **Step 5 — Add sidebar link in `src/app/layout/sidebar/sidebar.component.html`**:
    
    After the `Ordini` link (after `</a>` for orders, around line 79), add:

    ```html
      <a
        mat-list-item
        routerLink="/invoices"
        routerLinkActive="active"
      >
        <mat-icon>receipt_long</mat-icon>
        <span>Fatturazione</span>
      </a>
    ```

    **Important:** The `isExpandedRow` method in the template needs to be defined in the component. In the component TypeScript file, add:
    ```typescript
    isExpandedRow = (_index: number, row: PendingInvoiceSummary): boolean => {
      return this.expandedCustomerId() === row.customerId;
    };
    ```
    This is a function reference needed by `*matRowDef`'s `when` clause to conditionally render the expanded detail row.
  </action>
  <verify>
    <automated>cd /Users/gabrielesuardi/Desktop/fe-roscoff && npx tsc --noEmit --pretty 2>&1 | head -30</automated>
  </verify>
  <done>
    - TypeScript compiles without errors
    - `/invoices` route works with lazy-loaded InvoicePageComponent
    - Sidebar shows "Fatturazione" link navigating to `/invoices`
    - Page component has all required UI elements (custom grid, checkboxes, CTA, expandable detail)
    - CurrencyPipe formatting applied to all amount columns
  </done>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| Client→API | The invoice page sends GET/POST requests to `/api/invoice/*` endpoints |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-invoice-01 | S (Spoofing) | POST /api/invoice/bulk-invoice | mitigated | Request sent with JWT via existing AuthInterceptor (not modified by this task) |
| T-invoice-02 | T (Tampering) | Order selection data | mitigated | Selection state is client-side only; the bulk invoice POST payload is validated by the backend |
| T-invoice-03 | I (Information Disclosure) | GET /api/invoice/pending-summary | mitigated | Already behind auth guard; endpoint returns only authorized customer data |
</threat_model>

<verification>
- [ ] All new TypeScript compiles with no errors (run `npx tsc --noEmit`)
- [ ] Models added to `api.models.ts` without breaking existing imports
- [ ] InvoiceService has all 3 methods with correct endpoint paths
- [ ] InvoicePageComponent registers as a route under auth guard
- [ ] Sidebar link renders and navigates correctly
- [ ] No existing feature or route is modified or broken
</verification>

<success_criteria>
- TypeScript compilation passes with zero errors
- All new files exist at expected paths
- The invoice feature can be navigated to via the sidebar
- The page displays a functioning paginated table with checkboxes and expandable order detail rows
</success_criteria>

<output>
After completion, create `.planning/quick/260611-odc-implement-a-new-frontend-feature-compone/260611-odc-SUMMARY.md`
</output>
</plan>
