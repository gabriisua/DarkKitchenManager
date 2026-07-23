# Codebase Structure

**Analysis Date:** 2026-06-29

## Directory Layout

```
be-roscoff/
├── .gitignore
├── Roscoff.slnx                          # Solution file (new .slnx XML format)
├── src/
│   ├── Roscoff.Core/                     # Domain layer (zero dependencies)
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Catalog/                  # Catalog domain: plates, menus, categories, ingredients, allergens
│   │   │   ├── Client/                   # Client domain: customers, staff, delivery hubs, discounts
│   │   │   └── Invoice/                  # Invoice domain: orders, order items, pending invoices
│   │   ├── Enums/
│   │   │   └── OrderStatus.cs
│   │   └── Interfaces/
│   │       ├── IAuditableEntity.cs
│   │       └── ICurrentUserService.cs
│   │
│   ├── Roscoff.Application/              # Application layer: contracts, DTOs, CQRS, wrappers
│   │   ├── Dtos/
│   │   │   ├── Auth/
│   │   │   ├── Catalog/                  # Plate, Menu, Ingredient, Allergen, Order, Category DTOs
│   │   │   ├── Client/                   # Customer, Staff, DeliveryHub, Discount DTOs
│   │   │   ├── Common/                   # Pagination base classes + PaginatedResponseDto
│   │   │   └── Invoice/                  # Invoice flow DTOs + FIC response record
│   │   ├── Interfaces/                   # 21 service interfaces
│   │   ├── MediaTR/
│   │   │   ├── Discount/Commands/
│   │   │   ├── Discount/Queries/
│   │   │   └── Invoice/Commands/
│   │   ├── Wrappers/
│   │   │   ├── Result.cs
│   │   │   └── JwtSettings.cs
│   │   └── DependencyInjection.cs        # Empty (DI wired in Program.cs)
│   │
│   ├── Roscoff.Infrastructure/           # Implementation layer
│   │   ├── Data/
│   │   │   └── RoscoffDbContext.cs       # EF Core DbContext (260 lines)
│   │   ├── Helpers/
│   │   │   ├── FattureInCloudSettings.cs
│   │   │   └── WorkingDayCalculator.cs
│   │   ├── Migrations/                   # EF Core migrations (18 migration files)
│   │   ├── Pdf/                          # HTML template generators
│   │   │   ├── CortiliaLabelGenerator.cs
│   │   │   ├── DdtTemplateGenerator.cs
│   │   │   ├── FoorbanLabelGenerator.cs
│   │   │   ├── LabelTemplateGenerator.cs
│   │   │   ├── MenuTemplateGenerator.cs
│   │   │   └── TechnicalSheetTemplateGenerator.cs
│   │   └── Services/
│   │       ├── AllergenService.cs
│   │       ├── AuthService.cs
│   │       ├── CategoryService.cs
│   │       ├── ClientDiscountService.cs
│   │       ├── CustomerService.cs
│   │       ├── DeliveryHubService.cs
│   │       ├── FattureInCloudService.cs   # External REST API client
│   │       ├── FoodCostService.cs
│   │       ├── IngredientService.cs
│   │       ├── InvoiceManagerService.cs
│   │       ├── Jobs/
│   │       │   └── BackgroundJobs.cs      # Hangfire InvoiceProcessingJob
│   │       ├── MenuService.cs
│   │       ├── NutritionService.cs
│   │       ├── OrderService.cs
│   │       ├── PasswordService.cs
│   │       ├── PdfEngineService.cs        # PuppeteerSharp singleton
│   │       ├── PlateService.cs
│   │       ├── PrinterService.cs
│   │       ├── StaffService.cs
│   │       └── TcpZplPrintService.cs
│   │
│   └── Roscoff.Api/                      # HTTP host layer
│       ├── Controllers/
│       │   ├── BaseApiController.cs       # Abstract base with HandleResult() + user claims
│       │   ├── AllergenController.cs
│       │   ├── AuthController.cs
│       │   ├── CategoryController.cs
│       │   ├── ClientDiscountController.cs
│       │   ├── CustomerController.cs
│       │   ├── DeliveryHubsController.cs
│       │   ├── IngredientController.cs
│       │   ├── InvoiceController.cs
│       │   ├── MenuController.cs
│       │   ├── OrderController.cs
│       │   ├── PlateController.cs
│       │   ├── PrintController.cs
│       │   └── StaffController.cs
│       ├── Extensions/
│       │   ├── JwtSetup.cs               # Empty class
│       │   └── SwaggerSetup.cs           # Swagger + JWT security definition
│       ├── HttpService/
│       │   └── CurrentUserService.cs     # ICurrentUserService implementation
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Dockerfile
│       ├── Program.cs                    # Composition root (194 lines)
│       └── appsettings.json
```

## Directory Purposes

**`src/Roscoff.Core/` (Domain Layer):**
- Purpose: Domain entities, enums, and shared interfaces with zero external framework dependencies
- Contains: Entity classes with data annotations, enums for domain concepts
- Key files: `BaseEntity.cs`, `IAuditableEntity.cs`, `OrderStatus.cs`

**`src/Roscoff.Application/` (Application Layer):**
- Purpose: Use-case contracts (interfaces), data transfer objects, CQRS handlers, API response wrappers
- Contains: Service interfaces, DTOs (as C# records or classes), MediatR command/query handlers, `Result<T>` wrapper
- Key files: `Interfaces/*.cs` (21 interfaces), `Wrappers/Result.cs`, `MediaTR/Invoice/Commands/CreatePendingInvoiceCommand.cs`

**`src/Roscoff.Infrastructure/` (Infrastructure Layer):**
- Purpose: All concrete implementations — persistence via EF Core, external API integration (FattureInCloud), PDF generation, thermal printing
- Contains: Service implementations, DbContext with audit, EF migrations, PDF HTML template generators
- Key files: `Data/RoscoffDbContext.cs`, `Services/*.cs` (20 service files), `Pdf/*.cs` (6 generators)

**`src/Roscoff.Api/` (API Layer):**
- Purpose: HTTP host, routing, JWT auth setup, Swagger, CORS, Hangfire dashboard, background job scheduling
- Contains: API controllers, Swagger+JWT setup extensions, `CurrentUserService` (HTTP-context aware)
- Key files: `Program.cs` (composition root), `Controllers/*.cs` (14 controllers)

## Key File Locations

**Entry Points:**
- `src/Roscoff.Api/Program.cs`: Application startup, DI registration, middleware pipeline, DB migration + seed, Hangfire recurring job setup

**Configuration:**
- `src/Roscoff.Api/appsettings.json`: JWT settings, FattureInCloud settings, SQL connection string, VAT mappings
- `src/Roscoff.Api/appsettings.Staging.json`: Staging environment overrides (SQL connection string)
- `src/Roscoff.Api/Properties/launchSettings.json`: Dev server ports (5051 HTTP, 7295 HTTPS)
- `Roscoff.slnx`: Solution file listing all 4 projects

**Core Logic:**
- `src/Roscoff.Infrastructure/Services/`: All business logic implementations
- `src/Roscoff.Application/MediaTR/`: CQRS command/query handlers
- `src/Roscoff.Infrastructure/Data/RoscoffDbContext.cs`: EF Core configuration, audit interception, relationship mapping

**External Integration Clients:**
- `src/Roscoff.Infrastructure/Services/FattureInCloudService.cs`: REST client for FattureInCloud invoice API
- `src/Roscoff.Infrastructure/Services/TcpZplPrintService.cs`: TCP/IP client for ZPL thermal printers

**PDF Generation:**
- `src/Roscoff.Infrastructure/Services/PdfEngineService.cs`: PuppeteerSharp wrapper (singleton browser)
- `src/Roscoff.Infrastructure/Pdf/MenuTemplateGenerator.cs`: Menu catalog PDF template
- `src/Roscoff.Infrastructure/Pdf/TechnicalSheetTemplateGenerator.cs`: Technical data sheet PDF template

## Naming Conventions

**Files:**
- **C# files:** PascalCase matching the class name (e.g., `PlateService.cs`, `OrderController.cs`)
- **Migrations:** Timestamp prefix with description (e.g., `20260629084932_SyncStagingModel.cs`)
- **DTOs:** Descriptive suffix matching purpose: `*RequestDto`, `*ResponseDto`, `*CreateDto`, `*UpdateDto`, `*ReadDto`, `*QueryParameters`

**Directories:**
- All directories use PascalCase matching .NET project/folder conventions
- Domain entities organized by business sub-domain: `Entities/Catalog/`, `Entities/Client/`, `Entities/Invoice/`
- DTOs organized by API surface area: `Dtos/Auth/`, `Dtos/Catalog/`, `Dtos/Client/`, `Dtos/Invoice/`, `Dtos/Common/`

**Classes/Records:**
- **Domain entities:** PascalCase singular nouns (e.g., `Plate`, `Menu`, `Ingredient`, `Order`, `Customer`)
- **Service interfaces:** `I{Purpose}Service` (e.g., `IPlateService`, `IOrderService`)
- **Service implementations:** `{Purpose}Service` (e.g., `PlateService`, `OrderService`)
- **DTO records:** Descriptive purpose names (e.g., `PlateResponseDto`, `CreateOrderRequestDto`, `PendingCustomerSummaryDto`)
- **MediatR commands/queries:** `{Action}{Entity}{Command|Query}` (e.g., `SetPlateDiscountCommand`, `CreatePendingInvoicesCommand`)
- **Controllers:** `{Entity}Controller` (e.g., `PlateController`, `InvoiceController`)

## Where to Add New Code

**New Feature (e.g., new entity + CRUD):**
1. Domain model: Add entity class to `src/Roscoff.Core/Entities/{SubDomain}/{EntityName}.cs` extending `BaseEntity<TId>`
2. Service contract: Add `I{EntityName}Service.cs` to `src/Roscoff.Application/Interfaces/`
3. DTOs: Add request/response records to `src/Roscoff.Application/Dtos/{SubDomain}/{EntityName}Dto.cs`
4. Implementation: Add `{EntityName}Service.cs` to `src/Roscoff.Infrastructure/Services/`
5. Controller: Add `{EntityName}Controller.cs` to `src/Roscoff.Api/Controllers/` extending `BaseApiController`
6. DI: Register in `src/Roscoff.Api/Program.cs` with `builder.Services.AddScoped<I{EntityName}Service, {EntityName}Service>()`
7. DbSet: Add `DbSet<{EntityName}>` to `src/Roscoff.Infrastructure/Data/RoscoffDbContext.cs`
8. EF Migration: Run `dotnet ef migrations add` from Infrastructure project

**New CQRS Operation (e.g., new invoice command):**
1. Command/Query: Add class implementing `IRequest<TResponse>` to `src/Roscoff.Application/MediaTR/{Area}/Commands/` or `Queries/`
2. Handler: Add class implementing `IRequestHandler<TRequest, TResponse>` in the same file
3. Controller action: Inject `IMediator` and call `_mediator.Send(command)`

**New PDF Template:**
1. Add HTML generator class to `src/Roscoff.Infrastructure/Pdf/{Purpose}TemplateGenerator.cs`
2. Inject `IPdfEngineService` into the calling controller/service

**New External API Integration:**
1. Add settings class to `src/Roscoff.Infrastructure/Helpers/`
2. Add interface to `src/Roscoff.Application/Interfaces/`
3. Add implementation to `src/Roscoff.Infrastructure/Services/`
4. Register with `AddHttpClient<IInterface, Implementation>()` in Program.cs (or AddScoped if not HTTP-based)

**Tests:**
- No test project currently exists. If adding tests, create at `tests/Roscoff.Api.Tests/` or co-located per project: `src/Roscoff.Application.Tests/`

## Special Directories

**`src/Roscoff.Infrastructure/Migrations/`:**
- Purpose: EF Core Code-First migrations (auto-generated)
- Generated: Yes (by `dotnet ef migrations add`)
- Committed: Yes — required for deployment

**`src/Roscoff.Api/.idea/`:**
- Purpose: JetBrains Rider IDE configuration
- Generated: Yes
- Committed: No (listed in `.gitignore`)

**`bin/`, `obj/`:**
- Purpose: Build artifacts
- Generated: Yes
- Committed: No (listed in `.gitignore`)

---

*Structure analysis: 2026-06-29*
