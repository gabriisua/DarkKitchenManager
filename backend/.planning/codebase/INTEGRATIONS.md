# External Integrations

**Analysis Date:** 2026-06-29

## APIs & External Services

**Invoicing:**
- **FattureInCloud API v2** — Italian electronic invoicing service
  - SDK/Client: Custom `HttpClient` wrapper (`src/Roscoff.Infrastructure/Services/FattureInCloudService.cs`)
  - Interface: `IFattureInCloudService` (`src/Roscoff.Application/Interfaces/IFattureInCloudService.cs`)
  - Base URL: `https://api-v2.fattureincloud.it` (configurable in `appsettings.json`)
  - Auth: Bearer token via `FattureInCloudSettings:AccessToken`
  - Company ID: `FattureInCloudSettings:CompanyId` (current value: `"1605166"`)
  - VAT Mapping: `FattureInCloudSettings:VatMappings` — maps decimal VAT rates to FIC VAT IDs
  - Methods:
    - `CreateInvoiceForCustomerAsync(List<Order>)` — creates cumulative invoice (`POST c/{companyId}/issued_documents`)
    - `GetInvoiceUrlAsync(int documentId)` — retrieves public PDF URL (`GET c/{companyId}/issued_documents/{id}`)
    - `DeleteInvoiceAsync(int documentId)` — deletes issued document (`DELETE c/{companyId}/issued_documents/{id}`)
  - Registered via `IHttpClientFactory` in `Program.cs` line 106

## Data Storage

**Databases:**
- **SQL Server** — Primary data store
  - Provider: `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
  - Connection via `ConnectionStrings:DefaultConnection`
  - Migrations stored in `src/Roscoff.Infrastructure/Migrations/`
  - Auto-applied on startup via `context.Database.Migrate()` in `Program.cs` (with 5 retries)
  - Hangfire also uses SQL Server for job storage (`UseSqlServerStorage`)
  - Entity relational mapping configured in `RoscoffDbContext` (`src/Roscoff.Infrastructure/Data/RoscoffDbContext.cs`)
  - Entity sets: `Customers`, `DeliveryHubs`, `StaffMembers`, `Categories`, `Plates`, `Menus`, `MenuItems`, `Ingredients`, `Allergens`, `IngredientAllergens`, `PlateIngredients`, `ClientCategoryDiscounts`, `ClientPlateDiscounts`, `Orders`, `OrderItems`, `PendingInvoices`

**File Storage:**
- Local filesystem only — no cloud storage (S3, Azure Blob, etc.) detected

**Caching:**
- None — no Redis, MemoryCache, or distributed caching detected

## Authentication & Identity

**Auth Provider:**
- **Custom JWT-based** (not external identity provider)
  - Implementation: `AuthService` (`src/Roscoff.Infrastructure/Services/AuthService.cs`)
  - Password hashing: BCrypt (`BCrypt.Net-Next` 4.0.3) in `PasswordService` (`src/Roscoff.Infrastructure/Services/PasswordService.cs`)
  - JWT generation: `System.IdentityModel.Tokens.Jwt` with HMAC-SHA256
  - Refresh tokens: Stored in `Staff` entity (64-byte random, 7-day expiry)
  - Password reset: Token-based (32-byte random, 2-hour expiry)
  - Endpoints:
    - `POST /api/Auth/login` — returns JWT + refresh token
    - `POST /api/Auth/refresh` — rotates tokens
    - `POST /api/Auth/reset-password-request` — generates reset token
    - `POST /api/Auth/reset-password-confirm` — sets new password
    - `GET /api/Auth/me` — current user info
  - Roles: `StaffRoles.Manager` (hardcoded seed), also used as `[Authorize(Roles = "MANAGER")]`, `[Authorize(Roles = "MANAGER,ADMIN")]`
  - CORS: Frontend origins `http://localhost:4200` and `http://localhost` configured in `Program.cs`

## Monitoring & Observability

**Error Tracking:**
- None — no Sentry, App Insights, or similar detected

**Logs:**
- `ILogger<T>` via ASP.NET Core's built-in logging
- Log levels: `Information` default, `Warning` for `Microsoft.AspNetCore`
- Console logging only (no external sink like Seq, Elastic, or DataDog detected)

**Background Jobs Dashboard:**
- Hangfire dashboard at `/hangfire` endpoint (development only, no auth configured on dashboard)

## CI/CD & Deployment

**Hosting:**
- Docker container (Linux) — multi-stage build in `src/Roscoff.Api/Dockerfile`
- Exposes ports 8080 (HTTP) and 8081 (HTTPS)
- No orchestrator config (Kubernetes, docker-compose) checked in

**CI Pipeline:**
- Not detected — no `.github/workflows/`, `.gitlab-ci.yml`, or similar found

## Environment Configuration

**Required env vars (critical secrets):**
- `ConnectionStrings:DefaultConnection` — SQL Server connection string (currently hardcoded in `appsettings.json` for dev)
- `JwtSettings:Key` — Symmetric signing key (currently hardcoded in `appsettings.json` for dev)
- `FattureInCloudSettings:AccessToken` — FIC API bearer token (currently hardcoded in `appsettings.json` for dev)

**Secrets location:**
- Currently in `appsettings.json` and `appsettings.Staging.json` (plaintext)
- `.env` files listed in `.dockerignore` but not used by the .NET app
- **Risk:** Production secrets are hardcoded in config files committed to git

## Webhooks & Callbacks

**Incoming:**
- None detected

**Outgoing:**
- None detected (FattureInCloud is called synchronously via HTTP, not via webhook)

## Network-Level Integrations

**TCP Label Printing:**
- Protocol: Raw TCP socket to port 9100
- Purpose: Sending ZPL (Zebra Programming Language) labels to thermal label printers
- Implementation:
  - `PrinterService` (`src/Roscoff.Infrastructure/Services/PrinterService.cs`) — hardcoded IP `172.16.70.71:9100`, used for all label formats
  - `TcpZplPrintService` (`src/Roscoff.Infrastructure/Services/TcpZplPrintService.cs`) — configurable IP via method parameter, 2-second connection timeout
- Label formats (ZPL templates):
  - Standard: `LabelTemplateGenerator` (`src/Roscoff.Infrastructure/Pdf/LabelTemplateGenerator.cs`)
  - Cortilia: `CortiliaLabelGenerator` (`src/Roscoff.Infrastructure/Pdf/CortiliaLabelGenerator.cs`)
  - Foorban: `FoorbanLabelGenerator` (`src/Roscoff.Infrastructure/Pdf/FoorbanLabelGenerator.cs`)

**PDF Generation:**
- **PuppeteerSharp 25.1.1** — Headless Chromium for HTML-to-PDF
  - Implementation: `PdfEngineService` (`src/Roscoff.Infrastructure/Services/PdfEngineService.cs`)
  - Registered as singleton (`AddSingleton<IPdfEngineService, PdfEngineService>`)
  - Chromium auto-downloaded on first launch via `BrowserFetcher.DownloadAsync()`
  - Args: `--no-sandbox --disable-setuid-sandbox` (required for Docker)
  - Output: A4, 10mm margins, print background
  - Used by templates in `src/Roscoff.Infrastructure/Pdf/`:
    - `TechnicalSheetTemplateGenerator.cs`
    - `DdtTemplateGenerator.cs` (Delivery Note / DDT)
    - `MenuTemplateGenerator.cs`

## Scheduled Jobs

**Hangfire Recurring Jobs:**
- **`process-fic-invoices`** — runs every minute (`Cron.Minutely()`)
  - Job class: `InvoiceProcessingJob` (`src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs`)
  - Method: `ProcessPendingInvoicesAsync()`
  - Process: Groups pending invoices by customer, calls `FattureInCloudService.CreateInvoiceForCustomerAsync()`, updates status
  - Batch size: 50 records per run

---

*Integration audit: 2026-06-29*
