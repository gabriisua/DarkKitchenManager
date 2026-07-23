# Codebase Concerns

**Analysis Date:** 2026-06-29

## Tech Debt

### Inconsistent API Response Handling Pattern

**Issue:** Services expect two different response envelope shapes from the backend. Some expect `{ data: T }` while others expect `{ succeeded: boolean, data: T }`. A third pattern uses `res.data || res` as a fallback. This makes refactoring risky and adding new services error-prone.

**Files:**
- `src/app/core/services/plate.service.ts` (lines 16, 21, 25, 29, 33, 37, 41) — expects `{ data: T }`
- `src/app/core/services/menu.service.ts` (lines 18, 23, 27, 31, 35) — expects `{ data: T }`
- `src/app/core/services/allergen.service.ts` (lines 18, 33, 37, 41) — expects `{ data: T }`
- `src/app/core/services/ingredient.service.ts` (lines 21, 43, 49, 56, 65, 72) — expects `{ data: T }`
- `src/app/core/services/category.service.ts` (lines 14, 24, 27, 31, 36) — expects `{ data: T }`
- `src/app/core/services/sale.service.ts` (lines 34, 43) — checks `res.succeeded && res.data`
- `src/app/core/services/staff.service.ts` (lines 30-46, 49-65) — checks `res.succeeded && res.data`
- `src/app/core/services/customer.service.ts` (lines 39, 43-51) — checks `res.succeeded && res.data`
- `src/app/core/services/order.service.ts` (lines 30-32, 36-38, 53-54) — uses `response.data || response`
- `src/app/core/services/invoice.service.ts` (lines 33-34, 43-44, 53-54, 59-60, 65-66) — mixed, sometimes maps `res.data`, sometimes returns raw response

**Impact:** Adding a new feature or endpoint requires guessing which response shape the backend uses. Inconsistent error handling across services.

**Fix approach:** Standardize on a single response envelope across all services. Create a generic `ApiResponse<T>` type and a mapping utility function.

### Widespread Use of `any` Type

**Issue:** Despite `strict: true` in `tsconfig.json`, the `any` type is used extensively throughout services and components, defeating TypeScript's type safety guarantees.

**Files:** At least 25+ instances across services including:
- `src/app/core/services/auth.service.ts` (lines 32, 42, 51, 54, 85) — `res: any`, `currentUser: signal<any>`, manual JWT decode returning `any`
- `src/app/core/services/order.service.ts` (lines 15, 30, 36, 44, 45, 51) — `query: any`, `this.http.get<any>`
- `src/app/core/services/invoice.service.ts` (lines 43, 53, 59, 65) — all `this.http.get<any>`
- `src/app/core/services/sale.service.ts` (lines 34, 43, 55-76) — all return `Observable<any>`
- `src/app/features/dashboard/sale/sale.component.ts` (line 51-52) — `TemplateRef<any>`
- `src/app/features/dashboard/plate/plate-form/plate-form.component.ts` (lines 152, 184, 214, 275) — `event: any`, `pi: any`
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts` (lines 49, 65-100) — `SelectionModel<any>` and `item: any`
- `src/app/features/dashboard/order/order-create/order-create.component.ts` (lines 57-61, 57, 117, 122, 180, 184) — `any[]` for plates and customers
- `src/app/features/dashboard/order/order-details/order-details.component.ts` (line 49) — `params.get('id')` without null check throws `any`
- `src/app/features/dashboard/invoice/invoice-page.component.ts` (lines 86, 153, 217) — `res: any`, `res: any`
- `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts` (lines 206, 229) — `err: any`

**Impact:** Reduces compile-time error checking. Refactoring becomes dangerous — changing a model type may not produce compilation errors.

**Fix approach:** Replace `any` with proper typed interfaces. Use generics consistently.

### Duplicated Blob Download Pattern

**Issue:** The same multi-line blob download logic (createObjectURL, create anchor, click, revoke) is duplicated verbatim across at least 4 components.

**Files:**
- `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts` (lines 140-148)
- `src/app/features/dashboard/plate/plate-detail/plate-detail.component.ts` (lines 119-126)
- `src/app/features/dashboard/order/order-details/order-details.component.ts` (lines 82-93)
- `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts` (lines 227)

**Impact:** If the download behavior needs to change (e.g., add error handling for empty blobs), all 4 locations must be updated. Violates DRY principle.

**Fix approach:** Extract into a shared utility function in `src/app/core/utils/` and inject via a service.

### Inconsistent Use of HttpParams Building

**Issue:** Some services build `HttpParams` manually using PascalCase keys (like `'Page'`, `'PageSize'`), while others use the `buildPagedParams()` utility that auto-converts camelCase to PascalCase. Some methods in the same service use different approaches.

**Files:**
- `src/app/core/services/staff.service.ts` — `getStaff()` uses manual params (lines 17-28), `getPaged()` uses `buildPagedParams` (line 49)
- `src/app/core/services/customer.service.ts` — `getCustomers()` uses manual params (lines 26-38), `getPaged()` uses `buildPagedParams` (line 42)
- `src/app/core/services/order.service.ts` — uses manual params (lines 15-29), does NOT use `buildPagedParams`
- `src/app/core/services/invoice.service.ts` — `getPendingSummary()` uses manual params (lines 24-32), `getInvoiceHistory()` uses `buildPagedParams` (line 52)
- `src/app/core/services/category.service.ts` — uses manual params with lowercase keys (`'page'`, `'search'`, lines 18-23) — inconsistency with other PascalCase usages

**Impact:** Parameter naming inconsistencies between endpoints may cause silent API failures. Hard to trace which API expects which casing.

**Fix approach:** Standardize all services to use the `buildPagedParams()` utility exclusively. Remove manual `HttpParams` construction from services.

### Duplicated Pattern: Data Loading via BehaviorSubject + switchMap

**Issue:** The same reactive data loading pattern (`BehaviorSubject` -> `tap` -> `switchMap` -> `subscribe`) is duplicated in 6+ smart components with nearly identical error handling blocks.

**Files:**
- `src/app/features/dashboard/staff/staff-page.component.ts`
- `src/app/features/dashboard/customer/customer-page.component.ts`
- `src/app/features/dashboard/ingredient/ingredient-page.component.ts`
- `src/app/features/dashboard/category/category.component.ts`
- `src/app/features/dashboard/allergen/allergen-page.component.ts`
- `src/app/features/dashboard/plate/plate-page.component.ts`
- `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts`
- `src/app/features/dashboard/menu/menu-page.component.ts`
- `src/app/features/dashboard/sale/sale.component.ts`

**Impact:** Each component reimplements loading/error state management with subtle differences. Some reset `page` to 1 on filter change, some don't. Some use `error` signal, some don't.

**Fix approach:** Create a reusable composable or base class for list views with pagination, sorting, filtering, and error handling.

### Style Format Inconsistency

**Issue:** Components use a mix of `styleUrls`, `styleUrl`, `styles`, CSS files, and SCSS files inconsistently.

**Files:**
- `styleUrls: ['./*.css']` — used in some components (e.g., customer, order, invoice)
- `styleUrl: './*.css'` — single-file CSS (e.g., staff, invoice-history, sidebar)
- `styleUrls: ['./*.scss']` — used in other components (e.g., login, plate-form, menu-form, menu-detail)
- `styles: []` — inline styles in component metadata (e.g., sale, dashboard, category-form, staff-dialog)
- Some components have no style file at all (`app.component.ts`)

**Impact:** New developers must guess which style approach to follow. The project has no consistent styling convention.

**Fix approach:** Standardize on SCSS for all components that need stylesheets, with inline `styles` only for trivial overrides.

### Staging Environment Points to localhost

**Issue:** The staging environment file (`environment.staging.ts`) points to `http://localhost:8080/api`, which is the docker-compose port mapping, not an actual staging server.

**Files:**
- `src/environments/environment.staging.ts` (line 3)

**Impact:** There is no actual staging or production environment configured. The "staging" configuration is just localhost with production optimizations enabled.

**Fix approach:** Add proper staging and production environment files with real API endpoints.

### No Production Environment Configuration

**Issue:** Only `environment.ts` (development) and `environment.staging.ts` exist. There is no `environment.prod.ts` file.

**Files:**
- `src/environments/` directory listing

**Impact:** Building for production uses the development API URL. The production build configuration in `angular.json` has no `fileReplacements` section (only the staging config does, lines 56-66).

**Fix approach:** Add a proper production environment file and configure the build target accordingly.

## Security Considerations

### JWT Token Stored in localStorage

**Risk:** The authentication token is stored in `localStorage` under the key `x-auth-token`, making it accessible to any JavaScript running on the same origin. This is vulnerable to XSS attacks.

**Files:**
- `src/app/core/services/auth.service.ts` (lines 59-73) — `localStorage.setItem('x-auth-token', token)`

**Current mitigation:** None specific to localStorage. The error interceptor handles 401/403 by clearing the token.

**Recommendations:** Migrate to HttpOnly cookies for the auth token. If localStorage must be used, implement additional XSS protections (CSP headers, input sanitization).

### Manual JWT Decoding Without Signature Verification

**Risk:** The `decodeTokenPayload` method at line 85 of `auth.service.ts` performs a simple base64 decode of the JWT payload without verifying the token signature. Any tampered token will be accepted for expiry checks.

**Files:**
- `src/app/core/services/auth.service.ts` (lines 85-98, 101-106)

**Current mitigation:** None. The method only checks `payload.exp` expiry.

**Recommendations:** Use a proper JWT verification library on the backend. The frontend should not independently verify tokens — rely on the API to reject invalid tokens.

### Credential Caching via Cookie

**Risk:** The "remember me" feature stores the user's email in a cookie via `CookieService`. While not a direct security vulnerability (only email, not password), it could leak user identity.

**Files:**
- `src/app/features/auth/login/login.component.ts` (lines 51-59, 85-89)
- `src/app/core/services/cookie.service.ts` (lines 6-21)

**Current mitigation:** The cookie uses `Secure; SameSite=Strict` flags.

**Recommendations:** Consider using the browser's password manager instead of custom cookie-based email caching. The email value is decoded from URI components which adds minimal obfuscation.

## Performance Bottlenecks

### Large Eager Data Loading

**Problem:** Components load large datasets upfront for autocomplete/search functionality without pagination, causing unnecessary API load and memory use.

**Files:**
- `src/app/features/dashboard/order/order-create/order-create.component.ts` (line 103) — loads 500 plates at once (`pageSize: 500`)
- `src/app/features/dashboard/menu/menu-form/menu-form.component.ts` (line 111) — loads 200 customers at once (`pageSize: 200`)

**Cause:** Autocomplete filtering is done client-side on the full loaded dataset (see `filterPlates` at line 175-177 of `order-create.component.ts`).

**Improvement path:** Use server-side search for autocomplete (debounced API calls with search term) instead of loading all items upfront. The ingredient search in `plate-form.component.ts` (line 141) already does this correctly.

### Missing Unsubscribe in Some Observables

**Problem:** Several components subscribe to HTTP observables without using `takeUntilDestroyed` or any other cleanup mechanism. If components are destroyed before the HTTP request completes, the callback may execute on a destroyed view.

**Files:**
- `src/app/features/dashboard/plate/plate-detail/plate-detail.component.ts` (lines 58-78) — three sequential `subscribe()` calls with no cleanup
- `src/app/features/dashboard/plate/plate-form/plate-form.component.ts` (lines 183-231) — `loadPlateData` subscribes without cleanup
- `src/app/features/dashboard/allergen/allergen-dialog/allergen-dialog.component.ts` (lines 79-86) — subscribes to `getAll()` without cleanup

**Impact:** Potential memory leaks and "ExpressionChangedAfterItHasBeenChecked" errors if callbacks try to update signals on destroyed components.

**Fix approach:** Use `takeUntilDestroyed(this.destroyRef)` on all observable subscriptions in components that support Angular >= 16 destroy ref.

### Constructor-based Data Loading

**Problem:** Some components kick off HTTP requests in their constructor (or in an anonymous constructor-like init) before Angular's `ngOnInit` lifecycle runs.

**Files:**
- `src/app/features/dashboard/customer/customer-page.component.ts` (line 116) — calls `loadData()` in constructor
- `src/app/features/dashboard/invoice/invoice-history/invoice-history.component.ts` (line 104) — calls `loadData()` in constructor
- `src/app/features/dashboard/invoice/invoice-page.component.ts` (lines 76-105) — data loading logic directly in constructor
- `src/app/features/dashboard/order/order-details/order-details.component.ts` (lines 40-63) — data loading logic directly in constructor

**Cause:** Inconsistent understanding of Angular lifecycle. These components load data before bindings and inputs are resolved.

**Fix approach:** Move all data loading to `ngOnInit()`.

## Fragile Areas

### AuthService `loadUser()` Design

**Files:** `src/app/core/services/auth.service.ts` (lines 47-57)

**Why fragile:** `loadUser()` is called after login without checking if a request is already in-flight. It also silently swallows errors by setting `currentUser` to `null` on failure, which could hide authentication problems. The `currentUser` signal is typed as `any`.

**Safe modification:** Add an in-flight guard. Use a proper typed interface for the user model. Surface errors to the caller.

### Dual API Method Pattern in Customer and Staff Services

**Files:**
- `src/app/core/services/customer.service.ts` — `getCustomers()` (line 26) vs `getPaged()` (line 42)
- `src/app/core/services/staff.service.ts` — `getStaff()` (line 17) vs `getPaged()` (line 49)

**Why fragile:** Two methods doing essentially the same thing with different parameter types, different response parsing, and different error handling. It's unclear which method is the "right" one to use. Components may use the wrong method and get inconsistent results.

**Test coverage:** No tests exist for either method.

### SaleService Mixed Response Handling

**Files:** `src/app/core/services/sale.service.ts`

**Why fragile:** Global paged methods (`getPagedPlateDiscounts`, `getPagedCategoryDiscounts`) check `res.succeeded && res.data`, while per-customer CRUD methods return raw `Observable<any>` with no response validation at all. Signals in the service (`categoryDiscounts`, `plateDiscounts`) are never populated by any method.

**Safe modification:** Remove unused signals. Standardize response handling across all methods.

### MenuDetailComponent Print Logic Complexity

**Files:** `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts` (332 lines, second largest component)

**Why fragile:** The component manages print state by mutating `item._print` properties directly on the model objects (line 77-87, 157-172, 175-183). This in-place mutation pattern is error-prone and makes it hard to track state changes. The expiry date calculation logic is duplicated between `groupedMenuItems()` (lines 70-75) and `resetPrintForm()` (lines 158-162).

**Test coverage:** None.

### Login Component Uses async/await with RxJS

**Files:** `src/app/features/auth/login/login.component.ts` (lines 66-100)

**Why fragile:** The `submit()` method mixes `async/await` with `firstValueFrom()` wrapping RxJS observables. If the `AuthService`'s `login()` method changes its return type, the error will surface at runtime rather than compile time. The catch block catches all errors and displays a generic "Credenziali non valide" message, hiding potential networking or server errors.

**Safe modification:** Use a pure RxJS approach with proper error discrimination, or use Angular's `@let` / async pipe for template binding.

## Test Coverage Gaps

### No Tests Exist

**What's not tested:** The entire application has zero test files. Despite vitest being configured as a dev dependency and `tsconfig.spec.json` present, no `.spec.ts` or `.test.ts` files exist anywhere in the source tree.

**Files:** Entire `src/` directory — searched for `*.spec.ts` and `*.test.ts` patterns.

**Risk:** Any regression in core flows (authentication, paginated data loading, print label generation, invoice creation) will go undetected.

**Priority:** High

## Dependencies at Risk

### Angular 21 (Latest)

**Risk:** Angular 21 is the latest major version. While not immediately at risk, the project depends on Angular's release cadence. The `@angular/cdk` and `@angular/material` versions (^21.2.11) match the core Angular version.

**Impact:** None currently. The project is up to date.

**Migration plan:** Follow Angular's `ng update` path for future major versions.

### Prettier Configured But Not Enforced

**Risk:** Prettier is installed (`^3.8.1`) with a `.prettierrc` config file, but there is no lint-staged, husky, or CI step that enforces formatting.

**Files:** `.prettierrc`, `package.json` (devDependencies)

**Impact:** Code formatting may drift over time as developers use different editors or fail to run Prettier manually.

**Fix approach:** Add a pre-commit hook (husky + lint-staged) and a CI format check.

## Missing Critical Features

### No Global Error Handler

**Problem:** There is no Angular `ErrorHandler` implementation. Uncaught errors in the application will not be logged or reported.

**Files:** `src/app/app.config.ts` — no `ErrorHandler` provider registered.

**Blocks:** Production error monitoring and debugging.

### No Request Retry Logic

**Problem:** HTTP requests that fail due to transient errors (network flakiness, server restart) are not retried.

**Files:** `src/app/core/interceptors/` — no retry interceptor.

**Blocks:** Resilience in poor network conditions.

### No Offline Support

**Problem:** The application has no offline caching or service worker. All features require network connectivity.

**Blocks:** Cannot use the app in environments with intermittent connectivity (e.g., factory floor, delivery vehicles).

---

*Concerns audit: 2026-06-29*
