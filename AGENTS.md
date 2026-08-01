# PharmaSaaS — Agent Guide

> **Read [`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) first.**
> It is the single source of truth for all modules, API endpoints, actors, inputs/outputs, and role requirements.
> Every feature you implement must match what is defined there.

---

## 1. Solution Structure

```
PharmaSaaS.Domain/           — Entities, repository interfaces, Result/Error, constants (zero deps)
PharmaSaaS.Application/      — CQRS handlers, DTOs, validators, services, mappings
PharmaSaaS.Infrastructure/   — EF Core, Identity, JWT, MailKit, HybridCache+Redis, Hangfire
PharmaSaaS.Presentation/     — ASP.NET Web API host: controllers, middleware, Program.cs
PharmaSaaS.Application.Test/ — xUnit + FakeItEasy unit tests
```

Entrypoint: `Program.cs`. Every layer has its own `DependencyInjection.cs`.

---

## 2. Where Every File Lives (MANDATORY — never deviate)

This table is the law. When you create any file, look it up here first.

| Concept | Folder |
|---|---|
| Entity | `Domain/Entities/` |
| Error class | `Domain/Errors/` |
| Repository interface | `Domain/Repositories/` |
| Domain constants (Cache, Permissions, etc.) | `Domain/Constants/` |
| Result / IPaginatedList abstractions | `Domain/Abstraction/` |
| **Request DTO** (input to controller) | **`Application/Contracts/{Module}/`** |
| **Response DTO** (output from handler) | **`Application/Contracts/{Module}/`** |
| **FluentValidation validator** | **`Application/Contracts/{Module}/`** (same file as its DTO) |
| **CQRS command record** | **`Application/Features/{Module}/{UseCase}/`** |
| **CQRS query record** | **`Application/Features/{Module}/{UseCase}/`** |
| **CQRS handler** (`sealed class`) | **`Application/Features/{Module}/{UseCase}/`** |
| Service interface | `Application/Interfaces/Services/` |
| Service implementation | `Application/Services/` |
| Mapster mapping config | `Application/Mapping/` |
| CQRS messaging interfaces | `Application/Abstractions/Messaging/` |
| EF Core `DbContext`, configurations | `Infrastructure/Persistence/` |
| Repository implementations | `Infrastructure/Persistence/Repositories/` |
| Identity, JWT, auth infra | `Infrastructure/Authentication/` |
| Controllers | `Presentation/Controllers/` |
| Middleware | `Presentation/Middleware/` |
| Action attributes (Idempotent, etc.) | `Presentation/Attributes/` |
| Unit tests | `Application.Test/Features/{Module}/` |

### Concrete example — "Add Product" use case

```
Domain/Entities/Product.cs                          ← entity
Domain/Errors/ProductErrors.cs                      ← errors
Application/Contracts/Products/ProductRequest.cs    ← request DTO + validator
Application/Contracts/Products/ProductResponse.cs   ← response DTO
Application/Features/Products/Add/AddProductCommand.cs         ← command record
Application/Features/Products/Add/AddProductCommandHandler.cs  ← handler
Presentation/Controllers/ProductsController.cs      ← controller action
Application.Test/Features/Products/AddProductCommandHandlerTests.cs ← test
```

---

## 3. SOLID + Clean Architecture (NON-NEGOTIABLE)

These rules are enforced on every PR. No exceptions.

### Handlers are thin orchestrators — nothing more

A handler must only:
1. Validate business preconditions (via repo calls).
2. Call domain factory / behavior methods.
3. Persist via `UnitOfWork`.
4. Invalidate cache.
5. Return `Result`.

**Never put complex logic in a handler.** If any of the following appear, extract a service:
- Math / financial calculations.
- Multi-step validation loops.
- Business algorithms spanning multiple entities.

```csharp
// ✅ Correct — handler delegates to a service
public sealed class ProcessReturnCommandHandler(
    IUnitOfWork unitOfWork,
    IReturnRefundCalculatorService refundCalculator,
    IReturnItemValidatorService itemValidator) : IRequestHandler<...>
{
    public async Task<Result<ReturnResponse>> Handle(ProcessReturnCommand command, CancellationToken cancellationToken = default)
    {
        // validate → calculate → persist → return
    }
}

// ❌ Wrong — math directly in handler
public async Task<Result<ReturnResponse>> Handle(...)
{
    var refund = items.Sum(i => i.Qty * i.Price) * discountFactor; // NO
}
```

### Service interface lives in Application, implementation in Application.Services

```
Application/Interfaces/Services/IReturnRefundCalculatorService.cs  ← interface
Application/Services/ReturnRefundCalculatorService.cs              ← sealed class implements it
```

### Depend on abstractions, never concretions (DIP)

Always inject `IReturnRefundCalculatorService`, never `ReturnRefundCalculatorService`.

### DRY — shared logic belongs in a service

If two handlers need the same logic, it is a service. Period.

---

## 4. Coding Style (agent MUST match exactly)

- **Indentation:** 4 spaces (no tabs).
- **Namespaces:** file-scoped. One namespace per file.
- **Global usings:** declared in `GlobalUsings.cs` per project.
- **Private fields:** `_camelCase`.
- **DI:** primary constructors preferred; regular constructors only when init logic is needed.
- **Expression-bodied members** for single-expression methods: `void Update(string n) => Name = n;`
- **Records** for: commands, queries, DTOs, errors.
- **`sealed class`** for: entities, handlers, services.
- **Entities:** public auto-props + static `Create(...)` factory + behavior methods. No public constructors.
- **Handlers:** `public sealed class`, primary constructor for DI, single `Handle` method.
- **Null checks:** `is not { }` / `is null` — never `!= null` or `== null`.
- **Nav props:** initialize with `= default!;`.
- **No local variables for request unpacking:** Access directly via `command.Request.PropertyName`.
- **Pattern matching repository checks:** inline the null check with the repo call:
  ```csharp
  // ✅ Correct
  if (await unitOfWork.Products.GetAsync([id]) is not { } product)
      return Result.Failure<ProductResponse>(ProductErrors.NotFound);

  // ❌ Wrong
  var product = await unitOfWork.Products.GetAsync([id]);
  if (product is null)
      return Result.Failure<ProductResponse>(ProductErrors.NotFound);
  ```
- **Primary key lookups:** always `GetAsync([id])`, never `FindAsync(x => x.Id == id)`.
- **Folder names:** never name a folder `Log` — conflicts with `.gitignore`. Use `Create` instead.
- **Controller actions:**
  ```csharp
  return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
  ```

---

## 5. No Magic Strings — Ever

Every property name written as a string is a bug waiting to happen on rename. Use `nameof()` everywhere.

### Includes

```csharp
// ✅ Correct
await unitOfWork.Products.FindAsync(p => p.Id == id,
    [nameof(Product.Category), nameof(Product.DosageForm)]);

// ❌ Wrong
await unitOfWork.Products.FindAsync(p => p.Id == id, ["Category", "DosageForm"]);
```

### Nested includes

```csharp
// ✅ Correct
string[] includes = [
    nameof(Order.Customer),
    $"{nameof(Order.Items)}.{nameof(OrderItem.Product)}"
];
```

### ExecuteUpdateAsync

```csharp
// ✅ Correct
await unitOfWork.Inventory.ExecuteUpdateAsync(
    i => i.ProductId == productId,
    nameof(Inventory.Quantity), newQty,
    nameof(Inventory.IsManualShortage), false);
```

### RequestFilters.Check()

```csharp
// ✅ Correct
filters.Check([nameof(Product.Name), nameof(Product.Barcode), nameof(Product.CreatedAt)]);
```

> **Rule:** If you are writing a property name as a raw string literal anywhere in handler or repository code, stop and use `nameof()` instead.

---

## 6. Critical Framework Quirks

These are the things agents most commonly get wrong.

| Topic | What you MUST know |
|---|---|
| **No MediatR** | Custom CQRS: `IRequest<T>` / `IRequestHandler<T,R>` / `ISender`. Handlers auto-registered via `AddCQRSConfig()`. |
| **Result pattern** | `Result<TValue>` / `Error`. Controllers call `result.ToProblem()`. Lives in `Domain/Abstraction/`. |
| **Authorization** | `[HasPermission(Permissions.X)]` — custom handler + policy provider. `[SystemOwner]` — reads `X-System-Owner-Key` header, implements `IAuthorizationFilter` directly. NOT standard ASP.NET policies. |
| **Two databases** | `SharedDbContext` (Pharmacies, SubscriptionRequests) + `ApplicationDbContext` (per-tenant, all ops + Identity). Both SQL Server. |
| **UnitOfWork** | `CompleteAsync()` → ApplicationDb. `SharedCompleteAsync()` → SharedDb. |
| **Tenant** | Resolved from `X-Tenant-Id` header. `PharmacyId` also in JWT. Connection strings in `appsettings.json` under `TenantSettings:Tenants`. |
| **API versioning** | `x-api-version` header (not URL). Default `1.0`. |
| **Mapping** | Mapster (not AutoMapper). Tests use `MapsterTestFixture` to avoid re-init. |
| **Validation** | `SharpGrip.FluentValidation.AutoValidation.Mvc` — runs automatically. Controllers do NOT call validators manually. Call `RequestFilters.Check()` manually inside the handler for sort/search column sanitization. |
| **Cache** | `HybridCache` (Redis + local memory). All keys/tags are tenant-scoped. Use `ICacheService`, not `HybridCache` directly (except `[Idempotent]`). |
| **Background jobs** | Hangfire. Dashboard at `/jobs`, NOT `/hangfire`. |
| **Swagger** | NSwag (not Swashbuckle). |
| **C# extension syntax** | Experimental `extension(IServiceCollection services) { ... }` — requires .NET 10 preview SDK. |
| **StatusCodes in errors** | Use `PharmaSaaS.Domain.Constants.StatusCodes` (int constants), NOT `Microsoft.AspNetCore.Http.StatusCodes`. |

---

## 7. Result & Error Pattern

```csharp
// Domain/Errors/ProductErrors.cs
public record ProductErrors
{
    public static readonly Error NotFound =
        new("Product.NotFound", "No product found.", StatusCodes.NotFound);

    public static readonly Error DuplicatedBarcode =
        new("Product.DuplicatedBarcode", "Barcode already exists.", StatusCodes.Conflict);
}

// Handler
if (await unitOfWork.Products.GetAsync([id]) is not { } product)
    return Result.Failure<ProductResponse>(ProductErrors.NotFound);

return Result.Success(product.Adapt<ProductResponse>());

// Controller
return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
```

---

## 8. CQRS — File Structure & Code Templates

### Command record (Application/Features/{Module}/{UseCase}/)

```csharp
namespace PharmaSaaS.Application.Features.Products.Add;

public record AddProductCommand(ProductRequest Request) : IRequest<Result<ProductResponse>>;
```

### Handler (same folder as command)

```csharp
namespace PharmaSaaS.Application.Features.Products.Add;

public sealed class AddProductCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(AddProductCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Products.AnyAsync(p => p.Barcode == command.Request.Barcode, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.DuplicatedBarcode);

        var product = Product.Create(command.Request.Name, command.Request.CategoryId, command.Request.DosageFormId);

        await unitOfWork.Products.AddAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
```

### Query record + handler

```csharp
namespace PharmaSaaS.Application.Features.Products.Get;

public record GetProductQuery(int Id) : IRequest<Result<ProductResponse>>;

public sealed class GetProductQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductQuery query, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Products.GetAsync([query.Id]) is not { } product)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
```

---

## 9. DTOs & Validators (Application/Contracts/{Module}/)

Request DTO and its validator live in the same file.

```csharp
// Application/Contracts/Products/ProductRequest.cs
namespace PharmaSaaS.Application.Contracts.Products;

public record ProductRequest(
    string Name,
    string? Barcode,
    int CategoryId,
    int DosageFormId
);

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 200);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.DosageFormId).GreaterThan(0);
    }
}
```

Response DTO:

```csharp
// Application/Contracts/Products/ProductResponse.cs
namespace PharmaSaaS.Application.Contracts.Products;

public record ProductResponse(int Id, string Name, string? Barcode, int CategoryId, int DosageFormId);
```

---

## 10. Entity Pattern (Domain/Entities/)

```csharp
namespace PharmaSaaS.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public int DosageFormId { get; private set; }

    public Category Category { get; set; } = default!;
    public DosageForm DosageForm { get; set; } = default!;

    public static Product Create(string name, int categoryId, int dosageFormId)
        => new() { Name = name, CategoryId = categoryId, DosageFormId = dosageFormId };

    public void Update(string name, int categoryId, int dosageFormId)
    {
        Name = name;
        CategoryId = categoryId;
        DosageFormId = dosageFormId;
    }
}
```

---

## 11. Service Pattern (for complex business logic)

When a handler needs logic beyond simple orchestration, extract a service.

### Interface — `Application/Interfaces/Services/IMyService.cs`

```csharp
namespace PharmaSaaS.Application.Interfaces.Services;

public interface IMyService
{
    Task<decimal> CalculateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
```

### Implementation — `Application/Services/MyService.cs`

```csharp
namespace PharmaSaaS.Application.Services;

public sealed class MyService(IDependency dependency) : IMyService
{
    public async Task<decimal> CalculateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // focused, testable logic
    }
}
```

Register both in `Application/DependencyInjection.cs`.

---

## 12. Cache Pattern

All cache keys and tags are **tenant-scoped**.

```csharp
// Read handler — wrap result in cache
return await cacheService.GetOrCreateAsync(
    Cache.Keys.AllCategories(tenantId),
    () => /* repo call */,
    Cache.Expirations.Long,
    tags: [Cache.Tags.Categories(tenantId)],
    cancellationToken);

// Write handler — invalidate after CompleteAsync()
await unitOfWork.CompleteAsync(cancellationToken);
await cacheService.RemoveByTagAsync(Cache.Tags.Categories(tenantId));
```

`tenantId` comes from `ICurrentTenantGetter.GetTenantId()`.

```
Cache.Expirations.Short    = 5 min
Cache.Expirations.Medium   = 30 min
Cache.Expirations.Long     = 1 hour
Cache.Expirations.VeryLong = 24 hours
```

---

## 13. Pagination Pattern

End-to-end flow:

```
Controller creates RequestFilters
  → Handler calls filters.Check([nameof(Entity.Col), ...])
  → Repo method (FindPaginatedListAsync / GetPaginatedListAsync)
  → PaginatedList<T>.CreateAsync() builds IPaginatedList<T>
  → Returns to controller
```

`RequestFilters` lives in `Application/Contracts/Common/RequestFilters.cs`.

Call `filters.Check()` manually inside the handler (not the controller) to sanitize columns.

Use `EmptyPaginatedList.Create<T>()` when no records match — avoids null returns.

---

## 14. GenericRepository — Full API Surface

### Read (untracked — use for queries)
- `GetAllAsync`, `FindAsync`, `FindAllAsync` — with optional `includes[]`.
- `GetAllProjectionAsync<T>`, `FindAllProjectionAsync<T>` — Mapster `.ProjectToType<T>()`.

### Read (tracked — use before calling entity behavior methods)
- `GetAsync([id])` — PK lookup.
- `TrackedFindAsync`, `TrackedFindAllAsync`.

### Write
- `AddAsync`, `AddRangeAsync`, `Update`, `UpdateRange`, `Delete`, `DeleteRange`.

### Bulk (no tracking)
- `ExecuteDeleteAsync`, `ExecuteUpdateAsync` (1–3 property overloads).

### Helpers
- `AnyAsync`, `AnyWithWhereAsync`, `CountAsync`, `Attach`, `AttachRange`, `MarkPropertyAsModified`.

---

## 15. UnitOfWork — Transactions

```csharp
await unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    // multi-step writes
    await unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch
{
    await unitOfWork.RollbackTransactionAsync();
    throw;
}
```

Use only when multiple writes must be atomic.

---

## 16. Authorization

### `[HasPermission(Permissions.X)]`
- Extends `AuthorizeAttribute`. Custom `PermissionAuthorizationHandler` checks a `"permissions"` claim.
- Permissions are `RoleClaims` with `ClaimType = "permissions"`.
- Constants in `Domain/Constants/Permissions.cs`. Format: `"domain:action"` (e.g. `"product:read"`).

### `[SystemOwner]`
- Implements `IAuthorizationFilter` directly — NOT `AuthorizeAttribute`.
- Reads `configuration["SystemOwnerApiKey"]`, compares with `CryptographicOperations.FixedTimeEquals`.
- Returns `401` if wrong, `500` if key not configured.

---

## 17. Identity — Dual Layer

- Domain `User` / `Role` are thin wrappers around `ApplicationUser` / `ApplicationRole`.
- **Never access `ApplicationUser` directly from handlers.** Use `IUserRepository` / `IRoleRepository`.
- `user.CreateIdentity()` → creates `ApplicationUser`.
- `IdentityResult.ToDomain()` → converts to domain `Result`.

---

## 18. Middleware & Request Pipeline Order

```
UseExceptionHandler
→ UseOpenApi (dev only)
→ UseSerilogRequestLogging
→ UseHttpsRedirection
→ MapHangfireDashboard
→ UseCors
→ UseAuthentication
→ UseAuthorization
→ TenantSubscriptionMiddleware
→ UseRateLimiter
→ MapControllers
→ MapHealthChecks
```

`TenantSubscriptionMiddleware` skips `/swagger`, `/redoc`, `/health`, `/jobs`. All other requests require `X-Tenant-Id` header and a non-expired tenant subscription.

---

## 19. Rate Limiting

| Policy | Type | Where |
|---|---|---|
| `IpLimit` | Fixed window (20 req/60s per IP) | `AuthController` |
| `UserLimit` | Fixed window (60 req/60s per user) | Available |
| `Concurrency` | Concurrency (80 concurrent) | Most API controllers |
| `Sliding` | Sliding window | Available |

Apply via `[EnableRateLimiting(RateLimitingOptions.PolicyNames.X)]`.

---

## 20. XML Doc Comments

- **Controllers:** Every action MUST strictly include:
  - `<summary>`
  - `<remarks>` (with a sample request block)
  - `<param>` for every method parameter (including `cancellationToken`)
  - `<returns>`
  - `<response code="X">` for all possible status codes
  - `[ProducesResponseType(...)]` attributes matching every `<response code>`
  - `CancellationToken cancellationToken` as a parameter in the method signature, passed down to `sender.Send()`
  - Class MUST include `<summary>Manage {domain area}.</summary>`.
- **Repository interfaces:** `<summary>` on every method.
- **Handlers, commands, queries, DTOs, entities:** no XML docs.
- All English, multi-line `///`, period at end.

---

## 21. Idempotency

Add `[Idempotent]` to any write action that must be idempotent. Requires `X-Idempotency-Key` header.

- Sets `"in-progress"` for 30 s → returns `409` on duplicate concurrent request.
- Caches successful (2xx) `ObjectResult` for 5 min → replays on repeat.
- Non-2xx responses are not cached.

---

## 22. Testing Pattern

```csharp
// Naming: Handle_WhenCondition_ReturnsResult
public class AddProductCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenBarcodeIsDuplicated_ReturnsFailure()
    {
        // Arrange
        var command = new AddProductCommand(new ProductRequest("Test", "BARCODE", 1, 1));
        A.CallTo(() => _unitOfWork.Products.AnyAsync(A<Expression<Func<Product, bool>>>._, A<CancellationToken>._))
            .Returns(true);

        var handler = new AddProductCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        A.CallTo(() => _unitOfWork.Products.AnyAsync(A<Expression<Func<Product, bool>>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
```

---

## 23. Commands

```powershell
dotnet build PharmaSaaS.slnx
dotnet test PharmaSaaS.slnx
dotnet test .\PharmaSaaS.Application.Test\PharmaSaaS.Application.Test.csproj --verbosity detailed
dotnet run --project .\PharmaSaaS.Presentation\PharmaSaaS.Presentation.csproj
```

Dev URLs: `http://localhost:5290` · Swagger: `/swagger` · ReDoc: `/redoc` · Hangfire: `/jobs`

### Migrations

```powershell
dotnet ef migrations add Name --context SharedDbContext --project .\PharmaSaaS.Infrastructure\
dotnet ef migrations add Name --context ApplicationDbContext --project .\PharmaSaaS.Infrastructure\
```

---

## 24. Spec Sync Protocol (MUST follow)

[`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) is the living source of truth.

### When implementing a feature that exists in the spec
1. Implement it exactly as specified.
2. After completing, mark `✅ Implemented` in the **Implementation Status** table in `PharmaSaaS_SystemAnalysis.md`.
3. Mark done in `tasks/tasks.md`.

### When implementation differs from the spec
1. **Immediately notify the user:**
   > ⚠️ **Deviation from spec**: `POST /api/products/import` is not in the spec. Confirm to update `PharmaSaaS_SystemAnalysis.md`?
2. If confirmed — update the spec.
3. If not confirmed — document the deviation in `tasks/tasks.md` under `## Deviations`.

---

## 25. Implementation Status

See the **Implementation Status** table in [`PharmaSaaS_SystemAnalysis.md`](PharmaSaaS_SystemAnalysis.md) for the authoritative list.

Current summary:
- ✅ Auth (register, login, confirm email, reset password, refresh tokens)
- ✅ Categories, DosageForms
- ✅ Pharmacies, Subscription Requests
- ✅ Roles, Users (Employees), Profile
- ✅ Products
- ✅ Inventory (UpdateStock, ToggleShortage, GetShortages, LogExpiryReport, GetAllInventories, StockLevelReport, OutOfStockReport)
- ❌ Customers, Loyalty
- ❌ Shifts, Invoices, Expenses
- ❌ Suppliers, Purchase Orders, Supplier Payments
- ❌ Reports
- ❌ AI Prescription Parser
