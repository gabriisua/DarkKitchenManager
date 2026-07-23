# Coding Conventions

**Analysis Date:** 2026-06-29

## Naming Patterns

**Files:**
- Components: `*.component.ts` (e.g., `ingredient-page.component.ts`, `login.component.ts`)
- Services: `*.service.ts` (e.g., `auth.service.ts`, `ingredient.service.ts`)
- Models: `*.models.ts` (e.g., `api.models.ts`, `data-grid.models.ts`)
- Guards: `*.guard.ts` (e.g., `auth.guard.ts`)
- Interceptors: `*.interceptor.ts` (e.g., `auth.interceptor.ts`, `api.interceptor.ts`, `error.interceptor.ts`)
- Utilities: `*.util.ts` (e.g., `http-params.util.ts`)
- Templates: `*.component.html` (co-located with component)
- Styles: `*.component.css` or `*.component.scss` (co-located, mixed usage across the codebase)
- Config files: `*.config.ts` (e.g., `app.config.ts`)
- Routes: `*.routes.ts` (e.g., `app.routes.ts`)

**Functions:**
- camelCase for all function/method names (e.g., `loadData()`, `onSortChange()`, `deleteIngredient()`)
- Private methods prefixed with `private` keyword (e.g., `private loadData()`, `private initForm()`)
- Event handlers follow `on[EventName]` pattern (e.g., `onSearch()`, `onPageChange()`, `onFilterChange()`)
- CRUD methods use imperative names: `addIngredient()`, `editIngredient()`, `deleteIngredient()`
- Service methods use HTTP-verb-inspired names: `getPaged()`, `getById()`, `create()`, `update()`, `delete()`

**Variables:**
- camelCase for all variables (e.g., `searchTerm`, `isSubmitting`, `serverError`)
- `snake_case` or `camelCase` for API response fields (both appear, e.g., `customerBusinessName`)
- Boolean variables use `is` prefix (e.g., `isActive`, `isEditMode`, `isSubmitting`)
- Signals appended with matching type names (e.g., `data`, `totalItems`, `loading`, `error`)
- BehaviorSubject instances named with `Subject` or `query` suffix (e.g., `querySubject`, `plateQuery`)

**Types:**
- PascalCase for interfaces and enums (e.g., `AuthResponse`, `Ingredient`, `OrderStatus`, `ColumnDef<T>`)
- Generic type parameter `T` for reusable grid types (`ColumnDef<T>`, `PagedResponse<T>`)
- Request/response interfaces grouped by entity domain (e.g., `IngredientCreateRequest`, `LoginRequest`)
- Enums used for fixed status sets (e.g., `OrderStatus { Pending = 1, Confirmed = 2, ... }`)
- Interfaces defined as `export interface` (always exported)

## Code Style

**Formatting:**
- Prettier 3.8.1 configured in `.prettierrc`
- Print width: 100 characters
- Single quotes for strings (both TS/JS and HTML templates)
- Override for `*.html` files: parser set to `angular`
- EditorConfig (`.editorconfig`): 2-space indent, UTF-8, trailing whitespace trimmed
- `quote_type = single` in EditorConfig for `.ts` files

**Linting:**
- No ESLint configuration detected
- TypeScript strict mode enabled via `tsconfig.json`: `strict: true`, `noImplicitOverride: true`, `noPropertyAccessFromIndexSignature: true`, `noImplicitReturns: true`, `noFallthroughCasesInSwitch: true`
- Angular compiler strictness: `strictInjectionParameters: true`, `strictInputAccessModifiers: true`, `strictTemplates: true`

**Braces and Spacing:**
- One space before opening braces in function signatures: `loadData(): void {`
- No space before parentheses in function declarations: `onSearch(value: string)`
- Space after `//` for comments
- Consistent use of blank lines around section divider comments

## Import Organization

**Order:**
1. Angular framework imports (`@angular/core`, `@angular/common`, `@angular/forms`, `@angular/router`)
2. RxJS imports (`rxjs`, `rxjs/operators`)
3. Angular CDK / Material imports (`@angular/material/*`)
4. Application core imports (`../../../core/services/*`, `../../../core/interceptors/*`)
5. Shared model imports (`../../shared/models/api.models`)
6. Shared component imports (`../../shared/data-grid/data-grid.component`)
7. Local feature imports (`./ingredient-dialog/ingredient-dialog.component`)

Within each group, imports are typically alphabetized but not strictly enforced.

**Path Aliases:**
- No path aliases configured in `tsconfig.json` — all imports use relative paths (e.g., `../../../environments/environment`)

## Error Handling

**Patterns:**
- Angular HTTP interceptors handle cross-cutting errors: `auth.interceptor.ts` adds Bearer token, `error.interceptor.ts` catches 401/403 for auth failures, `api.interceptor.ts` prepends `environment.apiUrl`
- Service layer: errors propagate via RxJS `throwError` or `catchError` in interceptors
- Component layer: errors caught in `.subscribe({ error: ... })` callbacks
- `console.error()` used for logging errors to dev console
- User-facing errors shown via `UiService.showToast(message, 'error')` for non-blocking errors
- Blocking errors use `UiService.showAlert(title, message)` which renders a modal overlay
- Error messages extracted from `err.error?.message` when available, otherwise fallback defaults
- Delete operations use `UiService.askConfirm()` with a confirmation dialog pattern before proceeding

```typescript
// Standard error handling pattern in components
this.ingredientService.getPaged(query).subscribe({
  next: (res) => {
    this.data.set(res.items);
    this.totalItems.set(res.totalCount);
    this.loading.set(false);
  },
  error: (err) => {
    console.error('Errore caricamento ingredienti:', err);
    this.loading.set(false);
    this.data.set([]);
    this.totalItems.set(0);
    const errorMsg = 'Si è verificato un errore durante il caricamento.';
    this.error.set(errorMsg);
    this.uiService.showToast(errorMsg, 'error');
  }
});
```

## Logging

**Framework:** `console` (no dedicated logging library)

**Patterns:**
- `console.error()` in catch blocks for debugging
- No `console.log()` statements found in production code
- Error messages in Italian for user-facing text, English for technical console messages

## Comments

**When to Comment:**
- Section dividers with `// ==========================================` and comment text
- Explanation of non-obvious logic (e.g., "Se il backend usa il wrapper anche per il patch, lo estraiamo")
- Inline TODO notes, warnings about ordering (e.g., `// "new" messo rigorosamente PRIMA di ":id"`)
- JSDoc-style comments are NOT used — no `/** ... */` documentation blocks on functions

**JSDoc/TSDoc:**
- Not used. No JSDoc/TSDoc annotations found on any functions or classes.

**Code as documentation:**
- Section comments in Italian for UI-related features
- English comments for technical decisions and API notes
- Inline comments sometimes contain Italian descriptions of API behavior

```typescript
// ==========================================
// GET: Recupera lista paginata con filtri
// ==========================================

// --- INIZIO BLOCCO PLATES RIPARATO ---

// Stessa regola del "new": messo prima di ":id"
```

## Function Design

**Size:**
- Services: methods are 1-10 lines, focused on single HTTP operations
- Components CRUD: methods are 10-30 lines including subscribe blocks
- `loadData()` methods typically 10-20 lines
- `ngOnInit()` ranges from 3-15 lines

**Parameters:**
- 0-2 parameters typical for service methods
- Event handlers take single event/payload object parameter
- Delete operations take the entity row as parameter

**Return Values:**
- Services: return `Observable<T>` for HTTP operations; `void` for fire-and-forget methods (e.g., `loadAll()`)
- Component methods: return `void` (reactive, event-driven)
- `async/await` used in `LoginComponent.submit()` only — rest of codebase uses RxJS subscriptions

## Module Design

**Exports:**
- All components, services, guards, interceptors are exported at definition site
- No barrel (`index.ts`) files used — imports reference files directly
- Models exported individually via `export interface`

**Barrel Files:**
- Not used. Each file is imported directly by path.

## Angular Conventions

**Standalone:**
- All components use `standalone: true` (Angular 21+ style, no NgModules)
- `bootstrapApplication()` used in `main.ts`
- `app.config.ts` provides router and HTTP client with interceptors

**Dependency Injection:**
- Mixed approach — both constructor injection and `inject()` function used:
  - Services: predominantly constructor injection (`constructor(private http: HttpClient) {}`)
  - Components: predominantly `inject()` function (`private ingredientService = inject(IngredientService)`)
- `providedIn: 'root'` for all services (singleton services)

**State Management:**
- Angular signals (`signal<T>()`, `computed()`) for component state
- `BehaviorSubject` for query/request streams with `switchMap` for reactive data loading
- RxJS `takeUntilDestroyed(this.destroyRef)` for automatic cleanup (from `@angular/core/rxjs-interop`)

**Interceptors:**
- Functional interceptors (`HttpInterceptorFn`) — not class-based
- Three interceptors in order: `apiInterceptor` → `authInterceptor` → `errorInterceptor`

**Component Architecture Pattern:**
- Smart components (page-level) contain all state and logic
- Presentation components (e.g., `DataGridComponent`) receive inputs and emit outputs
- Content projection via `<ng-content>` with selectors (`gridSearch`, `gridFilters`)
- Template refs via `@ViewChild('actionsTemplate', { static: true })`

## HTML Templates

**Template Style:**
- Angular 17+ `@if` / `@for` / `@switch` control flow syntax used throughout
- Angular Material components with `mat-table`, `mat-paginator`, `mat-form-field`, `mat-dialog`
- `matTooltip` on action buttons for accessibility
- Two-way binding via `[(ngModel)]`
- Component templates use separate `.html` files; small components may use inline `template:`

```html
@if (error; as errMsg) {
  <div class="error-banner">...</div>
}

@for (col of columns; track trackByField($index, col)) {
  ...
}
```

---

*Convention analysis: 2026-06-29*
