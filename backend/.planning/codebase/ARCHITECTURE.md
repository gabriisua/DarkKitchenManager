<!-- refreshed: 2026-06-29 -->
# Architecture

**Analysis Date:** 2026-06-29

## System Overview

```text
┌──────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                            │
│                    Roscoff.Api (ASP.NET Core)                     │
│  Controllers ─── BaseApiController ─── Result<T> wrapper         │
│  JWT Auth ─── Swagger ─── Hangfire Dashboard                     │
│  HttpService (CurrentUserService)                                 │
└──────────────────────┬───────────────────────────────────────────┘
                       │ HTTP request / DI
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│                     APPLICATION LAYER                             │
│                   Roscoff.Application                            │
│  Interfaces (I*Service) ─── MediatR CQRS ─── DTOs ─── Wrappers  │
│  MediaTR/Discount/* ─── MediaTR/Invoice/*                        │
└──────────────────────┬───────────────────────────────────────────┘
                       │ Interface contract
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE LAYER                          │
│                  Roscoff.Infrastructure                           │
│  Services (implementations) ─── Data/RoscoffDbContext ─── PDF    │
│  Helpers (WorkingDayCalculator) ─── Migrations ─── Jobs          │
│  External: FattureInCloudService ─── TcpZplPrintService          │
└──────────────────────┬───────────────────────────────────────────┘
                       │ DbContext / Entity reference
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│                     CORE / DOMAIN LAYER                           │
│                      Roscoff.Core                                │
│  Entities ─── Enums ─── Interfaces (ICurrentUserService,         │
│  IAuditableEntity, BaseEntity<TId>)                              │
└──────────────────────────────────────────────────────────────────┘

External Systems:
  ┌──────────────┐  ┌──────────────────┐  ┌────────────────────┐
  │ SQL Server   │  │ FattureInCloud   │  │ ZPL Printer (TCP)  │
  │ (EF Core)    │  │ REST API         │  │ (Thermal Labels)   │
  └──────────────┘  └──────────────────┘  └────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Api Controllers | HTTP entry points, auth, response wrapping | `src/Roscoff.Api/Controllers/*.cs` |
| BaseApiController | Standard `Result<T>` response handling, user claims extraction | `src/Roscoff.Api/Controllers/BaseApiController.cs` |
| Application Interfaces | Contracts for all service operations | `src/Roscoff.Application/Interfaces/*.cs` |
| MediatR CQRS | Command/Query pattern for invoice & discount operations | `src/Roscoff.Application/MediaTR/*.cs` |
| DTOs | Request/Response records for API surface | `src/Roscoff.Application/Dtos/*.cs` |
| Result<T> Wrapper | Standardized API response envelope | `src/Roscoff.Application/Wrappers/Result.cs` |
| RoscoffDbContext | EF Core DbContext with audit interception | `src/Roscoff.Infrastructure/Data/RoscoffDbContext.cs` |
| Infrastructure Services | All service implementations (domain logic) | `src/Roscoff.Infrastructure/Services/*.cs` |
| PDF Engine | PuppeteerSharp-based HTML-to-PDF conversion | `src/Roscoff.Infrastructure/Services/PdfEngineService.cs` |
| Core Entities | Domain models with EF Core mapping attributes | `src/Roscoff.Core/Entities/**/*.cs` |
| Hangfire Jobs | Background invoice processing | `src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs` |

## Pattern Overview

**Overall:** Clean Architecture (4-layer) + CQRS (via MediatR)

**Key Characteristics:**
- Strict dependency direction: Api → Infrastructure → Application → Core (never inward)
- Service Interface + Implementation pattern across Application/Infrastructure boundary
- CQRS via MediatR for invoice/discount workflows (commands and queries in `MediaTR/`)
- Traditional service layer pattern for CRUD-heavy domains (catalog, staff, customers)
- `Result<T>` wrapper for standardized API responses across all endpoints
- Auto-audit via `IAuditableEntity` intercepted in `RoscoffDbContext.SaveChangesAsync`
- All prices stored as **cents (int)** to avoid floating-point precision issues

## Layers

**Core Layer (`Roscoff.Core`):**
- Purpose: Domain entities, enums, and shared interfaces with zero external dependencies
- Location: `src/Roscoff.Core/`
- Contains: `Entities/`, `Enums/`, `Interfaces/`
- Depends on: `Microsoft.EntityFrameworkCore.Abstractions` (for attributes only)
- Used by: All other layers

**Application Layer (`Roscoff.Application`):**
- Purpose: Use-case orchestration, DTOs, CQRS handlers, service contracts
- Location: `src/Roscoff.Application/`
- Contains: `Interfaces/`, `Dtos/`, `MediaTR/`, `Wrappers/`
- Depends on: `Roscoff.Core`, `MediatR`, `Microsoft.EntityFrameworkCore`
- Used by: `Roscoff.Api`, `Roscoff.Infrastructure`

**Infrastructure Layer (`Roscoff.Infrastructure`):**
- Purpose: All implementations — EF Core persistence, external API clients, PDF generation, printing
- Location: `src/Roscoff.Infrastructure/`
- Contains: `Services/`, `Data/`, `Migrations/`, `Pdf/`, `Helpers/`
- Depends on: `Roscoff.Core`, `Roscoff.Application`, `BCrypt.Net-Next`, `Hangfire`, `PuppeteerSharp`, `Microsoft.EntityFrameworkCore.SqlServer`
- Used by: `Roscoff.Api` (DI registration in Program.cs)

**API Layer (`Roscoff.Api`):**
- Purpose: HTTP host, routing, auth, Swagger, CORS, Hangfire dashboard
- Location: `src/Roscoff.Api/`
- Contains: `Controllers/`, `Extensions/`, `HttpService/`, `Program.cs`
- Depends on: `Roscoff.Application`, `Roscoff.Infrastructure`, `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`

## Data Flow

### Primary Request Path (e.g., Plate CRUD)

1. HTTP request hits controller action (`src/Roscoff.Api/Controllers/PlateController.cs:30`)
2. Controller validates input and calls application service via interface (`IPlateService`)
3. Service implementation in Infrastructure layer (`src/Roscoff.Infrastructure/Services/PlateService.cs:30`) executes business logic using `RoscoffDbContext`
4. Service maps domain entities to DTOs (`PlateResponseDto`) and returns them
5. Controller wraps result in `Result<T>.Success(data)` and delegates to `HandleResult()` (`BaseApiController.cs:19`)
6. ASP.NET returns JSON with standardized envelope: `{ succeeded, message, data, errors }`

### Invoice Bulk Creation Flow

1. `POST /api/invoice/bulk-invoice` receives `CreatePendingInvoicesCommand` (`src/Roscoff.Api/Controllers/InvoiceController.cs:29`)
2. `IMediator.Send(command)` dispatches to `CreatePendingInvoicesCommandHandler` (`src/Roscoff.Application/MediaTR/Invoice/Commands/CreatePendingInvoiceCommand.cs:27`)
3. Handler validates orders, creates `PendingInvoice` records, saves to DB
4. Hangfire `InvoiceProcessingJob.ProcessPendingInvoicesAsync()` (`src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs:24`) picks up pending records
5. Groups orders by customer, calls `FattureInCloudService.CreateInvoiceForCustomerAsync()` (`src/Roscoff.Infrastructure/Services/FattureInCloudService.cs:51`)
6. Updates order status and pending invoice status in DB

### PDF Generation Flow (Menu/Technical Sheet)

1. `GET /api/menu/{id}/pdf` calls `MenuController.GetPdf()` (`src/Roscoff.Api/Controllers/MenuController.cs:44`)
2. Menu is fetched via `MenuService`
3. `MenuTemplateGenerator.GenerateHtml(menu)` produces HTML string (`src/Roscoff.Infrastructure/Pdf/MenuTemplateGenerator.cs`)
4. `PdfEngineService.GeneratePdfFromHtmlAsync(html)` uses Puppeteer/Chromium (`src/Roscoff.Infrastructure/Services/PdfEngineService.cs:40`)
5. PDF bytes returned as `File()` result

**State Management:**
- All state is in SQL Server via EF Core (no in-memory caches)
- Hangfire manages background job state in SQL Server
- No distributed cache (Redis) detected
- PdfEngineService uses a singleton browser instance with `SemaphoreSlim` for thread safety

## Key Abstractions

**`Result<T>` Wrapper:**
- Purpose: Standardized API response envelope for all endpoints
- Location: `src/Roscoff.Application/Wrappers/Result.cs`
- Pattern: `Result<T>.Success(data, message?)` / `Result<T>.Failure(message, errors?)`
- Used uniformly across all controllers and some services

**`BaseEntity<TId>`:**
- Purpose: Base class for all domain entities, providing Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- Location: `src/Roscoff.Core/Entities/BaseEntity.cs`
- Pattern: Generic base class implementing `IAuditableEntity`
- Sub-types: `BaseEntity<Guid>` (Customer, Staff, Order, DeliveryHub) and `BaseEntity<int>` (Plate, Menu, Category, Ingredient, Allergen, PendingInvoice)

**`IAuditableEntity`:**
- Purpose: Audit trail contract for created/updated timestamps and user IDs
- Location: `src/Roscoff.Core/Interfaces/IAuditableEntity.cs`
- Auto-applied via `RoscoffDbContext.SaveChangesAsync()` override (`src/Roscoff.Infrastructure/Data/RoscoffDbContext.cs:232`)

**Service Interface Pattern:**
- Purpose: Clean separation of contract (Application layer) from implementation (Infrastructure layer)
- Examples: `IPlateService` → `PlateService`, `IAuthService` → `AuthService`, `IOrderService` → `OrderService`
- All service interfaces defined in `src/Roscoff.Application/Interfaces/`

**CQRS via MediatR:**
- Purpose: Commands and queries for complex write/read workflows
- Location: `src/Roscoff.Application/MediaTR/`
- Sub-areas: `Discount/Commands/`, `Discount/Queries/`, `Invoice/Commands/`
- Handler pattern: `IRequest<TResponse>` + `IRequestHandler<TRequest, TResponse>`

## Entry Points

**HTTP API:**
- Location: `src/Roscoff.Api/Program.cs`
- Triggers: HTTP requests on ports 5051 (HTTP) / 7295 (HTTPS)
- Responsibilities: JWT auth, CORS, Swagger, Hangfire dashboard, migration on startup

**Hangfire Background Jobs:**
- Location: `src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs`
- Triggers: `RecurringJob.AddOrUpdate<InvoiceProcessingJob>("process-fic-invoices", ...)` runs every minute (`Program.cs:189`)
- Responsibilities: Process queued invoices through FattureInCloud API

**Database Migration + Seed:**
- Executes on startup in `Program.cs:141-187`
- Auto-applies EF Core migrations with retry logic (5 retries)
- Seeds initial admin Staff user if none exist

## Architectural Constraints

- **Dependency direction:** Api → Infrastructure → Application → Core. Core never references any other project.
- **Threading:** ASP.NET Core request pipeline is multi-threaded. `PdfEngineService` uses `SemaphoreSlim(1,1)` singleton browser. Hangfire workers parallel by CPU count.
- **Global state:** `PdfEngineService` holds a singleton `IBrowser` instance — thread-safe via semaphore.
- **All monetary values stored as `int` cents** (not `decimal`) — prices like `TotalGrossCents`, `NetAmountCents`, `VatAmountCents`, `BasePrice`, `OverridePrice`, `CostPer1000g` etc.
- **Soft-delete pattern:** `IsActive` boolean on entities instead of hard deletes (Plate, Ingredient, Customer, Staff, Menu, Category)
- **Auth separation:** Staff and Customer are separate entities with separate auth flows. Refresh tokens stored for Staff only.

## Anti-Patterns

### Service Locator in Program.cs

**What happens:** All service DI registrations are manual (lines 83-106 of `Program.cs`) — 20+ `AddScoped` calls.
**Why it's wrong:** Error-prone when adding new services; easy to forget registration. `DependencyInjection.cs` in Application layer is an empty class.
**Do this instead:** Use `AddApplicationServices()` and `AddInfrastructureServices()` extension methods to self-register via reflection or assembly scanning.

### Inconsistent Controller Base Class Usage

**What happens:** Most controllers inherit from `BaseApiController` but `InvoiceController`, `OrderController`, `ClientDiscountController`, and `PrintController` inherit from `ControllerBase` directly, bypassing `HandleResult()`.
**Why it's wrong:** `InvoiceController` manually returns `Ok(Result<...>.Success(...))` instead of `HandleResult()`, breaking the standardized error-handling pattern.
**Do this instead:** Extend `BaseApiController` for all API controllers.

### MediatR + Service Layer Duality

**What happens:** Some workflows are pure MediatR CQRS (discounts), some are pure service calls (catalog CRUD), and some mix both in one controller (`InvoiceController`, `ClientDiscountController`).
**Why it's wrong:** Inconsistent architectural approach — developers must guess which pattern to follow.
**Do this instead:** Adopt a single pattern per bounded context (e.g., CQRS for write-heavy invoicing, service layer for CRUD-heavy catalog).

## Error Handling

**Strategy:** Global `Result<T>` wrapper returned from every endpoint. No global exception filter/middleware detected.

**Patterns:**
- Controllers catch null returns and map to `Result<T>.Failure("message")`
- `HandleResult()` maps `Succeeded` flag + error message content to HTTP status (200 success, 400 bad request, 404 not found)
- Infrastructure services throw typed exceptions (e.g., `HttpRequestException`, `InvalidOperationException`, `ArgumentException`)
- Some controllers use try/catch (`StaffController.cs:48`, `ClientDiscountController.cs:35`)
- No global exception middleware or `IExceptionHandler` detected

## Cross-Cutting Concerns

**Logging:** `ILogger<T>` injected into services and background jobs via constructor injection. Uses `Microsoft.Extensions.Logging`.

**Validation:**
- Data annotations on DTOs (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`)
- Server-side validation with `ModelState.IsValid` checks in controllers
- Some validation duplicated in service methods (manually checking `string.IsNullOrWhiteSpace`)

**Authentication:**
- JWT Bearer token auth configured in `Program.cs:33-52`
- `StaffRoles` constants define: `MANAGER`, `ADMINISTRATOR`, `OPERATOR`, `LOGISTIC`
- `[Authorize(Roles = "MANAGER")]` used on controllers
- Refresh token rotation for Staff
- Custom `CurrentUserService` extracts user from JWT claims (`src/Roscoff.Api/HttpService/CurrentUserService.cs`)

---

*Architecture analysis: 2026-06-29*
