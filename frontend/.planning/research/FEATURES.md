# Feature Research: Reusable Data Grid + API Service Layer

**Domain:** Angular 21 back-office admin panel — data grid with CRUD for restaurant management entities
**Researched:** 2026-05-29
**Confidence:** HIGH (source code verified, production libraries examined)

## Feature Landscape

### Table Stakes (Users Expect These)

Features that users in a back-office admin panel assume exist. Missing these = product feels broken.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| **Server-side pagination** | Users navigate hundreds/thousands of records. No one pages through data client-side in a back-office. | LOW | Angular Material `MatPaginator` with server-driven `Page`/`PageSize`. Page resets to 1 on filter/sort change. |
| **Column sorting** | Click column header → sort asc, click again → desc. Essential for scanning data. | LOW | `MatSort` with server-side handling: emit `SortColumn` + `SortDirection` to API. Reset to page 1 on sort change. |
| **Global text search** | Free-text search across entity fields. Already exists in Customer and Staff components. | LOW | Debounced input field. Maps to `Search` query parameter. Must debounce 300-400ms to avoid excessive API calls. |
| **Loading state** | Skeleton/spinner shown while data loads. Without it, users perceive the app as unresponsive. | LOW | Conditionally show `MatProgressSpinner` over the table body. Not a full-page loader — only the grid area. |
| **Empty state** | "No records found" when data is empty or filters return zero results. | LOW | Show centered message in the table body with colspan covering all columns. Already partially implemented with `*matNoDataRow`. |
| **Error state** | API call fails → show error message with retry action. Crucial for debugging and UX. | LOW | Error banner replaces table body with "Failed to load data" + retry button. Use `MatSnackBar` or inline message. |
| **Per-row actions** | Edit/delete/view buttons on each row. Every entity has these (edit icon, delete icon). | LOW | Configurable action column with icon buttons. Accept action handlers from parent component. |
| **Consistent column headers** | Column labels are human-readable and language-appropriate (Italian for this app). | LOW | Column config includes `label` string. Already per-entity in existing templates. |
| **Column visibility control** | Not all columns fit on screen. Users need to choose which columns to show/hide. | MEDIUM | Column chooser menu (toolbar button or dropdown). Persist preference to localStorage per table. Table-stakes in 2026 back-office grids — Angular Material's native table doesn't have it, but production libraries (ngx-mat-simple-table, Kendo UI) all include it. |
| **Horizontal scroll for many columns** | Entities like Ingredient have 10+ nutritional columns. Must scroll horizontally without breaking layout. | LOW | Fixed table min-width, overflow-x: auto on wrapper. Ensure sticky columns stay visible during scroll. |
| **Page size selector** | Users want to see 10, 25, 50, or 100 rows per page. | LOW | `MatPaginator` `pageSizeOptions` input. Configurable per grid instance. |

### Current State Audit (What's Missing)

Audit of existing entity pages against the table-stakes list above:

| Entity | Pagination | Sorting | Search | Loading | Empty | Error | Actions | Filters |
|--------|------------|---------|--------|---------|-------|-------|---------|---------|
| Customer | ✅ server | ✅ | ✅ | ❌ | ✅ inline | ❌ inline | ✅ | Type, IsActive |
| Staff | ✅ server | ✅ | ✅ | ❌ | ✅ inline | ❌ console | ✅ | Email, Role |
| Allergen | ❌ load-all | ❌ | ❌ | ❌ | ✅ inline | ❌ | ✅ | Name, Code (on API) |
| Ingredient | ❌ load-all | ❌ | ❌ | ❌ | ✅ inline | ❌ | ❌ | API-level only |
| Plate | ❌ mock data | ❌ | ❌ | ❌ | ✅ inline | ❌ | ❌ | Planned |

**Key gaps:** No loading indicators, no error states with retry, Allergen and Ingredient lack pagination entirely (load-all in memory), Plate currently uses mock data. Sorting is done server-side but only Customer and Staff have it wired.

### Entity-Specific Filter Patterns

Each entity has different filter requirements beyond the standard `PagedRequest` parameters. The grid must support a **filter section** rendered by the smart component, not the grid itself.

| Entity | Filters | Filter Type | Notes |
|--------|---------|-------------|-------|
| **Customer** | `Type` (text), `IsActive` (boolean tri-state) | Text input + Select/Chip toggle | Type maps to API `Type` param; IsActive maps to API `IsActive` |
| **Ingredient** | `Name` (search), `MinEnergyKcal` + `MaxEnergyKcal` (range), `MinCost` + `MaxCost` (range), `IsActive` | Text input + number range inputs + toggle | Range filters are pairs — must always send both or neither to avoid API ambiguity |
| **Plate** | `Name`, `CategoryId` (lookup), `IsActive`, `MinPrice` + `MaxPrice` | Text + Select + toggle + number range | CategoryId likely needs a separate API call to populate the dropdown |
| **Staff** | `Email`, `Role` (text) | Text inputs | Simple text filters |
| **Allergen** | `Name`, `Code` | Text inputs | Simple text filters |

**Critical design point:** Filters are the **smart component's responsibility**, not the grid's. The grid only needs to:
1. Expose a filter slot (content projection / transclusion) where the smart component puts filter controls
2. Provide a unified event system so filter changes trigger API reloads with reset-to-page-1 semantics

### Differentiators (Competitive Advantage for This App)

Within this specific back-office project, these features differentiate the new grid from the current duplicated-code situation.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| **Declarative column config** | Replace ~50 lines of per-entity `ng-container matColumnDef` boilerplate with a typed `ColumnDef[]` array. This is the entire point of the refactor. | MEDIUM | `ColumnDef<T>` interface with `key`, `label`, `sortable`, `width`, `sticky`, `cellClass`, `displayValue` fields. Use `NgTemplateOutlet` for custom cell rendering escape hatch. Implementation patterned on `ngx-mat-simple-table` and `@proangular/pro-table`. |
| **Custom cell templates via content projection** | Some columns need chips (status), formatted dates, currency, or links. Smart parent provides `TemplateRef` per column; grid renders it via `NgTemplateOutlet`. | MEDIUM | Parent defines `<ng-template #myCell let-element>...</ng-template>` and column config references it. Grid uses `*ngTemplateOutlet` with row context. |
| **Server-side mode with unified query contract** | All grids automatically wire to `Page`, `PageSize`, `Search`, `SortColumn`, `SortDirection` + entity-specific filter params. Single source of truth. | MEDIUM | Standardized `PagedRequest<TFilters>` interface. Grid emits `(queryChange)` with complete request object. Smart component subscribes, calls API, passes results back. |
| **Filter section slot** | Entity-specific filter controls rendered above the grid but managed by the smart component. Clean separation — grid doesn't know about entities. | LOW | `<ng-content select="[gridFilters]">` or structural directive. Smart component places filter forms there. |
| **Page reset on filter change** | Changing any filter or sort automatically resets to page 1. | LOW | Smart component handles this with a `resetPage` flag when filter/sort change. |
| **Confirmation dialog for destructive actions** | Delete actions show a confirmation dialog (currently all use `confirm()`). | LOW | Replace `confirm()` with `MatDialog` confirmation. The grid provides an action handler; the smart component orchestrates the dialog. |
| **Consistent styling across all grid instances** | Single source of CSS — grid component owns all table styling. No per-entity CSS for table layout. | LOW | Grid component provides `host` CSS classes. Entity pages only add page-specific filter styling. |
| **State persistence (localStorage)** | Column order, visibility, and widths persist between sessions. Users don't reconfigure the grid every visit. | MEDIUM | Save/restore column state to localStorage keyed by `tableId`. Only add if users complain about reconfiguring columns. Can defer to v1.x. |

### Anti-Features (Things to Deliberately NOT Build)

Features that seem good for a reusable grid but create problems in this specific context.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| **Inline editing** | "Users could edit cells directly in the table" | Adds enormous complexity: needs editable cell templates, dirty state tracking, validation per cell type, optimistic updates vs server errors, row-level save/cancel. This is a back-office admin panel, not a spreadsheet. Edits happen in dialogs/forms. | Keep existing dialog/edit-form pattern. Click action button → open dialog. |
| **Client-side sorting/filtering** | "Faster than server calls" | Contradicts the server-side pagination contract. If we already paginate server-side, client-side sort/filter on only the current page produces incorrect results. | Server-side always. Never enable client-side mode. |
| **Virtual scrolling** | "Handles 100K rows" | Overkill for this app. Customer/Staff/Allergen datasets are likely <10K rows. Introduces CDK virtual scroll quirks (sticky header fix needed). The state management is more complex. | Standard pagination is sufficient. Add only if user testing shows performance issues with pagination. |
| **Drag-and-drop column reorder** | "Power users want custom layouts" | Adds CDK drag-drop complexity, conflicts with sticky columns, and the UI for reorder handles is tricky. Low usage in back-office admin contexts. | Simplify: use column chooser with move up/down buttons. Add drag reorder only if users specifically request it. |
| **Multi-row selection + bulk actions** | "Select 10 customers and bulk-deactivate" | No backend endpoints support bulk operations. Would need backend changes (out of scope). | Each row has individual action buttons. Add bulk operations when backend supports them. |
| **Export to Excel/CSV** | "Export filtered data" | Requires a backend export endpoint or client-side XLSX generation (adding a heavy dependency). Not mentioned in requirements. | Defer to future phase. If added, use backend-driven export (the proper way for server-filtered data). |
| **Column resize** | "Resize columns by dragging borders" | Nice-to-have but adds complexity with fixed column widths and horizontal scroll. Low priority for a back-office tool. | Use CSS `max-width` / `overflow: hidden` with `text-overflow: ellipsis`. Let content define natural widths. |
| **Nested/expandable rows** | "Show ingredient details inside the table" | Adds significant template complexity (`multiTemplateDataRows`). The detail view pattern (click → navigate/dialog) is already established. | Existing dialog/detail-page pattern. |
| **Dark mode** | "Looks modern" | Zero business value for a back-office admin tool used by staff during work hours. Adds CSS variable overhead. | Stay with default Material theme. |
| **Keyboard navigation** | "Tab through cells" | Table cells are not form controls — keyboard nav adds complexity without clear benefit in a read-first grid. Action buttons provide all needed interactivity. | Row hover + click is sufficient. |

### Entity-Specific Column Complexity

Understanding the variability across entities helps define the column config interface:

| Entity | Columns | Special Rendering | Sortable Columns |
|--------|---------|-------------------|------------------|
| Customer | businessName, email, type, contactPhone, isActive, actions | isActive → mat-chip, businessName → default text | businessName, email, type, isActive |
| Staff | username, email, role, isActive, lastLogin, actions | isActive → mat-chip, lastLogin → date pipe | username, email, role, isActive, lastLogin |
| Allergen | id, name, code, description, actions | code → mat-chip | name, code (post-refactor) |
| Ingredient | name, cost, kcal, yield, actions | cost → currency formatting, yield → percentage | name, costPer1000g, energyKcalPer100g (post-refactor) |
| Plate | name, basePrice, categoryId, description, actions | price → currency formatting | name, basePrice, categoryId (post-refactor) |

**Actions column pattern:** Every entity has an Edit and Delete action button. Some also have a View/navigate action (Plate links to detail page). The actions column must be configurable per entity.

## Feature Dependencies

```
Column Config (GridConfigProvider)
    └──requires──> ColumnDef<T> interface
                       └──requires──> Typed grid component (DataGridComponent<T>)
                                          └──requires──> Angular 21 + Material

Custom Cell Templates
    └──requires──> Content projection via NgTemplateOutlet
                       └──requires──> ColumnDef has cellRef field (TemplateRef)

Server-Side Data Flow
    └──requires──> PagedRequest + PagedResponse interfaces
    └──requires──> Grid emits (queryChange) with full params
    └──requires──> Smart component subscribes, calls API, passes data + total count back in

Filter Section
    └──requires──> Content projection slot in grid (ng-content select)
    └──requires──> Smart component owns filter state
    └──requires──> Smart component calls grid.refresh() or passes updated query

Page Reset Logic
    └──requires──> Smart component detects filter/sort change vs pagination change
    └──enhances──> Server-side data flow (correct UX)

State Persistence (v1.x)
    └──requires──> Column config + grid ID
    └──enhances──> Column visibility control
```

### Dependency Notes

- **Column Config requires ColumnDef interface:** The generic grid must declare `<T>` for row type safety. Each entity provides typed column definitions. Without this, the grid can't offer type-safe cell rendering.
- **Server-side data flow is the backbone:** The grid emits structured query events, the smart component handles the API call. This is the core architectural principle — the grid never calls the API directly.
- **Filter sections are external to the grid:** The grid provides a slot via content projection. The smart component renders entity-specific filter controls. This keeps the grid truly generic and reusable across entities.
- **Page reset is smart-component logic:** When filter or sort changes, the smart component must reset the page index before sending the query. The grid just emits the change event; the smart component decides the page value.

## MVP Definition

### Launch With (v1)

Minimum viable grid — what's needed to replace the 5 duplicated entity tables.

- [x] **Generic DataGridComponent<T>** — Reusable grid with typed column config, server-side pagination, sorting, loading/empty/error states
- [x] **ColumnDef<T> interface** — Declarative column config with `key`, `label`, `sortable`, `width`, `sticky`, `cellClass`, `displayValue`
- [x] **Custom cell templates** — Escape hatch for status chips, dates, currency via `NgTemplateOutlet`
- [x] **Actions column** — Configurable per-row action buttons (edit/delete/view) with smart-component handlers
- [x] **Server-side data flow** — Grid emits `(queryChange)` with `PagedRequest`, smart component handles API calls
- [x] **Filter section slot** — Content projection slot for entity-specific filter controls
- [x] **Standardized API service** — Unified `getAll(params: PagedRequest<TFilters>)` pattern across all entity services
- [x] **Standardized PagedRequest/PagedResponse** — Shared TypeScript interfaces for API contract
- [x] **Page reset on filter/sort change** — Smart component resets to page 1 when filters or sort column change
- [x] **Loading indicator** — `MatProgressSpinner` overlaid on table during API calls
- [x] **Error state with retry** — Error message replaces table body on API failure with retry button
- [x] **Empty state** — Centered message when data array is empty
- [x] **Consistent styling** — Grid owns all table CSS; entity pages only add filter styling
- [x] **Horizontal scroll** — Fixed columns with overflow-x for wide tables (Ingredient nutrition data)

Includes migration of **all 5 entities** (Customer, Staff, Allergen, Ingredient, Plate) to use the new grid.

### Add After Validation (v1.x)

Features to add once the core grid is stable and deployed.

- [ ] **Column chooser** — Users show/hide columns via toolbar menu. Trigger: user feedback about too many columns (especially Ingredient with 10+ nutrition fields).
- [ ] **Column order persistence** — Save column visibility/order to localStorage. Trigger: users complain they reconfigure columns every visit.
- [ ] **Page size options per table** — Support different page sizes per entity (e.g., Ingredient with many fields might default to 25 instead of 10). Trigger: performance testing reveals better UX with different defaults.

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] **Drag-and-drop column reorder** — Defer: complex to implement well with sticky columns. Only if users strongly request it.
- [ ] **State persistence (full)** — Column order, widths, visibility. Only if users spend significant time per session reconfiguring.
- [ ] **Export to CSV/Excel** — Defer: requires backend export endpoint. Not in current requirements.
- [ ] **Virtual scrolling** — Defer: Overkill for current dataset sizes. Only if pagination causes performance issues.
- [ ] **Multi-row selection + bulk actions** — Defer: requires backend bulk endpoints (out of scope).

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Grid framework + reusable component | HIGH (eliminates duplication) | MEDIUM (new component) | P1 |
| Declarative column config | HIGH (50% of refactor value) | MEDIUM (ColumnDef design) | P1 |
| Custom cell templates | HIGH (necessary for chips/currency) | MEDIUM (NgTemplateOutlet wiring) | P1 |
| Server-side pagination | HIGH (performance) | LOW (MatPaginator) | P1 |
| Column sorting | HIGH (data exploration) | LOW (MatSort) | P1 |
| Global text search | HIGH (already exists) | LOW (debounced input) | P1 |
| Loading state | MEDIUM (UX) | LOW (spinner overlay) | P1 |
| Error state with retry | MEDIUM (debugging) | LOW (conditional template) | P1 |
| Empty state | MEDIUM (UX) | LOW (matNoDataRow) | P1 |
| Filter section slot | HIGH (required for entities) | LOW (ng-content) | P1 |
| Standardized API services | HIGH (eliminates duplication) | MEDIUM (refactor 5 services) | P1 |
| Actions column | HIGH (CRUD workflow) | LOW (configurable buttons) | P1 |
| Page reset on filter/sort | MEDIUM (UX correctness) | LOW (smart component logic) | P1 |
| Horizontal scroll | MEDIUM (Ingredient table) | LOW (CSS overflow) | P1 |
| Column chooser | MEDIUM (column management) | MEDIUM (menu + persistence) | P2 |
| State persistence | LOW (quality-of-life) | MEDIUM (localStorage service) | P2 |
| Export to CSV/Excel | LOW (not requested) | HIGH (backend needed) | P3 |
| Drag-and-drop reorder | LOW (edge case) | HIGH (CDK complexity) | P3 |
| Inline editing | LOW (existing patterns work) | VERY HIGH (complex) | ANTI |
| Virtual scrolling | LOW (not needed) | HIGH (complex state) | P3 |
| Dark mode | ZERO (back-office) | MEDIUM (CSS vars) | ANTI |

**Priority key:**
- P1: Must have for launch (MVP)
- P2: Should have, add when possible (v1.x)
- P3: Nice to have, future consideration (v2+)
- ANTI: Deliberately not building (see anti-features)

## Competitor Feature Analysis

Not applicable in the usual sense — this is an internal refactor, not a market product. However, the following libraries informed the feature set:

| Feature | ngx-mat-simple-table (v1.3) | @proangular/pro-table (v21) | Our Approach |
|---------|------------------------------|------------------------------|--------------|
| Column config | `ColumnDef[]` JSON array | `TableColumn<T>` typed | `ColumnDef<T>` typed (closer to pro-table) |
| Custom templates | `cellDef` attribute directive | `TemplateRef` via column config | `TemplateRef` via column config + `NgTemplateOutlet` |
| Server-side mode | `(page)`, `(sortChange)`, `(filterChange)` events | Events + smart component pattern | Emit `PagedRequest` via `(queryChange)` — single unified event |
| Filter section | Built-in per-column dropdown filters | Not built-in (parent provides) | Content projection slot (parent owns filters) |
| State persistence | `StStateStoringDirective` | Not built-in | Defer to v1.x with localStorage service |
| Export | `StExportDirective` (XLSX via ExcelJS) | Not built-in | Defer to v2+ (backend-driven) |
| Column chooser | Built-in toolbar menu | Not built-in | Defer to v1.x |
| Virtual scroll | Config flag via CDK | Not built-in | Defer to v2+ (not needed now) |
| Complexity | High (many features = larger bundle) | Medium (focused on essentials) | **Lean** — build only what the 5 entities need |

## Smart Component Responsibilities

Each entity page using the grid follows this pattern:

```typescript
// Smart component (entity page)
@Component({
  template: `
    <div class="page-filters">
      <!-- Entity-specific filter controls -->
      <mat-form-field>
        <mat-label>Type</mat-label>
        <input matInput (input)="onFilterChange({ type: $event.target.value })">
      </mat-form-field>
      <!-- etc... -->
    </div>

    <data-grid
      [columns]="columnDefs"
      [data]="dataSignal()"
      [totalCount]="totalCountSignal()"
      [loading]="loadingSignal()"
      [error]="errorSignal()"
      [page]="currentPage()"
      [pageSize]="currentPageSize()"
      (queryChange)="onGridQuery($event)"
      (action)="onRowAction($event)"
    >
      <!-- Custom cell templates -->
      <ng-template #statusCell let-row>
        <mat-chip [color]="row.isActive ? 'primary' : 'warn'" selected>
          {{ row.isActive ? 'Attivo' : 'Inattivo' }}
        </mat-chip>
      </ng-template>
    </data-grid>
  `
})
export class CustomerListComponent {
  // Owns: filter state, API calls, query orchestration
  // Grid: receives data, emits events, renders
}
```

## Sources

- **Source code audit:** 5 existing entity components + 5 services — `.planning/PROJECT.md`, `src/app/features/dashboard/*/`, `src/app/core/services/*/`
- **ngx-mat-simple-table (v1.3):** https://github.com/xonaib/ng-simple-table — Declarative Angular Material table library, MIT license. Researched for column config patterns, server-side mode, and feature scope. **HIGH confidence.**
- **@proangular/pro-table (v21):** https://github.com/ProAngular/pro-table — Type-safe Angular Material table abstraction. Researched for typed columns, expandable rows, smart pattern. **HIGH confidence.**
- **ngx-mat-table-toolkit:** https://github.com/admcfarland/ngx-mat-table-toolkit — Client/server paginators, column flattening, row action configs. **MEDIUM confidence** (less actively maintained).
- **ngxsmk-datatable:** https://www.npmjs.com/package/ngxsmk-datatable — Feature-rich datatable. Researched for feature checklist validation. **LOW confidence** (smaller community).
- **AG Grid Angular:** https://www.ag-grid.com/angular-data-grid/ — Industry standard but third-party dependency. Used as feature reference only (not adopting). **HIGH confidence** for feature checklist.
- **Kendo UI Grid:** https://www.telerik.com/kendo-angular-ui/components/grid — Enterprise Angular grid. Used as feature checklist reference. **HIGH confidence.**
- **DEV Community articles:**
  - "Building a reusable and configurable table with Angular Material" (2023) — Architectural pattern for generic mat-table wrapper. **MEDIUM confidence** (dated but relevant pattern).
  - "Reusable Angular Material Table in Angular 20" by Andreea Magdici (2025) — NgTemplateOutlet pattern for custom cells. **HIGH confidence.**
  - "Building your first generic Angular component" (2025) — Smart/dumb component guidance. **MEDIUM confidence.**
  - "I got tired of rebuilding the same Angular table..." by Zonaib Bokhari (2026) — First-hand library author experience on what features matter. **HIGH confidence.**

---

*Feature research for: Roscoff reusable data grid + API service layer*
*Researched: 2026-05-29*
