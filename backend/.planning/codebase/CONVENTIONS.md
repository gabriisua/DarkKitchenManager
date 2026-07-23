# Coding Conventions

**Analysis Date:** 2026-06-29

## Naming Patterns

**Files:**
- PascalCase for all C# files matching the class/interface name: `OrderService.cs`, `BaseEntity.cs`, `IOrderService.cs`
- Folders use PascalCase for project directories: `Roscoff.Api/`, `Roscoff.Core/`, `Roscoff.Infrastructure/`, `Roscoff.Application/`
- Sub-folders use PascalCase: `Controllers/`, `Entities/`, `Services/`, `Interfaces/`, `Dtos/`

**Functions/Methods:**
- PascalCase for all public methods: `CreateOrderAsync()`, `GetByIdAsync()`, `HandleResult<T>()`
- PascalCase for private methods: `MapToDto()`, `GenerateJwtToken()`, `AddWorkingDays()`
- Async suffix for all async methods: `GetPagedAsync()`, `SaveChangesAsync()`

**Variables:**
- camelCase for local variables: `searchTerm`, `validPage`, `totalCount`
- Underscore-prefixed camelCase for private fields: `_orderService`, `_context`, `_pdfEngine`
- PascalCase for public/static readonly fields: `AllRoles`, `CutOffHour`
- PascalCase for constants: `CutOffHour`, `LogoBase64` pattern (private const in one file)

**Types:**
- PascalCase for all types: `OrderService`, `CustomerController`, `Result<T>`
- `I` prefix for interfaces: `IOrderService`, `IAuditableEntity`, `IRoscoffDbContext`
- Generic type parameter `T` for single-parameter generics: `Result<T>`, `BaseEntity<TId>`, `PaginatedResponseDto<T>`
- DTO suffix for data transfer objects: `CreateOrderRequestDto`, `PlateResponseDto`, `LoginRequestDto`
- Handlers named with purpose: `CreatePendingInvoicesCommandHandler`, `SetPlateDiscountHandler`

## Code Style

**Formatting:**
- No `.editorconfig`, `.prettierrc`, or `Directory.Build.props` files detected — no automated formatting enforcement
- All `.csproj` files use `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- File-scoped namespaces used throughout (e.g., `namespace Roscoff.Api.Controllers;` not block-scoped)
- Default .NET formatting conventions assumed (no custom rules detected)

**Linting:**
- No ESLint/StyleCop/Analyzer configuration files detected
- No Roslyn analyzer NuGet packages in `.csproj` files
- No custom `.editorconfig` in the repository

## Import Organization

**Order:**
1. `System.*` namespaces first (System, System.Text, System.Security, etc.)
2. Microsoft namespaces (Microsoft.AspNetCore.*, Microsoft.EntityFrameworkCore, Microsoft.IdentityModel.*)
3. Third-party packages (BCrypt.Net, Hangfire, MediatR, PuppeteerSharp)
4. Project-internal namespaces (Roscoff.Core.*, Roscoff.Application.*, Roscoff.Infrastructure.*, Roscoff.Api.*)

Example from `OrderService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Invoice;
using Roscoff.Core.Enums;
using Roscoff.Infrastructure.Data;
```

**Path Aliases:**
- Not used. All imports are full `using` directives with explicit namespaces.

## Error Handling

**Strategy:**
- Service layer returns tuples with `(bool Success, string Message)` or `(bool Success, string Message, T? Data)` patterns instead of throwing exceptions for business logic errors
- Use `Result<T>` wrapper class from `Roscoff.Application.Wrappers` for API response standardization
- Controllers use `HandleResult<T>()` from `BaseApiController` to translate `Result<T>` into appropriate HTTP status codes
- Exceptions are caught in specific places (background jobs, external API calls) and logged with `ILogger`

**Patterns:**

Service tuple returns:
```csharp
// File: src/Roscoff.Infrastructure/Services/OrderService.cs
public async Task<(bool Success, string Message, OrderResponseDto? Data)> CreateOrderAsync(
    CreateOrderRequestDto request, CancellationToken cancellationToken = default)
{
    if (customer == null) return (false, "Cliente non trovato o inattivo.", null);
    // ...
    return (true, "Ordine creato con successo.", responseDto);
}
```

Result<T> factory methods:
```csharp
// File: src/Roscoff.Application/Wrappers/Result.cs
public static Result<T> Success(T data, string? message = null)
{
    return new Result<T>(data, message);
}
public static Result<T> Failure(string message, List<string>? errors = null)
{
    return new Result<T>(message, errors);
}
```

Controller response translation (`BaseApiController`):
```csharp
// File: src/Roscoff.Api/Controllers/BaseApiController.cs
protected IActionResult HandleResult<T>(Result<T> result)
{
    if (result == null) return NotFound(new Result<string> { Succeeded = false, Message = "Risorsa non trovata." });
    if (result.Succeeded) return Ok(result);
    if (!string.IsNullOrWhiteSpace(result.Message) && result.Message.Contains("non trovat", StringComparison.OrdinalIgnoreCase))
        return NotFound(result);
    return BadRequest(result);
}
```

Exception handling (background jobs):
```csharp
// File: src/Roscoff.Infrastructure/Services/Jobs/BackgroundJobs.cs
catch (Exception ex)
{
    _logger.LogError(ex, "Errore durante la generazione della fattura cumulativa per il cliente con ID {CustomerId}", customerGroup.Key);
    foreach (var invoice in customerGroup) { invoice.Status = PendingInvoiceStatus.Failed; ... }
}
```

## Logging

**Framework:** `Microsoft.Extensions.Logging.ILogger<T>` from ASP.NET Core

**Patterns:**
- Constructor-injected `ILogger<T>` for service classes
- Logging used primarily in background job processing and external API error handling
- Structured logging with placeholders: `_logger.LogError(ex, "Errore eliminazione fattura {DocId} su FiC", ficDocumentId)`
- `LogInformation` for operational milestones: `_logger.LogInformation("Avvio controllo fatture in coda...")`
- `LogWarning` for retry scenarios: `_logger.LogWarning("Database non ancora pronto. Tentativi rimasti: {retries}...")`
- Controllers do NOT log directly — only service/infrastructure layer logs
- Minimal logging depth — most methods in `Services/` and `Controllers/` do not log at all

Example:
```csharp
// File: src/Roscoff.Infrastructure/Services/InvoiceManagerService.cs
_logger.LogError(ex, "Errore recupero URL fattura {FicDocumentId}", ficDocumentId);
```

## Comments

**When to Comment:**
- XML doc comments (`/// <summary>`) used on all public controller action methods
- Inline comments explain "why" not "what" — business logic rationale
- Italian language for most comments (some English mixed in)
- Section separators with comment blocks: `// =========================================================` and `#region`/`#endregion`
- "NUOVO" (NEW) markers used to flag recently added code
- Comments warn about potential issues: `// ATTENZIONE: In produzione invia il token via email, non restituirlo nell'API!`
- TODO-like markers in Italian: `// Nota: Se nel DB i tuoi utenti usano un ID di tipo 'int'...`

**JSDoc/TSDoc:**
- Not applicable (C# XML doc comments used instead)

Example:
```csharp
/// <summary>
/// Ottiene la lista degli ordini filtrata e paginata (supporta PageSize = -1).
/// </summary>
[HttpGet]
public async Task<IActionResult> GetPaged([FromQuery] OrderQueryParameters filter, CancellationToken cancellationToken)
```

## Function Design

**Size:**
- Services: methods typically 5–80 lines; paginated query methods are the longest (60–80 lines for complex filtering)
- Controllers: methods typically 3–15 lines, delegating to service layer
- `OrderService.CreateOrderAsync()`: ~88 lines (largest service method)

**Parameters:**
- Maximum 4–5 parameters for service methods; controllers use DTOs with `[FromBody]` or `[FromQuery]`
- Complex query parameters use dedicated DTO classes inheriting from `BasePaginationRequestDto`
- `CancellationToken` is always the last parameter with `= default` default value
- Tuples for returning multiple values: `(bool Success, string Message, OrderResponseDto? Data)`

Example:
```csharp
public async Task<(bool Success, string Message)> UpdateStatusAsync(
    Guid id, OrderStatus newStatus, CancellationToken cancellationToken = default)
```

**Return Values:**
- Services return tuples, DTOs, or `PaginatedResponseDto<T>` (never expose entities to controllers)
- Controllers return `IActionResult` wrapping `Result<T>`
- Null return from service signals "not found" — controller translates to HTTP 404
- `true`/`false` from service — controller translates to `Result<T>.Success()` or `Result<T>.Failure()`

## Module Design

**Exports:**
- Classes are `public` by default in this project (internal not used except for `Program.cs`)
- Interfaces in `Roscoff.Application.Interfaces` are the public API boundary
- Implementation classes in `Roscoff.Infrastructure.Services` are registered via DI in `Program.cs`
- Static template generators in `Roscoff.Infrastructure.Pdf` are `public static class`

**Barrel Files:**
- Not used. Each C# file contains a single type.
- Namespace maps to folder structure (standard .NET convention)

## Project Structure Conventions

**Architecture Layers (Clean Architecture style):**
| Project | Dependencies | Purpose |
|---------|-------------|---------|
| `Roscoff.Core` | EF Core Abstractions only | Entities, Enums, Core interfaces |
| `Roscoff.Application` | Core, MediatR, EF Core | Interfaces, DTOs, MediatR CQRS, Wrappers |
| `Roscoff.Infrastructure` | Core, Application, BCrypt, EF Core SQL Server, Hangfire, PuppeteerSharp | Services, DbContext, PDF generators, Migrations |
| `Roscoff.Api` | Application, Infrastructure | Controllers, Program.cs, Startup config |

**DI Registration:**
- All services registered explicitly in `Program.cs` with `.AddScoped<IInterface, Implementation>()`
- One exception: `PdfEngineService` registered as Singleton (manages a shared Chromium browser instance)
- `IFattureInCloudService` registered via `AddHttpClient<I, T>()`
- MediatR registered via `RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)`

**Entity Design:**
- All entities inherit from `BaseEntity<TId>` (int for Catalog entities, Guid for Client/Invoice entities)
- `IAuditableEntity` provides audit fields: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- `BaseEntity` initializes defaults: `CreatedAt = DateTime.UtcNow`
- EF Core table mapping via `[Table("name")]` attribute on entity classes
- Composite keys on join tables use `[Key]` on multiple properties in `OnModelCreating`
- Soft delete via `IsActive` boolean property on most entities

**DTO Design:**
- Read-only DTOs use `record` types (positional): `public record OrderResponseDto(Guid Id, ...)`
- Request DTOs with validation use `class` with `[Required]`, `[StringLength]`, `[EmailAddress]` attributes
- Partial-update DTOs use nullable properties: `public string? Name { get; set; }`
- Pagination parameters inherit from `BasePaginationRequestDto`

## Inconsistencies Found

- **Namespace inconsistency:** `src/Roscoff.Api/Controllers/` uses both `Roscoff.Api.Controllers` (e.g., `AuthController`) and `Roscoff.API.Controllers` (e.g., `OrderController`, `CategoryController`)
- **Response pattern inconsistency:** Some controllers use `HandleResult(Result<T>.Success(...))` (e.g., `IngredientController`), others use `Ok(Result<T>.Success(...))` directly (e.g., `OrderController`, `InvoiceController`)
- **Auth attribute inconsistency:** Some controllers use `[Authorize(Roles = StaffRoles.Manager)]` with the `StaffRoles` static class (e.g., `CustomerController`, `AllergenController`), while `InvoiceController` uses raw string `[Authorize(Roles = "MANAGER")]`
- **CancellationToken usage inconsistent:** Some controllers pass `CancellationToken` to services, others do not
- **Empty stub files:** `src/Roscoff.Api/Extensions/JwtSetup.cs` and `src/Roscoff.Application/DependencyInjection.cs` are empty classes with no implementation

---

*Convention analysis: 2026-06-29*
