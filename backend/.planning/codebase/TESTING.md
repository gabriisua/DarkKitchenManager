# Testing Patterns

**Analysis Date:** 2026-06-29

## Test Framework

**Runner:**
- **Not detected.** No test project, test runner configuration, or test framework NuGet packages found in the solution.
- The solution (`Roscoff.slnx`) contains only 4 projects: `Roscoff.Api`, `Roscoff.Application`, `Roscoff.Core`, `Roscoff.Infrastructure`.
- No `*.test.*`, `*.spec.*`, or `*Tests.*` files exist anywhere in the repository.
- No `xunit`, `nunit`, `mstest`, or `Shouldly` packages in any `.csproj`.

**Assertion Library:**
- Not detected. No assertion libraries (FluentAssertions, Shouldly, etc.) present.

**Run Commands:**
```bash
# No test commands available — no test project configured
# The only runnable command would be:
dotnet run --project src/Roscoff.Api/Roscoff.Api.csproj
```

## Test File Organization

**Location:**
- No test files exist. No test projects or test directories found.

**Naming:**
- No naming convention established (no test files to reference).

**Structure:**
- No test structure exists. No test project in the solution file.

## Test Structure

**No tests exist in the codebase.** The following patterns would be appropriate based on the project architecture:

- **Unit tests for services:** `Roscoff.Infrastructure.Tests` project mirroring `Services/` structure
- **Unit tests for domain logic:** `Roscoff.Core.Tests` covering `WorkingDayCalculator`, entity invariants
- **Integration tests:** For `RoscoffDbContext` queries and EF Core behavior
- **Controller tests:** For API endpoint behavior via integration testing

## Mocking

**Framework:**
- Not detected. No Moq, NSubstitute, FakeItEasy, or other mocking frameworks in any `.csproj`.
- No `Microsoft.EntityFrameworkCore.InMemory` or `SQLite` for database mocking.

**Patterns:**
- No mocking patterns established.

**What to Mock:**
- Based on the codebase structure, the following should be mocked in tests:
  - `IRoscoffDbContext` - to isolate service tests from actual database
  - Service interfaces (e.g., `IOrderService`, `IPlateService`) - for controller tests
  - `IFattureInCloudService` - to avoid calling external API
  - `IPdfEngineService` - to avoid launching Chromium

**What NOT to Mock:**
- Domain entities - use real entity instances
- Value objects and DTOs - instantiate directly
- `WorkingDayCalculator` - test with real logic (no external dependencies)

## Fixtures and Factories

**Test Data:**
- No test data or fixture files exist in the repository.

**Location:**
- `src/Roscoff.Infrastructure/Migrations/` contains EF Core migration snapshots but no seed data for testing.

## Coverage

**Requirements:**
- **None enforced.** No coverage tool configured (`coverlet`, `dotCover`, `ReportGenerator` not found).
- No coverage threshold, no CI pipeline for test coverage.

**View Coverage:**
```bash
# No coverage commands available
```

## Test Types

**Unit Tests:**
- **Not present.** No unit test project exists.

**Integration Tests:**
- **Not present.** No integration test project exists.

**E2E Tests:**
- **Not present.** No E2E test framework or project.

## Testability Assessment

The architecture has several patterns that would facilitate adding tests:

**Good for testing:**
- **Interface-based service contracts** (`IOrderService`, `ICustomerService`, etc.) make it easy to mock services for controller tests.
- **`IRoscoffDbContext` abstraction** exists (`src/Roscoff.Application/Interfaces/IRoscoffDbContext.cs`) and is used by MediatR handlers and background jobs — this abstraction can be mocked with an in-memory EF Core context or a mock framework.
- **`Result<T>` wrapper** provides a consistent response structure that is easy to assert against.
- **`BaseApiController.HandleResult()`** centralizes response logic — test once.
- **Services use constructor DI** with clear dependencies.
- **`WorkingDayCalculator`** (`src/Roscoff.Infrastructure/Helpers/WorkingDayCalculator.cs`) is a pure logic class with no external dependencies — ideal for unit testing.
- **Value calculations in cents (int)** avoid floating-point rounding issues in assertions.

**Challenging for testing:**
- **Most infrastructure services depend on `RoscoffDbContext` directly** (e.g., `OrderService`, `IngredientService`, `PlateService`) rather than `IRoscoffDbContext` — requires a full `DbContext` setup or a more complex mock.
- **No `DbContext` pooling context factory** — each service new's up its own context via DI.
- **`PdfEngineService` launches a real Chromium browser** via PuppeteerSharp — requires either mocking `IPdfEngineService` or having Chrome installed in test environments.
- **Tuple return types** from services (`(bool, string, T?)`) are less conventional for test assertions than proper response objects.
- **`Program.cs`** contains inline dependency registration, Hangfire setup, JWT config, CORS, and DB seeding — difficult to test without extracting to a startup class.
- **No abstractions for `HttpContext`** in controllers — testing `AuthenticatedUserId`, `AuthenticatedUserRole` properties requires setting up `HttpContext.User` manually.

## Recommended Testing Setup

Based on the existing tech stack, the following is recommended for adding tests:

**Framework selection (not present but compatible):**
- **xUnit** — most common with .NET 10 projects, works well with the existing solution
- **Moq** or **NSubstitute** — for mocking interfaces
- **FluentAssertions** or **Shouldly** — for readable assertions
- **Microsoft.EntityFrameworkCore.InMemory** — for testing EF Core queries without a real SQL Server

**Suggested project structure (to add):**
```
src/
├── Roscoff.Core.Tests/           # Unit tests for domain logic
├── Roscoff.Application.Tests/     # Tests for MediatR handlers
├── Roscoff.Infrastructure.Tests/  # Tests for services, WorkingDayCalculator
└── Roscoff.Api.Tests/             # Integration tests for controllers
```

**Priority test candidates (highest value first):**
1. `WorkingDayCalculator.CalculateDeliveryDate()` — pure business logic, no dependencies
2. `FoodCostService.CalculatePlateFoodCostAsync()` — core pricing logic
3. `OrderService.CreateOrderAsync()` — complex orchestration with discounts, working days, pricing
4. `PasswordService` — password validation and hashing
5. `AuthService.LoginAsync()` / `RefreshTokenAsync()` — authentication flow
6. MediatR handlers: `CreatePendingInvoicesCommandHandler`, `SetPlateDiscountHandler`
7. `BaseApiController.HandleResult()` — response formatting

---

*Testing analysis: 2026-06-29*
