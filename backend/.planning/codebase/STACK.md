# Technology Stack

**Analysis Date:** 2026-06-29

## Languages

**Primary:**
- C# (.NET) - All backend code across 4 projects: API, Application, Core, Infrastructure

**Secondary:**
- ZPL (Zebra Programming Language) - Label printer templates embedded in C# string builders in `src/Roscoff.Infrastructure/Pdf/`

## Runtime

**Environment:**
- .NET 10.0 (`net10.0`) - All four projects target this framework

**Package Manager:**
- NuGet
- Lockfile: Not detected (no `packages.lock.json` checked in)

## Frameworks

**Core:**
- ASP.NET Core 10.0 - Web API host (`src/Roscoff.Api/Roscoff.Api.csproj`)
- Entity Framework Core 10.0 - ORM / data access (`src/Roscoff.Infrastructure/`, `src/Roscoff.Application/`)
- MediatR 14.1.0 - CQRS mediator pattern (`src/Roscoff.Application/Roscoff.Application.csproj`)

**Testing:**
- Not detected — no test project or test files found in solution

**Build/Dev:**
- Swashbuckle.AspNetCore 6.4.0 - Swagger/OpenAPI generation (`src/Roscoff.Api/`)
- Docker - Containerized deployment (`src/Roscoff.Api/Dockerfile`, `.dockerignore`)

## Key Dependencies

**Critical:**
| Package | Version | Project | Purpose |
|---------|---------|---------|---------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.0 | `Roscoff.Api` | JWT auth for API |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 | `Roscoff.Api`, `Roscoff.Infrastructure` | SQL Server database provider |
| `BCrypt.Net-Next` | 4.0.3 | `Roscoff.Api`, `Roscoff.Infrastructure` | Password hashing |
| `Hangfire` | 1.8.23 | `Roscoff.Api`, `Roscoff.Infrastructure` | Background job processing |
| `MediatR` | 14.1.0 | `Roscoff.Application` | CQRS / request pipeline |
| `System.IdentityModel.Tokens.Jwt` | 8.14.0 | `Roscoff.Api`, `Roscoff.Infrastructure` | JWT token handling |
| `PuppeteerSharp` | 25.1.1 | `Roscoff.Infrastructure` | PDF generation from HTML |
| `Microsoft.EntityFrameworkCore.Abstractions` | 10.0.0 | `Roscoff.Core` | Minimal EF abstractions in domain layer |

**Infrastructure:**
- `Microsoft.EntityFrameworkCore.Tools` 10.0.0 - EF migrations tooling (dev dependency)

## Configuration

**Environment:**
- `ASPNETCORE_ENVIRONMENT` environment variable controls profile (`Development`/`Staging`/`Production`)
- Config files used:
  - `src/Roscoff.Api/appsettings.json` — base config (connection string, JWT, FattureInCloud)
  - `src/Roscoff.Api/appsettings.Staging.json` — staging overrides (connection string only)
  - `Properties/launchSettings.json` — local dev URLs (`http://localhost:5051`, `https://localhost:7295`)

**Key configs required:**
- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `JwtSettings:Key` — 256-bit+ symmetric signing key
- `JwtSettings:Issuer`/`Audience` — token validation
- `FattureInCloudSettings:AccessToken` — FIC API OAuth token
- `FattureInCloudSettings:CompanyId` — FIC company ID
- `FattureInCloudSettings:VatMappings` — VAT rate → FIC ID mapping

**Build:**
- Multi-stage Docker build via `src/Roscoff.Api/Dockerfile`
- Solution file: `Roscoff.slnx`

## Platform Requirements

**Development:**
- .NET SDK 10.0
- SQL Server (local or Docker)
- Visual Studio 2026+ / JetBrains Rider / VS Code with C# extension
- Chromium (downloaded automatically by PuppeteerSharp on first run)

**Production:**
- Linux container (`mcr.microsoft.com/dotnet/aspnet:10.0`)
- SQL Server instance
- Network access to FattureInCloud API (`https://api-v2.fattureincloud.it`)
- Network access to TCP label printers (port 9100)
- Chromium installed (auto-downloaded or pre-bundled)

---

*Stack analysis: 2026-06-29*
