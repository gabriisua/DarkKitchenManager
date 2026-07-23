# Pitfalls: Angular Reusable Data Grid & API Service Standardization

**Domain:** Angular 21 back-office app — introducing a reusable data grid component and standardizing API service patterns (per-entity → shared pagination contract)

**Researched:** 2026-05-29

**Confidence:** HIGH (verified against codebase audit + community patterns)

---

## Critical Pitfalls

### Pitfall 1: The Over-Generic Grid — Premature Abstraction Death Spiral

**What goes wrong:**
The reusable grid component starts simple but accumulates configuration flags, slot templates, and conditional behaviors for every entity's edge case. After 3-4 rounds of "can we just add one more `@Input()`", the grid becomes a 20-param configuration monster that nobody understands. Adding a new feature to one entity's table requires understanding all the confusing interaction of all those knobs. The component is now *more* complex than the 5 duplicated tables it replaced.

**Why it happens:**
The 5 entities (Allergen, Customer, Ingredient, Plate, Staff) have genuinely different column layouts, action buttons, filter configurations, and cell types. The developer wants a single grid to handle all of them. Each entity has "just one small difference" — and those differences accumulate. Because the project currently lacks tests (CONCERNS.md: near-zero test coverage), there's no safety net to detect when a change for Allergen breaks Staff.

**How to avoid:**
- **Start with a minimal grid** that handles only the common denominator: pagination controls, column rendering via column config, sort indicators. No cell templates, no action buttons, no special formatting.
- **Add features via composition, not configuration.** For entity-specific cell rendering (e.g., a "status" badge for Staff, a "cost" formatting for Ingredient), use Angular `ngTemplateOutlet` with named slots rather than adding `@Input()` properties for every rendering variant.
- **Establish the "three-strike rule":** If a third entity needs a special behavior, only then consider making it a generic feature. Two entities can tolerate a small duplication.
- **Ban `@Input() cellConfig: any`** — use typed column definition interfaces with discriminated unions for known cell types (text, badge, date, currency, action).

**Warning signs:**
- The grid's `@Input()` count exceeds 8 properties
- Column config interface has a `type: string` field with 6+ hardcoded string values
- Template contains 10+ `*ngIf`/`@if` branches for rendering variants
- Adding a new entity's table requires reading 500 lines of grid component code
- A column config object starts having optional fields like `actionButtonConfig`, `badgeColorMap`, `dateFormat`, `linkRouterLink`

**Phase to address:**
Grid design phase (GRID-01/GRID-02). The grid's public API (inputs, outputs, column config interface) must be reviewed for scope creep *before* the first entity migration. After VIEW-01 (Customer), review whether the column config needs any extensions. Apply the rule: "Would at least 3 entities use this?"

---

### Pitfall 2: Type Safety Collapse — The `any` Contagion

**What goes wrong:**
The generic grid component is typed as `DataGridComponent<T>`, but somewhere in the chain — column definition interface, cell rendering, action emission, or filter propagation — the generic type parameter `T` is lost and replaced with `any`. This defeats TypeScript strict mode entirely. The 47+ existing `any` instances in the codebase (CONCERNS.md) act as seed points for this contagion. The grid component *looks* typed but provides zero compile-time safety.

**Why it happens:**
- Angular's template type checking can't always infer generic component types correctly in templates (especially with `ngTemplateOutlet`)
- Action buttons need to emit the row type, but it's easier to write `EventEmitter<any>` than to propagate the generic
- Cell value accessors like `row[column.key]` lose type information
- The existing codebase already normalizes `any` — it's the path of least resistance

**How to avoid:**
- **Define a concrete `ColumnDefinition<T>` interface** before writing the component. The `field` property must be `keyof T`, not `string`. The `cell` accessor must return `T[keyof T]`, not `any`.
  ```typescript
  export interface ColumnDefinition<T> {
    field: keyof T;
    header: string;
    sortable?: boolean;
    pipe?: ValuePipe; // typed transform, not a function returning any
  }
  ```
- **Use Angular's generic component inference.** Angular can infer `T` from `@Input() data: T[]`. Let the input type drive all other types:
  ```typescript
  export class DataGridComponent<T> {
    @Input() data: T[] = [];
    @Input() columns: ColumnDefinition<T>[] = [];
    @Output() rowAction = new EventEmitter<{ action: string; row: T }>();
  }
  ```
- **Ban `EventEmitter<any>`** in the grid component. The `rowAction` output must maintain the generic type.
- **Use `ngTemplateOutlet` with typed context** for custom cell templates:
  ```html
  <ng-template [ngTemplateOutlet]="col.template" [ngTemplateOutletContext]="{ $implicit: row }" />
  ```
- **Add a compile-time check:** Write a dummy usage of the grid in a test file that would fail to compile if generics are lost.

**Warning signs:**
- The grid component has `@Output() onRowClick = new EventEmitter<any>()`
- Column config has `cellValue?: (row: any) => string` instead of `cellValue?: (row: T) => T[keyof T]`
- Entity pages pass data to the grid with `as any` casts
- Service layer still returns `Observable<any>` after the refactoring
- TypeScript strict mode is enabled but the codebase has growing `any` usage

**Phase to address:**
Should be enforced during GRID-01 (grid component creation) and SVC-01/SVC-02 (API service standardization). The `ColumnDefinition<T>` interface must be type-safe from day one. The standardized `PagedResponse<T>` already exists in `api.models.ts` (line 175) — ensure every service returns `Observable<PagedResponse<T>>`, not `Observable<any>`.

---

### Pitfall 3: MatTableDataSource ↔ Signal Sync Loop (The `effect()` Trap)

**What goes wrong:**
The existing codebase uses signals for state and syncs them to `MatTableDataSource` via `effect()`. This creates subtle bugs:
1. **`NG0600` error** — writing to `dataSource.data` inside an `effect()` triggers a warning/error because Angular treats it as writing to a signal inside a reactive context
2. **Circular updates** — if the grid emits a page change event that updates the signal, which triggers the effect, which updates the data source, which triggers change detection, which re-emits...
3. **Stale data** — using `untracked()` to suppress the error can cause the data source to show stale data if changes are missed
4. **Sort/pagination reset** — when data is reassigned to the data source, Material table loses sort state and paginator position

**Why it happens:**
`MatTableDataSource` is not a signal-based API. Syncing signal state to it requires imperative code. The natural Angular reactive approach (`effect()`) fights against Material's design. Developers reach for `effect()` because the codebase already uses this pattern (CONVENTIONS.md: "effect() used to sync signals to MatTableDataSource"), but the Angular team explicitly warns against writing to signals inside effects without `allowSignalWrites: true`.

**How to avoid:**
- **Do NOT sync signals to `MatTableDataSource` via `effect()`.** Instead, set `dataSource.data` directly in the subscription callback or the signal's update handler.
  ```typescript
  // ❌ BAD: effect with untracked hack
  effect(() => {
    untracked(() => { this.dataSource.data = this.data(); });
  });

  // ✅ GOOD: Direct assignment in subscription
  this.service.query(params).subscribe(res => {
    this.dataSource.data = res.items;
    this.totalItems = res.totalCount;
  });
  ```
- **Or skip `MatTableDataSource` entirely.** Use the native `<table mat-table>` with `@for` or `*ngFor` on rows and manual sort/paginator binding. This avoids the data-source-sync problem entirely and is simpler.
- **If using `MatTableDataSource`, create it once and reassign `.data` directly** — never create a new `MatTableDataSource` instance on each data load (that kills the paginator/sort bindings).
- **Preserve paginator state across data loads** by reading `paginator.pageIndex` before reassigning data and restoring it after.

**Warning signs:**
- Effect blocks with `untracked(() => { this.dataSource.data = ... })` appearing in components
- `allowSignalWrites: true` in effect options
- Users report table jumping to page 1 after a sort change
- Paginator shows wrong page after data refresh
- Intermittent `NG0600` errors in console

**Phase to address:**
GRID-02 (grid implementation) and VIEW-01/VIEW-02/VIEW-03 (entity migrations). The data binding strategy must be decided and documented *before* the first entity view is migrated.

---

### Pitfall 4: Smart/Dumb Component Boundary Erosion

**What goes wrong:**
The grid component starts as a pure "dumb" component (receives data via `@Input()`, emits events via `@Output()`), but over time it accumulates logic: it starts computing derived values, formatting data, handling loading states, managing local UI state, and eventually making API calls. Once the grid imports a service or calls `HttpClient`, the separation is broken. The component is now both a smart page *and* a reusable grid — impossible to test, impossible to reuse.

**Why it happens:**
- It's convenient — "I'll just add a loading spinner input" becomes "I'll handle the loading state inside the grid"
- Entity-specific formatting (date format, currency symbol) seems harmless but couples the grid to business logic
- Action buttons in table rows need confirmation dialogs or navigation — easier to handle inside the grid than to emit and let the parent handle it
- The codebase has no pattern for "this cell content needs a click handler that opens a dialog"

**How to avoid:**
- **Strict `@Input()` / `@Output()` only rule:** The grid component must never:
  - Inject a service (including `HttpClient`, `Router`, `MatDialog`)
  - Import router-related modules
  - Manage loading/error state (receive it as input)
  - Format data (receive pre-formatted data or pipe references)
- **Use content projection (`<ng-content>`) or templating slots for action columns**, not configuration objects. Action buttons belong to the parent page, configured via template:
  ```html
  <!-- Parent page template -->
  <app-data-grid [data]="customers()" [columns]="columns" (pageChange)="onPageChange($event)">
    <ng-template let-row>
      <button mat-icon-button (click)="editCustomer(row)"><mat-icon>edit</mat-icon></button>
    </ng-template>
  </app-data-grid>
  ```
- **Test the boundary:** Write a unit test that instantiates the grid with mock inputs and asserts it only emits events (doesn't call services, doesn't navigate, doesn't open dialogs).
- **The grid should not know about entity types.** If you see `customer.name` or `staff.email` anywhere inside the grid component code, the boundary is breached.

**Warning signs:**
- Grid component file has `inject()` calls for any service
- Grid template has `(click)="router.navigate(...)` or `(click)="dialog.open(...)"`
- Grid component manages a `loading` signal or `error` signal internally
- Adding a new entity's table requires modifying the grid component code
- Grid has `@Input() loading: boolean` — this is actually OK (it's a state input from parent), but if it also has internal loading logic, that's the erosion

**Phase to address:**
ARCH-01 (smart/dumb architecture decision) must define the boundary contract. Enforce the rule during GRID-01 implementation. Re-check during each VIEW migration.

---

### Pitfall 5: API Response Wrapping Inconsistency — The Double Wrap Trap

**What goes wrong:**
Current services each handle API response wrapping differently. The `StaffService` (`staff.service.ts` lines 29-45) expects a `{ succeeded, data: { items, totalCount } }` wrapper and maps through items. The `CustomerService` (`customer.service.ts` line 28) returns raw `Observable<any>`. When standardizing, introducing a generic `PagedResponse<T>` wrapper that all services return, existing consumers break if any entity's backend endpoint returns a different shape. The grid component receives `PagedResponse<T>` but some endpoints return a subtly different wrapper.

**Why it happens:**
The backend was updated to standardize endpoints (PROJECT.md: "backend has been updated to standardize paginated GET endpoints"), but each entity's endpoint may have slight variations in response shape. The frontend `PagedResponse<T>` interface (`api.models.ts` line 175) assumes `{ items, totalCount, page, pageSize, totalPages }`, but the backend might wrap this in `{ data: { items, ... } }` or `{ succeeded: true, data: { items, ... } }`. If the standardization happens in the service layer but the response mapping is inconsistent, the grid receives undefined/hijacked data.

**How to avoid:**
- **Centralize response unwrapping in a single place.** Create an interceptor or a utility function that maps ALL API responses to the standardized `PagedResponse<T>` shape. Do NOT do it per-service.
  ```typescript
  // api-response.interceptor.ts
  export function apiResponseInterceptor() {
    // Unwraps { data: { items, totalCount, ... } } → PagedResponse<T>
    // Or { succeeded: true, data: { items, ... } } → PagedResponse<T>
    // Normalizes to one shape before any service sees it
  }
  ```
- **Define a `PaginatedQueryParams` interface** shared by all services, and a single generic `getPaged<T>(endpoint, params)` method that handles parameter building and response unwrapping. Never let each service build `HttpParams` manually.
  ```typescript
  // base-api.service.ts
  getPaged<T>(endpoint: string, params: PaginatedQueryParams): Observable<PagedResponse<T>> {
    let httpParams = new HttpParams()
      .set('Page', params.page)
      .set('PageSize', params.pageSize);
    // ... standard params
    return this.http.get(`${this.apiUrl}/${endpoint}`, { params: httpParams }).pipe(
      map((res: any) => this.unwrapPagedResponse<T>(res))
    );
  }
  ```
- **Test every entity endpoint** against the standardized response shape before migrating. Create a spec that calls each endpoint and asserts the response matches `PagedResponse<T>`. Without tests, this is a blind spot.

**Warning signs:**
- Each service has its own `HttpParams` construction logic
- Services do manual `.pipe(map(...))` to reshape response data
- `any` casts appear in the response processing chain
- `PagedResponse<T>` exists in `api.models.ts` but no service returns it
- Different entities have different error handling for empty results (some check `succeeded`, others don't)

**Phase to address:**
SVC-01 and SVC-02 (API service standardization). These phases MUST define and implement the response unwrapping strategy *before* any VIEW migration. A single endpoint that returns a different shape will break the grid silently.

---

### Pitfall 6: Pagination State Fragmentation — Split-Brain Page Tracking

**What goes wrong:**
The existing app already has this bug: `StaffComponent` uses 0-based page indexing (`page: 0`, `event.pageIndex`) while `CustomerComponent` uses 1-based (`page: 1`, `event.pageIndex + 1`) (CONCERNS.md: Inconsistent Pagination). When standardizing services, if the pagination contract is not explicitly defined and enforced, some entity pages will show wrong data. The grid component inherits this inconsistency — one entity always shows "page 2" when you click "page 1."

**Why it happens:**
Angular Material's `MatPaginator` uses 0-based index (`pageIndex: 0 = first page`). Many backends use 1-based (`page: 1 = first page`). The developer must add `+1` when sending to the backend and `-1` when reading from the backend response. This is easy to get wrong and even easier to forget when standardizing.

**How to avoid:**
- **Define a `PaginatedQueryParams` interface that uses 1-based page (matching the API contract):**
  ```typescript
  export interface PaginatedQueryParams {
    page: number; // 1-based — matches backend API
    pageSize: number;
    // ...
  }
  ```
- **Convert ONLY at the grid boundary.** The grid emits `MatPageEvent` (0-based), the smart component converts to `PaginatedQueryParams` (1-based):
  ```typescript
  onPageChange(event: PageEvent): void {
    this.query.update(q => ({
      ...q,
      page: event.pageIndex + 1, // 0-based → 1-based conversion here
      pageSize: event.pageSize
    }));
  }
  ```
- **Never pass `MatPageEvent` directly to a service.** The conversion must be explicit and visible.
- **Document the convention in code:** Add a comment block to the grid component and the standardized service explaining the page indexing convention. Comment rot is real, but having it somewhere is better than nowhere.

**Warning signs:**
- `page: 0` appears in any component initialization alongside `page: 1` in another
- No conversion layer between `PageEvent.pageIndex` and API calls
- Backend returns `page: 0` in the response but frontend expects 1-based
- Users report that clicking page 2 shows the same data as page 1
- Pagination state is stored in multiple signals instead of a single query state

**Phase to address:**
SVC-02 (standardized paginated params) must define the convention. ARCH-01 (smart/dumb) must enforce the conversion boundary. Each VIEW migration must audit the existing page indexing and fix it.

---

### Pitfall 7: Migrating All Entities At Once — The Big Bang Table Swap

**What goes wrong:**
The developer replaces all 5 entity table views with the new grid component in a single commit. The diff touches 15 files, introduces new services, new models, new components, and changes every entity page. When something breaks (wrong column mapping, missing filter, wrong response parsing), there's no way to tell which entity caused it. Debugging is a nightmare. Rollback loses everything.

**Why it happens:**
The refactoring seems mechanical — "they're all tables, just swap them." The developer wants to minimize merge conflicts by doing it all at once. But each entity has unique quirks (Allergen has a simple table, Staff has complex action buttons, Customer has dual CRUD paths). These unique cases leak through the abstraction and cause cascading failures.

**How to avoid:**
- **Migrate one entity at a time.** PROJECT.md already defines this — VIEW-01 (Customer) first, then VIEW-02 (Ingredient), etc. This order is intentional: Customer has the most complex filters, so if the grid handles Customer, it handles everything.
- **Commit each entity migration independently.** Each commit should be: "Migrate Customer list to reusable grid." Small diffs are reviewable and reverting one doesn't revert all.
- **Run the app after each migration.** Don't wait until all 5 are done to test. After VIEW-01, verify Customer works end-to-end before starting VIEW-02.
- **Keep the old component as a fallback.** Wrap the old and new in a feature flag/route param so you can toggle back if the migration breaks something. Remove the old component only after the new one is verified in production.
- **Surface regressions immediately:** The existing codebase has no tests, so manual verification is critical. Create a smoke-test checklist for each entity before and after migration:
  - Load page → see data
  - Sort ascending/descending
  - Navigate to page 2, 3
  - Apply each filter
  - Click an action button (edit/delete)
  - Verify URL params reflect state

**Warning signs:**
- A commit description says "migrate all entities to new grid"
- PR has 20+ files changed across 5 feature directories
- Developer says "I'll test them all after I finish the last one"
- No smoke-test checklist exists for any entity
- Entity pages that were working before the migration are broken afterward

**Phase to address:**
Each VIEW migration phase. The order (VIEW-01 → VIEW-02 → VIEW-03 → VIEW-04 → VIEW-05) is a critical safety measure. Skipping an entity or reordering without reason increases risk.

---

### Pitfall 8: Subscription Leak Cascade — Unmanaged Query Streams

**What goes wrong:**
The existing codebase has 15+ unmanaged subscriptions (CONCERNS.md: Subscription Management — No Cleanup). When introducing the standardized grid pattern, the natural approach is to use `BehaviorSubject` + `switchMap` for query parameter changes:
```typescript
query$ = new BehaviorSubject<PaginatedQueryParams>({...});
data$ = this.query$.pipe(
  switchMap(q => this.service.getPaged(q))
);
```
If `data$` is subscribed in `ngOnInit()` and never cleaned up, every navigation to the entity page adds another subscription. After 5 navigations to Customer, there are 5 redundant request streams. After 100, memory pressure grows and the app degrades.

**Why it happens:**
The codebase has no established subscription management pattern. `takeUntilDestroyed()` (Angular 16+) exists but is not used anywhere. Developer inertia: "the old code didn't clean up, so neither will I." The standardization effort introduces new observables but doesn't introduce cleanup because no one is enforcing it.

**How to avoid:**
- **Standardize subscription cleanup as part of the grid pattern.** Require `takeUntilDestroyed()` on all subscriptions in smart components:
  ```typescript
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.query$.pipe(
      switchMap(q => this.service.getPaged(q)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(res => {
      this.data.set(res.items);
      this.total.set(res.totalCount);
    });
  }
  ```
- **Or eliminate subscriptions entirely.** Use `toSignal()` to convert the observable to a signal:
  ```typescript
  readonly data = toSignal(
    this.query$.pipe(switchMap(q => this.service.getPaged(q))),
    { initialValue: { items: [], totalCount: 0, ... } }
  );
  ```
  This automatically handles cleanup and integrates with the signal-based template.
- **Enforce via lint rule or code review.** Make unmanaged subscriptions a rejectable offense in PR reviews. The existing codebase has the problem — don't let the new code inherit it.
- **Document the chosen pattern** in a CONTEXT.md or CONVENTIONS.md update so all developers use the same approach.

**Warning signs:**
- New entity components have `.subscribe()` without `takeUntilDestroyed()` or `takeUntil(this.destroy$)`
- No `DestroyRef` injection in any smart component
- `BehaviorSubject` is used but never completed
- Developer says "Angular auto-completes HTTP observables" (true, but `switchMap` doesn't auto-complete the outer observable)
- Entity pages slow down after repeated navigation (symptom of accumulated subscriptions)

**Phase to address:**
ARCH-01 (smart/dumb pattern definition) should mandate the subscription cleanup approach. SVC-01/SVC-02 (service standardization) should provide the observable streams. VIEW migrations must implement cleanup.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| `@Input() columns: any[]` instead of `ColumnDefinition<T>[]` | Saves typing generics | Zero type safety on column mappings, runtime errors on field access | Never — defeats TypeScript strict mode entirely |
| `effect()` + `untracked()` to sync grid data | Quick fix for NG0600 error | Fragile sync, hard to debug, missed updates | Only as a temporary bridge during migration; replace with direct assignment |
| Copy-paste grid code before abstracting | Ship faster for first entity | Duplicated logic across 2+ entities before you learn what's common | Acceptable for 2 entities max. After the 2nd, extract the commonality |
| Hybrid signal/observable per entity service | Minimal refactoring of existing services | Inconsistent patterns, cognitive overhead, harder to onboard new devs | Only during migration period; all new services must follow the standard |
| `as any` cast on grid data to bypass type errors | Unblocks the compilation | Hides type mismatches, causes runtime errors, defeats refactoring | Never — fix the type properly |
| One big commit migrating all entities | "Done faster" | Impossible to review, rollback loses everything, debugging nightmare | Never — always one entity per commit |

---

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| **Grid → Router** | Grid component directly calls `router.navigate()` or has `[routerLink]` in its template | Grid emits a `rowAction` event; parent smart component handles navigation |
| **Grid → MatDialog** | Grid component injects `MatDialog` and opens edit/create dialogs | Grid emits an event with the row data; parent opens the dialog |
| **Grid → HTTP interceptors** | Grid assumes all endpoints are wrapped in `{ data: ... }` but the auth interceptor changes the response shape for 401s | Response type should be `PagedResponse<T>`; unify unwrapping in one interceptor layer |
| **Paginator → Backend** | Passing `MatPaginator.pageIndex` (0-based) directly to the API | Add `+1` conversion at the smart component boundary |
| **Signal Form (future) → Grid** | Binding signal-form values directly to grid filter inputs when signal forms are still experimental | Keep using Reactive Forms for filter sections until signal forms are stable and adopted |
| **Existing CRUD dialogs → New service** | Old dialogs keep using `Observable<any>` services after standardization | Refactor dialog service calls together with the entity view migration (same phase) |

---

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| **Rebuilding MatTableDataSource on every data load** | Sort/paginator state resets, table flickers, change detection cycles | Create `MatTableDataSource` once, reassign `.data` property only | Immediately on first use |
| **No trackBy on table rows** | Entire table DOM is replaced on any data change (even pagination) | Add `trackBy: (i, row) => row.id` to `@for` or `*ngFor` | At ~100 rows |
| **Re-fetching full dataset on every filter change** | Network waterfall, UI freezes during loading | Debounce filter inputs (300ms), use `switchMap` to cancel in-flight requests | At any scale — UX issue first, performance second |
| **Signal effect syncing data to non-signal APIs (MatTableDataSource)** | Missed updates, NG0600 errors, circular sync | Direct assignment in subscribe handler, not via `effect()` | Intermittent — hard to reproduce |
| **Overly deep column config objects** | Change detection evaluates deeply nested objects on every CD cycle | Keep column config flat, use `trackBy`, mark grid as `OnPush` | At 500+ rows with 10+ columns |
| **No `ChangeDetectionStrategy.OnPush` on grid component** | Every keystroke in a filter input re-evaluates the entire table | Set `changeDetection: ChangeDetectionStrategy.OnPush` on grid | At any scale — CD is wasted on every unrelated change |

---

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| **Putting token in localStorage and accessing it from grid-related code** | XSS exposes JWT (existing issue, CONCERNS.md) — unrelated to grid but the refactoring touches service layer where token is accessed | Don't touch auth code during this refactoring. Document that auth is out of scope. |
| **Exposing `id` fields in grid columns that shouldn't be visible (e.g., internal IDs)** | Information disclosure, ID enumeration attack surface | Column definition should whitelist displayable fields, not list all `keyof T`. Never auto-generate columns from the entity interface. |
| **No sanitization for cell values rendered as HTML** | XSS if a data field contains `<script>` tags (e.g., a customer's `businessName`) | Always render cell values as plain text, never innerHTML. If HTML rendering is needed, use Angular's `DomSanitizer` explicitly. |
| **Action buttons that emit raw row data without permission check** | User could trigger delete/edit on rows they shouldn't have access to | Parent smart component must enforce permissions before acting on row events, not the grid |

---

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| **No loading state during pagination** | User clicks page 2 → nothing happens for 2-3 seconds → confusion | Grid should show a skeleton or overlay spinner during data fetch. Parent passes `loading` input. |
| **No empty state** | User sees a blank white rectangle where the table should be | Grid shows "No results" message with optional CTA when data is empty. Parent passes different content for "no data" vs "filters returned nothing". |
| **No error state** | API fails silently (existing codebase problem — most errors go to `console.error`) | Grid receives an `error: string | null` input and shows an error banner with retry button |
| **Filters reset on page change** | User sets filters, navigates to page 2 → filters disappear, back to page 1 | Filter state and pagination state are part of the same query signal/object. Changing one preserves the other. |
| **Sort indicator disappears after data refresh** | User sorts by column, data refreshes → sort arrow gone, data unsorted | Paginated sort state must be preserved by the smart component and re-sent to the API on every request. Grid must re-apply sort indicator based on API response (not client-side sorting). |
| **No keyboard navigation** | Power users cannot Tab through table rows or activate actions via keyboard | Use `mat-table` with proper ARIA roles, ensure action buttons are focusable. Angular 21 has built-in `@angular/cdk/a11y` grid patterns. |

---

## "Looks Done But Isn't" Checklist

- [ ] **Column configuration:** Does the grid use `keyof T` for field access? If it uses `string` or `any`, type safety is not achieved.
- [ ] **Action row slot:** Can the parent define custom action buttons per row without the grid knowing about them? If not, the grid is responsible for entity-specific UI.
- [ ] **Empty state:** Does the grid handle `data = []` gracefully? Or does it show a broken table with just headers?
- [ ] **Error state:** Does the grid have an input for `error: string | null`? Or is error handling invisible?
- [ ] **Loading state:** Does the parent pass `loading` to the grid? Or is the loading state managed inside the grid?
- [ ] **Server-side sort:** Does the grid emit `sortChange` events that the parent uses to re-fetch? Or does it try client-side sorting on a single page?
- [ ] **Paginator state preservation:** After data reload, does the paginator stay on the current page? Or reset to page 1?
- [ ] **Response unwrapping:** Is there exactly one place where API responses get unwrapped to `PagedResponse<T>`? Or is each service doing its own unwrapping?
- [ ] **Page indexing:** Is the 0-based ↔ 1-based conversion documented and applied consistently at the smart component boundary?
- [ ] **Subscription cleanup:** Does every `.subscribe()` have `takeUntilDestroyed()` or equivalent? Or are subscriptions leaking?
- [ ] **`any` audit:** After migration, has the `any` count in services and entity pages gone down (not up)? Run `rg 'Observable<any>' --include '*.ts'` to check.
- [ ] **Filter ↔ grid coupling:** Are filter component and grid component independent? Or is there a bidirectional binding that couples them?
- [ ] **One entity at a time:** Are you migrating one entity per commit? Or is there a multi-entity diff that will be hard to review/revert?

---

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Over-generic grid | MEDIUM | 1. Extract entity-specific grid features into parent templates (content projection). 2. Freeze grid `@Input()` API. 3. Reject PRs that add new inputs without team consensus. 4. Consider creating a second concrete grid variant if the abstractions fight each other. |
| Type safety collapse | HIGH | 1. Replace every `any` in the grid component with concrete types. 2. If `T` is lost in template, rename the component class and re-import — Angular will re-infer from inputs. 3. Add a compile-time test. 4. Run `rg 'Observable<any>'` to find remaining leaks. |
| Signal/MatTableDataSource sync loop | MEDIUM | 1. Remove `effect()` sync. 2. Set `dataSource.data` directly in the subscription. 3. Remove `allowSignalWrites` flags. 4. Add `ChangeDetectionStrategy.OnPush`. |
| Smart/dumb boundary eroded | HIGH | 1. Extract service calls and router logic from grid into parent. 2. Replace `@Output()` events until grid is pure. 3. Add unit test that verifies grid doesn't import any service. 4. Code review enforcement going forward. |
| API double-wrap or response mismatch | MEDIUM | 1. Create central unwrapping interceptor. 2. Test each entity's endpoint against the standard shape. 3. Fix the inconsistent endpoint or add per-entity mapping in the unwrapping layer. |
| Pagination inconsistency | LOW | 1. Pick 1-based as the standard (matches API contract). 2. Fix all existing entity pages to use `pageIndex + 1` conversion. 3. Remove 0-based initial values. 4. Verify each entity in the browser. |
| Subscription leak cascade | MEDIUM | 1. Add `takeUntilDestroyed()` to all new `.subscribe()` calls. 2. Convert existing entity subscriptions during VIEW migrations. 3. After migration, run `rg '\.subscribe\('` and audit remaining ones. 4. Consider using `toSignal()` for new code. |

---

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Over-generic grid | GRID-01/GRID-02 | After GRID-02, count `@Input()` properties (target: ≤8). Review column config interface. |
| Type safety collapse | GRID-01, SVC-02 | After SVC-02, run `rg 'Observable<any>' --include '*.ts'` and `rg 'EventEmitter<any>'` — must be 0 in new code. |
| MatTableDataSource ↔ Signal sync | GRID-02 | Review data binding approach in CONTEXT.md. Verify no `effect()` syncs data to `MatTableDataSource`. |
| Smart/dumb boundary erosion | ARCH-01, GRID-01 | Unit test: grid component with mock inputs, verify it only emits events (no service calls). |
| API response wrapping inconsistency | SVC-01, SVC-02 | After SVC-02, create a spec that calls each entity endpoint and asserts `PagedResponse<T>` shape. |
| Pagination state fragmentation | SVC-02, each VIEW | After each VIEW migration, manually test: page 1 → page 2 → back to page 1 shows correct data. |
| Big-bang migration | VIEW-01 through VIEW-05 | Enforce one commit per VIEW phase. Review size of each diff (target: <10 files changed). |
| Subscription leak cascade | ARCH-01, each VIEW | After each VIEW, run code search for `.subscribe(` — every instance must have cleanup. |
| Performance traps | GRID-01, GRID-02 | Verify `ChangeDetectionStrategy.OnPush`, `trackBy`, debounced filters. |
| UX pitfalls | GRID-01, each VIEW | Verify loading state, empty state, error state inputs exist and work. |

---

## Sources

- **Codebase audit:** CONCERNS.md documented 47+ `any` instances, unmanaged subscriptions, inconsistent pagination, duplicate interceptors, and dead code — all directly relevant to this refactoring
- **Codebase conventions:** CONVENTIONS.md documented the existing `effect()` + `MatTableDataSource` sync pattern (anti-pattern to avoid)
- **Angular community:** "Hidden Cost of Reusable Components" (Raj Chhatrala, Mar 2026) — over-abstraction and premature generic component traps
- **Angular community:** "Building a Reusable Table Component in Angular" (Kapil Kumar, May 2025) — demonstrates `keyof T` pattern for column definitions
- **Angular community:** "Creating a Generic Table Component in Angular: Keep it DRY!" (Mamdouhibr, Sep 2024) — generic grid approach with type safety emphasis
- **Angular community:** "Smart vs Dumb Component Mistakes That Quietly Destroy Angular Apps" (Coding master, Feb 2026) — boundary erosion patterns
- **Angular community:** "How to use Angular Signals with Material Table data source" (Stack Overflow, 2023-2024) — documented the `effect()` + `untracked()` hack and its pitfalls
- **Angular community:** "Angular 18: Building a Reusable Angular Material Table Component with Signals" (M Business Solutions, Nov 2024) — alternative approaches avoiding `MatTableDataSource`
- **Angular docs — Signals:** angular.dev/guide/signals — official signal usage, warnings about writing signals in effects
- **Angular docs — HttpClient:** angular.dev/guide/http/making-requests — recommends encapsulated data access services
- **Project documentation:** PROJECT.md (GRID-01 through ARCH-01), CONCERNS.md, CONVENTIONS.md — project-specific findings

---

*Pitfalls research for: fe-roscoff Angular data grid & API service standardization*
*Researched: 2026-05-29*
