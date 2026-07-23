# External Integrations

**Analysis Date:** 2026-06-29

## APIs & External Services

**Backend REST API (.NET):**
- Single backend API consumed by the entire frontend
  - Base URL: configured via `environment.apiUrl` — defaults to `http://localhost:5051/api` (dev), `http://localhost:8080/api` (staging)
  - Client: Angular `HttpClient` with functional interceptors (`apiInterceptor`, `authInterceptor`, `errorInterceptor`)
  - Response wrapping: Most endpoints return `{ data: T, succeeded: boolean, message?: string }` wrapper; services use `.pipe(map(res => res.data))` to unwrap
  - Parameter style: PascalCase query params (converted from camelCase via `buildPagedParams()` in `src/app/core/utils/http-params.util.ts`)
  - Auth: Bearer JWT token in `Authorization` header

**API Endpoints consumed:**

| Endpoint | Service | Methods |
|----------|---------|---------|
| `/Auth` | `src/app/core/services/auth.service.ts` | `POST /login`, `GET /me`, `POST /reset-password-request`, `POST /reset-password-confirm` |
| `/Staff` | `src/app/core/services/staff.service.ts` | `GET /`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id` |
| `/Customer` | `src/app/core/services/customer.service.ts` | `GET /`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id` |
| `/customers` (hubs) | `src/app/core/services/customer.service.ts` | `GET /:customerId/hubs`, `POST /:customerId/hubs`, `PUT /:customerId/hubs/:hubId`, `DELETE /:customerId/hubs/:hubId` |
| `/Allergen` | `src/app/core/services/allergen.service.ts` | `GET /`, `GET /all`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id` |
| `/Ingredient` | `src/app/core/services/ingredient.service.ts` | `GET /`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id` |
| `/Category` | `src/app/core/services/category.service.ts` | `GET /active`, `GET /`, `POST /`, `PUT /:id`, `DELETE /:id` |
| `/Plate` | `src/app/core/services/plate.service.ts` | `GET /`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id`, `GET /:id/food-cost`, `GET /:id/nutrition`, `GET /:id/technical-sheet` |
| `/Menu` | `src/app/core/services/menu.service.ts` | `GET /`, `GET /:id`, `POST /`, `PUT /:id`, `DELETE /:id`, `GET /:id/pdf`, `GET /:id/items/:plateId/label/classic`, `GET /:id/items/:plateId/label/custom` |
| `/order` (lowercase) | `src/app/core/services/order.service.ts` | `GET /`, `GET /:id`, `POST /`, `PATCH /:id/status`, `GET /:id/ddt` |
| `/invoice` (lowercase) | `src/app/core/services/invoice.service.ts` | `GET /pending-summary`, `GET /pending-summary/:customerId/orders`, `POST /bulk-invoice`, `GET /history`, `DELETE /:ficDocumentId`, `GET /:ficDocumentId/pdf` |
| `/ClientDiscount` | `src/app/core/services/sale.service.ts` | `GET /plates/paged`, `GET /categories/paged`, `GET /customers/:customerId/categories`, `POST /customers/:customerId/categories`, `DELETE /customers/:customerId/categories/:categoryId`, `GET /customers/:customerId/plates`, `POST /customers/:customerId/plates`, `DELETE /customers/:customerId/plates/:plateId`, `GET /customers/:customerId/plates/:plateId/effective-price` |
| `/Print` | `src/app/core/services/print.service.ts` | `POST /standard/:plateId/single`, `POST /standard/batch`, `POST /cortilia/:plateId/single`, `POST /cortilia/batch`, `POST /foorban/:plateId/single`, `POST /foorban/batch` |

**Print Service — Label Formats:**
- Three label types: `standard`, `cortilia`, `foorban` — each supporting single and batch print
- Label printing via backend which likely drives ZPL printers

## Data Storage

**Databases:**
- None directly — all data accessed through the backend REST API

**File Storage:**
- PDF/Blob downloads for: menu PDF (`/Menu/:id/pdf`), menu item labels (`/Menu/:id/items/:plateId/label/classic`, `/label/custom`), plate technical sheets (`/Plate/:id/technical-sheet`), order DDT documents (`/order/:id/ddt`), invoice PDFs (`/invoice/:ficDocumentId/pdf`)
- Downloads use `HttpClient` with `responseType: 'blob'`

**Caching:**
- None detected at the frontend layer (no service workers, no HTTP cache interceptors, no IndexedDB)

## Authentication & Identity

**Auth Provider:**
- Custom JWT-based authentication against the backend `/Auth` endpoint
  - Implementation: `src/app/core/services/auth.service.ts`
  - Token storage: `localStorage` under key `x-auth-token`
  - Token validation: Client-side expiry check via JWT payload `exp` claim (base64 decode in `decodeTokenPayload()`)
  - Auth header: Bearer token injected by `auth.interceptor.ts` (`src/app/core/interceptors/auth.interceptor.ts`)
  - Login uses `withCredentials: true` for cookie-based flows
  - Session persistence: Token read from localStorage on app init in `AuthService.init()`

**Auth Guard:**
- `CanMatchFn` guard at `src/app/core/guards/auth.guard.ts` — protects all dashboard routes

**Auth Error Handling:**
- `error.interceptor.ts` (`src/app/core/interceptors/error.interceptor.ts`) intercepts 401/403 responses on non-public endpoints (`login`, `reset-password`, `forgot-password` are excluded) and calls `authService.handleAuthFailure()` which clears token and redirects to `/`

**Password Reset Flow:**
- Two-step: `POST /Auth/reset-password-request` (email) and `POST /Auth/reset-password-confirm` (token + password)

## Monitoring & Observability

**Error Tracking:**
- None detected — errors are handled only via the `errorInterceptor` which silently clears auth on 401/403; no external error reporting (Sentry, etc.)

**Logs:**
- Console-based — `console.error` in `src/main.ts` bootstrap catch; no structured logging

## CI/CD & Deployment

**Hosting:**
- Docker container with nginx:alpine (`Dockerfile`)
  - Multi-stage build: Node 20-alpine for compilation, nginx:alpine for serving
  - Compiled output: `/dist/bo-configurator/browser` served from `/usr/share/nginx/html`
  - SPA routing: nginx `try_files` fallback to `index.html`
  - Port 80 exposed
  - Staging build configuration (`--configuration staging`)

**CI Pipeline:**
- None detected (no `.github/workflows/`, `.gitlab-ci.yml`, or similar)

## Environment Configuration

**Required env vars:**
- None at runtime (Angular SPA) — all configuration is baked at build time via environment files

**Build-time configuration:**
- `apiUrl`: Backend API base URL (set in `src/environments/environment.ts` or `environment.staging.ts`)
- `production`: Boolean flag (used for build optimization)

**Secrets location:**
- No `.env` files present
- No secrets in the codebase

## Webhooks & Callbacks

**Incoming:**
- None — the frontend is a pure SPA client; webhooks would hit the backend directly

**Outgoing:**
- None — no outbound webhook calls from the frontend

---

*Integration audit: 2026-06-29*
