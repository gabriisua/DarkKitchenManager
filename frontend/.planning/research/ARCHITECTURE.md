# Architecture Research: Reusable Data Grid with Smart/Dumb Components

**Domain:** Angular 21 back-office app — reusable data grid with standardized API layer
**Researched:** 2026-05-29
**Confidence:** HIGH

## Standard Architecture

### System Overview

The refactored architecture separates concerns into three clear layers. Dumb components know nothing about services or API calls. Smart components orchestrate data fetching and pass prepared data down. This eliminates the current pattern where every entity page duplicates the same `<mat-table>` markup, paginator logic, and filter state management.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     SMART COMPONENTS (Container Layer)                    │
│                                                                           │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌──────────────────┐ │
│  │  StaffPageComponent  │  │  CustomerPageComp.   │  │  PlatePageComp.  │ │
│  │  • Injects service   │  │  • Injects service   │  │  • Injects srv   │ │
│  │  • Manages filter    │  │  • Manages filter    │  │  • Manages filt  │ │
│  │  • Builds query      │  │  • Builds query      │  │  • Builds query  │ │
│  │  • Handles CRUD      │  │  • Handles CRUD      │  │  • Handles CRUD  │ │
│  │  dialogs             │  │  dialogs             │  │  dialogs         │ │
│  └──────────┬───────────┘  └──────────┬───────────┘  └────────┬─────────┘ │
│             │                         │                        │          │
│             │ (data, total, loading)  │ (data, total, loading) │          │
│             ▼                         ▼                        ▼          │
├─────────────────────────────────────────────────────────────────────────┤
│                      DUMB COMPONENT (Presentational)                      │
│                                                                           │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  app-data-grid<T>                                                     │ │
│  │  • @Input columns: ColumnDef<T>[]                                     │ │
│  │  • @Input data: T[]                                                   │ │
│  │  • @Input totalItems: number                                          │ │
│  │  • @Input loading: boolean                                            │ │
│  │  • @Output sortChange: SortChangeEvent                                │ │
│  │  • @Output pageChange: PageChangeEvent                                │ │
│  │  • Renders <mat-table> + <mat-paginator> + sort headers               │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      API SERVICE LAYER                                    │
│                                                                           │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌──────────────────┐ │
│  │  StaffService        │  │  CustomerService     │  │  PlateService    │ │
│  │  getPaged(q): Obs    │  │  getPaged(q): Obs    │  │  getPaged(q):Obs │ │
│  │  • buildStaffParams  │  │  • buildCustomerP.   │  │  • buildPlateP.  │ │
│  └──────────┬───────────┘  └──────────┬───────────┘  └────────┬─────────┘ │
│             │                         │                        │          │
│             └──────────┬──────────────┴────────────────────────┘          │
│                        │                                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  HttpClient + HttpParams                                             │ │
│  │  (via interceptors: apiInterceptor → authInterceptor)                │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                     EXTERNAL: REST API                                    │
│  GET /api/{entity}?Page=&PageSize=&SortColumn=&SortDirection=&Search=    │
│  + entity-specific params: &Email=&Role= / &Type=&IsActive= / etc.      │
│  Response: { succeeded: true, data: { items: T[], totalCount: N, ... } } │
└─────────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Inputs/Outputs | Typical Implementation |
|-----------|---------------|----------------|------------------------|
| `DataGridComponent<T>` | Renders paginated, sortable table. No business logic. Zero service injection. | `@Input() columns: ColumnDef<T>[]` `@Input() data: T[]` `@Input() totalItems: number` `@Input() loading: boolean` `@Output() sortChange = output<SortChange>()` `@Output() pageChange = output<PageChange>()` | Standalone, `ChangeDetectionStrategy.OnPush`, wraps `<mat-table>` with `<mat-paginator>` + sort headers. Uses `@for` control flow. |
| `StaffPageComponent` | Smart: manages `StaffQuery` state, calls `staffService.getPaged()`, passes data to grid. Handles CRUD dialogs. | (none — top-level routed component) | Injects StaffService + MatDialog. Has `BehaviorSubject<StaffQuery>` piped through `switchMap`. Owns filter form controls. |
| `CustomerPageComponent` | Smart: manages `CustomerQuery`, calls `customerService.getPaged()`. | (none — top-level routed component) | Same pattern as staff. Filter section with `Type` dropdown + `IsActive` toggle. |
| `PlatePageComponent` | Smart: manages `PlateQuery`, calls `plateService.getPaged()`. | (none — top-level routed component) | Same pattern. Filter with `Name` input + `CategoryId` dropdown + `IsActive` + price range. |
| `IngredientPageComponent` | Smart: manages `IngredientQuery`, calls `ingredientService.getPaged()`. | (none — top-level routed component) | Same pattern. Filter with `Name` + numeric range inputs + `IsActive`. |
| `AllergenPageComponent` | Smart: manages `AllergenQuery`, calls `allergenService.getPaged()`. | (none — top-level routed component) | Filter with `Name` + `Code` inputs. |
| `FilterSectionComponent` (optional) | Dumb: renders entity-specific filter controls. Emits filter values. | `@Input() config: FilterField[]` `@Output() filterChange = output<Record<string, any>>()` | Could be generic with dynamic form fields, or kept as inline HTML in each smart page depending on complexity. |

### Data Flow Direction

**All data flows DOWN** through inputs. **All events flow UP** through outputs.

```
User action (sort click)
    │
    ▼
DataGridComponent ──sortChange()──→ SmartPageComponent
                                        │
                                   Updates querySubject
                                        │
                                   switchMap → service.getPaged(query)
                                        │
                                   HTTP GET /api/{entity}?...
                                        │
                                   Response: { data: { items, totalCount } }
                                        │
                                   Sets dataSignal, totalSignal, loadingSignal
                                        │
    ←── data & total passed as @Input() ──
    │
DataGridComponent re-renders
```

### Component Boundary Rules

1. **Smart page components NEVER render `<mat-table>` directly** — they always compose `<app-data-grid>`
2. **DataGridComponent NEVER injects any service** — not `HttpClient`, not entity services
3. **DataGridComponent NEVER manages filter state** — it only renders what it receives
4. **Smart components manage filter state via signals** — filter form values → query object → service call
5. **Smart components own CRUD operations** — add/edit/delete dialogs are called from the page component, not the grid
6. **Action columns are NOT handled by the grid** — the smart component renders action buttons via its own template or `ng-content` slot

## Type System Design

### Query Parameter Interface Hierarchy

The core insight for standardization: **common params live in a base interface, each entity extends with its own filter fields**.

```typescript
// === Shared: src/app/shared/models/api.models.ts ===

// Generic API response wrapper (already exists partially)
export interface ApiResponse<T> {
  succeeded: boolean;
  data: T;
  message?: string;
}

// Generic paged response (already exists, not used consistently)
export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Base query params — shared by ALL paged GET endpoints
export interface BaseQueryParams {
  page: number;
  pageSize: number;
  search?: string;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc' | '';
  dateFrom?: string;         // ISO date string
  dateTo?: string;           // ISO date string
}

// === Entity-specific query params ===

export interface StaffQueryParams extends BaseQueryParams {
  email?: string;
  role?: string;
}

export interface CustomerQueryParams extends BaseQueryParams {
  type?: string;
  isActive?: boolean | null;
}

export interface PlateQueryParams extends BaseQueryParams {
  categoryId?: number;
  isActive?: boolean | null;
  minPrice?: number;
  maxPrice?: number;
}

export interface IngredientQueryParams extends BaseQueryParams {
  name?: string;
  minEnergyKcal?: number;
  maxEnergyKcal?: number;
  minCost?: number;
  maxCost?: number;
  isActive?: boolean | null;
}

export interface AllergenQueryParams extends BaseQueryParams {
  name?: string;
  code?: string;
}
```

### Column Configuration Interface

```typescript
// === src/app/shared/components/data-grid/data-grid.models.ts ===

export interface ColumnDef<T> {
  /** Property key on the data type (e.g., 'name', 'email') */
  field: keyof T | string;
  /** Header label displayed in the table */
  header: string;
  /** Whether the column is sortable (default: false) */
  sortable?: boolean;
  /** Custom cell template (optional — if not provided, renders field value as string) */
  cell?: (row: T) => string | number | boolean | null | undefined;
  /** CSS width (e.g., '100px', '15%') */
  width?: string;
  /** Whether to stick this column (e.g., actions column on right) */
  sticky?: 'start' | 'end';
  /** Text alignment in cells */
  align?: 'left' | 'center' | 'right';
}
```

### Sort/Page Event Types

```typescript
export interface SortChange {
  column: string;
  direction: 'asc' | 'desc' | '';
}

export interface PageChange {
  page: number;      // 0-indexed (MatPaginator default)
  pageSize: number;
}
```

## Recommended Project Structure

```
src/
├── app/
│   ├── core/
│   │   └── services/
│   │       ├── staff.service.ts           ★ Refactor: add getPaged(StaffQueryParams)
│   │       ├── customer.service.ts        ★ Refactor: add getPaged(CustomerQueryParams)
│   │       ├── plate.service.ts           ★ Refactor: add getPaged(PlateQueryParams)
│   │       ├── ingredient.service.ts      ★ Refactor: add getPaged(IngredientQueryParams)
│   │       ├── allergen.service.ts        ★ Refactor: add getPaged(AllergenQueryParams)
│   │       └── ...
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   └── data-grid/
│   │   │       ├── data-grid.component.ts       ★ NEW: Generic dumb grid
│   │   │       ├── data-grid.component.html     ★ NEW
│   │   │       ├── data-grid.component.scss     ★ NEW
│   │   │       └── data-grid.models.ts          ★ NEW: ColumnDef, SortChange, PageChange
│   │   └── models/
│   │       └── api.models.ts                    ★ ADD: BaseQueryParams, entity-specific QueryParams
│   │
│   └── features/
│       └── dashboard/
│           ├── staff/
│           │   ├── staff-page.component.ts      ★ NEW smart container
│           │   ├── staff-page.component.html    ★ NEW (composes <app-data-grid>)
│           │   └── staff-dialog/ (unchanged)
│           ├── customer/
│           │   ├── customer-page.component.ts   ★ NEW smart container
│           │   └── ...
│           ├── ingredient/
│           │   ├── ingredient-page.component.ts ★ NEW smart container
│           │   └── ...
│           ├── plate/
│           │   ├── plate-page.component.ts      ★ NEW smart container
│           │   └── ...
│           └── allergen/
│               ├── allergen-page.component.ts   ★ NEW smart container
│               └── ...
```

### Structure Rationale

- **`shared/components/data-grid/`**: The grid is framework-level infrastructure, not feature code. It belongs in `shared/` because all features consume it. This matches the existing pattern where `shared/models/` and `shared/ui.overlay/` house cross-cutting utilities.
- **`core/services/` stays unchanged**: Services remain in `core/services/`. Only method signatures change (add `getPaged()` method, keep existing methods for backward compat or consolidate).
- **Page components in `features/`**: The smart/dumb split happens inside the feature directory. The old `staff.component.ts` becomes `staff-page.component.ts`. The old table markup is deleted — replaced by `<app-data-grid>`.

## Standardized Service Pattern

Every entity service needs a consistent `getPaged()` method. The pattern:

```typescript
// Example: StaffService
import { BaseQueryParams, StaffQueryParams, PagedResponse, ApiResponse } from '../../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class StaffService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Staff`;

  /**
   * Standardized paged query method.
   * Every entity service implements this with its own QueryParams type.
   */
  getPaged(query: StaffQueryParams): Observable<ApiResponse<PagedResponse<Staff>>> {
    let params = new HttpParams()
      .set('Page', query.page.toString())
      .set('PageSize', query.pageSize.toString());

    // Common params
    if (query.search) params = params.set('Search', query.search);
    if (query.sortColumn) params = params.set('SortColumn', query.sortColumn);
    if (query.sortDirection) params = params.set('SortDirection', query.sortDirection);

    // Entity-specific params
    if (query.email) params = params.set('Email', query.email);
    if (query.role) params = params.set('Role', query.role);

    return this.http.get<ApiResponse<PagedResponse<Staff>>>(this.apiUrl, { params });
  }
}
```

### HttpParams Builder Helper (Optional but Recommended)

To avoid the repetitive `if`-chain in every service, extract a helper:

```typescript
// src/app/shared/utils/http-params.builder.ts
export function buildHttpParams(query: Record<string, unknown>): HttpParams {
  let params = new HttpParams();
  params = params.set('Page', String(query['page'] ?? 0));
  params = params.set('PageSize', String(query['pageSize'] ?? 10));

  for (const [key, value] of Object.entries(query)) {
    if (key === 'page' || key === 'pageSize') continue; // already set
    if (value === undefined || value === null || value === '') continue;
    params = params.set(
      // Convert camelCase to PascalCase for API
      key.charAt(0).toUpperCase() + key.slice(1),
      String(value)
    );
  }
  return params;
}
```

This eliminates the if-chain in every service:

```typescript
getPaged(query: StaffQueryParams): Observable<ApiResponse<PagedResponse<Staff>>> {
  return this.http.get<ApiResponse<PagedResponse<Staff>>>(
    this.apiUrl,
    { params: buildHttpParams(query as Record<string, unknown>) }
  );
}
```

## Smart Page Component Pattern

```typescript
// src/app/features/dashboard/staff/staff-page.component.ts

@Component({
  selector: 'app-staff-page',
  standalone: true,
  imports: [DataGridComponent, StaffFilterComponent, ...],
  templateUrl: './staff-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StaffPageComponent implements OnInit, OnDestroy {
  private staffService = inject(StaffService);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  // Grid data state
  readonly dataSignal = signal<Staff[]>([]);
  readonly totalSignal = signal(0);
  readonly loadingSignal = signal(false);

  // Column definitions (static, defined once)
  readonly columns: ColumnDef<Staff>[] = [
    { field: 'username', header: 'Username', sortable: true },
    { field: 'email', header: 'Email', sortable: true },
    { field: 'role', header: 'Ruolo', sortable: true },
    { field: 'isActive', header: 'Stato', sortable: true,
      cell: (s) => s.isActive ? 'Attivo' : 'Inattivo' },
    { field: 'lastLogin', header: 'Ultimo Accesso', sortable: true,
      cell: (s) => s.lastLogin ? new Date(s.lastLogin).toLocaleString() : '-' },
  ];

  // Query management
  private readonly querySubject = new BehaviorSubject<StaffQueryParams>({
    page: 0,
    pageSize: 10,
    search: '',
    sortColumn: 'username',
    sortDirection: 'asc',
  });

  ngOnInit() {
    // Wire up the paginated data stream
    this.querySubject.pipe(
      tap(() => this.loadingSignal.set(true)),
      switchMap(query => this.staffService.getPaged(query)),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.dataSignal.set(res.data.items);
          this.totalSignal.set(res.data.totalCount);
        }
        this.loadingSignal.set(false);
      },
      error: () => {
        this.dataSignal.set([]);
        this.totalSignal.set(0);
        this.loadingSignal.set(false);
      },
    });
  }

  // Called by filter section or search input
  updateQuery(partial: Partial<StaffQueryParams>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partial || 'pageSize' in partial;
    this.querySubject.next({
      ...current,
      ...partial,
      page: isNavigation ? (partial.page ?? current.page) : 0,
    });
  }

  // Called by DataGridComponent output
  onSortChange(event: SortChange): void {
    this.updateQuery({ sortColumn: event.column, sortDirection: event.direction });
  }

  onPageChange(event: PageChange): void {
    this.updateQuery({ page: event.page, pageSize: event.pageSize });
  }

  // CRUD handlers (smart component owns these)
  addStaff(): void { /* dialog logic */ }
  editStaff(staff: Staff): void { /* dialog logic */ }
  deleteStaff(staff: Staff): void { /* confirm + delete */ }
}
```

### Template

```html
<!-- staff-page.component.html -->
<div class="page-container">
  <div class="page-header">
    <h1>Staff</h1>
    <button mat-raised-button color="primary" (click)="addStaff()">
      <mat-icon>add</mat-icon> Nuovo Staff
    </button>
  </div>

  <!-- Entity-specific filter section (smart component owns this) -->
  <app-staff-filters (filterChange)="updateQuery($event)" />

  <!-- Dumb grid component receives all data as inputs -->
  <app-data-grid
    [columns]="columns"
    [data]="dataSignal()"
    [totalItems]="totalSignal()"
    [loading]="loadingSignal()"
    (sortChange)="onSortChange($event)"
    (pageChange)="onPageChange($event)"
  />
</div>
```

## Architectural Patterns

### Pattern 1: Generic Data Grid with Angular Material `<mat-table>`

**What:** A reusable `DataGridComponent<T>` that wraps Angular Material's `<mat-table>` with pagination, sorting, and configurable columns. Uses TypeScript generics so every entity gets full type safety.

**When to use:** Every entity page that displays a paginated data table. This is the primary consumer.

**Trade-offs:**
- ✅ Eliminates ~80% duplicated HTML across entity pages (each entity previously had its own `<table mat-table>` with identical structure)
- ✅ Single point of change for table layout, pagination styling, loading state
- ✅ Type-safe: `ColumnDef<T>.field` is constrained to `keyof T`
- ❌ Custom cell rendering (e.g., status chips, action buttons) needs a slot mechanism — the grid can't know how to render every possible cell format
- ❌ If an entity needs radically different table behavior, the generic abstraction may fight you (but for this domain, all tables are standard paginated lists)

**Example:**

```typescript
// data-grid.component.ts
import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ColumnDef, SortChange, PageChange } from './data-grid.models';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatSortModule,
    MatPaginatorModule, MatProgressBarModule,
  ],
  template: `
    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="data()" matSort
             (matSortChange)="onSort($event)">

        @for (col of columns(); track col.field) {
          <ng-container [matColumnDef]="String(col.field)">
            <th mat-header-cell *matHeaderCellDef
                [mat-sort-header]="col.sortable ? String(col.field) : undefined"
                [style.width]="col.width ?? 'auto'"
                [style.text-align]="col.align ?? 'left'">
              {{ col.header }}
            </th>
            <td mat-cell *matCellDef="let row"
                [style.text-align]="col.align ?? 'left'">
              {{ col.cell ? col.cell(row) : row[col.field] }}
            </td>
          </ng-container>
        }

        <tr mat-header-row *matHeaderRowDef="columnKeys(); sticky: true"></tr>
        <tr mat-row *matRowDef="let row; columns: columnKeys();"></tr>

        @if (data().length === 0 && !loading()) {
          <tr class="mat-row">
            <td class="mat-cell text-center"
                [attr.colspan]="columnKeys().length">
              Nessun dato trovato
            </td>
          </tr>
        }
      </table>
    </div>

    <mat-paginator [length]="totalItems()"
                   [pageSize]="10"
                   [pageSizeOptions]="[5, 10, 25, 100]"
                   (page)="onPage($event)"
                   [showFirstLastButtons]="true"
                   aria-label="Seleziona pagina">
    </mat-paginator>
  `,
  styles: [`
    .table-container { border-radius: 8px; overflow: hidden; }
    .text-center { text-align: center; padding: 24px; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataGridComponent<T extends Record<string, any>> {
  /** Required: column definitions */
  readonly columns = input.required<ColumnDef<T>[]>();
  /** Required: row data */
  readonly data = input.required<T[]>();
  /** Required: total items count for paginator */
  readonly totalItems = input.required<number>();
  /** Loading state — shows progress bar */
  readonly loading = input(false);

  /** Emitted when user sorts a column */
  readonly sortChange = output<SortChange>();
  /** Emitted when user changes page */
  readonly pageChange = output<PageChange>();

  /** Derived: array of column field names (for matColumnDef) */
  protected columnKeys = computed(() => this.columns().map(c => String(c.field)));
  protected String = String; // template needs access

  protected onSort(sort: Sort): void {
    this.sortChange.emit({ column: sort.active, direction: sort.direction as 'asc' | 'desc' | '' });
  }

  protected onPage(event: PageEvent): void {
    this.pageChange.emit({ page: event.pageIndex, pageSize: event.pageSize });
  }
}
```

### Pattern 2: Standardized Typed Service Methods

**What:** Every entity service exposes a `getPaged(params: TQueryParams): Observable<ApiResponse<PagedResponse<TEntity>>>` method. This creates a uniform contract that the smart page component can rely on.

**When to use:** On all entity services that feed data to the grid.

**Trade-offs:**
- ✅ Enables the smart page to use identical patterns (`BehaviorSubject` + `switchMap`) regardless of entity
- ✅ Type-safe query building — TypeScript catches missing params
- ✅ Can use an `HttpParams` builder helper to eliminate if-chain repetition
- ❌ Existing services have non-uniform patterns — need refactoring per service
- ❌ Date handling (Z-suffix normalization in StaffService) is a service-level concern that each service still handles independently

### Pattern 3: BehaviorSubject + switchMap Query Pipeline

**What:** The smart page component manages query state through a `BehaviorSubject<QueryParams>`. User actions (search, sort, page) push partial updates via `updateQuery()`. The pipeline automatically debounces/cancels stale requests via `switchMap`.

**When to use:** Every smart page component that fetches paginated data.

**Trade-offs:**
- ✅ Proven pattern already used in `StaffComponent` — no new concepts
- ✅ `switchMap` automatically cancels in-flight requests when a new query arrives
- ✅ Single source of truth for the entire query state
- ❌ The `updateQuery()` method has a subtle reset-to-page-0-or-not logic that must be replicated consistently
- ❌ If the app grows to need cross-page shared filter state (e.g., "show inactive items everywhere"), this pattern needs to evolve to a service-level store

### Pattern 4: Action Column via Content Projection (ng-content)

**What:** The DataGrid doesn't know about action buttons (edit/delete). Instead of baking them into the grid, the smart page provides an actions template via `ng-content` projection or a dedicated `@Input()` for custom cell rendering.

**Approach A — Custom cell template function:**
```typescript
// In smart page component definition:
columns: ColumnDef<Staff>[] = [
  { field: 'username', header: 'Username' },
  // ... standard columns
  {
    field: 'actions',
    header: 'Azioni',
    // No sortable — actions column isn't data-driven
    // The smart page provides a custom cell renderer:
  },
];

// Alternative: Use an action slot
<app-data-grid [columns]="columns" ... >
  <!-- Actions rendered by smart component, accessing row data -->
  <ng-template #actions let-row>
    <button (click)="editStaff(row)">Edit</button>
    <button (click)="deleteStaff(row)">Delete</button>
  </ng-template>
</app-data-grid>
```

**Recommendation:** For this codebase, use an `actionsTemplate` `@Input()` on the grid that receives a `TemplateRef` with access to the row. The smart page passes a template with edit/delete buttons. This keeps the grid reusable while allowing entity-specific action buttons.

**Trade-offs:**
- ✅ Keeps action button styling and behavior in the smart page
- ✅ Grid doesn't need to know about entity-specific dialogs
- ❌ Adds complexity to grid API — need `TemplateRef` input + row context
- ❌ If most entities share similar action buttons, consider a default actions slot with optional override

## Anti-Patterns

### Anti-Pattern 1: Grid Injects Services

**What people do:** The data grid component directly injects `StaffService` or `HttpClient` and fetches its own data.

**Why it's wrong:** The grid is no longer reusable — it's tied to a specific entity. You'd need a separate grid component per entity, which is exactly the duplication we're eliminating.

**Do this instead:** The grid is purely presentational. It receives data via `@Input()` and emits events via `@Output()`. The smart page component owns all service calls.

### Anti-Pattern 2: Grid Owns Filter State

**What people do:** The grid component manages `search`, `sortColumn`, `sortDirection` internally and exposes them as two-way bindings or emits raw filter events.

**Why it's wrong:** Filter state is business logic (what filters to show, how they combine). The smart page needs to manage this because it builds the query object for the API call. If the grid owns filter state, the smart page has to sync its state back, creating a split-brain problem.

**Do this instead:** The grid only manages internal UI state (which column is being hovered). Filter values, sort state, and pagination are managed entirely in the smart page component. The grid emits "the user clicked sort on column X" and the smart page decides what to do.

### Anti-Pattern 3: Generic Grid Accepts `any[]` for Data

**What people do:** `@Input() data: any[]` — bypasses TypeScript generics for simplicity.

**Why it's wrong:** You lose all type safety. `ColumnDef.field` can be set to a string that doesn't exist on the entity type. Custom cell renderers receive `any` instead of the typed entity.

**Do this instead:** Use `DataGridComponent<T extends Record<string, any>>` with generic `ColumnDef<T>[]` input. This is the key pattern that makes the grid both reusable AND type-safe.

### Anti-Pattern 4: Shared `buildHttpParams` Utility in Every Service Without Discriminating Null/Undefined

**What people do:** The utility blindly calls `.set()` for every property, sending `?IsActive=null` or `?Email=undefined` in the URL.

**Why it's wrong:** The backend sees the string `"null"` or `"undefined"` as a value, not as "no filter". This can cause wrong query results or server errors.

**Do this instead:** The `buildHttpParams` helper must skip values that are `undefined`, `null`, or `''` (empty string). For boolean fields sent only when explicitly set, check `isActive !== undefined && isActive !== null`.

## Data Flow

### Request Flow (Paged Query)

```
1. User types in search field
        │
2. SmartPageComponent.onSearch(value)
        │
3. updateQuery({ search: value, page: 0 })
        │  (resets to page 0 when search changes)
4. querySubject.next(newQuery)
        │
5. switchMap cancels previous Observable (if in-flight)
        │  creates new Observable from service.getPaged()
6. StaffService.getPaged(query)
        │
7. buildHttpParams(query) → HttpParams
        │
8. this.http.get<ApiResponse<PagedResponse<Staff>>>(url, { params })
        │
9. interceptors: apiInterceptor → authInterceptor
        │
10. HTTP GET /api/Staff?Page=0&PageSize=10&Search=john&SortColumn=username&SortDirection=asc
        │
11. Response: { succeeded: true, data: { items: [...], totalCount: 42, ... } }
        │
12. .subscribe() callback in SmartPageComponent
        │
13. dataSignal.set(res.data.items)
        │    totalSignal.set(res.data.totalCount)
        │    loadingSignal.set(false)
        │
14. Template re-renders with <app-data-grid [data]="dataSignal()" [totalItems]="totalSignal()">
```

### Sort Flow

```
1. User clicks "Username" column header
        │
2. <mat-table matSort> triggers (matSortChange) event
        │
3. DataGridComponent.onSort(sort: Sort) emits sortChange.emit()
        │
4. SmartPageComponent.onSortChange(sortChange) calls updateQuery({ sortColumn, sortDirection })
        │
5. Same pipeline as search flow from step 4 above
```

### CRUD Flow (Edit)

```
1. User clicks edit icon in action column
        │
2. DataGridComponent emits action via TemplateRef click → SmartPageComponent.editStaff(row)
        │
3. SmartPageComponent opens MatDialog with row data
        │
4. Dialog closes with updated data
        │
5. SmartPageComponent calls staffService.update(id, data).subscribe()
        │
6. On success: refreshTable() calls updateQuery({}) — re-emits current query
        │
7. Grid re-renders with fresh data
```

### State Management

| State | Owner | Mechanism | Notes |
|-------|-------|-----------|-------|
| Query params (page, sort, filters) | Smart page component | `BehaviorSubject<TQueryParams>` | Single source of truth for what's being fetched |
| Grid data (rows) | Smart page component | `signal<TEntity[]>` | Populated from service response, passed to grid via `@Input()` |
| Total items count | Smart page component | `signal<number>` | Passed to grid paginator via `@Input()` |
| Loading state | Smart page component (or via UiService) | `signal<boolean>` | Shows progress bar in grid |
| Column definitions | Smart page component (static) | `ColumnDef<T>[]` constant | Defined once, never changes — stable reference for `OnPush` |
| Filter form values | Smart page component | `FormGroup` or individual signals | Maps to query params on submit/change |
| Auth/user state | `AuthService` (singleton) | Signals | Unchanged by this architecture |
| Toast/confirm/loader | `UiService` (singleton) | Signals | Unchanged by this architecture |

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 3-5 entities (current) | Smart page per entity with inline filter sections. No need for generic filter component or state service. |
| 10-20 entities | Extract a generic `FilterSectionComponent` driven by a filter config array. Consider a `QueryService` that manages URL-based filter persistence. |
| 20+ entities | Build a config-driven grid factory or base class for smart pages. Consider a service-level query state cache for URL sync + browser back button. |

### Scaling Priorities

1. **First bottleneck:** Inconsistent service patterns. The current mix of signal-based and Observable-based services will cause confusion as the grid pattern is rolled out. Fix: standardize all services to return `Observable<ApiResponse<PagedResponse<T>>>` for GET operations.

2. **Second bottleneck:** Filter state management. Each smart page currently duplicates the `updateQuery()` method and the page-0-reset logic. At 10+ entities, extract a `QueryState` class or a base class that manages this logic.

3. **Third bottleneck:** URL synchronization. Users may expect filters/search to persist across navigation. At that point, sync query params to URL query string via Angular Router.

## Integration Points

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Smart page ↔ DataGridComponent | `@Input()` data, total / `@Output()` events | Strict parent→child data flow. Grid never reaches back to parent. |
| Smart page ↔ Entity service | Method call + `Observable` return | Page calls `service.getPaged()`, subscribes, maps to signals. |
| Smart page ↔ Filter form | Component-internal | Filter values are local state, mapped to query on change. |
| Smart page ↔ CRUD dialogs | `MatDialog.open()` + `afterClosed()` | Existing pattern, unchanged. |
| DataGridComponent ↔ Angular Material | `MatTable`, `MatPaginator`, `MatSort` directives | Grid wraps Material primitives, owns all Material imports. |

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| REST API (`/api/{entity}`) | `HttpClient.get()` with `HttpParams` | Uniform contract: common params + entity-specific. Response always `{ succeeded, data }`. |
| Auth API | Existing `AuthService` — unchanged | JWT tokens via interceptor. |

## Suggested Build Order (Phase Implications)

This order minimizes risk by building infrastructure first, then migrating entities one at a time:

1. **Infrastructure: Type definitions + builder helper**
   - Add `BaseQueryParams` and entity-specific extensions to `api.models.ts`
   - Create `buildHttpParams()` helper
   - Create `data-grid.models.ts` with `ColumnDef<T>`, `SortChange`, `PageChange`
   - ⚡ No breaking changes, pure addition

2. **Infrastructure: `DataGridComponent<T>`**
   - Build the standalone generic grid component
   - Wire up `mat-table`, `mat-sort`, `mat-paginator`
   - Support loading state, empty state, column alignment
   - ⚡ Can be dropped into any existing page for testing

3. **Service refactoring: One entity service (`StaffService`)**
   - Add `getPaged(StaffQueryParams)` method
   - Keep existing methods for backward compatibility
   - Refactor `StaffComponent` → `StaffPageComponent` using new grid
   - ⚡ Proves the pattern end-to-end with one entity

4. **Migration: Remaining entity services (Customer, Ingredient, Plate, Allergen)**
   - Add `getPaged()` to each service
   - Create corresponding page components
   - ⚡ Repeat proven pattern, low risk

5. **Cleanup: Remove old component files**
   - Delete old `staff.component.ts`, `staff.component.html` (etc.) after migration verified
   - ⚡ Only after all entities are migrated

## Sources

- Angular Material `MatTable` documentation: https://material.angular.io/components/table/overview
- Angular Material `MatPaginator` documentation: https://material.angular.io/components/paginator/overview
- Angular Material `MatSort` documentation: https://material.angular.io/components/sort/overview
- Angular HTTP `HttpParams` API: https://angular.dev/api/common/http/HttpParams (HIGH confidence)
- Angular Signals component API (`input()`, `output()`, `model()`): https://blog.angular-university.io/angular-signal-components (MEDIUM confidence — community article, aligns with official docs)
- Angular Smart/Dumb component pattern: https://medium.com/@monikasomashekar9/smart-vs-dumb-components-in-angular-d8d87edaefc5 (MEDIUM confidence — multiple sources agree on pattern)
- Reusable Angular table component with generics: https://medium.com/@mamdouhibr67/creating-a-generic-table-component-in-angular-keep-it-dry-3b71c384a47f (LOW confidence — community article, but pattern validated against Angular docs)
- Angular Material Data Table server pagination complete guide: https://blog.angular-university.io/angular-material-data-table (MEDIUM confidence)
- Existing codebase analysis: `StaffComponent`, `CustomerService`, `StaffService`, `api.models.ts` (HIGH confidence — direct observation)

---

*Architecture research for: fe-roscoff data grid refactoring*
*Researched: 2026-05-29*
