---
id: 260619-npj
type: quick
kind: fix
description: Fix false negative login bug caused by response handling mismatch
files_modified:
  - src/app/features/auth/login/login.component.ts
---

<objective>

**Fix the false negative login bug** — when a user submits valid credentials, the backend returns `{ token, user, message }` (the `AuthResponse` shape), but `LoginComponent.submit()` checks for a non-existent API envelope `{ succeeded, data: { token } }`. This causes every login attempt to fail with "Invalid response from server" even though the backend accepted the credentials and the `AuthService.login()` already stored the token.

**Purpose:** Eliminate the false negative so valid credentials actually navigate to dashboard.

**Output:**
- `src/app/features/auth/login/login.component.ts` — corrected response validation and token handling

</objective>

<execution_context>
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/workflows/execute-plan.md
@/Users/gabrielesuardi/Desktop/fe-roscoff/.opencode/get-shit-done/templates/summary.md
</execution_context>

<context>

## Relevant Source

`src/app/core/services/auth.service.ts` — `login()` method:
```typescript
login(data: LoginRequest): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data, {
    withCredentials: true
  }).pipe(
    tap(res => {
      this.setToken(res.token);        // <-- token already set here
      this.isAuthenticated.set(true);
    })
  );
}
```

Returns `AuthResponse` from `src/app/shared/models/api.models.ts`:
```typescript
export interface AuthResponse {
  token: string;
  user?: Staff;
  message?: string;
}
```

## The Bug

In `LoginComponent.submit()` (lines 77-91):

```typescript
const res: any = await firstValueFrom(this.auth.login(loginData));

// ❌ Checks for envelope format that doesn't exist:
if (!res || !res.succeeded || !res.data || !res.data.token) {
  this.serverError = 'Invalid response from server';  // Always hits this
  return;
}

// ... remember-email logic ...

this.auth.setToken(res.data.token);  // ❌ Wrong path; also redundant
this.auth.loadUser();
await this.router.navigateByUrl('/dashboard');
```

**Two problems:**
1. **False negative gate** (line 79): checks `res.succeeded` and `res.data.token` but the actual response is `{ token, user?, message? }` — `res.succeeded` is `undefined`, so the condition is always `true` and shows "Invalid response from server".
2. **Redundant setToken + wrong path** (line 90): `AuthService.login()` already calls `setToken(res.token)` in its `tap` operator. The component's `res.data.token` path is also wrong (should be `res.token`).

</context>

<tasks>

<task type="auto">
  <name>Fix login response validation to match AuthResponse shape</name>
  <files>src/app/features/auth/login/login.component.ts</files>
  <action>
    In `submit()` method, apply three edits:

    **Edit 1 — Fix response validation gate (lines 79-82):**
    Replace:
    ```typescript
      if (!res || !res.succeeded || !res.data || !res.data.token) {
        this.serverError = 'Invalid response from server';
        return;
      }
    ```
    With:
    ```typescript
      if (!res?.token) {
        this.serverError = 'Invalid response from server';
        return;
      }
    ```

    **Edit 2 — Remove redundant setToken call (line 90):**
    Remove the line:
    ```typescript
      this.auth.setToken(res.data.token);
    ```
    Because `AuthService.login()` already calls `this.setToken(res.token)` in its `tap` operator before the observable resolves.

    **Edit 3 — Add AuthResponse import** (after existing imports, before component decorator):
    Add the import to use the proper type instead of `any`:
    ```typescript
    import { AuthResponse } from '../../../shared/models/api.models';
    ```
    Then change the response variable from `const res: any` to `const res: AuthResponse`.

    Do NOT change the catch block error message `'Credenziali non valide'` — that remains for actual HTTP/network errors. Do NOT touch the login template, auth service, or any other file.
  </action>
  <verify>
    <automated>npx tsc --noEmit --strict 2>&1 | grep -i "login.component" || echo "No type errors in login component"</automated>
  </verify>
  <done>
    - `submit()` checks `res?.token` instead of `res.succeeded && res.data.token`
    - No redundant `setToken` call in the component
    - `res` uses `AuthResponse` type instead of `any`
    - TypeScript compiles without errors
    - When backend returns `{ token: "..." }`, the login navigates to /dashboard
    - When backend returns 401, the catch block shows "Credenziali non valide"
  </done>
</task>

</tasks>

<verification>

1. **TypeScript compilation**: `npx tsc --noEmit --strict` shows no errors for `login.component.ts`
2. **Logic verification**: The gate `if (!res?.token)` correctly validates the `AuthResponse` shape
3. **No regressions**: Remember-email cookie logic, `loadUser()`, and navigation remain untouched
4. **Edge case — empty token**: If backend returns `{ token: "" }`, `!res?.token` catches it (empty string is falsy)
5. **Edge case — HTTP error**: Caught by try/catch, no change in error message "Credenziali non valide"

</verification>

<success_criteria>

- [ ] `LoginComponent.submit()` no longer references `res.succeeded` or `res.data` (envelope format that doesn't exist)
- [ ] Response validation matches `AuthResponse` type: checks `res?.token`
- [ ] No duplicate `setToken` — service's `tap` handles it, component only calls `loadUser()` + navigation
- [ ] TypeScript compilation passes with strict mode

</success_criteria>

<output>
After completion, create `.planning/quick/260619-npj-fix-the-false-negative-login-bug-caused-/260619-npj-SUMMARY.md`
</output>
