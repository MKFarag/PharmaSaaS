# PharmaSaaS — Agent Guide

> **Read `PharmaSaaS_SystemAnalysis.xlsx` first** — it defines all modules, API endpoints, actors, inputs/outputs, use cases, and role requirements. This is the single source of truth for what every feature should do. The file is binary (xlsx = zipped XML); extract with PowerShell:
> ```powershell
> Add-Type -AssemblyName System.IO.Compression.FileSystem
> $z = [System.IO.Compression.ZipFile]::OpenRead("PharmaSaaS_SystemAnalysis.xlsx")
> foreach ($e in $z.Entries) { ... }
> $z.Dispose()
> ```

## Project overview

.NET 10 ASP.NET Core Web API — Clean Architecture + DDD + CQRS. Mid-development MVP.

```
PharmaSaaS.Domain/         — Entities, repositories, Result pattern, constants (no deps)
PharmaSaaS.Application/    — CQRS commands/queries/handlers, DTOs, FluentValidation, Mapster
PharmaSaaS.Infrastructure/ — EF Core, Identity, JWT, MailKit, HybridCache+Redis, Hangfire
PharmaSaaS.Presentation/   — ASP.NET Web API host (Program.cs, controllers, middleware)
PharmaSaaS.Application.Test/ — xUnit + FakeItEasy unit tests
```

Entrypoint: `PharmaSaaS.Presentation/Program.cs`. Dependency wiring in `DependencyInjection.cs` at each layer.

## Quirks an agent WILL miss

- **No MediatR.** Custom CQRS: `IRequest<T>` / `IRequestHandler<T, R>` / `ISender`. Handlers auto-registered via `AddCQRSConfig()` scanning assemblies.
- **Custom Result pattern** — `Result<TValue>` / `Error(Code, Description, StatusCode)`. Controllers use `result.ToProblem()`.
- **Custom authorization** — `[SystemOwner]` (checks `X-System-Owner-Key` header), `[HasPermission(Permissions.X)]` (custom `AuthorizationHandler` + `AuthorizationPolicyProvider`). NOT standard ASP.NET policies.
- **Two-database architecture:** Shared DB (`SharedDbContext`: Pharmacies, SubscriptionRequests) + Per-tenant DB (`ApplicationDbContext`: all ops + Identity tables). Both SQL Server.
- **`UnitOfWork` wraps both contexts** — call `CompleteAsync()` for ApplicationDb, `SharedCompleteAsync()` for SharedDb.
- **Tenant resolved from `X-Tenant-Id` header** (not from subdomain/route). `PharmacyId` claim also in JWT.
- **Tenant connection strings in `appsettings.json`** under `TenantSettings:Tenants`, NOT in shared DB.
- **API versioning via `x-api-version` header** (not URL). Default 1.0.
- **C# experimental extension syntax** — `extension(IServiceCollection services) { ... }`. Requires .NET 10 preview SDK.
- **HybridCache** (Microsoft.Extensions.Caching.Hybrid) with Redis + local memory.
- **Mapster** for mapping (not AutoMapper). Tests use `MapsterTestFixture` fixture to avoid re-init.
- **Hangfire** for background jobs (separate connection in config, dashboard at `/hangfire`).
- **NSwag** for Swagger/OpenAPI (not Swashbuckle).

## Commands

```powershell
dotnet build PharmaSaaS.slnx
dotnet test PharmaSaaS.slnx
dotnet test .\PharmaSaaS.Application.Test\PharmaSaaS.Application.Test.csproj --verbosity detailed
dotnet run --project .\PharmaSaaS.Presentation\PharmaSaaS.Presentation.csproj
```

Dev URLs: `http://localhost:5290`, Swagger at `/swagger`, ReDoc at `/redoc`.

## Migrations

```powershell
dotnet ef migrations add Name --context SharedDbContext --project .\PharmaSaaS.Infrastructure\
dotnet ef migrations add Name --context ApplicationDbContext --project .\PharmaSaaS.Infrastructure\
```

Provisioning auto-applies on startup via `TenantProvisioningService.ProvisionAllTenantsAsync()`.

## Testing

- xUnit + FakeItEasy + coverlet.
- Handlers instantiated with `A.Fake<T>()` deps. Tests verify call counts via `MustHaveHappenedOnceExactly()`.
- No integration or E2E tests yet. No CI/CD. No Docker.

## Coding style (agent MUST match this)

- **4-space indentation** (no tabs). File-scoped namespaces. Global usings per project in `GlobalUsings.cs`.
- **Private fields:** `_camelCase`. Primary constructors preferred for simple DI; regular ctors only for init logic.
- **Expression-bodied members** for simple methods/properties (`void Update(string n) => Name = n;`).
- **Records** for commands, queries, DTOs, errors. **`sealed class`** for entities, services, handlers.
- **Entities:** public auto-props + static `Create(...)` factory + behavior methods (no constructors).
- **Handlers:** single `Handle` method, `public sealed class`, primary ctor for DI.
- **Null checks:** `is not { }` / `is null` pattern matching. `default!` for required nav props.
- **Controller actions:** `result.IsSuccess ? Ok(result.Value) : result.ToProblem()` ternary.
- **Test naming:** `Handle_WhenCondition_ReturnsResult`. Three-part: `// Arrange` / `// Act` / `// Assert`.
- **Test mocking:** `A.Fake<T>()` + `A.CallTo(...).Returns(...)` + `.MustHaveHappenedOnceExactly()`.

### XML doc comments

- **Controllers:** Every action method gets full XML: `<summary>`, `<remarks>` (with sample request), `<param>` (each), `<returns>`, `<response code="">`. Class-level `<summary>` matching the pattern `Manage {domain area}.`.
- **Repository interfaces:** `<summary>` on every method. Application handlers, commands, queries, DTOs, entities: **no XML docs**.
- All English, multi-line `///` format, period at end.

## Implemented features (only these exist)

Auth (register, login, confirm email, reset password, refresh tokens), Categories, DosageForms, Pharmacies, SubscriptionRequests, Roles, Users, Profile.

**Not yet implemented:** Products, Inventory, POS, Procurement, Customers, Reports, AI Prescription Parser.
