# Codebase Concerns

**Analysis Date:** 2026-06-29

## Tech Debt

### Empty / Unused Classes
- **Issue:** Two classes exist with no implementation — they serve no purpose.
- **Files:**
  - `src/Roscoff.Api/Extensions/JwtSetup.cs` — Empty class, no members
  - `src/Roscoff.Application/DependencyInjection.cs` — Empty class, no members
- **Impact:** Dead code creates confusion about where DI registration should live. Currently all DI registration is done inline in `Program.cs` instead.
- **Fix approach:** Remove both files, or implement them if they are planned extension points.

### "DEBUG ERRORE" Message in Production Code
- **Issue:** A debugging error message prefix is returned to the API consumer.
- **Files:** `src/Roscoff.Infrastructure/Services/InvoiceManagerService.cs` (line 287)
- **Code:**
  ```csharp
  return (false, null, $"DEBUG ERRORE: {ex.Message}");
  ```
- **Impact:** Exposes internal error details to API clients. Inappropriate for production.
- **Fix approach:** Replace with a generic user-facing message and log the actual exception.

### Mixed Italian/English Comments
- **Issue:** Comments throughout the codebase are inconsistently written in Italian or English, sometimes switching mid-file.
- **Files:** All `.cs` files — inline comments, XML doc, and region labels use both languages.
- **Impact:** Reduces readability for non-Italian-speaking developers. Inconsistent documentation style.
- **Fix approach:** Adopt English-only comments as the standard.

### Two Separate SaveChanges in Background Job
- **Issue:** The background invoice processor calls `SaveChangesAsync` twice — once to lock records as "Processing" (line 46), once to save results (line 87).
- **Files:** `src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs`
- **Impact:** If the server crashes between line 46 and line 87, all locked records remain stuck in "Processing" state permanently. No recovery mechanism exists.
- **Fix approach:** Use a single SaveChanges at the end, or implement a startup recovery that resets stale "Processing" records.

### Hardcoded Default Text for Product Metadata
- **Issue:** Default values for product descriptions are hardcoded Italian strings.
- **Files:** `src/Roscoff.Infrastructure/Services/PlateService.cs` (lines 148-151)
- **Code:**
  ```csharp
  ProductType = !string.IsNullOrWhiteSpace(dto.ProductType) ? dto.ProductType : "Preparazione gastronomica",
  StorageConditions = !string.IsNullOrWhiteSpace(dto.StorageConditions) ? dto.StorageConditions : "Conservare in frigorifero tra 0°C e +4°C.",
  PreservationTechnology = !string.IsNullOrWhiteSpace(dto.PreservationTechnology) ? dto.PreservationTechnology : "Confezionato in atmosfera protettiva (ATM).",
  ```
- **Impact:** Inflexible — can't change defaults without code changes. Not localizable.
- **Fix approach:** Move to configuration or resource files.

### BaseApiController Uses ClaimTypes.Name Instead of ClaimTypes.NameIdentifier
- **Issue:** `AuthenticatedUserId` property reads `ClaimTypes.Name` (line 51), but JWT token sets the subject claim as `JwtRegisteredClaimNames.Sub` (line 139 of AuthService).
- **Files:**
  - `src/Roscoff.Api/Controllers/BaseApiController.cs` (line 51)
  - `src/Roscoff.Infrastructure/Services/AuthService.cs` (line 139)
- **Impact:** The `ClaimTypes.Name` maps to `JwtRegisteredClaimNames.UniqueName`, not `Sub`. This works by accident because `ClaimTypes.Name` is not explicitly set in the JWT, so it returns null, and `Guid.TryParse(null)` returns `Guid.Empty`. The user ID is never correctly extracted from the JWT.
- **Fix approach:** Change `ClaimTypes.Name` to `ClaimTypes.NameIdentifier` in `BaseApiController.cs`.

### Auth Refresh Endpoint Missing [AllowAnonymous]
- **Issue:** The refresh token endpoint on `AuthController.cs` (line 85) has no `[AllowAnonymous]` attribute.
- **Files:** `src/Roscoff.Api/Controllers/AuthController.cs` (line 85)
- **Impact:** The refresh endpoint requires a valid JWT to call, making it impossible to use when the JWT has expired (which is the entire use case for refresh tokens).
- **Fix approach:** Add `[AllowAnonymous]` to the `Refresh` endpoint.

### Order Entity Duplicates BaseEntity Audit Fields
- **Issue:** `Order.cs` explicitly declares `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` fields, but also inherits from `BaseEntity<Guid>` which already provides them via the `IAuditableEntity` interface.
- **Files:** `src/Roscoff.Core/Entities/Invoice/Order.cs` (lines 58-61)
- **Impact:** Possible EF Core mapping confusion. The explicit fields might shadow the inherited ones, leading to unexpected behavior with the audit interceptor in `RoscoffDbContext.SaveChangesAsync`.
- **Fix approach:** Remove the explicitly declared audit fields from `Order.cs` and rely on the base class.

### PendingInvoice Doesn't Implement IAuditableEntity
- **Issue:** `PendingInvoice.cs` has its own `CreatedAt` property but does not implement `IAuditableEntity`, so the audit interceptor in `RoscoffDbContext.SaveChangesAsync` will never set audit timestamps on it.
- **Files:** `src/Roscoff.Core/Entities/Invoice/PendingInvoice.cs`
- **Impact:** `CreatedAt` on pending invoices is set to `DateTime.UtcNow` at entity construction in C# code, but `CreatedBy` and `UpdatedBy` are never populated.
- **Fix approach:** Implement `IAuditableEntity` on `PendingInvoice`.

## Known Bugs

### Password Reset Token Leaked via API Response
- **Symptoms:** The `ResetPasswordRequestAsync` endpoint returns the reset token in the API response body as `debugToken`.
- **Files:** `src/Roscoff.Api/Controllers/AuthController.cs` (line 49)
- **Code:**
  ```csharp
  var (messaggio, token) = await _authService.ResetPasswordRequestAsync(request.Email);
  return HandleResult(Result<object>.Success(new { message = messaggio, debugToken = token }));
  ```
- **Trigger:** Calling `POST /api/auth/reset-password-request` with a valid email.
- **Impact:** Anyone who can call this API can retrieve a valid password reset token for any known email address.
- **Workaround:** The comment on line 48 acknowledges this: "ATTENZIONE: In produzione invia il token via email, non restituirlo nell'API!"
- **Fix approach:** Remove `debugToken` from the response. Send the token via email instead.

### Order Status Update Has No State Machine Validation
- **Symptoms:** Orders can transition from any status to any other status — e.g., back from "Delivered" to "Pending".
- **Files:** `src/Roscoff.Infrastructure/Services/OrderService.cs` (lines 206-214)
- **Code:**
  ```csharp
  public async Task<(bool Success, string Message)> UpdateStatusAsync(Guid id, OrderStatus newStatus, ...)
  {
      var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ...);
      order.Status = newStatus;
      ...
  }
  ```
- **Trigger:** Calling any order status update endpoint.
- **Impact:** Orders can enter invalid states. No business logic validation on transitions.
- **Fix approach:** Implement a status transition map (e.g., `Pending -> Confirmed -> Shipped -> Delivered`) and reject invalid transitions.

### MenuService Update Uses Guid.Empty Instead of Null Check for CustomerId
- **Symptoms:** When updating a menu, if `CustomerId` is not explicitly set in the DTO (null), it gets incorrectly reset to `Guid.Empty` (all zeros), which clears the menu's customer association.
- **Files:** `src/Roscoff.Infrastructure/Services/MenuService.cs` (lines 200-203)
- **Code:**
  ```csharp
  if (dto.CustomerId != Guid.Empty)
  {
      menu.CustomerId = dto.CustomerId;
  }
  ```
- **Trigger:** `PUT /api/menu/{id}` with a DTO that omits `CustomerId`.
- **Impact:** Since `dto.CustomerId` is a `Guid?` that defaults to `Guid.Empty` when null, the condition `dto.CustomerId != Guid.Empty` is `false` for both "not provided" and "explicitly set to empty". The correct check should be `dto.CustomerId.HasValue`.
- **Fix approach:** Change to `if (dto.CustomerId.HasValue)`.

### WorkingDayCalculator Ignores Public Holidays
- **Symptoms:** The delivery date calculator only skips Saturdays and Sundays, but does not account for Italian public holidays.
- **Files:** `src/Roscoff.Infrastructure/Helpers/WorkingDayCalculator.cs`
- **Trigger:** Orders placed around holidays (e.g., Easter, Christmas, Ferragosto) will have incorrect delivery date calculations.
- **Impact:** Delivery dates calculated during holiday periods will be wrong.
- **Fix approach:** Add a configurable list of public holidays to exclude.

## Security Considerations

### Production Credentials Committed to Git
- **Risk:** Database passwords, JWT signing keys, and third-party API tokens are stored in `appsettings.json` and `appsettings.Staging.json`, both of which are tracked in git.
- **Files:**
  - `src/Roscoff.Api/appsettings.json` — Contains DB password `Roscoff@2026!`, JWT secret, FattureInCloud AccessToken and CompanyId
  - `src/Roscoff.Api/appsettings.Staging.json` — Contains staging DB password `SuperSecretStagingPwd123!`
- **Current mitigation:** None. The `.gitignore` file does not exclude `appsettings*.json`.
- **Recommendations:**
  1. Move all secrets to environment variables or a secrets manager (e.g., Azure Key Vault, Docker secrets, or `.env` files)
  2. Use `dotnet user-secrets` for development
  3. Remove sensitive data from git history using `git filter-branch` or BFG Repo-Cleaner
  4. Rotate all exposed credentials immediately

### Hardcoded Admin Seed Password
- **Risk:** The initial admin user is created with a hardcoded password `"123stella"` in `Program.cs`.
- **Files:** `src/Roscoff.Api/Program.cs` (line 158)
- **Current mitigation:** The seed only runs if no staff members exist. The password should be changed after first login.
- **Recommendations:**
  1. Remove hardcoded password from source
  2. Require setting the admin password via environment variable at startup
  3. Force password change on first login

### FattureInCloud Access Token Exposed
- **Risk:** The FattureInCloud API access token is stored in plaintext in `appsettings.json` and committed to git.
- **Files:** `src/Roscoff.Api/appsettings.json` (line 20)
- **Impact:** Anyone with access to the git repository can use this token to interact with the FattureInCloud API on behalf of the company.
- **Recommendations:**
  1. Move to environment variable or secrets manager
  2. Rotate the token immediately
  3. Set appropriate API scope restrictions in FattureInCloud dashboard

### Exception Details Leaked to API Clients
- **Risk:** Several controllers return raw exception messages in HTTP responses, exposing internal implementation details.
- **Files:**
  - `src/Roscoff.Api/Controllers/StaffController.cs` (lines 58-62) — Returns `ex.Message` from `ArgumentException`
  - `src/Roscoff.Api/Controllers/PrintController.cs` (lines 85-87, 111-113) — Returns `ex.Message` in error detail
  - `src/Roscoff.Infrastructure/Services/InvoiceManagerService.cs` (line 287) — Returns `"DEBUG ERRORE: {ex.Message}"`
- **Recommendations:**
  1. Log the full exception server-side
  2. Return only a generic error message to the client
  3. In development, use a consistent error response envelope that includes a correlation ID

## Performance Bottlenecks

### Puppeteer/Chromium Singleton with Full Browser Download
- **Problem:** `PdfEngineService.cs` downloads Chromium on first use (line 24: `await browserFetcher.DownloadAsync()`) and holds a full browser process in memory for the application's lifetime.
- **Files:** `src/Roscoff.Infrastructure/Services/PdfEngineService.cs`
- **Cause:** PuppeteerSharp requires a full Chromium binary. The `--no-sandbox` flag (line 29) is used for Docker environments.
- **Impact:**
  - ~150MB+ memory footprint from Chromium process
  - Slow first request (downloads Chromium if not cached)
  - `--no-sandbox` is a security concern in production
- **Improvement path:**
  1. Pre-download Chromium during Docker build (not at runtime)
  2. Consider a lighter alternative like QuestPDF or IronPDF for simpler PDFs
  3. Use a dedicated PDF microservice if PDF generation volume grows

### Hangfire Polling Every Minute
- **Problem:** The invoice processing background job runs every 60 seconds.
- **Files:** `src/Roscoff.Api/Program.cs` (line 192)
- **Code:** `Cron.Minutely()`
- **Impact:** For an operation that may have nothing to process most of the time, this generates unnecessary database queries and Hangfire scheduler overhead.
- **Improvement path:** Increase interval to 5-15 minutes, or trigger processing on-demand when invoices are queued.

## Fragile Areas

### FattureInCloud Integration Has No Resilience
- **Files:** `src/Roscoff.Infrastructure/Services/FattureInCloudService.cs`
- **Why fragile:**
  - No retry logic for transient HTTP failures
  - No circuit breaker pattern
  - No configurable timeout on `HttpClient` calls
  - If FattureInCloud API is slow or down, requests hang until default HttpClient timeout
  - Error messages include raw API response bodies (potential data leakage)
- **Safe modification:** When adding retry logic, use `IHttpClientBuilder.AddPolicyHandler()` with Polly rather than wrapping every method individually.
- **Test coverage:** None.

### Printer Service Uses Hardcoded Network Configuration
- **Files:**
  - `src/Roscoff.Infrastructure/Services/PrinterService.cs` (line 13: `_printerIp = "172.16.70.71"`, line 14: `_printerPort = 9100`)
  - `src/Roscoff.Infrastructure/Services/TcpZplPrintService.cs`
- **Why fragile:**
  - Printer IP and port hardcoded in `PrinterService.cs`
  - Synchronous TCP calls (`TcpClient` without async in `SendToPrinter`)
  - No retry mechanism
  - Socket exceptions are rethrown (line 188), causing HTTP 500 for the caller
  - No printer status/health check endpoint
- **Safe modification:** Move printer configuration to `appsettings.json`, add retry with exponential backoff, use async TCP methods consistently.

### No Unit Tests Across the Entire Codebase
- **Files:** All `.cs` files
- **Why fragile:** Zero test files exist. Every service, controller, and helper has no automated test coverage.
- **Risk:** Any change risks regression. There is no safety net for refactoring.
- **Priority:** High
- **Improvement path:** Start with unit tests for core domain logic (`WorkingDayCalculator`, `FoodCostService`, `PasswordService`), then add integration tests for services.

## Scaling Limits

### Database-Centric Architecture
- **Current capacity:** Single SQL Server database instance used for both operational data and background job storage (Hangfire uses the same connection string).
- **Limit:** As order/invoice volume grows, Hangfire's schema in the same database will compete with operational queries.
- **Scaling path:** Separate Hangfire into its own database or use Redis as the Hangfire storage backend.

### Sequential Print Job Processing
- **Current capacity:** Print jobs are processed sequentially on the main thread (synchronous TCP in `PrinterService.SendToPrinter`).
- **Limit:** If multiple users send print jobs simultaneously, requests queue up on the web server thread pool. The TCP write itself is fast (< 100ms) but the synchronous blocking is unnecessary.
- **Scaling path:** Make print calls async, consider a print job queue with Hangfire.

## Dependencies at Risk

### BCrypt.Net-Next v4.0.3
- **Risk:** Version pinned but not monitored for security updates.
- **Impact:** If a vulnerability is found in BCrypt.Net-Next, all password hashes could be affected.
- **Migration plan:** Keep version updated via Dependabot or periodic manual review.

### PuppeteerSharp (Inferred Dependency)
- **Risk:** Requires Chromium binary. Version mismatch between PuppeteerSharp and the downloaded Chromium revision can cause breakage.
- **Impact:** PDF generation stops working if Chromium can't be downloaded or is incompatible.
- **Migration plan:** Pin Chromium revision in Dockerfile, pre-download during build.

## Missing Critical Features

### Audit Trail for Order Status Changes
- **Problem:** Order status changes are not logged historically. `UpdateStatusAsync` overwrites the current status without recording the previous state, who changed it, or when.
- **Files:** `src/Roscoff.Infrastructure/Services/OrderService.cs`
- **Blocks:** Cannot answer "who changed this order's status and when?" — a basic requirement for food production compliance.

### Printer Health Check Endpoint
- **Problem:** There is no API endpoint to check if a network printer is reachable before attempting to print.
- **Files:** `src/Roscoff.Infrastructure/Services/PrinterService.cs`, `src/Roscoff.Infrastructure/Services/TcpZplPrintService.cs`
- **Blocks:** Frontend cannot verify printer availability before sending a print job.

## Test Coverage Gaps

| Untested Area | Files | Risk | Priority |
|---|---|---|---|
| **Entire codebase** | All `.cs` files | Any change risks regression | **High** |
| WorkingDayCalculator | `src/Roscoff.Infrastructure/Helpers/WorkingDayCalculator.cs` | Incorrect delivery dates during holidays/weekends | High |
| FoodCostService | `src/Roscoff.Infrastructure/Services/FoodCostService.cs` | Incorrect food cost calculation affecting margins | High |
| InvoiceManagerService | `src/Roscoff.Infrastructure/Services/InvoiceManagerService.cs` | Financial data integrity | High |
| FattureInCloudService | `src/Roscoff.Infrastructure/Services/FattureInCloudService.cs` | External API integration — can't test without mocking | High |
| AuthService | `src/Roscoff.Infrastructure/Services/AuthService.cs` | Authentication bypass | Critical |
| ClientDiscountService | `src/Roscoff.Infrastructure/Services/ClientDiscountService.cs` | Incorrect pricing affecting revenue | High |
| Controllers (authorization) | `src/Roscoff.Api/Controllers/*.cs` | Unauthorized access to endpoints | Critical |

---

*Concerns audit: 2026-06-29*
