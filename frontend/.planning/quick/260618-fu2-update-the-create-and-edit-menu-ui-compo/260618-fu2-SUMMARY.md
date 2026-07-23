---
phase: quick
plan: 260618-fu2
subsystem: ui
tags: [angular, material, menu, b2b, pricing, datepicker]
provides:
  - Customer dropdown in menu create/edit form for B2B price lists
  - Per-plate override price with Euro→cents conversion
  - Per-plate validity date pickers (AvailableFrom/AvailableTo) with date consistency validation
affects: [menu-detail, sales-discounts]
tech-stack:
  added: [MatDatepickerModule, MatNativeDateModule]
  patterns: [Euro↔Cents conversion via overridePriceEuro/100, optional field spreading in payload]
key-files:
  modified:
    - src/app/shared/models/api.models.ts
    - src/app/features/dashboard/menu/menu-form/menu-form.component.ts
    - src/app/features/dashboard/menu/menu-form/menu-form.component.html
    - src/app/features/dashboard/menu/menu-form/menu-form.component.scss
key-decisions: []
requirements-completed: []
duration: 15min
completed: 2026-06-18
---

# Quick Task 260618-fu2: Update the Create and Edit Menu UI Components

**Customer dropdown, per-plate B2B prices with Euro↔cents conversion, and per-plate validity date pickers for the Menu create/edit form**

## Performance

- **Duration:** 15 min
- **Started:** 2026-06-18T09:20:00Z (approx)
- **Completed:** 2026-06-18T09:29:00Z
- **Tasks:** 4
- **Files modified:** 4

## Accomplishments

- Added `customerId` field to `Menu` and `MenuCreateRequest` interfaces, and `overridePrice`/`availableFrom`/`availableTo` to `MenuItemDto` and request menu items
- Integrated `CustomerService.getPaged` to populate a customer dropdown in the form with "Nessun cliente (Menu Globale)" default option
- Added per-plate price input (Euro) with cents conversion on submit via `Math.round(overridePriceEuro * 100)`, and cents→Euro on load (`overridePrice / 100`)
- Added per-plate `AvailableFrom`/`AvailableTo` date pickers using Angular Material Datepicker, with validation preventing `AvailableTo` being earlier than `AvailableFrom`
- Added compact SCSS styles for inline price (120px) and date (150px) fields within item rows

## Task Commits

Each task was committed atomically:

1. **Task 1: Update API models** — `5f87710` (feat)
2. **Task 2: Add CustomerService injection and form logic** — `cc6a686` (feat)
3. **Task 3: Update template with customer dropdown and per-plate fields** — `ce5e61a` (feat)
4. **Task 4: Add SCSS styles for new form fields** — `3f63144` (feat)

## Files Modified

- `src/app/shared/models/api.models.ts` — Menu, MenuItemDto, MenuCreateRequest interfaces updated with customerId, overridePrice, availableFrom, availableTo
- `src/app/features/dashboard/menu/menu-form/menu-form.component.ts` — CustomerService DI, customers signal, loadCustomers(), date validation, cent conversion, MatDatepickerModule imports
- `src/app/features/dashboard/menu/menu-form/menu-form.component.html` — Customer dropdown in form card, per-plate price + date pickers in item rows
- `src/app/features/dashboard/menu/menu-form/menu-form.component.scss` — .item-price-field (120px) and .item-date-field (150px) styles with compact 40px height

## Decisions Made

- None — plan executed exactly as specified. Followed the existing Euro↔cents conversion pattern from `sale-plate-form.component.ts`.

## Deviations from Plan

None — plan executed exactly as written.

## Issues Encountered

None — all tasks completed cleanly. Build passes with no TypeScript errors.

## Known Stubs

None — all fields are wired with real data sources (CustomerService, menu API).

## Threat Flags

None — all new model fields are consumed only by the Menu form component, which is already behind auth guards.

## Next Phase Readiness

- Menu create/edit form fully supports B2B price lists
- Ready for future work on sales/discounts pages that read customer-specific pricing
- Menu detail view was intentionally not modified per plan constraints

---

*Phase: quick*
*Completed: 2026-06-18*
