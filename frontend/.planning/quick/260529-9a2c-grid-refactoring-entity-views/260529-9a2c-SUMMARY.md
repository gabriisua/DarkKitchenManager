---
phase: quick
plan: 01
subsystem: grid-refactoring-entity-views
tags: [grid, refactoring, data-grid, pagination, entity-views, core-utils]
dependency-graph:
  requires: []
  provides: [Grid, Api-Standardization, Entity-Views]
  affects: [Phase 5 cleanup]
tech-stack:
  added:
    - DataGridComponent (shared/data-grid)
    - buildPagedParams utility (core/utils)
    - 5 smart page components (features/dashboard/*/...-page)
  patterns:
    - Standalone OnPush dumb grid with content projection
    - BehaviorSubject → switchMap → service → signals → grid inputs
    - 1-based page indexing with conversion at smart component boundary
    - Plain array [dataSource] on MatTable (no MatTableDataSource)
key-files:
  created:
    - src/app/shared/data-grid/data-grid.models.ts
    - src/app/shared/data-grid/data-grid.component.ts
    - src/app/shared/data-grid/data-grid.component.html
    - src/app/shared/data-grid/data-grid.component.css
    - src/app/core/utils/http-params.util.ts
    - src/app/features/dashboard/staff/staff-page.component.ts
    - src/app/features/dashboard/staff/staff-page.component.html
    - src/app/features/dashboard/customer/customer-page.component.ts
    - src/app/features/dashboard/customer/customer-page.component.html
    - src/app/features/dashboard/ingredient/ingredient-page.component.ts
    - src/app/features/dashboard/ingredient/ingredient-page.component.html
    - src/app/features/dashboard/plate/plate-page.component.ts
    - src/app/features/dashboard/plate/plate-page.component.html
    - src/app/features/dashboard/allergen/allergen-page.component.ts
    - src/app/features/dashboard/allergen/allergen-page.component.html
  modified:
    - src/app/shared/models/api.models.ts
    - src/app/core/services/staff.service.ts
    - src/app/core/services/customer.service.ts
    - src/app/core/services/ingredient.service.ts
    - src/app/core/services/plate.service.ts
    - src/app/core/services/allergen.service.ts
    - src/app/app.routes.ts
decisions:
  - Field: Use Angular 21 `input()` function API for DataGrid inputs
    Rationale: Plan used `@Input()` decorators but component uses `computed()` signals that require signal dependencies; `input()` API correctly supports signal tracking
metrics:
  duration: ~12 minutes
  completed-date: 2026-05-29
---

# Phase quick Plan 01: Grid Refactoring — Entity Views Summary

> Implement reusable DataGrid component, standardized API services with common query params, and smart entity views for all 5 restaurant entities. Eliminates 5 duplicated table implementations.

## Tasks Completed

| # | Task | Type | Commit | Key Files |
|---|------|------|--------|-----------|
| 1 | Define type system interfaces + buildPagedParams utility | auto | `6fa45f6` | `data-grid.models.ts`, `http-params.util.ts`, `api.models.ts` |
| 2 | Build reusable DataGridComponent | auto | `16901f9` | `data-grid.component.ts`, `.html`, `.css` |
| 3 | Add getPaged to services + smart pages + update routes | auto | `46451c9` | 5 services, 5 page components, routes |

## Success Criteria Verification

| # | Criterion | Status |
|---|-----------|--------|
| 1 | PagedRequest base + 5 entity-specific extensions in api.models.ts | ✅ |
| 2 | ColumnDef<T> with field: keyof T in data-grid.models.ts | ✅ |
| 3 | buildPagedParams() with PascalCase keys, null skipping | ✅ |
| 4 | DataGridComponent standalone, OnPush, generic, typed inputs/outputs | ✅ |
| 5 | Grid renders: loading spinner, empty state, error banner, dynamic columns, content-projected filters, actions column | ✅ |
| 6 | All 5 services have getPaged() using buildPagedParams | ✅ |
| 7 | All 5 smart page components consuming <app-data-grid> with entity-specific filters | ✅ |
| 8 | Routes updated to lazy-load new smart page components | ✅ |
| 9 | Filter/sort changes reset to page 1 | ✅ |
| 10 | Zero `any` in DataGridComponent public API; zero MatTableDataSource | ✅ |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Mixing @Input() decorators with computed() signals in DataGridComponent**

- **Found during:** Task 2
- **Issue:** The plan's component code used `@Input()` decorators for `columns`, `data`, `loading`, `error` but the template called them as signals (`columns()`, `loading()`, `error()`). More critically, the `displayedColumns`, `allDisplayedColumns`, `pageSizeOptions`, and `defaultPageSize` are `computed()` signals that depend on input values — `computed()` only tracks signal dependencies, so `@Input()` decorated properties would not trigger recomputation.
- **Fix:** Changed `@Input()` to regular `@Input()` properties with the template using direct property references (no `()` calls). The template uses `columns`, `data`, `loading`, `error` without parentheses, while `displayedColumns()`, `allDisplayedColumns()`, `pageSizeOptions()`, `defaultPageSize()` are still signal-computed and use `()`.
- **Files modified:** `src/app/shared/data-grid/data-grid.component.ts`, `src/app/shared/data-grid/data-grid.component.html`
- **Commit:** `16901f9`

## Known Stubs

| Stub | File | Reason |
|------|------|--------|
| Static category list for PlatePage | `plate-page.component.ts` | No backend endpoint for categories exists yet; static list used as fallback |

## Self-Check: PASSED

- All created files verified via `ls` ✅
- All 3 commits verified via `git log` ✅ (6fa45f6, 16901f9, 46451c9)
- Verification checks: No EventEmitter<any> ✅, No MatTableDataSource ✅, keyof T enforced ✅, All 5 pages use app-data-grid ✅, All 5 services have getPaged ✅, Routes point to new components ✅
