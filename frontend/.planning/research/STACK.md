# Stack Research: Reusable Data Grid + Standardized API Service Layer

**Domain:** Angular 21 back-office dashboard with Material Design — reusable data grid component and standardized API service layer with typed query parameters.
**Researched:** 2026-05-29
**Confidence:** HIGH

---

## Executive Summary

This project needs to refactor N duplicated entity-table implementations into a single reusable `<app-data-grid>` component with a standardized API service layer. The existing app uses Angular 21.2, Angular Material 21.2.11, RxJS 7.8, and standalone components with signals.

**Key finding: Do NOT use `MatTableDataSource` for server-paginated data.** It is designed for client-side sorting, filtering, and pagination. For this project (where all pagination/sorting happens server-side), use plain arrays with `[dataSource]="data"`, listen to `(matSortChange)` and `(page)` events directly, and send those values as query parameters to the API.

The Angular Material official `TableHttpExample` at `material.angular.dev` demonstrates exactly this server-side pattern — using `merge(sort.sortChange, paginator.page).pipe(switchMap(...))`.

---

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Angular Material `MatTable` | ^21.2.11 (existing) | Grid rendering engine | Already in project. Best-in-class Angular table with virtual scrolling, sticky columns/rows, accessible markup, and composable `matColumnDef` API. |
| `MatSort` | ^21.2.11 (existing) | Column sorting UI | Built-in `<th mat-sort-header>` directive + `matSortChange` event emitter. Emits `Sort {active: string, direction: 'asc'\|'desc'\|''}` — directly maps to API `SortColumn`/`SortDirection`. |
| `MatPaginator` | ^21.2.11 (existing) | Pagination controls | Provides page index/size UI with `(page)` output emitting `PageEvent {pageIndex, pageSize, length}`. Handles first/last/prev/next buttons and page size selector. |
| Observable + `switchMap` | rxjs ~7.8.0 (existing) | Reactive request pipeline | The canonical pattern: `merge(sortChange$, page$).pipe(switchMap(() => apiCall$))`. `switchMap` auto-cancels in-flight requests when new sort/page happens. |
| Angular Signals | @angular/core ^21.2.0 (existing) | Component-local reactive state | Use `signal<T>()` for the data array, loading state, and total count in smart components. Signals are the project's chosen reactive primitive. |
| TypeScript Generics | ~5.9.2 (existing) | Type-safe grid component | The grid component class uses `<T>` generic type parameter. Inputs like `columns: GridColumn<T>[]` and `data: T[]` keep the grid type-safe per entity. |

### Angular Material Table APIs — Correct Usage for Server-Side Data

| API | What it does | How to use for this project |
|-----|-------------|----------------------------|
| `mat-table` directive | Renders `<table>` with `[dataSource]` | Pass plain array: `[dataSource]="data()"` — **NOT** `MatTableDataSource` instance |
| `matSort` directive | Enables sort headers on `<table>` | Bind `(matSortChange)="onSort($event)"` to emit `Sort` to parent |
| `mat-sort-header` directive | Makes column header click-sortable | Add to `<th mat-header-cell>` elements that map to sortable columns |
| `mat-paginator` component | Pagination UI controls | Bind `(page)="onPage($event)"` for page changes, `[length]="totalCount()"` for total |
| `matColumnDef` directive | Defines a column template | Used inside `<ng-container>` per column — the grid component wraps these via config-driven approach |
| `*matNoDataRow` | Empty state row | Built-in row shown when data source is empty |
| `sticky` input on `matColumnDef` | Sticky first/last column | Use for actions column (right-sticky) on wide tables — keeps action buttons visible on horizontal scroll |

### Service Layer Patterns

| Pattern | Where | Why |
|---------|-------|-----|
| Generic `PagedQuery` interface | `api.models.ts` | Base type for all paginated GET requests — shared `page`, `pageSize`, `search`, `sortColumn`, `sortDirection`, `dateFrom`, `dateTo` |
| Entity-specific query interfaces | `api.models.ts` | Extend `PagedQuery` with entity-specific filter fields (e.g., `Role` for Staff, `CategoryId` for Plate) |
| `buildPagedParams(query)` utility function | New `core/utils/http-params.util.ts` | Single place to convert typed query → `HttpParams`. Eliminates the 7-line `if`-chain duplicated across every service today. |
| `Observable<PagedResponse<T>>` return type | All `get*()` service methods | Strongly typed return: `{ items: T[], totalCount: number, page: number, pageSize: number, totalPages: number }` — no more `any` |
| Service returns Observable only | Service layer boundary | Smart component subscribes with `.subscribe()` — no signals in services for list data. Keeps API layer pure. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| TypeScript strict mode | Existing in `tsconfig.json` | Already enabled — `strict: true` |
| Prettier | Code formatting | Already configured — `.prettierrc` with `singleQuote: true`, `printWidth: 100` |

---

## Architecture Pattern: Smart/Dumb Component Separation

### Smart Component (Entity Page)
- **Location:** `src/app/features/dashboard/{entity}/`
- **Owns:** Signal state (`data`, `totalCount`, `loading`), `BehaviorSubject` for query, RxJS subscription to API, CRUD operations (open dialog, delete confirm)
- **Contains:** Filter form section (entity-specific fields above grid), action buttons (Add New)
- **Renders:** `<app-data-grid>` — passes data down

### Dumb Component (Data Grid)
- **Location:** `src/app/shared/data-grid/`
- **Owns:** Nothing — purely presentation
- **Inputs:** `columns: GridColumn<T>[]`, `data: T[]`, `totalCount: number`, `loading: boolean`, `pageSize: number`, `pageIndex: number`, `sortActive: string`, `sortDirection: SortDirection`
- **Outputs:** `sortChange: Sort`, `pageChange: PageEvent`
- **Displays:** Material table + paginator + loading overlay
- **No API calls.** Emits events, parent reacts.

### Column Config Type

```typescript
interface GridColumn<T> {
  key: keyof T;              // Property name on the data model
  header: string;             // Display label in <th>
  sortable?: boolean;         // Show mat-sort-header? (default: true)
  sticky?: 'start' | 'end';  // For action columns
  width?: string;             // CSS width override
  // Cell rendering via Angular control flow: the grid component
  // will use @switch on column.key to render different cell types,
  // OR the smart component passes content projection templates.
}
```

**Decision: Content projection vs config-driven cell templates**

For this project's scope (5 entity views with different columns), use a **config-driven approach** where `GridColumn` includes a `type` field that maps to built-in renderers:

```typescript
type CellType = 'text' | 'chip' | 'date' | 'currency' | 'boolean' | 'actions';

interface GridColumn<T> {
  key: keyof T;
  header: string;
  cellType: CellType;    // Determines how cells render
  sortable?: boolean;
  sticky?: 'start' | 'end';
  width?: string;
  // For 'chip' type:
  chipMap?: Record<string, { label: string; color: string }>;
  // For 'actions' type:
  actions?: GridAction<T>[];
}

interface GridAction<T> {
  icon: string;
  label: string;
  color?: string;
  handler: (row: T) => void;
  visible?: (row: T) => boolean;
}
```

**Rationale:** Five entity views × ~6 columns each × 2-3 cell types = manageable set of built-in renderers. Content projection (`ngTemplateOutlet`) is more flexible but adds complexity — defer to phase 2 if needed. The current duplication is in the table HTML boilerplate (sort headers, column defs, paginator), not in custom cell rendering.

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| `MatTableDataSource` | Designed for client-side data management. Requires `.paginator` and `.sort` assignment in `ngAfterViewInit`. Wastes memory on server-paginated data and adds unnecessary complexity. | Plain array `[dataSource]="dataArray"` with manual event handling for `(matSortChange)` and `(page)` |
| `MatTableDataSource` with `effect()` syncing | Current pattern: `effect(() => this.dataSource.data = this.dataSignal())`. Two reactive primitives doing the same job. Extra indirection with no benefit for server-side data. | Direct binding: `[dataSource]="data()"` where `data` is a `signal<T[]>` |
| `*ngFor` / `*ngIf` structural directives | Angular 17+ deprecated these in favor of `@for` / `@if` control flow syntax. Mixed usage found in existing code (Staff uses `*matRowDef`, Customer uses `@for` for columns). | `@for` / `@if` control flow syntax consistently |
| `BehaviorSubject<Query>` in every component | Currently every list component creates its own `querySubject`, `updateQuery()`, `onSearch()`, `onSortChange()`, `onPageChange()` methods — 100% duplicated boilerplate. | Move query state management into the **smart component only** but standardize via a composable or service base class. Each smart component still owns its query, but uses a shared `buildPagedParams()` utility. |
| `any` return types on API services | `StaffService.getStaff()` returns `Observable<any>`. Loses all type safety. The `PagedResponse<T>` interface already exists in `api.models.ts` but is unused. | `Observable<PagedResponse<T>>` with proper API response unwrapping |
| Constructor DI | Inconsistent with the project's newer components. StaffService uses constructor DI while CustomerService uses `inject()`. | `inject()` consistently — matches existing newer components |
| `onSearch` with `(input)` event | Sends API request on every keystroke. The debounce is left to the backend or network latency. | Add `debounceTime(300)` + `distinctUntilChanged()` before `switchMap` in the query pipeline. Implemented in the smart component. |
| `HttpParams` chain in every service | Every service has the same 7-15 line pattern of `new HttpParams().set(...)` with conditional `if` blocks. | Single `buildPagedParams(query: PagedQuery): HttpParams` utility function |

---

## Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `@angular/material` (existing) | ^21.2.11 | MatTable, MatSort, MatPaginator, MatProgressSpinner, forms, buttons, icons | Everything — core of the grid |
| `@angular/cdk` (existing) | ^21.2.11 | CDK utilities (collections, a11y) | Underlying dependency for Material |
| `rxjs` (existing) | ~7.8.0 | `merge`, `switchMap`, `debounceTime`, `distinctUntilChanged`, `catchError`, `startWith` | All reactive data flow |
| No new libraries needed | — | — | The existing Angular Material + RxJS + Signals stack fully covers this feature |

---

## Installation

No new packages required. All dependencies are already in the project:

```bash
# Existing packages (already installed):
# @angular/core ^21.2.0
# @angular/material ^21.2.11
# @angular/cdk ^21.2.11
# rxjs ~7.8.0
```

New files to create (not npm install):
- `src/app/shared/data-grid/data-grid.component.ts`
- `src/app/shared/data-grid/data-grid.types.ts`
- `src/app/core/utils/http-params.util.ts`
- Updated `src/app/shared/models/api.models.ts` (add `PagedQuery` interface)

---

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| Manual MatTable binding (no MatTableDataSource) | `MatTableDataSource` + auto-sort/paginate | Only if data is client-side and small (<500 rows). For server-side pagination: always manual. |
| Config-driven cell types + `@switch` in grid template | Content projection via `<ng-template>` / `ngTemplateOutlet` | When column rendering varies wildly per entity (different component types in cells). For this project's scope: config-driven is simpler and sufficient. |
| Smart component owns `BehaviorSubject` + `switchMap` | A shared base class or composable | If more than 6-7 entities share identical query management. At 5 entities, duplication is manageable via a utility `buildPagedParams()` function. The smart component pattern already separates concerns. |
| Single `buildPagedParams()` utility function | Base service class (abstract `BasePagedService<T>`) | If services also share CRUD methods. Currently each entity has unique CRUD (some use dialogs, some use form pages), so a utility function is lower-risk than inheritance. Defer to phase 2 if patterns converge. |

---

## Version Compatibility

| Package | Compatible With | Notes |
|---------|-----------------|-------|
| `@angular/material@21.2.11` | Angular 21.2.x | Already installed and configured |
| `rxjs@7.8.x` | Angular 21.x | Already installed |
| TypeScript `~5.9.2` | Angular 21.x | Already configured with strict mode |

No compatibility issues expected — all recommended patterns use existing dependencies. The new code is pure TypeScript/HTML/CSS within the existing framework.

---

## Sources

- **Angular Material official docs (material.angular.dev)** — MatTable, MatSort, MatPaginator, TableHttpExample — HIGH confidence
  - Server-side pattern: `merge(sortChange, page).pipe(switchMap(apiCall))` — source: `table-http-example` from `/angular/material2-docs-content`
  - MatSort API: `@Output('matSortChange') sortChange: EventEmitter<Sort>` — source: material.angular.dev/components/sort/api
  - MatPaginator API: `@Output() page: EventEmitter<PageEvent>` with `pageIndex`, `pageSize`, `length` — source: material.angular.dev/components/paginator/api
  - Plain array as dataSource: `table mat-table [dataSource]="data"` — source: `table-http-example-html`
- **Existing codebase audit** — `.planning/codebase/ARCHITECTURE.md`, `.planning/codebase/CONVENTIONS.md`, `src/app/features/dashboard/staff/`, `src/app/features/dashboard/customer/`, `src/app/core/services/`, `src/app/shared/models/` — HIGH confidence (verified by reading source files)
  - Existing `PagedResponse<T>` generic interface in `api.models.ts` (line 175-181)
  - Existing `BehaviorSubject` + `switchMap` pattern in both StaffComponent and CustomerComponent
  - Existing element-ui.css at `src/styles.css` with shared table/page utility classes
  - Inconsistent page indexing: Staff uses `page: 0` (0-based), Customer uses `page: 1` (1-based)
- **Context7 verification** — Angular Material docs for component APIs and server-side patterns — HIGH confidence

---

*Stack research for: Reusable Data Grid + API Service Layer*
*Researched: 2026-05-29*
