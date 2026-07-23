# Roscoff Back-Office (bo-configurator)

## What This Is

An Angular 21 back-office dashboard for restaurant management. Staff manage customers, allergens, ingredients, and plates (dishes) through a Material Design UI with JWT authentication. Currently being refactored to consume standardized paginated API endpoints with a unified data grid architecture.

## Core Value

Staff can efficiently browse, filter, and manage all restaurant entities through consistent, fast, paginated tables.

## Requirements

### Validated

- ✓ JWT authentication (login, logout, token refresh, session persistence) — existing
- ✓ Password reset flow (request + confirm via email token) — existing
- ✓ Staff CRUD with paginated table and create/edit dialogs — existing
- ✓ Customer CRUD with table view and form view — existing
- ✓ Allergen CRUD with paginated table and dialogs — existing
- ✓ Ingredient CRUD with split-panel (list + inline create) — existing
- ✓ Plate CRUD with card grid, multi-step creation form, detail view with food cost/nutrition — existing
- ✓ Angular Material UI with sidebar navigation, navbar with user menu — existing
- ✓ HTTP interceptors for bearer token injection and API base URL — existing
- ✓ Global toast/confirm/loader overlay service — existing

### Active

- [ ] **GRID-01**: Create a reusable, generic Data Grid component using Angular 21 standalone components
- [ ] **GRID-02**: Grid must handle pagination (page/pageSize), column sorting, and consistent styling
- [ ] **GRID-03**: Grid accepts generic inputs (columns config, data, total items) and emits outputs (onSortChange, onPageChange)
- [ ] **SVC-01**: Standardize all API GET services with common query parameters: `Page`, `PageSize`, `Search`, `SortColumn`, `SortDirection`, `DateFrom`, `DateTo`
- [ ] **SVC-02**: Create/update TypeScript interfaces for standardized paginated request params and API responses
- [ ] **VIEW-01**: Implement Customer list view with reusable grid + filter section (filters: `Type`, `IsActive`)
- [ ] **VIEW-02**: Implement Ingredient list view with reusable grid + filter section (filters: `Name`, `MinEnergyKcal`, `MaxEnergyKcal`, `MinCost`, `MaxCost`, `IsActive`)
- [ ] **VIEW-03**: Implement Plate list view with reusable grid + filter section (filters: `Name`, `CategoryId`, `IsActive`, `MinPrice`, `MaxPrice`)
- [ ] **VIEW-04**: Implement Staff list view with reusable grid + filter section (filters: `Email`, `Role`)
- [ ] **VIEW-05**: Implement Allergen list view with reusable grid + filter section (filters: `Name`, `Code`)
- [ ] **ARCH-01**: Use smart/dumb component architecture — entity page is smart (API call + filter forms), grid component is dumb (renders data)

### Out of Scope

- Backend API implementation — deferred to separate project
- Auth pages (login, forgot password, reset password) — not related to grid refactor
- Dashboard welcome/home page — not a table-based view
- Mobile/native app — web-only back-office

## Context

Existing Angular 21 app built with standalone components, Angular Material, and RxJS. Services currently use inconsistent patterns — some load data into signals, others return Observables directly. The backend has been updated to standardize paginated GET endpoints with a uniform query parameter contract. This refactor aligns the frontend with the backend contract while eliminating duplicated table HTML/CSS across entity pages.

The OpenAPI spec for the new backend endpoints has been provided and shows the exact parameter shapes for each entity.

## Constraints

- **Tech Stack**: Angular 21, Angular Material, TypeScript, RxJS — must stay within existing stack
- **Architecture**: Standalone components with feature-based directory structure
- **API Contract**: All paginated GET endpoints share `Page`, `PageSize`, `Search`, `SortColumn`, `SortDirection`, `DateFrom`, `DateTo` + entity-specific params
- **No Backend**: Only frontend changes — no backend modification

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| All tables migrated to new grid (including Allergen) | Consistency across the entire app | — Pending |
| Backend work deferred | Keep scope focused on frontend | — Pending |
| Customer filter uses `Type` (not `Name`) | Matches OpenAPI spec | — Pending |
| Smart/dumb component pattern | Clean separation of concerns | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-05-29 after initialization*
