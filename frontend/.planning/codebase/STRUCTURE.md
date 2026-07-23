# Codebase Structure

**Analysis Date:** 2026-06-29

## Directory Layout

```
fe-roscoff/
├── .angular/                 # Angular CLI cache (generated)
├── .opencode/                # OpenCode configuration
├── .planning/                # Project planning documents
│   └── ROADMAP.md
├── .vscode/                  # VS Code settings
├── dist/                     # Build output (generated)
├── node_modules/             # Dependencies (generated)
├── public/                   # Static assets (favicon.ico)
├── src/                      # Application source
│   ├── app/
│   │   ├── core/             # Infrastructure layer
│   │   │   ├── guards/       # Route guards (auth.guard)
│   │   │   ├── interceptors/ # HTTP interceptors (api, auth, error)
│   │   │   ├── services/     # Domain services (14 total)
│   │   │   └── utils/        # Utilities (http-params)
│   │   ├── features/         # Feature modules
│   │   │   ├── auth/         # Authentication pages
│   │   │   └── dashboard/    # All back-office CRUD pages
│   │   │       ├── allergen/
│   │   │       ├── category/
│   │   │       ├── customer/
│   │   │       ├── ingredient/
│   │   │       ├── invoice/
│   │   │       ├── menu/
│   │   │       ├── order/
│   │   │       ├── plate/
│   │   │       ├── sale/
│   │   │       └── staff/
│   │   ├── layout/           # App shell (navbar, sidebar, layout)
│   │   ├── shared/           # Reusable components & models
│   │   │   ├── data-grid/    # Generic data table component
│   │   │   ├── models/       # Shared API type definitions
│   │   │   └── ui.overlay/   # Global toast/confirm/alert overlay
│   │   ├── app.component.ts  # Root component
│   │   ├── app.config.ts     # Application bootstrap config
│   │   └── app.routes.ts     # Route definitions
│   ├── assets/               # Images (logo, photos)
│   ├── environments/         # Environment configs (dev, staging)
│   ├── index.html            # HTML entry point
│   ├── main.ts               # Application bootstrap entry
│   ├── material-theme.scss   # Angular Material theme
│   └── styles.css            # Global styles
├── .editorconfig
├── .gitignore
├── .prettierrc               # Prettier formatting config
├── angular.json              # Angular CLI project config
├── Dockerfile                # Docker build for staging/prod
├── package.json              # Dependencies & scripts
├── tsconfig.json             # Base TypeScript config
├── tsconfig.app.json         # App-specific TS config
└── tsconfig.spec.json        # Test-specific TS config
```

## Directory Purposes

**`src/app/core/` (Infrastructure):**
- Purpose: Non-UI application logic — HTTP communication, auth, utility functions
- Contains: Services (14 files), HTTP interceptors (3), auth guard (1), param builder (1)
- Key files:
  - `services/auth.service.ts` — JWT token lifecycle, login, user state signals
  - `services/ui.service.ts` — Global signals for loader, toast, confirm, alert
  - `services/plate.service.ts` — Example entity CRUD service with `buildPagedParams()`
  - `services/order.service.ts` — Order service with manual HttpParams (notable outlier)
  - `interceptors/api.interceptor.ts` — Prepends `apiUrl` to relative requests
  - `interceptors/auth.interceptor.ts` — Attaches JWT Bearer token
  - `interceptors/error.interceptor.ts` — Catches 401/403, triggers auth failure
  - `guards/auth.guard.ts` — `CanMatchFn` that checks JWT validity
  - `utils/http-params.util.ts` — camelCase → PascalCase query param converter

**`src/app/features/` (Feature Pages):**
- Purpose: UI pages organized by business domain
- Contains: Auth (3 pages), Dashboard (10 main entities + ~10 sub-components)
- Key files:
  - `auth/login/login.component.ts` — Login form with cookie-based "remember me"
  - `auth/forgot-password/` — Email-based password reset request
  - `auth/reset-password/` — Token + new password form
  - `dashboard/dashboard.component.ts` — Welcome/home placeholder
  - `dashboard/plate/plate-page.component.ts` — Canonical example of list page pattern
  - `dashboard/order/order-page.component.ts` — List with date filters, status dialog
  - `dashboard/order/order-create/order-create.component.ts` — Multi-step order form with autocomplete
  - `dashboard/sale/sale.component.ts` — Tabbed view (plate discounts + category discounts)
  - `dashboard/invoice/invoice-page.component.ts` — Bulk invoice generation with expandable rows

**`src/app/layout/` (App Shell):**
- Purpose: Persistent UI shell rendered after login
- Contains: 3 components
- Key files:
  - `layout/layout.component.ts` — Sidenav container with toggle
  - `layout/navbar/navbar.component.ts` — Top toolbar with user menu, logout
  - `layout/sidebar/sidebar.component.ts` — Collapsible navigation with 3 accordion groups (Cucina, Utenze, OrdiniFatture)

**`src/app/shared/` (Reusable UI):**
- Purpose: Generic components and type definitions shared across features
- Contains: 2 components + 2 model files
- Key files:
  - `data-grid/data-grid.component.ts` — Generic `<app-data-grid>` with 6 cell types, sorting, pagination
  - `data-grid/data-grid.models.ts` — `ColumnDef<T>`, `PagedRequest`, `SortChange`, `PageChange`, `GridConfig`
  - `models/api.models.ts` — All ~50 API types (entities, requests, responses)
  - `ui.overlay/ui.overlay.component.ts` — Global overlay for toast, confirm, alert

**`src/environments/`:**
- Purpose: Environment-specific configuration
- Contains: 2 files
- Key files:
  - `environment.ts` — Dev: `apiUrl: 'http://localhost:5051/api'`
  - `environment.staging.ts` — Staging: `apiUrl: 'http://localhost:8080/api'`

## Key File Locations

**Entry Points:**
- `src/main.ts`: Application bootstrap — `bootstrapApplication(AppComponent, { providers: [...] })`
- `src/index.html`: HTML shell — `<app-root>` custom element, Material Icons + Roboto font from Google CDN
- `src/app/app.component.ts`: Root component — `<router-outlet>` + `<app-ui-overlay>`
- `src/app/app.config.ts`: `ApplicationConfig` — router, HTTP client with interceptors
- `src/app/app.routes.ts`: Route definitions — lazy loading via `loadComponent`, auth guard on protected routes

**Configuration:**
- `angular.json`: Angular CLI project config (project name: `bo-configurator`, build with `@angular/build:application`)
- `tsconfig.json`: Base TypeScript config (extends to `tsconfig.app.json`, `tsconfig.spec.json`)
- `.prettierrc`: Prettier config — `printWidth: 100`, `singleQuote: true`, angular HTML parser
- `package.json`: Dependencies — Angular 21, Material 21, RxJS 7.8, TypeScript 5.9, Prettier 3.8, Vitest 4.0

**Core Logic:**
- `src/app/core/services/`: 14 services — `allergen`, `auth`, `category`, `cookie`, `customer`, `ingredient`, `invoice`, `menu`, `order`, `plate`, `print`, `sale`, `staff`, `ui`
- `src/app/core/interceptors/`: 3 interceptors — `api.interceptor.ts`, `auth.interceptor.ts`, `error.interceptor.ts`

**Testing:**
- No test files found (`.spec.ts` or `.test.ts`) in the codebase
- Test config: `vitest` in `devDependencies`, `tsconfig.spec.json` exists
- No test helper files or fixtures detected

## Naming Conventions

**Files:**
- `kebab-case.component.ts` for Angular components — e.g., `plate-page.component.ts`, `order-create.component.ts`
- `kebab-case.service.ts` for services — e.g., `auth.service.ts`, `plate.service.ts`
- `kebab-case.interceptor.ts` for interceptors — e.g., `auth.interceptor.ts`
- `kebab-case.guard.ts` for guards — e.g., `auth.guard.ts`
- `dot.separated` for shared types — e.g., `data-grid.models.ts`, `api.models.ts`
- `kebab-case.util.ts` for utilities — e.g., `http-params.util.ts`
- SCSS/CSS files match their component file — e.g., `plate-page.component.css`

**Directories:**
- `kebab-case` for all directories — e.g., `data-grid/`, `ui.overlay/`, `order-status-dialog/`
- Feature directories use singular entity name — e.g., `plate/`, `menu/`, `customer/`, `invoice/`

**Components (TypeScript):**
- PascalCase class name matching file function — `PlatePageComponent` in `plate-page.component.ts`
- Selector prefix `app-` with kebab-case — `app-dash-plate`, `app-data-grid`, `app-ui-overlay`
- Standalone components — no `NgModule` declarations

**Signals:**
- Readonly signals exposed publicly: `readonly data = signal<T[]>([])`, `readonly loading = signal(false)`
- BehaviorSubject for query pipelines: `readonly querySubject = new BehaviorSubject<EntityRequest>(initial)`

**Services:**
- `@Injectable({ providedIn: 'root' })` — tree-shakable singletons
- Method names follow CRUD: `getPaged()`, `getById()`, `create()`, `update()`, `delete()`
- Some entity-specific methods: `getFoodCost()`, `getNutrition()`, `downloadTechnicalSheet()`

## Where to Add New Code

**New Feature (e.g., new entity):**
1. Create feature directory: `src/app/features/dashboard/<entity>/`
2. Create components:
   - `<entity>-page.component.ts` — List view with DataGrid (follow `plate-page.component.ts` pattern)
   - `<entity>-form.component.ts` — Create/edit form (optional)
   - `<entity>-detail.component.ts` — Detail view (optional)
   - `<entity>-dialog/` — Dialog sub-components (optional)
3. Add API models to `src/app/shared/models/api.models.ts` (entity interface, create/update request, paged request)
4. Add service in `src/app/core/services/<entity>.service.ts` (extends CRUD pattern)
5. Add route in `src/app/app.routes.ts` under the protected layout children
6. Add sidebar link in `src/app/layout/sidebar/sidebar.component.ts` and its HTML template

**New Shared Component:**
- Implementation: `src/app/shared/<component-name>/`
- Name files: `<component-name>.component.ts`, `<component-name>.component.html`, `<component-name>.component.css`
- Follow standalone pattern, add to `imports` of consuming components

**New Utility:**
- Add file in `src/app/core/utils/<utility-name>.util.ts`
- Export pure functions (no classes needed unless stateful)

**Utilities:**
- Shared helpers: `src/app/core/utils/` or `src/app/shared/` (if presentation-related)

## Special Directories

**`.angular/`:**
- Purpose: Angular CLI cache and build artifacts
- Generated: Yes (by `ng serve` / `ng build`)
- Committed: No (in `.gitignore`)

**`dist/`:**
- Purpose: Build output
- Generated: Yes (by `ng build`)
- Committed: No (in `.gitignore`)

**`node_modules/`:**
- Purpose: npm dependencies
- Generated: Yes (by `npm install`)
- Committed: No (in `.gitignore`)

**`public/`:**
- Purpose: Static files served directly without processing
- Generated: No
- Committed: Yes
- Contains: `favicon.ico`

**`src/assets/`:**
- Purpose: Static assets processed by Angular build
- Generated: No
- Committed: Yes
- Contains: Logo image, product photos

---

*Structure analysis: 2026-06-29*
