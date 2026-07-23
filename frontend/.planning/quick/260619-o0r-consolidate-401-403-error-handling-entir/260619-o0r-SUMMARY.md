---
phase: quick
plan: 01
subsystem: core/interceptors
tags: ["error-handling", "401", "403", "auth", "interceptors", "consolidation"]
dependency-graph:
  requires: []
  provides: ["error.interceptor.ts is the single 401/403 handler"]
  affects: ["auth.service.ts::handleAuthFailure callers"]
tech-stack:
  added: []
  patterns: ["Centralized error interceptor with public-endpoint guard pattern"]
key-files:
  created: []
  modified:
    - src/app/core/interceptors/auth.interceptor.ts
    - src/app/core/interceptors/error.interceptor.ts
decisions: []
metrics:
  duration: "~5 min"
  completed-date: "2026-06-19"
---

# Phase Quick Plan 01: Consolidate 401/403 Error Handling into error.interceptor.ts

Strip duplicate 401/403 logic from auth.interceptor.ts and add public-endpoint awareness to error.interceptor.ts, making error.interceptor.ts the single source of truth for auth-failure handling.

## Summary

The auth interceptor was redundantly handling 401/403 errors with a `catchError` pipe and `publicEndpoints` guard, but since Angular interceptors process errors in reverse order (error interceptor runs first), the error interceptor's unconditional `handleAuthFailure()` always fired first — making the auth interceptor's guard dead code. This change removes the duplicate from the auth interceptor and moves the `isPublic` guard into the error interceptor so login/reset-password/forgot-password failures correctly skip logout/redirect.

## Results

**auth.interceptor.ts** — Token-only interceptor (6 insertions, 31 deletions):
- Removed `publicEndpoints` array, `catchError` pipe, `Router` injection
- Removed unused imports: `HttpErrorResponse`, `Router`, `catchError`, `throwError`
- Only attaches Bearer token, no error handling at all

**error.interceptor.ts** — Centralized 401/403 handler (5 insertions, 4 deletions):
- Added `publicEndpoints` array: `['login', 'reset-password', 'forgot-password']`
- Added `isPublic` guard so public-endpoint 401/403 skips `handleAuthFailure()`
- Removed redundant `router.navigate(['/'])` — already inside `handleAuthFailure()`
- Removed unused `Router` import and injection

## Deviations from Plan

None — plan executed exactly as written.

## Success Criteria

| Scenario | Behavior | Status |
|----------|----------|--------|
| Login 401 | `isPublic=true` → skip `handleAuthFailure` → error propagates to component | ✅ |
| Protected API 401 | `isPublic=false` → calls `handleAuthFailure` → clear token + navigate to / | ✅ |
| Protected API 403 | Same as 401 (handled identically) | ✅ |
| auth.interceptor.ts | Only attaches Bearer token, no error handling | ✅ |
| Duplicate 401/403 logic | Removed from codebase | ✅ |

## Verification

- TypeScript compilation passes for both files (project tsconfig.app.json)
- auth.interceptor.ts: zero references to `catchError`, `throwError`, `HttpErrorResponse`, `Router`, `publicEndpoints`
- error.interceptor.ts: contains `publicEndpoints` (all 3 values), `isPublic` guard, `handleAuthFailure` call
- error.interceptor.ts: zero references to `Router`

## Self-Check: PASSED

- [x] auth.interceptor.ts exists and compiles
- [x] error.interceptor.ts exists and compiles
- [x] Commit 1: `a1f3891` — strip 401/403 from auth.interceptor.ts
- [x] Commit 2: `a5a9e8e` — add public-endpoint guard to error.interceptor.ts
- [x] auth.interceptor.ts has 0 forbidden patterns (catchError, publicEndpoints, HttpErrorResponse, Router)
- [x] error.interceptor.ts has required patterns (publicEndpoints, isPublic, handleAuthFailure)
- [x] error.interceptor.ts has 0 references to Router
