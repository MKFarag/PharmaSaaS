# PharmaSaaS — Agent Guide

> **Read [`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) first** — it is the single source of truth for all modules, API endpoints, actors, inputs/outputs, use cases, and role requirements. Every feature you implement must match what is defined there.

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
- **Hangfire** for background jobs (separate connection in config, dashboard at `/jobs` — NOT `/hangfire`).
- **NSwag** for Swagger/OpenAPI (not Swashbuckle).

## Commands

```powershell
dotnet build PharmaSaaS.slnx
dotnet test PharmaSaaS.slnx
dotnet test .\PharmaSaaS.Application.Test\PharmaSaaS.Application.Test.csproj --verbosity detailed
dotnet run --project .\PharmaSaaS.Presentation\PharmaSaaS.Presentation.csproj
```

Dev URLs: `http://localhost:5290`, Swagger at `/swagger`, ReDoc at `/redoc`, Hangfire at `/jobs`.

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
- **No local variables for requests:** Never assign `var request = command.Request;`. Access properties directly via `command.Request.X`.
- **Pattern matching for repository checks:** Combine null checks with the repository call using `is not { }` or `is { }` directly in the `if` statement instead of assigning to a variable first. Example: `if (await repo.GetAsync([id]) is not { } entity) return ...;`.
- **Primary key lookup:** Always use `GetAsync([id])` instead of `FindAsync(x => x.Id == id)` when searching strictly by primary key.
- **Folder names:** Never name a folder `Log` (e.g. `LogExpiryReport`), as it conflicts with `.gitignore` rules. Use alternatives like `Create` instead.
- **No magic strings — ever.** `nameof()` is mandatory for every string that references an entity property: includes, `ExecuteUpdateAsync` property names, sort/search columns, and any other place a property name is written as a string. Hardcoded strings are a bug waiting to happen on rename.
- **Controller actions:** `result.IsSuccess ? Ok(result.Value) : result.ToProblem()` ternary.
- **Test naming:** `Handle_WhenCondition_ReturnsResult`. Three-part: `// Arrange` / `// Act` / `// Assert`.
- **Test mocking:** `A.Fake<T>()` + `A.CallTo(...).Returns(...)` + `.MustHaveHappenedOnceExactly()`.

## No magic strings — full rules

This project forbids hardcoded property/navigation names as strings. **Always use `nameof()`.**

### Includes

```csharp
// ✅ Correct
var product = await _unitOfWork.Products.FindAsync(
    p => p.Id == id,
    [nameof(Product.Category), nameof(Product.DosageForm)]);

// ❌ Wrong
var product = await _unitOfWork.Products.FindAsync(
    p => p.Id == id,
    ["Category", "DosageForm"]);
```

### Nested includes (dot-notation)

```csharp
// ✅ Correct
string[] includes = [
    nameof(Order.Customer),
    $"{nameof(Order.Items)}.{nameof(OrderItem.Product)}"
];

// ❌ Wrong
string[] includes = ["Customer", "Items.Product"];
```

### ExecuteUpdateAsync property names

```csharp
// ✅ Correct
await _unitOfWork.Inventory.ExecuteUpdateAsync(
    i => i.ProductId == productId,
    nameof(Inventory.Quantity), newQty,
    nameof(Inventory.IsManualShortage), false);

// ❌ Wrong
await _unitOfWork.Inventory.ExecuteUpdateAsync(
    i => i.ProductId == productId,
    "Quantity", newQty,
    "IsManualShortage", false);
```

### Sort / search columns in `RequestFilters.Check()`

```csharp
// ✅ Correct
filters.Check([
    nameof(Product.Name),
    nameof(Product.Barcode),
    nameof(Product.CreatedAt)
]);

// ❌ Wrong
filters.Check(["Name", "Barcode", "CreatedAt"]);
```

> **Rule:** If you are writing a property name as a raw string literal anywhere in handler or repository code, stop and use `nameof()` instead.

## Result & Error pattern

`Result` / `Result<TValue>` live in `Domain/Abstraction/`. Errors are static `readonly` fields on `record` classes in `Domain/Errors/`.

```csharp
// Error definition (Domain/Errors/XErrors.cs)
public record ProductErrors
{
    public static readonly Error NotFound =
        new("Product.NotFound", "No product found", StatusCodes.NotFound);
    public static readonly Error DuplicatedBarcode =
        new("Product.DuplicatedBarcode", "...", StatusCodes.Conflict);
}

// Handler usage
if (entity is null)
    return Result.Failure<ProductResponse>(ProductErrors.NotFound);

return Result.Success(entity.Adapt<ProductResponse>());

// Controller usage
return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
```

`StatusCodes` used in errors is the **domain** `PharmaSaaS.Domain.Constants.StatusCodes` (int constants), NOT `Microsoft.AspNetCore.Http.StatusCodes`.

`result.ToProblem()` (in `Presentation/GlobalExtensions.cs`) builds a `ProblemDetails` with an `error` extension containing `{ code, description }`.

## FluentValidation

`SharpGrip.FluentValidation.AutoValidation.Mvc` is registered — validation runs **automatically** before the action executes. Controllers do **not** manually call validators for request body/query models. However, `RequestFilters.Check()` must still be called manually inside the handler to sanitize sort/search columns.

Validators are discovered from both `Presentation` and `Application` assemblies via `AddValidatorsFromAssembly()`.

## Authorization

### `[HasPermission(Permissions.X)]`
- `HasPermissionAttribute` extends `AuthorizeAttribute(permission)`.
- `PermissionAuthorizationPolicyProvider` creates a policy on demand wrapping a `PermissionRequirement`.
- `PermissionAuthorizationHandler` checks that `context.User.Claims` contains a claim with `Type == "permissions"` and `Value == requirement.Permission`.
- Permissions are stored as `RoleClaims` with `ClaimType = Permissions.Type` (`"permissions"`).

### `[SystemOwner]`
- `SystemOwnerAttribute` implements `IAuthorizationFilter` directly (NOT `AuthorizeAttribute`).
- Reads `configuration["SystemOwnerApiKey"]` and compares using `CryptographicOperations.FixedTimeEquals` (timing-safe).
- Returns `401` if key missing or wrong, `500` if `SystemOwnerApiKey` not configured.
- `TenantSubscriptionMiddleware` skips tenant resolution for endpoints decorated with `[SystemOwner]`.

### `Permissions` constants (`Domain/Constants/Permissions.cs`)
Groups: `Role`, `User`, `Category`, `DosageForm`, `Product`, `Inventory`. Format: `"domain:action"`, e.g. `"product:read"`, `"inventory:toggle-shortage"`. `Permissions.GetAll()` returns all via reflection.

### Default Roles (`Domain/Constants/DefaultRoles.cs`)
`DefaultRoles.Admin` and `DefaultRoles.User` — have fixed `Id` and `ConcurrencyStamp` constants for seeding. `Role.IsDefault = true` marks the seeded default role.

## Identity — dual layer

Domain `User` / `Role` entities are **thin wrappers** around ASP.NET `ApplicationUser` / `ApplicationRole`. Never interact with `ApplicationUser` directly from handlers — always go through `IUserRepository` / `IRoleRepository`.

- `user.CreateIdentity()` → creates `ApplicationUser` (Infrastructure extension).
- `role.CreateIdentity()` → creates `ApplicationRole`.
- `IdentityResult.ToDomain()` → converts to domain `Result`.
- `UserRepository` / `RoleRepository` use `UserManager<ApplicationUser>` / `RoleManager<ApplicationRole>` internally.

## Cache pattern

All cache keys and tags are **tenant-scoped** using `tenantId` as prefix.

```csharp
// Keys (Domain/Constants/Cache.cs)
Cache.Keys.AllCategories(tenantId)      // "{tenantId}:categories:all"
Cache.Keys.RolesWithoutDefault(tenantId)
Cache.Keys.AllInventory(tenantId)
Cache.Keys.AllShortages(tenantId)

// Tags (invalidate a group)
Cache.Tags.Categories(tenantId)         // "{tenantId}:categories"
Cache.Tags.Roles(tenantId)
Cache.Tags.Inventory(tenantId)
Cache.Tags.Shortages(tenantId)

// Expirations
Cache.Expirations.Short   // 5 min
Cache.Expirations.Medium  // 30 min
Cache.Expirations.Long    // 1 hour
Cache.Expirations.VeryLong // 24 hours
```

**Read handler** — wrap with `GetOrCreateAsync(key, factory, expiration, tags: [tag])`.  
**Write handler** — after `CompleteAsync()`, call `cacheService.RemoveByTagAsync(Cache.Tags.X(tenantId))`. The `tenantId` comes from `ICurrentTenantGetter.GetTenantId()`.

`CacheService` wraps `HybridCache`. Local cache expiration is automatically set to `expiration * 0.2`.

## Rate limiting

Four policies registered in `DependencyInjection.cs`:

| Policy | Type | Usage |
|---|---|---|
| `IpLimit` | Fixed window | `AuthController` (20 req/60s per IP) |
| `UserLimit` | Fixed window | Available (60 req/60s per user) |
| `Concurrency` | Concurrency | Most API controllers (80 concurrent) |
| `Sliding` | Sliding window | Available |

Apply via `[EnableRateLimiting(RateLimitingOptions.PolicyNames.X)]` on the controller class.

## Pagination pattern

End-to-end flow for paginated, searchable, sortable lists:

```
RequestFilters (from controller)
  → Handler receives RequestFilters, calls repo method
  → GenericRepositoryWithPagination.ApplySearchFilter() / .OrderBy() via System.Linq.Dynamic.Core
  → PaginatedList<T>.CreateAsync() builds the result
  → Returns IPaginatedList<T> to controller
```

**`RequestFilters`** (`Application/Contracts/Common/RequestFilters.cs`) — input record with `PageNumber` (default 1), `PageSize` (default 10, max 250), `SearchValue`, `SearchColumn`, `SortColumn`, `SortDirection` (default `"ASC"`). Validated by `RequestFiltersValidator` (alphanumeric_underscore for column names, ASC/DESC only).

**`RequestFilters.Check()`** (`Application/GlobalExtensions.cs`) — extension on `RequestFilters`. Call in the handler **after** FluentValidation. Pass `HashSet<string>` of allowed sort/search column names; it sanitizes against them (case-insensitive) and falls back to the first allowed column if invalid. Also validates `SortDirection` via `OrderBy.IsValid()`.

**`IPaginatedRepository<TEntity>`** (`Domain/Repositories/IPaginatedRepository.cs`) — 8 overloads split into `GetPaginatedListAsync` (no predicate) and `FindPaginatedListAsync` (with `Expression<Func<TEntity, bool>>`). Each comes with combinations of: search, includes, or basic (page+primaryKey only).

**`GenericRepositoryWithPagination<TEntity>`** (`Infrastructure/Persistence/Repositories/`) — the implementation. Uses `QueryableExtensions`:
- `.ApplySearchFilter(searchValue, searchColumn, ColumnType)` — dynamic filter via `System.Linq.Dynamic.Core` (supports `String`, `Int`, `Bool`).
- `.OrderBy(sortColumn, sortDirection)` — dynamic sorting via `string OrderBy`.
- `.ApplyIncludesSafely(includes)` — applies `.Include()` with auto `AsSplitQuery()` when include depth ≥ 2 or count ≥ 3. Pass `QueryHint.SplitQuery` anywhere in the includes array to force split query regardless.

**`PaginatedList<T>`** (`Infrastructure/Persistence/Common/PaginatedList.cs`) — static `CreateAsync(IQueryable, pageNumber, pageSize)` builds `IPaginatedList<T>` with `Items`, `TotalPages`, `HasPreviousPage`, `HasNextPage`.

**`EmptyPaginatedList.Create<T>()`** (`Domain/Abstraction/IPaginatedList.cs`) — returns a zero-page `IPaginatedList<T>` with empty `Items` list. Use when no records match, avoids null checks.

Repository `IGenericRepositoryWithPagination<TEntity>` combines `IGenericRepository<TEntity> + IPaginatedRepository<TEntity>`.

## GenericRepository — full API surface

Beyond pagination, `IGenericRepository<TEntity>` provides:

**Read (AsNoTracking):** `GetAllAsync`, `FindAsync`, `FindAllAsync` — all with optional `includes[]` overloads.  
**Read (Tracked):** `GetAsync` (PK lookup), `TrackedFindAsync`, `TrackedFindAllAsync` — use these before calling entity behavior methods that EF must track.  
**Projection:** `GetAllProjectionAsync<TProjection>`, `FindAllProjectionAsync<TProjection>` — with optional includes and `selector`/`distinct` overloads. Projection uses Mapster's `.ProjectToType<T>()`.  
**Write:** `AddAsync`, `AddRangeAsync`, `Update`, `UpdateRange`, `Delete`, `DeleteRange`.  
**Bulk (no tracking):** `ExecuteDeleteAsync`, `ExecuteUpdateAsync` (1–3 property overloads via `EF.Property<T>`).  
**Helpers:** `AnyAsync`, `AnyWithWhereAsync`, `CountAsync`, `Attach`, `AttachRange`, `MarkPropertyAsModified`.

## UnitOfWork — transaction support

`IUnitOfWork` also exposes:
```csharp
Task BeginTransactionAsync(CancellationToken cancellationToken = default);
Task CommitTransactionAsync(CancellationToken cancellationToken = default);
Task RollbackTransactionAsync();
```
Use for multi-step operations that must be atomic. Begin → do work → Commit (or Rollback on failure).

## Middleware & request pipeline order

```
UseExceptionHandler → UseOpenApi (dev) → UseSerilogRequestLogging
→ UseHttpsRedirection → MapHangfireDashboard → UseCors
→ UseAuthentication → UseAuthorization → TenantSubscriptionMiddleware
→ UseRateLimiter → MapControllers → MapHealthChecks
```

**`TenantSubscriptionMiddleware`** — after auth. Skips `/swagger`, `/redoc`, `/health`, `/jobs`. For all other requests: requires `X-Tenant-Id` header → looks up tenant in `TenantSettings` → rejects with `403` if `tenant.EndDate < today`.

## Idempotency

`[Idempotent]` action filter (`Presentation/Attributes/IdempotentAttribute.cs`) — add to any write action that must be idempotent. Requires `X-Idempotency-Key` header. Uses `HybridCache` directly (not `ICacheService`):
- Sets `"in-progress"` for 30s while processing → returns `409` on duplicate concurrent request.
- Caches successful (2xx) `ObjectResult` for 5 min (local 1 min) → replays on repeat.
- Non-2xx responses are not cached (key removed).

## Logging

Serilog configured via `appsettings.json`. Logs written to:
- Console (human-readable template)
- `logs/log-{date}.txt` (JSON format, daily rolling)

Minimum level: `Warning` for most sources. `UseSerilogRequestLogging()` logs HTTP requests.

## XML doc comments

- **Controllers:** Every action method gets full XML: `<summary>`, `<remarks>` (with sample request), `<param>` (each), `<returns>`, `<response code="">`. Class-level `<summary>` matching the pattern `Manage {domain area}.`.
- **Repository interfaces:** `<summary>` on every method. Application handlers, commands, queries, DTOs, entities: **no XML docs**.
- All English, multi-line `///` format, period at end.

## Implemented features (only these exist)

See the **Implementation Status** table at the bottom of [`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) for the authoritative, up-to-date list.

Summary as of last update:
- ✅ Auth (register, login, confirm email, reset password, refresh tokens)
- ✅ Categories, DosageForms (Lookups)
- ✅ Pharmacies, Subscription Requests
- ✅ Roles, Users (Employees), Profile
- ✅ Products
- ✅ Inventory (UpdateStock, ToggleShortage, GetShortages, LogExpiryReport, GetAllInventories, StockLevelReport, OutOfStockReport)
- ❌ Customers, Loyalty
- ❌ Shifts, Invoices, Expenses
- ❌ Suppliers, Purchase Orders, Supplier Payments
- ❌ Reports
- ❌ AI Prescription Parser

---

## Spec Sync Protocol (agent MUST follow this)

[`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) is the **source of truth** for all planned features. The agent must treat it as a living document.

### When implementing a task

If the user asks to implement something **that already exists in the MD spec**:
- After completing the implementation, update the **Implementation Status** table at the bottom of `PharmaSaaS_SystemAnalysis.md` to mark the module as `✅ Implemented`.
- Also mark it as done in `tasks/tasks.md` under the `[x]` completed section.
- Do NOT modify the spec details (endpoints, inputs, outputs) unless the implementation genuinely differs.

### When the user adds extra endpoints or modifies behavior

If the implementation **differs from the MD spec** (different route, extra endpoints, different input/output, different behavior):

1. **Immediately inform the user** with a clear note, e.g.:
   > ⚠️ **Deviation from spec**: This endpoint (`POST /api/auth/register`) is not in the spec (which only defines `POST /api/auth/login`). Confirm to update `PharmaSaaS_SystemAnalysis.md`?

2. If the user **confirms**, update `PharmaSaaS_SystemAnalysis.md` directly — add the new endpoint row to the relevant module table and update the Implementation Status table.

3. If the user **does not confirm**, leave the MD as-is and document the deviation in `tasks/tasks.md` under a `## Deviations` section.

### tasks/tasks.md

Always keep `tasks/tasks.md` up to date. The file lives at `g:/ASP.NET/PharmaSaaS/tasks/tasks.md` and is the single source of progress tracking. See the file itself for the current state.
