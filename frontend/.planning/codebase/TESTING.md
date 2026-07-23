# Testing Patterns

**Analysis Date:** 2026-06-29

## Test Framework

**Runner:**
- Vitest 4.0.8
- Config: Angular CLI test builder `@angular/build:unit-test` configured in `angular.json` (line 87-89)
- No standalone `vitest.config.*` file detected

**Assertion Library:**
- Vitest built-in assertions (vitest/globals type support configured)

**Run Commands:**
```bash
npm test              # ng test — runs Vitest via Angular CLI
```

## Test File Organization

**Location:**
- No test files currently exist in the codebase
- `tsconfig.spec.json` includes pattern `src/**/*.spec.ts` — expected location is co-located with source files
- `tsconfig.app.json` explicitly excludes `src/**/*.spec.ts`

**Naming:**
- Expected convention (from tsconfig): `*.spec.ts` (e.g., `auth.service.spec.ts`, `login.component.spec.ts`)
- No `.test.ts` files detected

**Structure:**
```
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── auth.service.spec.ts       # expected
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts
│   │   │   └── auth.interceptor.spec.ts   # expected
│   │   └── guards/
│   ├── features/
│   │   ├── dashboard/
│   │   │   ├── ingredient/
│   │   │   │   ├── ingredient-page.component.ts
│   │   │   │   └── ingredient-page.component.spec.ts # expected
│   │   │   └── ingredient-dialog/
│   │   │       ├── ingredient-dialog.component.ts
│   │   │       └── ingredient-dialog.component.spec.ts # expected
│   │   └── auth/
│   │       └── login/
│   │           ├── login.component.ts
│   │           └── login.component.spec.ts # expected
│   └── shared/
│       ├── data-grid/
│       └── models/
```

## Test Structure

**Suite Organization:**
- No existing tests to reference. Expected convention (standard Angular + Vitest):
  - `describe('ServiceName', () => { ... })` for services
  - `describe('ComponentName', () => { ... })` for components
  - `beforeEach` for TestBed configuration setup
  - `it('should ...', () => { ... })` for individual test cases

**Patterns:**
- TestBed likely needed for component tests (Angular standalone components)
- `HttpTestingController` for HTTP client mocking in service tests
- Component harnesses from `@angular/cdk/testing` for Material component interaction

**Coverage gaps:**
- Zero test coverage — no `*.spec.ts` files exist anywhere in the codebase
- All services, components, guards, interceptors, and utilities are untested

## Mocking

**Framework:** Vitest built-in (`vi.fn()`, `vi.spyOn()`)

**Patterns:**
- No existing mocks to reference
- Expected patterns for this codebase:
  - HTTP calls: `HttpClient` mocking via `HttpTestingController` or Vitest `vi.fn()` on service methods
  - Services: Override in TestBed providers or inject mocks
  - Router: `provideRouter` or mock `Router` with `navigate` spy
  - `MatDialog`: Mock `open()` method returning `{ afterClosed: () => of(result) }`
  - Signals: Directly call `.set()` on signal mocks (Angular signals are plain functions)

```typescript
// Expected pattern for dialog mocking
const mockDialogRef = {
  afterClosed: () => of(true)
};
const dialogSpy = vi.fn().mockReturnValue(mockDialogRef);
```

**What to Mock:**
- HTTP requests (all services depend on `HttpClient`)
- `MatDialog.open()` in component tests
- `Router.navigate()` for navigation assertions
- External service calls when testing components

**What NOT to Mock:**
- Angular signals (can be tested directly)
- `UiService` (keep real for integration or mock its signal state)
- Simple utility functions like `buildPagedParams`

## Fixtures and Factories

**Test Data:**
- No existing test fixtures detected
- Expected approach: inline test data objects matching the `api.models.ts` interfaces

**Location:**
- Expected: co-located `*.spec.ts` files or `__mocks__/` directories within feature modules

## Coverage

**Requirements:** Not enforced (no coverage configuration detected)

**View Coverage:**
```bash
npm test -- --coverage       # If Vitest coverage is configured
```

**Current state:** 0% — no tests exist

## Test Types

**Unit Tests:**
- Not yet implemented
- Priority candidates:
  - All services in `src/app/core/services/` (auth, ingredient, plate, menu, customer, staff, allergen, order, sale, invoice, print, cookie, category, ui)
  - All interceptors in `src/app/core/interceptors/` (auth, api, error)
  - Auth guard in `src/app/core/guards/`
  - Utility `buildPagedParams` in `src/app/core/utils/`

**Integration Tests:**
- Not yet implemented
- Priority candidates:
  - Page components with data grid integration (e.g., `IngredientPageComponent`, `CustomerPageComponent`)
  - Dialog flow (open dialog → fill form → submit → refresh table)

**E2E Tests:**
- Not used. No Cypress, Playwright, or Protractor configuration detected

## Common Patterns

**Async Testing:**
- Expected with Vitest:
```typescript
it('should load ingredients', async () => {
  const result = await firstValueFrom(service.getPaged(mockParams));
  expect(result.items).toHaveLength(3);
});
```

**Error Testing:**
- Expected pattern:
```typescript
it('should handle API error', async () => {
  httpMock.expectOne(req => req.url.includes('/Ingredient')).flush(
    { message: 'Server error' },
    { status: 500, statusText: 'Server Error' }
  );
  // Assert error state in component signals
});
```

## Test Dependencies

**Available in package.json:**
- `vitest` ^4.0.8 — test runner
- `jsdom` ^28.0.0 — DOM environment for component tests (in devDependencies)
- `@angular/build` ^21.2.8 — provides `@angular/build:unit-test` builder (uses Vitest under the hood)
- `@angular/compiler-cli` ^21.2.0 — required for Angular template compilation in tests

**Missing (not explicitly configured but likely needed):**
- No `vitest.config.ts` — Vitest config inherited from Angular CLI builder defaults
- No `src/test-setup.ts` — no test setup file for global mocks (e.g., `localStorage`, `document.cookie`)
- No Angular Testing Library (`@testing-library/angular`) — not in dependencies but recommended for component tests
- No `@angular/cdk/testing` harnesses configured — needed for Material component interaction

## Configuration

**tsconfig.spec.json:**
```json
{
  "extends": "./tsconfig.json",
  "compilerOptions": {
    "outDir": "./out-tsc/spec",
    "types": ["vitest/globals"]
  },
  "include": ["src/**/*.d.ts", "src/**/*.spec.ts"]
}
```

**angular.json test builder:**
```json
"test": {
  "builder": "@angular/build:unit-test"
}
```

---

*Testing analysis: 2026-06-29*
