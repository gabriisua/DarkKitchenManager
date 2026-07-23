# Roadmap: Roscoff Back-Office (bo-configurator)

## Overview

Refactor the existing five duplicated entity-table implementations (Staff, Customer, Allergen, Ingredient, Plate) into a single reusable `<app-data-grid>` component with a standardized API service layer. The project builds infrastructure first (type system, utility functions, grid component), proves the pattern with two complex entities (Staff and Customer), migrates the remaining three, and finishes with cleanup and polish.

## Phases

- [ ] **Phase 1: Foundation — Type System & Utilities** - Standardized query/response contracts and HTTP utility
- [ ] **Phase 2: Data Grid Component** - Reusable, type-safe generic data grid with loading/empty/error states
- [ ] **Phase 3: Service Standardization + Staff & Customer Migration** - Standard API services + first entity views migrated to grid
- [ ] **Phase 4: Remaining Entity Migrations** - Ingredient, Plate, and Allergen views migrated to grid
- [ ] **Phase 5: Cleanup & Polish** - Old components removed, codebase consistency verified

## Phase Details

### Phase 1: Foundation — Type System & Utilities
**Goal**: Standardized query/response contracts and utility functions are available for all downstream consumers.
**Depends on**: Nothing (first phase)
**Requirements**: SVC-01, SVC-02, SVC-03, SVC-05, SVC-06
**Success Criteria** (what must be TRUE):
  1. `PagedRequest` interface exists with all common query parameters (page, pageSize, search, sortColumn, sortDirection, dateFrom, dateTo)
  2. Entity-specific request interfaces (`StaffPagedRequest`, `CustomerPagedRequest`, etc.) extend `PagedRequest` with their unique filter fields
  3. `buildPagedParams()` utility converts typed query objects to `HttpParams`, handling camelCase-to-PascalCase, null/undefined/empty skipping, and date formatting
  4. `PagedResponse<T>` interface with `items`, `totalCount`, `page`, `pageSize`, `totalPages` is defined and used as the canonical response type
**Plans**: TBD

### Phase 2: Data Grid Component
**Goal**: Developers can use a single reusable `<app-data-grid>` component across all entity pages with consistent behavior.
**Depends on**: Phase 1
**Requirements**: GRID-01, GRID-02, GRID-03, GRID-04, GRID-05, GRID-06, GRID-07, GRID-08, GRID-09, GRID-10, GRID-11, GRID-12, ARCH-01, ARCH-03
**Success Criteria** (what must be TRUE):
  1. Grid receives a typed `ColumnDef<T>[]` input and renders a Material table with correct columns, headers, and cell values
  2. Grid shows a loading spinner overlay while `[loading]` is true, an empty-state message when data array is empty, and an error banner with retry button when `[error]` is set
  3. Grid emits `(onSortChange)` with `SortColumn`/`SortDirection` on column header click and `(onPageChange)` with page/pageSize on paginator interaction; paginator displays total record count
  4. Grid supports content projection (`<ng-content>`) for entity-specific filter sections above the table and custom cell rendering via `TemplateRef` for special columns (status chips, dates, currency, actions)
  5. Grid uses `ChangeDetectionStrategy.OnPush`, enforces `keyof T` on column defs, and has zero `any` type usage in its public API
**Plans**: TBD
**UI hint**: yes

### Phase 3: Service Standardization + Staff & Customer Migration
**Goal**: Staff and Customer entity pages use the standardized grid with working server-side pagination, sorting, and filtering.
**Depends on**: Phase 2
**Requirements**: SVC-04, STAF-01, STAF-02, STAF-03, CUST-01, CUST-02, CUST-03, CUST-04
**Success Criteria** (what must be TRUE):
  1. Staff and Customer services each provide a `getPaged(params)` method returning `Observable<ApiResponse<PagedResponse<T>>>` using `buildPagedParams()` internally
  2. Staff list view shows the reusable grid with Email and Role filter section; changing filters, sorting, or pagination triggers an API call and re-renders the grid
  3. Customer list view shows the reusable grid with Type (text) and IsActive (boolean tri-state) filter section; filter/sort/page changes update the grid via server calls
  4. Any filter change or sort column change resets the page to 1 automatically
  5. API responses are unwrapped consistently in a single centralized location (interceptor or utility)
**Plans**: TBD
**UI hint**: yes

### Phase 4: Remaining Entity Migrations
**Goal**: All five restaurant entities (Ingredient, Plate, Allergen) use the unified data grid with standardized API calls.
**Depends on**: Phase 3
**Requirements**: INGR-01, INGR-02, INGR-03, INGR-04, PLAT-01, PLAT-02, PLAT-03, PLAT-04, ALLE-01, ALLE-02, ALLE-03
**Success Criteria** (what must be TRUE):
  1. Ingredient list view shows the grid with Name text filter, EnergyKcal range (min/max), Cost range (min/max), and IsActive toggle; range filters send both or neither parameter
  2. Plate list view shows the grid with Name text filter, CategoryId dropdown (populated from API), IsActive toggle, and Price range (min/max)
  3. Allergen list view shows the grid with Name and Code text filters
  4. Each entity's grid renders correct columns with proper cell formatting (status chips for isActive, currency for prices, dates for timestamps, action buttons for CRUD)
**Plans**: TBD
**UI hint**: yes

### Phase 5: Cleanup & Polish
**Goal**: Old components are removed and the codebase is verified for consistency and type safety.
**Depends on**: Phase 4
**Requirements**: ARCH-02
**Success Criteria** (what must be TRUE):
  1. All old duplicated entity list components (StaffComponent, CustomerComponent, etc.) are deleted or fully replaced
  2. No `any` type usage exists in newly written grid-related code across all migrated entities
  3. All entity pages consistently follow the smart/dumb component pattern — smart component owns API + state, grid is pure presentation
**Plans**: TBD

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 0/0 | Not started | - |
| 2. Data Grid | 0/0 | Not started | - |
| 3. Service + Staff/Customer | 0/0 | Not started | - |
| 4. Remaining Entities | 0/0 | Not started | - |
| 5. Cleanup & Polish | 0/0 | Not started | - |
