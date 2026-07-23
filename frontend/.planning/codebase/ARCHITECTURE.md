<!-- refreshed: 2026-06-29 -->
# Architecture

**Analysis Date:** 2026-06-29

## System Overview

This is a standalone Angular 21 single-page application (SPA) — a back-office management system for "Roscoff Meal," a food production company. It provides CRUD management for menus, plates, ingredients, allergens, orders, invoices, customers, staff, categories, and sales/discounts.

The app communicates with a .NET (C#) REST API backend, with JWT token-based auth. All state is managed via Angular Signals (no NgRx/Redux). The architecture follows a **feature-first, layered** pattern with a reusable shared data grid as the central UI abstraction.

```text
┌─────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                              │
│                                                                      │
│   Auth Components        │    Dashboard Feature Pages               │
│   (login, forgot-pwd,    │    (plate, menu, order, customer,        │
│    reset-pwd)            │     ingredient, allergen, staff,         │
│                          │     category, sale, invoice)             │
│   `src/app/features/auth/` │   `src/app/features/dashboard/*/`      │
└──────────────────────────┴──────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         SHARED / UI LAYER                           │
│                                                                      │
│   DataGridComponent       │   UiOverlayComponent    │   UiService    │
│   (reusable paginated     │   (toast / confirm /    │   (busy/toast/ │
│    sortable table)        │    alert overlays)      │    confirm)    │
│   `src/app/shared/`       │   `src/app/shared/`     │   `core/`      │
└──────────────────────────┴──────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         CORE LAYER                                   │
│                                                                      │
│   Services (14)          │   Interceptors (3)     │   Guards / Utils │
│   - Entity CRUD +        │   - apiInterceptor     │   - auth.guard   │
│     paged data access    │   - authInterceptor    │   - http-params  │
│   `src/app/core/services/`│   - errorInterceptor  │     .util        │
│                           │   `core/interceptors/` │   `core/*/`      │
└──────────────────────────┴──────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         API / BACKEND                                 │
│                                                                      │
│   REST endpoints under `{environment.apiUrl}/`                       │
│   e.g. `/api/Plate`, `/api/Auth`, `/api/Menu`, `/api/Order`         │
│   .NET backend, JSON wrapper: `{ data: T }`                         │
└─────────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| `AppComponent` | Root shell: `<router-outlet>` + `<app-ui-overlay>` | `src/app/app.component.ts` |
| `LayoutComponent` | Auth-gated shell: sidenav, navbar, `<router-outlet>` | `src/app/layout/layout.component.ts` |
| `NavbarComponent` | Top toolbar with user menu, logout, sidebar toggle | `src/app/layout/navbar/navbar.component.ts` |
| `SidebarComponent` | Collapsible navigation with 3 section groups | `src/app/layout/sidebar/sidebar.component.ts` |
| `DataGridComponent` | Generic paginated/sortable table (typed via generics) | `src/app/shared/data-grid/data-grid.component.ts` |
| `UiOverlayComponent` | Global toast, confirm dialog, alert dialog | `src/app/shared/ui.overlay/ui.overlay.component.ts` |
| `LoginComponent` | Login form with "remember me" cookie | `src/app/features/auth/login/login.component.ts` |
| `ForgotPasswordComponent` | Email-based reset request form | `src/app/features/auth/forgot-password/forgot-password.component.ts` |
| `ResetPasswordComponent` | Token + new password form | `src/app/features/auth/reset-password/reset-password.component.ts` |
| `DashboardComponent` | Home placeholder (welcome card) | `src/app/features/dashboard/dashboard.component.ts` |
| Feature Page Components (12) | Entity CRUD list views, all follow same pattern | `src/app/features/dashboard/*/` |
| Feature Form/Dialog Components (9) | Create/edit forms and sub-dialogs | `src/app/features/dashboard/*/*-form/`, `*-dialog/` |
| `AuthService` | JWT token lifecycle, login, user state | `src/app/core/services/auth.service.ts` |
| `UiService` | Global UI signals (loader, toast, confirm, alert) | `src/app/core/services/ui.service.ts` |
| Entity Services (12) | HTTP CRUD + paged queries for each domain | `src/app/core/services/*.service.ts` |

## Pattern Overview

**Overall:** Feature-first, standalone component architecture with Signals-driven state management.

**Key Characteristics:**
- **All components are standalone** — no `NgModule` wrappers, `bootstrapApplication` in `main.ts`
- **Signals over RxJS Subjects** for UI state (loading, error, data); `BehaviorSubject` used only for query pipelines
- **BehaviorSubject + switchMap + takeUntilDestroyed** is the standard data-loading pipeline
- **DataGridComponent** is the single reusable table — all list pages use it via column definitions
- **API response wrapper** convention: backend wraps results in `{ data: T }` — services unwrap with `.pipe(map(res => res.data))`
- **Lazy loading** via `loadComponent` in routes — zero eager feature code

## Layers

**Presentation (Features):**
- Purpose: UI pages and form dialogs organized by business domain
- Location: `src/app/features/`
- Contains: Auth pages (`auth/`), Dashboard feature pages (`dashboard/*/`)
- Depends on: Core services, Shared components
- Used by: Router (lazy-loaded via `loadComponent`)

**Shared (UI):**
- Purpose: Reusable presentational components and shared type definitions
- Location: `src/app/shared/`
- Contains: `DataGridComponent`, `UiOverlayComponent`, `api.models.ts`, `data-grid.models.ts`
- Depends on: Core `UiService`, Angular Material
- Used by: All feature pages

**Core (Infrastructure):**
- Purpose: Services, interceptors, guards, utilities
- Location: `src/app/core/`
- Contains: 14 entity services (`services/`), 3 HTTP interceptors (`interceptors/`), auth guard (`guards/`), param builder util (`utils/`)
- Depends on: Angular `HttpClient`, `Router`, environment config
- Used by: Feature pages, shared components

**Configuration (Root):**
- Purpose: App bootstrap, routing, environment
- Location: `src/` (root files)
- Contains: `main.ts`, `app.config.ts`, `app.routes.ts`, `environments/`
- Depends on: Core interceptors, guards

## Data Flow

### Primary Request Path (List Page)

1. User interacts with page (search, sort, paginate) → triggers `updateQuery()` in feature component
2. Component emits partial query to `BehaviorSubject<EntityPagedRequest>` via `.next()`
3. `BehaviorSubject` pipe: `tap(set loading) → switchMap(query => service.getPaged(query))`
4. Service builds `HttpParams` (via `buildPagedParams()` helper), calls `http.get()`, unwraps `response.data`
5. Returned `PagedResponse<T>` mapped to display signals: `data.set()`, `totalItems.set()`, `loading.set(false)`
6. `DataGridComponent` receives data, total, loading as `@Input()` signals — renders table
7. Error path: `catchError` in subscribe → sets `error` signal → shows toast via `UiService`

### Login Flow

1. `LoginComponent` submits form → calls `AuthService.login(credentials)` with `withCredentials: true`
2. Backend returns `AuthResponse { token, user }` — service calls `setToken(token)` (stores in localStorage)
3. `isAuthenticated` signal set to `true` → `AuthService.init()` called in `AppComponent` constructor
4. Router navigates to `/dashboard` — `authGuard` checks `getToken()` passes
5. All subsequent HTTP requests: `authInterceptor` attaches `Authorization: Bearer <token>` header

### Mutation Flow (Create/Edit/Delete)

1. Feature component opens form dialog or navigates to form route
2. Form submits → calls service method (`create()`, `update()`, `delete()`)
3. On success: `UiService.showToast()` → refreshes parent list via `querySubject.next({...current})` to re-trigger load
4. On error: `UiService.showToast(message, 'error')` or `showAlert(title, message)`
5. Delete actions always go through `UiService.askConfirm()` for confirmation

**State Management:**
- All state is local to component via `signal()` — no shared state store
- `AuthService.currentUser` and `isAuthenticated` are global signals (`providedIn: 'root'`)
- `UiService` holds global signals for overlay state (busy, toast, confirm, alert)
- Query state is in `BehaviorSubject` per component — re-fetched on navigation (no caching)

## Key Abstractions

**DataGridComponent `<app-data-grid>`:**
- Purpose: Reusable typed, paginated, sortable data table
- Examples: Used in every list page: `plate-page`, `customer-page`, `menu-page`, `staff-page`, `order-page`, `sale-component`, `allergen-page`, `ingredient-page`, `category-page`
- Pattern: Generic type `<T extends Record<string, any>>` with `ColumnDef<T>` configuration, `@Input()` columns/data/total/loading/error, `@Output()` sortChange/pageChange/retry, `@ContentChild('actionsTemplate')` for action buttons
- Files: `src/app/shared/data-grid/data-grid.component.ts`, `data-grid.models.ts`

**UiService / UiOverlayComponent:**
- Purpose: Global UI feedback layer — loader spinner, toast notifications, confirmation dialogs, error alerts
- Examples: Every feature page calls `uiService.showToast()`, `uiService.askConfirm()`, `uiService.showLoader()`
- Pattern: Singleton service with `signal()` state → `UiOverlayComponent` subscribes and conditionally renders overlays
- Files: `src/app/core/services/ui.service.ts`, `src/app/shared/ui.overlay/ui.overlay.component.ts`

**buildPagedParams():**
- Purpose: Converts typed query objects (camelCase) to Angular `HttpParams` (PascalCase for API)
- Pattern: Iterates object entries, skips null/undefined/empty, converts `camelCase` → `PascalCase`, handles arrays
- Files: `src/app/core/utils/http-params.util.ts`

## Entry Points

**main.ts (Application Bootstrap):**
- Location: `src/main.ts`
- Triggers: Browser loads page
- Responsibilities: Calls `bootstrapApplication(AppComponent, { providers: [...] })`, registers router and HTTP interceptors

**AppComponent (Root Component):**
- Location: `src/app/app.component.ts`
- Triggers: After `bootstrapApplication`
- Responsibilities: Renders `<router-outlet>` and global `<app-ui-overlay>`, calls `AuthService.init()` to check JWT validity on startup

**Router (Navigation):**
- Location: `src/app/app.routes.ts`
- Triggers: User navigates or types URL
- Responsibilities: Lazy-loads pages, applies `authGuard` with `canMatch` to protected routes, redirects unknown paths to `/`

**HTTP Interceptor Chain (Ordered):**
1. `apiInterceptor` — Prepends `environment.apiUrl` to relative URLs
2. `authInterceptor` — Attaches JWT `Bearer` token from localStorage
3. `errorInterceptor` — Catches 401/403 errors, triggers `authService.handleAuthFailure()` for non-public endpoints

## Architectural Constraints

- **Threading:** Single-threaded (browser event loop). All async work via RxJS observables and Angular zone.
- **Global state:** Three singleton services with module-level state: `AuthService` (token, user), `UiService` (overlays), `CookieService` (browser cookie convenience wrapper)
- **Circular imports:** None detected — core → shared → features dependency direction is strictly one-way
- **No NgModules:** Zero `@NgModule` declarations — standalone components throughout. `app.config.ts` uses `ApplicationConfig` instead.
- **No state management library:** All state is in Signals or BehaviorSubjects. No NgRx, NgXS, or Akita.

## Anti-Patterns

### Inconsistent API Wrapper Handling

**What happens:** Some services consistently unwrap `response.data` (`plate.service.ts`, `menu.service.ts`, `customer.service.ts`), while others handle it inconsistently — e.g., `order.service.ts` uses `response.data || response` as fallback, and `invoice-page.component.ts` has inline `res.data || res` fallback checks in the UI layer.
**Why it's wrong:** Mixing response unwrapping between services and components creates inconsistency. Components that handle `res.data` directly break the service abstraction boundary.
**Do this instead:** Standardize all services to always unwrap in the service layer with `.pipe(map(res => res.data))`, returning the typed model directly. Components should never reference `response.data`.

### Inline query params in services

**What happens:** `order.service.ts` builds `HttpParams` manually with `.set()` calls, while most other services use the shared `buildPagedParams()` utility.
**Why it's wrong:** Duplicates param-building logic and increases maintenance burden when param conventions change.
**Do this instead:** Refactor `order.service.ts` to use `buildPagedParams()` like all other services.

## Error Handling

**Strategy:** Per-component error handling with centralized UI feedback.

**Patterns:**
- Each feature component wraps its data subscription with `error` handler that calls `uiService.showToast(message, 'error')` and resets loading state
- `errorInterceptor` catches 401/403 globally and triggers `authService.handleAuthFailure()` (clears token, redirects to `/`)
- Delete operations always confirm via `uiService.askConfirm()` before executing
- Form validation is client-side via Angular `Validators` + template error display

## Cross-Cutting Concerns

**Logging:** `console.error()` in error handlers — no formal logging framework. Errors are not sent to any external service.

**Validation:** Reactive forms with Angular `Validators` (required, minLength, email). Simple field-level validation only — no cross-field or async validators except in `order-create.component.ts` (async customer search via autocomplete).

**Authentication:** JWT-based. Token stored in `localStorage` under key `x-auth-token`. Auth service checks token expiry on load and on every `getToken()` call. Token payload decoded client-side for expiry check (no refresh token mechanism — expired tokens force re-login).

---

*Architecture analysis: 2026-06-29*
