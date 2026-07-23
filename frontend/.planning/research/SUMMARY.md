# Project Research Summary

**Project:** Roscoff — Reusable Data Grid + Standardized API Service Layer
**Domain:** Angular 21 back-office admin panel for restaurant management
**Researched:** 2026-05-29
**Confidence:** HIGH

## Executive Summary

This project refactors 5 duplicated entity-table implementations (Staff, Customer, Allergen, Ingredient, Plate) into a single reusable `<app-data-grid>` component with a standardized API service layer. The app runs on Angular 21.2 + Angular Material 21.2.11 + RxJS 7.8 with standalone components and signals. The core architectural decision: **use plain arrays with `[dataSource]="data"` for the Material table, NOT `MatTableDataSource`** — which is designed for client-side data and causes signal-sync problems that already plague the codebase. All pagination, sorting, and filtering will be server-driven through a unified `PagedQuery`/`PagedResponse` contract.

The recommended approach is a **smart/dumb component split**: entity page components own all state, API calls, and CRUD dialogs, while the grid is a pure presentational component receiving data via `@Input()` and emitting events via `@Output()`. Feature-wise, the v1 focuses on table-stakes (server pagination, sorting, search, loading/empty/error states) plus a config-driven column system with content projection for entity-specific cell rendering. Five entities are migrated one at a time, starting with Customer (most complex filters) to prove the pattern.

**Key risks:** (1) **Over-abstraction** — the grid accumulating configuration flags for every entity edge case; mitigate with a minimal initial API and the "three-strike rule." (2) **Type safety collapse** — the `any` contagion (47+ existing instances) spreading into the generic component; mitigate by enforcing `keyof T` on `ColumnDef` and banning `EventEmitter<any>`. (3) **Big-bang migration** — swapping all entities at once in an untested codebase; mitigate by migrating one entity per commit with a smoke-test checklist. (4) **Pagination fragmentation** — the app already mixes 0-based and 1-based page indexing; mitigate by standardizing on 1-based in the query interface and converting at the smart-component boundary.

## Key Findings

### Recommended Stack

All technologies are **already in the project** — no new dependencies required. The refactoring is pure TypeScript/HTML/CSS within the existing framework.

**Core technologies:**
- **Angular Material `MatTable`** (`^21.2.11`) — Grid rendering engine. Best-in-class Angular table. Use plain array `[dataSource]="data"`, NOT `MatTableDataSource`.
- **`MatSort`** (`^21.2.11`) — Column sorting UI. Emits `Sort {active, direction}` that maps directly to API `SortColumn`/`SortDirection`.
- **`MatPaginator`** (`^21.2.11`) — Pagination controls. Use `(page)="onPage($event)"` with manual event handling.
- **RxJS `switchMap`** (`~7.8.0`) — Reactive request pipeline. Canonical pattern: `merge(sortChange$, page$).pipe(switchMap(() => apiCall$))`.
- **Angular Signals** (`^21.2.0`) — Component-local reactive state for data arrays, loading state, total count in smart components.
- **TypeScript Generics** (`~5.9.2`) — Type-safe grid via `DataGridComponent<T>` with `ColumnDef<T>[]` inputs constrained by `keyof T`.
- **`buildPagedParams(query)` utility** — New utility converting typed query → `HttpParams`. Eliminates the 7-line `if`-chain duplicated across every service.

**No new libraries required.** The existing Angular Material + RxJS + Signals stack fully covers this feature.

### Expected Features

**Must have (table stakes):**
- **Server-side pagination** — `MatPaginator` with `Page`/`PageSize` sent to API; page resets to 1 on filter/sort change
- **Column sorting** — `MatSort` emitting `SortColumn` + `SortDirection` to API
- **Global text search** — Debounced input (300ms) mapped to `Search` query parameter. Already exists in Customer/Staff but needs standardization
- **Loading state** — `MatProgressSpinner` overlay on table body during API calls (currently missing in ALL entities)
- **Empty state** — Centered "No records found" message using `*matNoDataRow` (partially implemented)
- **Error state with retry** — Error banner replacing table body on API failure (currently most errors go to `console.error`)
- **Per-row actions** — Configurable edit/delete/view icon buttons. Every entity has these
- **Horizontal scroll** — Fixed `overflow-x: auto` for wide tables (Ingredient has 10+ nutritional columns)
- **Page size selector** — Configurable `pageSizeOptions` per grid (10/25/50/100 default)
- **Filter section slot** — Entity-specific filter controls rendered via `ng-content` above the grid; grid stays generic

**Should have (differentiators):**
- **Declarative column config** (`ColumnDef<T>[]`) — Replaces ~50 lines of per-entity `ng-container matColumnDef` boilerplate with a typed array. This is the entire point of the refactor.
- **Custom cell templates** — `NgTemplateOutlet` escape hatch for status chips, formatted dates, currency, links. Smart component provides `TemplateRef` per column
- **Unified query contract** — Grid emits `(queryChange)` with structured `PagedRequest`. Smart component subscribes, calls API, passes results back. Single source of truth for all grid state
- **Page reset on filter/sort change** — Changing filters or sort column automatically resets to page 1
- **Confirmation dialog for destructive actions** — Replace `confirm()` with `MatDialog` confirmation

**Defer (v1.x / v2+):**
- Column chooser (show/hide columns) — v1.x
- Column order/visibility persistence (localStorage) — v1.x
- Export to CSV/Excel — v2+ (backend-driven)
- Drag-and-drop column reorder — v2+
- Virtual scrolling — v2+ (not needed for current dataset sizes)
- Multi-row selection + bulk actions — v2+ (requires backend bulk endpoints)
- Inline editing — **ANTI-FEATURE** (deliberately not building: adds enormous complexity, edits belong in dialogs)

### Architecture Approach

**Three-layer smart/dumb separation:** Smart page components (containers) own all state, services, and CRUD logic. The `DataGridComponent<T>` is a pure presentational component that receives data and emits events. The API service layer provides standardized `getPaged()` methods returning typed `Observable<PagedResponse<T>>`. All data flows DOWN via `@Input()`, all events flow UP via `@Output()`.

**Major components:**
1. **`DataGridComponent<T>`** — Dumb grid. Renders `<mat-table>` with `<mat-paginator>` + sort headers. Inputs: `columns: ColumnDef<T>[]`, `data: T[]`, `totalItems: number`, `loading: boolean`. Outputs: `sortChange`, `pageChange`. Never injects services. Standalone, `OnPush` change detection.
2. **Smart Page Components** (Staff, Customer, Plate, Ingredient, Allergen) — Own `BehaviorSubject<TQueryParams>`, inject entity service, manage CRUD dialogs. Wire up `switchMap` pipeline. Pass data to grid via signals.
3. **Entity Services** — Add `getPaged(query: TQueryParams)` method returning `Observable<ApiResponse<PagedResponse<T>>>`. Use `buildHttpParams()` utility to avoid `if`-chain repetition.
4. **Type System** — `BaseQueryParams` (page, pageSize, search, sortColumn, sortDirection, dateFrom, dateTo) + entity-specific extensions (e.g., `StaffQueryParams extends BaseQueryParams { email?, role? }`).
5. **`buildHttpParams(query)` utility** — Single function converting typed query → `HttpParams`. Skips null/undefined/empty values.

**Key patterns:**
- Smart component: `BehaviorSubject` → `switchMap` → service → signals → grid inputs
- Grid is generic: `DataGridComponent<T extends Record<string, any>>` with `ColumnDef<T>.field: keyof T`
- Cell rendering via config-driven `cellType: 'text' | 'chip' | 'date' | 'currency' | 'boolean' | 'actions'`
- Action column via `actionsTemplate` `@Input()` with `TemplateRef` + row context (smart component provides buttons)

### Critical Pitfalls

1. **Over-generic grid (premature abstraction death spiral)** — Grid accumulates 20+ `@Input()` properties for every entity's edge case, becoming more complex than the 5 duplicated tables. **Prevention:** Start minimal, add features via composition (content projection), apply "three-strike rule" (if 3+ entities need it, then make it generic). Keep `@Input()` count ≤ 8.

2. **Type safety collapse (`any` contagion)** — Generic `T` is lost somewhere in the chain and replaced with `any`, defeating TypeScript strict mode. The codebase already has 47+ `any` instances. **Prevention:** Define `ColumnDef<T>.field: keyof T` before writing the component. Ban `EventEmitter<any>` in the grid. Enforce `Observable<PagedResponse<T>>` return types — never `Observable<any>`.

3. **MatTableDataSource ↔ Signal sync loop** — The existing pattern syncing signals to `MatTableDataSource` via `effect()` creates `NG0600` errors, stale data, and circular updates. **Prevention:** Do NOT use `MatTableDataSource` at all. Use plain array `[dataSource]="data()"` with manual event handling. Or if using it, assign `.data` directly in subscription, never in `effect()`.

4. **Smart/dumb boundary erosion** — Grid starts importing services, routing, dialogs for convenience, becoming untestable and unreusable. **Prevention:** Enforce strict `@Input()`/`@Output()`-only rule. Grid must never inject services, manage loading state, or format data. Test the boundary with a unit test.

5. **Big-bang migration** — Swapping all 5 entities in one commit creates an unreviewable diff with cascading failures. No test coverage to catch regressions. **Prevention:** Migrate one entity per commit. Start with Customer (most complex). Keep old component as fallback. Create smoke-test checklist per entity.

6. **API response wrapping inconsistency** — StaffService expects `{ succeeded, data: { items, totalCount } }`, CustomerService returns raw `Observable<any>`. Standardizing without central unwrapping breaks silently. **Prevention:** Create a central HTTP interceptor or utility that unwraps all responses to `PagedResponse<T>` before services see them. Test every endpoint against the standard shape.

7. **Pagination state fragmentation** — Staff uses 0-based page indexing, Customer uses 1-based. Passing `MatPaginator.pageIndex` directly to API shows wrong data. **Prevention:** Standardize query interface on 1-based (matching backend). Convert at the smart component boundary: `page = event.pageIndex + 1`. Document the convention.

8. **Subscription leak cascade** — `BehaviorSubject` + `switchMap` subscriptions in smart components without cleanup accumulate on every navigation. **Prevention:** Mandate `takeUntilDestroyed()` on all subscriptions. Or use `toSignal()` to convert observables to signals (auto-cleanup). Enforce via code review.

## Implications for Roadmap

Based on combined research, the following phase structure minimizes risk by building infrastructure first, proving the pattern with one entity, then migrating the rest:

### Phase 1: Type System & Utilities (Infrastructure)
**Rationale:** Pure additions with zero breaking changes. Establishes the contracts every other phase depends on. No behavioral changes to existing code.
**Delivers:**
- `BaseQueryParams` interface + entity-specific extensions (`StaffQueryParams`, `CustomerQueryParams`, etc.) in `api.models.ts`
- `PagedResponse<T>` — already partially defined, ensure universal adoption
- `buildHttpParams(query): HttpParams` utility in `core/utils/http-params.util.ts`
- `ColumnDef<T>`, `SortChange`, `PageChange` interfaces in `shared/data-grid/data-grid.models.ts`
- `GridAction<T>` and `CellType` types for config-driven rendering
**Addresses:** FEATURES.md P1 — standardized API contract
**Avoids:** PITFALLS.md #6 (API response inconsistency), #7 (pagination fragmentation) — both solved at the type level before implementation
**Research flag:** **Standard patterns** — well-documented TypeScript interfaces. No research needed.

### Phase 2: DataGridComponent<T> (Dumb Grid)
**Rationale:** Build the grid in isolation before any entity depends on it. Can be tested with mock data. Establishes the component API that all smart pages consume.
**Delivers:**
- `DataGridComponent<T>` standalone component with `OnPush` change detection
- `MatTable` with `[dataSource]="data()"` (plain array, NOT `MatTableDataSource`)
- `MatSort` with `(matSortChange)` events emitted to parent
- `MatPaginator` with `(page)` events (0-based, converted at parent boundary)
- Loading state via `[loading]` input + `MatProgressSpinner`
- Empty state via `@if`/`*matNoDataRow`
- Error state via `[error]` input + error banner with retry button
- Config-driven column rendering with cell type support (text, chip, date, currency, boolean, actions)
- `actionsTemplate` `@Input()` for per-row action buttons via `TemplateRef` + row context
**Addresses:** FEATURES.md P1 — grid framework, declarative column config, custom cell templates, loading/empty/error states, horizontal scroll
**Avoids:** PITFALLS.md #1 (over-generic grid — start minimal, ≤8 inputs), #2 (type safety collapse — `keyof T` enforced), #3 (signal sync loop — no `MatTableDataSource`), #4 (boundary erosion — strict input/output only)
**Research flag:** **Standard patterns** — Angular Material docs + existing codebase patterns cover this. Skip research-phase.

### Phase 3: Service Standardization + Staff/Customer Migration (Proof of Pattern)
**Rationale:** Staff has the most complex filters and action buttons. If the grid handles Staff well, it handles everything. Proving the end-to-end pattern (service → smart component → grid) with one entity catches integration issues early. Customer is the second most complex and has the dual-CRUD pattern.
**Delivers:**
- `StaffService.getPaged(StaffQueryParams)` standardized method using `buildHttpParams()`
- `CustomerService.getPaged(CustomerQueryParams)` standardized method
- Central API response unwrapping (interceptor or utility) normalizing all responses to `PagedResponse<T>`
- `StaffPageComponent` smart component: `BehaviorSubject` + `switchMap` + signals + filter section + grid composition
- `CustomerPageComponent` smart component: same pattern
- Staff and Customer migrated to use `<app-data-grid>`, old components deprecated
**Addresses:** FEATURES.md P1 — standardized API services, server-side data flow, filter section slot, page reset on filter/sort change, actions column
**Avoids:** PITFALLS.md #5 (subscription leaks — enforce `takeUntilDestroyed()`), #6 (API double-wrap — central unwrapping), #7 (pagination fragmentation — 1-based standard, convert at boundary), #8 (big-bang — only 2 entities)
**Research flag:** **Needs research** — exact API response shapes for Staff and Customer endpoints must be verified. Backend may have slight variations in response wrapping. Use `/gsd-research-phase` to confirm each endpoint returns `{ succeeded, data: { items, totalCount, ... } }`.

### Phase 4: Remaining Entity Migrations (Allergen, Ingredient, Plate)
**Rationale:** Remaining entities are simpler than Staff/Customer. Allergen and Ingredient currently load all data in memory (no pagination) — switching to server-paginated requires verifying backend pagination support. Plate currently uses mock data — may need backend coordination.
**Delivers:**
- `AllergenService.getPaged()`, `IngredientService.getPaged()`, `PlateService.getPaged()` standardized methods
- `AllergenPageComponent`, `IngredientPageComponent`, `PlatePageComponent` smart components
- All 3 entities migrated to `<app-data-grid>`, old components deprecated
- Ingredient range-filter pairs (`MinEnergyKcal`/`MaxEnergyKcal`) handled as paired inputs in the filter section
**Addresses:** FEATURES.md P1 — complete migration of all 5 entities
**Avoids:** PITFALLS.md #8 (big-bang — one entity per commit within this phase)
**Research flag:** **Needs research** — Allergen/Ingredient currently lack server pagination. Need to verify backend pagination endpoints exist and match `PagedQuery` contract. Plate uses mock data — verify real endpoint status. Use `/gsd-research-phase` per entity before migration.

### Phase 5: Cleanup & Polish (v1.x Features)
**Rationale:** After all entities are migrated and stable, remove old code and add quality-of-life features.
**Delivers:**
- Delete old `staff.component.ts`, `customer.component.ts`, etc. and associated HTML/CSS files
- Roadmap audit: run `rg 'Observable<any>'` to verify `any` count decreased
- Column chooser (show/hide columns) via toolbar menu
- Column visibility persistence to `localStorage` keyed by `tableId`
- Optional: different page size defaults per entity
**Addresses:** FEATURES.md P2 — column chooser, state persistence
**Avoids:** PITFALLS.md #1 (if grid API needs extension, do it here based on real usage, not speculation)
**Research flag:** **Standard patterns** — localStorage persistence is well-documented. Column chooser can reference `ngx-mat-simple-table` patterns. Skip research-phase.

### Phase Ordering Rationale

- **Infrastructure-first (Phase 1 → 2):** Types and the grid component have zero dependencies on entity specifics. Building them first avoids circular dependencies during migration and establishes the contracts that all smart pages rely on.
- **One entity proves the pattern (Phase 3):** Customer has the most complex filter requirements (Type dropdown + IsActive toggle). If the grid handles Customer, it handles everything. This phase catches integration issues with API response shapes, pagination indexing, and filter state management before the remaining entities are migrated.
- **Remaining entities in parallel-safe sequence (Phase 4):** Allergen, Ingredient, and Plate are less complex than Customer/Staff but have unique quirks (range filters, mock data). Each is migrated independently with its own commit. If one entity's backend endpoint has a different response shape, it doesn't block the others.
- **Polish after stability (Phase 5):** Column chooser and state persistence are premature optimizations before the core grid is stable across all entities. Adding them after all entities are migrated ensures the column config API is settled.
- **Anti-features never built:** Inline editing, drag-drop reorder, virtual scrolling, bulk actions, and dark mode are deliberately excluded (see anti-features in FEATURES.md). The roadmap should not include them.

### Research Flags

**Phases needing deeper research during planning:**
- **Phase 3 (Service + Staff/Customer migration):** Verify exact API response shapes for Staff and Customer endpoints. Confirm `PagedResponse<T>` unwrapping works for both. Audit existing pagination indexing (Staff 0-based vs Customer 1-based) to determine conversion rules.
- **Phase 4 (Allergen, Ingredient, Plate):** Allergen and Ingredient currently load-all-in-memory — verify backend pagination endpoints exist. Plate uses mock data — confirm real API endpoint availability and response shape. Ingredient range filters (MinEnergyKcal/MaxEnergyKcal) need backend verification.

**Phases with standard patterns (skip research-phase):**
- **Phase 1 (Type definitions):** Straightforward TypeScript interfaces. Well-documented pattern.
- **Phase 2 (DataGrid Component):** Angular Material docs + community sources fully cover this. No unknowns.
- **Phase 5 (Cleanup & Polish):** localStorage persistence and column chooser are standard frontend patterns.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All technologies verified in existing codebase (`package.json`, imports). Angular Material server-side patterns confirmed with `TableHttpExample`. No new libraries needed. |
| Features | HIGH | Source code audit of all 5 entity components confirmed current state. Production libraries (ngx-mat-simple-table, pro-table, Kendo UI, AG Grid) used as feature references. Community articles validate patterns. |
| Architecture | HIGH | Smart/dumb pattern validated across multiple sources. Type system design (BaseQueryParams hierarchy) directly addresses existing inconsistencies. Build order derived from working patterns in Staff/Customer. |
| Pitfalls | HIGH | Codebase audit (CONCERNS.md, CONVENTIONS.md, PROJECT.md) provided concrete evidence for each pitfall. Community articles on generic component traps and smart/dumb erosion cross-validate. Recovery strategies are specific and testable. |

**Overall confidence:** HIGH — all research areas have strong, cross-validated sources. The main uncertainties are backend API response shapes (which will be verified during Phase 3/4) and the exact complexity of Ingredient range-filter handling (low risk, addressable in the filter section).

### Gaps to Address

- **Backend API response shapes per entity:** `PROJECT.md` states "backend has been updated to standardize paginated GET endpoints," but this must be verified per entity. During Phase 3, test each endpoint's response against `PagedResponse<T>`. If endpoints differ, create per-entity mapping in the central unwrapping layer.
- **Backend-driven debounce responsibility:** The current pattern relies on backend handling of rapid queries. Frontend should add `debounceTime(300)` + `distinctUntilChanged()` for search input regardless. Confirm backend doesn't have its own debounce that conflicts.
- **No test coverage:** The codebase has near-zero test coverage (CONCERNS.md). Manual smoke-test checklists per entity are essential during migration. Consider adding a compile-time type-safety test for the grid component.
- **Existing `PagedResponse<T>` usage:** `api.models.ts` already defines `PagedResponse<T>` (line 175) but it's not used by services. Verify the interface matches actual API responses before enforcing it. If backend returns a different `totalCount`/`totalItems` naming, adjust.
- **Date handling (Z-suffix normalization):** `StaffService` applies date Z-suffix normalization that other services don't. Determine if this is a backend issue or if all date params need normalization in `buildHttpParams()`.
- **Exact null handling for boolean filters:** Customer's `IsActive` filter sends `null` when not set. Backend must interpret this as "no filter" vs "filter isActive=false". Define the contract: for tri-state booleans (true/false/null), omit the param entirely when null.

## Sources

### Primary (HIGH confidence)
- **Angular Material official docs** (material.angular.dev) — MatTable, MatSort, MatPaginator, server-side `TableHttpExample` pattern. Production documentation with verified code examples.
- **Existing codebase audit** — All 5 entity components in `src/app/features/dashboard/*/`, 5 services in `src/app/core/services/*/`, `shared/models/api.models.ts`, `CONCERNS.md`, `CONVENTIONS.md`, `PROJECT.md`. Direct observation of every file referenced.
- **Angular Signals / Components API** — `input()`, `output()`, `signal()`, `ChangeDetectionStrategy.OnPush` from official Angular docs.

### Secondary (MEDIUM confidence)
- **ngx-mat-simple-table (v1.3)** — https://github.com/xonaib/ng-simple-table — Column config patterns, server-side mode, feature scope reference.
- **@proangular/pro-table (v21)** — https://github.com/ProAngular/pro-table — Typed column patterns, smart/dumb separation.
- **Community articles:**
  - "Building a reusable and configurable table with Angular Material" (2023) — Grid architecture pattern
  - "Reusable Angular Material Table in Angular 20" by Andreea Magdici (2025) — `NgTemplateOutlet` custom cells
  - "I got tired of rebuilding the same Angular table..." by Zonaib Bokhari (2026) — Library author experience
  - "Hidden Cost of Reusable Components" by Raj Chhatrala (2026) — Premature abstraction warnings
  - "Smart vs Dumb Component Mistakes That Quietly Destroy Angular Apps" (2026) — Boundary erosion
- **ANG Grid Angular / Kendo UI Grid** — Feature checklist reference (not adopting third-party libraries).

### Tertiary (LOW confidence)
- **DEV Community / Medium articles** on generic Angular table components — Used for pattern validation only. All patterns cross-checked against official docs.
- **Stack Overflow** — Signal + MatTableDataSource sync issues (2023-2024) — Confirmed the `effect()` + `untracked()` anti-pattern exists in the codebase.

---

*Research completed: 2026-05-29*
*Ready for roadmap: yes*
