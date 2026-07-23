---
id: 260619-npj
kind: fix
subsystem: auth
tags: [login, angular, response-handling]
key-files:
  modified:
    - src/app/features/auth/login/login.component.ts
key-decisions:
  - "Use res?.token single-property gate instead of envelope check — matches actual AuthResponse shape"
  - "Removed redundant setToken call — AuthService.login() tap already sets token before observable resolves"
  - "Typed res as AuthResponse instead of any for type safety"
duration: ~3min
completed: 2026-06-19
---

# Quick Task 260619-npj: Fix false negative login bug caused by response handling mismatch

**Fixed login response validation gate to match AuthResponse shape, removed redundant setToken, and added proper type for `res`**

## Performance

- **Duration:** ~3 min
- **Started:** 2026-06-19
- **Completed:** 2026-06-19
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments

- Fixed false negative in `LoginComponent.submit()` — the old code checked `res.succeeded && res.data.token` (envelope format that doesn't exist), causing every login to fail with "Invalid response from server"
- Replaced with a single `if (!res?.token)` gate that correctly validates the actual `AuthResponse` shape (`{ token, user?, message? }`)
- Removed redundant `this.auth.setToken(res.data.token)` call — `AuthService.login()` already calls `setToken(res.token)` in its `tap` operator before the observable resolves
- Typed `res` as `AuthResponse` instead of `any` for compile-time safety
- Kept remember-email cookie logic, `loadUser()`, dashboard navigation, and `catch` block ("Credenziali non valide") unchanged

## Task Commits

1. **Task 1: Fix login response validation to match AuthResponse shape** — `40689cc` (fix)

## Files Modified

- `src/app/features/auth/login/login.component.ts` — Fixed validation gate, removed redundant setToken, added AuthResponse type

## Decisions Made

- **Login response gate:** `if (!res?.token)` — matches the actual `AuthResponse` interface. Catches both missing response and empty/falsy token. Simpler than the old envelope-style check.
- **setToken ownership:** `AuthService.login()` owns token storage via its `tap` operator. Component should not duplicate it.
- **Type safety:** Using `AuthResponse` instead of `any` enables compile-time validation of response shape.

## Deviations from Plan

None — plan executed exactly as written.

## Issues Encountered

None.

## Self-Check: PASSED

- [x] LoginComponent.submit() no longer references `res.succeeded` or `res.data`
- [x] Response validation matches AuthResponse type: checks `res?.token`
- [x] No duplicate setToken — service's tap handles it
- [x] TypeScript compilation passes with strict mode (verified via `npx tsc --noEmit --strict`)
