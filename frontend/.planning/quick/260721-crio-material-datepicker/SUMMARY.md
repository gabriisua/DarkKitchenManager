---
slug: crio-material-datepicker
status: complete
date: 2026-07-21
---

# Summary: Replace Native Date Inputs with Material Datepicker

Replaced browser-native `<input type="date">` with Angular Material `<mat-datepicker>` for Crio date controls.

## Changes

### Template (`menu-detail.component.html`)
- Replaced native date inputs with `<mat-form-field>` + `<mat-datepicker>` components
- Used unique picker references (`#thawPicker`, `#prodPicker`) for each conditional branch
- Labels: "Data di Produzione" (frozen) / "Data di Scongelamento" (thawed)

### Styles (`menu-detail.component.scss`)
- Replaced `.crio-date-input-group`, `.crio-date-label`, `.crio-date-input` with `.crio-datepicker-field`
- Uses `::ng-deep` to hide subscript wrapper and style the Material field compactly

Build: ✅ successful
