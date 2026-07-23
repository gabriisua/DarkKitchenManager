# Requirements: Roscoff Back-Office

**Defined:** 2026-05-29
**Core Value:** Staff can efficiently browse, filter, and manage all restaurant entities through consistent, fast, paginated tables.

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### Data Grid Component

- [ ] **GRID-01**: Grid is a reusable standalone Angular 21 component accepting generic `ColumnDef<T>[]` input with typed `key: keyof T`, label, sortable flag, width, sticky, and cell display value
- [ ] **GRID-02**: Grid renders data using Angular Material `MatTable` with plain array `[dataSource]` binding (no `MatTableDataSource`)
- [ ] **GRID-03**: Grid supports server-side pagination via `MatPaginator` with configurable `pageSizeOptions` — emits `(onPageChange)` output
- [ ] **GRID-04**: Grid supports server-side column sorting via `MatSort` — emits `(onSortChange)` output with `SortColumn`/`SortDirection`
- [ ] **GRID-05**: Grid shows loading spinner overlay while data is being fetched
- [ ] **GRID-06**: Grid shows empty state message when no records exist
- [ ] **GRID-07**: Grid shows error state with retry button when API call fails
- [ ] **GRID-08**: Grid supports content projection slot for entity-specific filter sections above the table
- [ ] **GRID-09**: Grid supports custom cell rendering via `TemplateRef` for special columns (status chips, dates, currency)
- [ ] **GRID-10**: Grid supports configurable action column with entity-specific action buttons (edit/delete/view)
- [ ] **GRID-11**: Grid uses `OnPush` change detection strategy
- [ ] **GRID-12**: Grid shows total record count in paginator

### API Service Layer

- [ ] **SVC-01**: Create `PagedRequest` interface with common query parameters: `page`, `pageSize`, `search`, `sortColumn`, `sortDirection`, `dateFrom`, `dateTo`
- [ ] **SVC-02**: Create entity-specific request interfaces extending `PagedRequest` for each entity's unique filters
- [ ] **SVC-03**: Create `buildPagedParams()` utility function that converts typed query params to `HttpParams` (including camelCase to PascalCase transformation for API)
- [ ] **SVC-04**: Add `getPaged(params)` method to each entity service returning `Observable<ApiResponse<PagedResponse<T>>>`
- [ ] **SVC-05**: Ensure standardized `PagedResponse<T>` interface with `items`, `totalCount`, `page`, `pageSize`, `totalPages`
- [ ] **SVC-06**: Standardize API response unwrapping in a single, centralized way

### Customer View

- [ ] **CUST-01**: Smart `CustomerPageComponent` owns API call, filter state, and CRUD dialogs
- [ ] **CUST-02**: Filter section with `Type` (text input) and `IsActive` (boolean tri-state)
- [ ] **CUST-03**: Uses reusable grid component with columns: email, type, shippingAddress, isActive, contactPhone, actions
- [ ] **CUST-04**: Filter/sort changes reset to page 1

### Ingredient View

- [ ] **INGR-01**: Smart `IngredientPageComponent` owns API call, filter state, and CRUD
- [ ] **INGR-02**: Filter section with `Name` (text), `MinEnergyKcal`/`MaxEnergyKcal` (number range), `MinCost`/`MaxCost` (number range), `IsActive` (toggle)
- [ ] **INGR-03**: Uses reusable grid with columns: name, energyKcalPer100g, costPer1000g, isActive, actions
- [ ] **INGR-04**: Range filters send both or neither to avoid API ambiguity

### Plate View

- [ ] **PLAT-01**: Smart `PlatePageComponent` owns API call, filter state, and CRUD
- [ ] **PLAT-02**: Filter section with `Name` (text), `CategoryId` (dropdown), `IsActive` (toggle), `MinPrice`/`MaxPrice` (number range)
- [ ] **PLAT-03**: Uses reusable grid with columns: name, categoryId, basePrice, isActive, actions
- [ ] **PLAT-04**: CategoryId dropdown populated from API lookup

### Staff View

- [ ] **STAF-01**: Smart `StaffPageComponent` owns API call, filter state, and CRUD dialogs
- [ ] **STAF-02**: Filter section with `Email` (text) and `Role` (text)
- [ ] **STAF-03**: Uses reusable grid with columns: username, email, role, isActive, lastLogin, actions

### Allergen View

- [ ] **ALLE-01**: Smart `AllergenPageComponent` owns API call, filter state, and CRUD dialogs
- [ ] **ALLE-02**: Filter section with `Name` (text) and `Code` (text)
- [ ] **ALLE-03**: Uses reusable grid with columns: name, code, description, actions

### Architecture

- [ ] **ARCH-01**: Smart/dumb component architecture — entity pages are smart (API + filters), grid is dumb (rendering only)
- [ ] **ARCH-02**: Clean up/replace old duplicated entity list components after migration
- [ ] **ARCH-03**: Remove any `any` type usage in new code (enforce `keyof T` on column defs)

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Grid Enhancements

- **GRID-13**: Column chooser with visibility toggles and localStorage persistence
- **GRID-14**: Column reorder via drag-and-drop
- **GRID-15**: Export to CSV/Excel

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Backend API implementation | Frontend-only refactor |
| Auth pages (login, forgot password, reset password) | Not related to grid refactor |
| Dashboard welcome/home page | Not a table-based view |
| Inline editing | Too complex, edits happen in dialogs |
| Client-side sorting/filtering | Contradicts server-side pagination contract |
| Virtual scrolling | Overkill for expected dataset sizes |
| Mobile/native app | Web-only back-office |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| GRID-01 | Phase 2 | Pending |
| GRID-02 | Phase 2 | Pending |
| GRID-03 | Phase 2 | Pending |
| GRID-04 | Phase 2 | Pending |
| GRID-05 | Phase 2 | Pending |
| GRID-06 | Phase 2 | Pending |
| GRID-07 | Phase 2 | Pending |
| GRID-08 | Phase 2 | Pending |
| GRID-09 | Phase 2 | Pending |
| GRID-10 | Phase 2 | Pending |
| GRID-11 | Phase 2 | Pending |
| GRID-12 | Phase 2 | Pending |
| SVC-01 | Phase 1 | Pending |
| SVC-02 | Phase 1 | Pending |
| SVC-03 | Phase 1 | Pending |
| SVC-04 | Phase 3 | Pending |
| SVC-05 | Phase 1 | Pending |
| SVC-06 | Phase 1 | Pending |
| CUST-01 | Phase 3 | Pending |
| CUST-02 | Phase 3 | Pending |
| CUST-03 | Phase 3 | Pending |
| CUST-04 | Phase 3 | Pending |
| INGR-01 | Phase 4 | Pending |
| INGR-02 | Phase 4 | Pending |
| INGR-03 | Phase 4 | Pending |
| INGR-04 | Phase 4 | Pending |
| PLAT-01 | Phase 4 | Pending |
| PLAT-02 | Phase 4 | Pending |
| PLAT-03 | Phase 4 | Pending |
| PLAT-04 | Phase 4 | Pending |
| STAF-01 | Phase 3 | Pending |
| STAF-02 | Phase 3 | Pending |
| STAF-03 | Phase 3 | Pending |
| ALLE-01 | Phase 4 | Pending |
| ALLE-02 | Phase 4 | Pending |
| ALLE-03 | Phase 4 | Pending |
| ARCH-01 | Phase 2 | Pending |
| ARCH-02 | Phase 5 | Pending |
| ARCH-03 | Phase 2 | Pending |

**Coverage:**
- v1 requirements: 39 total
- Mapped to phases: 39
- Unmapped: 0 ✓

---
*Requirements defined: 2026-05-29*
*Last updated: 2026-05-29 after initial definition*
