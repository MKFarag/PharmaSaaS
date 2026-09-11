# PharmaSaaS — Agent Engineering Rules

> **This is the mandatory engineering rulebook for any AI agent working in this codebase.**
> Read `PharmaSaaS_SystemAnalysis.md` first whenever it exists. Every rule below is non-negotiable.

---

## 1. Solution Structure & File Placement

### Projects and Responsibilities

| Project | Layer | Responsibility |
|---|---|---|
| `PharmaSaaS.Domain` | Domain | Entities, repository interfaces, error definitions, Result/Error types, enums, constants, pagination abstractions |
| `PharmaSaaS.Application` | Application | CQRS commands/queries/handlers, DTOs (Contracts), validators, service interfaces, service implementations, Mapster mapping configuration, CQRS abstractions |
| `PharmaSaaS.Infrastructure` | Infrastructure | EF Core DbContext, entity configurations, repository implementations, UnitOfWork, Identity (Auth), JWT, email, caching, background jobs, middleware, tenant service |
| `PharmaSaaS.Presentation` | Presentation | Controllers, middleware, DI wiring, Swagger configuration, global extensions (ToProblem), Program.cs |
| `PharmaSaaS.Application.Test` | Tests (Application) | Unit tests for command/query handlers and services (xUnit + FakeItEasy) |
| `PharmaSaaS.Infrastructure.Test` | Tests (Infrastructure) | Integration tests for infrastructure concerns |

### Where Every File Lives

| Artifact | Location |
|---|---|
| Entity | `PharmaSaaS.Domain/Entities/{Entity}.cs` |
| Repository interface (custom only) | `PharmaSaaS.Domain/Repositories/I{Entity}Repository.cs` |
| Error definitions | `PharmaSaaS.Domain/Errors/{Entity}Errors.cs` |
| Result / Error types | `PharmaSaaS.Domain/Abstraction/` |
| Enums | `PharmaSaaS.Domain/Enums/` |
| Constants | `PharmaSaaS.Domain/Constants/` |
| Request DTO + Validator | `PharmaSaaS.Application/Contracts/{Feature}/{Entity}Request.cs` |
| Response DTO | `PharmaSaaS.Application/Contracts/{Feature}/{Entity}Response.cs` |
| CQRS Command | `PharmaSaaS.Application/Features/{Feature}/{Action}/{Action}{Entity}Command.cs` |
| CQRS Query | `PharmaSaaS.Application/Features/{Feature}/{Action}/{Action}{Entity}Query.cs` |
| CQRS Handler | `PharmaSaaS.Application/Features/{Feature}/{Action}/{Action}{Entity}CommandHandler.cs` (or `QueryHandler.cs`) |
| Service interface | `PharmaSaaS.Application/Interfaces/Services/I{Name}Service.cs` |
| Service implementation | `PharmaSaaS.Application/Services/{Name}Service.cs` |
| Mapster mapping config | `PharmaSaaS.Application/Mapping/MappingConfiguration.cs` (single file, `IRegister`) |
| IUnitOfWork interface | `PharmaSaaS.Application/Interfaces/Infrastructure/IUnitOfWork.cs` |
| DbContext | `PharmaSaaS.Infrastructure/Persistence/ApplicationDbContext.cs` |
| Entity Configuration | `PharmaSaaS.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs` |
| Repository implementation (custom only) | `PharmaSaaS.Infrastructure/Persistence/Repositories/{Entity}Repository.cs` |
| UnitOfWork implementation | `PharmaSaaS.Infrastructure/Persistence/UnitOfWork.cs` |
| Controller | `PharmaSaaS.Presentation/Controllers/{Feature}Controller.cs` |
| Middleware | `PharmaSaaS.Presentation/Middleware/{Name}Middleware.cs` |
| Unit test | `PharmaSaaS.Application.Test/{Feature}/{Action}{Entity}HandlerTest.cs` |

### Worked Example — Categories Feature

```
PharmaSaaS.Domain/Entities/Category.cs
PharmaSaaS.Domain/Errors/CategoryErrors.cs
PharmaSaaS.Application/Contracts/Categories/CategoryRequest.cs        <- DTO + Validator (same file)
PharmaSaaS.Application/Contracts/Categories/CategoryResponse.cs
PharmaSaaS.Application/Features/Categories/Add/AddCategoryCommand.cs
PharmaSaaS.Application/Features/Categories/Add/AddCategoryCommandHandler.cs
PharmaSaaS.Application/Features/Categories/GetAll/GetAllCategoriesQuery.cs
PharmaSaaS.Application/Features/Categories/GetAll/GetAllCategoriesQueryHandler.cs
PharmaSaaS.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs
PharmaSaaS.Presentation/Controllers/CategoriesController.cs
PharmaSaaS.Application.Test/Categories/AddCategoryCommandHandlerTest.cs
```

---

## 2. Architecture Rules

### Clean Architecture Boundaries (enforced)
- **Dependency direction:** Domain <- Application <- Infrastructure <- Presentation. No layer references outward.
- **Handlers are thin orchestrators.** They validate via the generic repository (`AnyAsync`, `GetAsync`), call entity factory/behavior methods, persist via `unitOfWork.CompleteAsync()`, and return `Result<T>`. Multi-step business logic (calculations, loops across entities, financial math) is extracted into a Service.
- **DIP:** Application defines interfaces (`IUnitOfWork`, `ICacheService`, `INotificationService`, etc.). Infrastructure implements them. Controllers receive `ISender` only.
- **No unjustified complexity, ever.** If two solutions both satisfy the requirement and one is simpler, always choose the simpler one. No added abstractions, patterns, or generalizations "for the future." When in doubt, ask.

### DRY -- Critical
DRY violations are treated as bugs, not style issues.
- If the same logic appears in more than one handler, validator, or service -> extract it.
- Always check: "Does this logic already exist somewhere?" before writing it.
- `nameof()` is mandatory for all string references to property names (includes, sort columns, search columns, `ExecuteUpdateAsync` property names). Never use a raw string literal where `nameof()` applies. Real examples from this codebase:
  ```csharp
  // Includes (never "Product", always nameof)
  string[] includes = [nameof(Invoice.Items), nameof(Invoice.Returns), nameof(Invoice.Customer)];

  // Nested navigation includes
  $"{nameof(Customer.CurrentTier)}.{nameof(LoyaltyTier.DiscountRules)}"

  // Sort/search columns
  request.Filters.Check([nameof(Customer.FullName), nameof(Customer.TotalSpend)],
                        [nameof(Customer.FullName), nameof(Customer.PhoneNumber)]);
  ```

### Complex Logic -> Service (mandatory)
A handler that contains financial calculations, multi-step business algorithms, or logic spanning multiple entities **must** delegate to a service.
- Service interface -> `PharmaSaaS.Application/Interfaces/Services/I{Name}Service.cs`
- Service implementation -> `PharmaSaaS.Application/Services/{Name}Service.cs`
- Real example: `IInvoiceService` / `InvoiceService` for invoice total calculation and stock deduction.

### Custom CQRS (no MediatR)
This project uses a **custom CQRS implementation** -- no MediatR dependency.

```csharp
// IRequest<TResponse> -- in Application/Abstractions/Messaging/IRequest.cs
public interface IRequest<out TResponse> { }
public interface IRequest { }

// IRequestHandler -- in Application/Abstractions/Messaging/IRequestHandler.cs
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

// ISender -- in Application/Abstractions/Messaging/ISender.cs
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
```

Commands and queries are dispatched via `ISender.Send(...)`. Controllers inject `ISender sender`.

---

## 3. Repository Awareness

### Existing Generic Repositories

All standard data access goes through `IUnitOfWork`. Do **not** create a new repository without asking the user first.

#### `IGenericRepository<TEntity>`
Located at `PharmaSaaS.Domain/Repositories/IGenericRepository.cs`.

| Capability | Method |
|---|---|
| PK lookup (tracked) | `GetAsync(object[] keyValues, ct)` |
| Get all | `GetAllAsync(ct)` / `GetAllAsync(string[] includes, ct)` |
| Find first (no-track) | `FindAsync(predicate, ct)` / `FindAsync(predicate, includes, ct)` |
| Find all (no-track) | `FindAllAsync(predicate, ct)` / with includes / with pagination |
| Find first (tracked) | `TrackedFindAsync(predicate, ct)` / with includes |
| Find all (tracked) | `TrackedFindAllAsync(predicate, ct)` / with includes |
| Project all to DTO | `GetAllProjectionAsync<TProjection>(ct)` / with includes / with custom selector |
| Project filtered to DTO | `FindAllProjectionAsync<TProjection>(predicate, ct)` / with includes / with custom selector |
| Add / AddRange | `AddAsync(entity, ct)` / `AddRangeAsync(entities, ct)` |
| Delete / DeleteRange | `Delete(entity)` / `DeleteRange(entities)` |
| Bulk delete | `ExecuteDeleteAsync(predicate, ct)` |
| Update / UpdateRange | `Update(entity)` / `UpdateRange(entities)` |
| Bulk update (1-3 props) | `ExecuteUpdateAsync<TValue>(predicate, propertyName, newValue, ct)` |
| Count | `CountAsync(ct)` / `CountAsync(predicate, ct)` |
| Exists | `AnyAsync(predicate, ct)` / `AnyWithWhereAsync(anyPredicate, wherePredicate, ct)` |
| Attach | `Attach(entity)` / `AttachRange(entities)` |
| Mark property modified | `MarkPropertyAsModified<TProperty>(entity, property)` |

#### `IPaginatedRepository<TEntity>`
Located at `PharmaSaaS.Domain/Repositories/IPaginatedRepository.cs`.

| Capability | Method |
|---|---|
| Paginate all | `GetPaginatedListAsync<TProjection>(pageNum, pageSize, defaultSortCol, ct)` |
| Paginate all with includes | `GetPaginatedListAsync<TProjection>(pageNum, pageSize, defaultSortCol, includes, ct)` |
| Paginate with search/sort | `GetPaginatedListAsync<TProjection>(pageNum, pageSize, searchValue, searchCol, sortCol, sortDir, colType, ct)` |
| Paginate filtered | `FindPaginatedListAsync<TProjection>(predicate, pageNum, pageSize, defaultSortCol, ct)` |
| Paginate filtered with search | `FindPaginatedListAsync<TProjection>(predicate, pageNum, pageSize, searchValue, searchCol, sortCol, sortDir, colType, ct)` |

#### `IGenericRepositoryWithPagination<TEntity>`
Combines both -- implements `IGenericRepository<TEntity>` and `IPaginatedRepository<TEntity>`. Use this for entities that need pagination.

### IUnitOfWork -- Entity to Repository Mapping

```csharp
// Non-paginated (IGenericRepository<T>)
IGenericRepository<Category> Categories
IGenericRepository<DosageForm> DosageForms
IGenericRepository<LoyaltyTier> LoyaltyTiers
IGenericRepository<DiscountRule> DiscountRules
IGenericRepository<InvoiceItem> InvoiceItems
IGenericRepository<PurchaseOrderItem> PurchaseOrderItems

// Paginated (IGenericRepositoryWithPagination<T>)
IGenericRepositoryWithPagination<Product> Products
IGenericRepositoryWithPagination<Inventory> Inventories
IGenericRepositoryWithPagination<ExpiryReport> ExpiryReports
IGenericRepositoryWithPagination<Customer> Customers
IGenericRepositoryWithPagination<Shift> Shifts
IGenericRepositoryWithPagination<Invoice> Invoices
IGenericRepositoryWithPagination<Return> Returns
IGenericRepositoryWithPagination<Expense> Expenses
IGenericRepositoryWithPagination<Supplier> Suppliers
IGenericRepositoryWithPagination<PurchaseOrder> PurchaseOrders
IGenericRepositoryWithPagination<SupplierPayment> SupplierPayments

// Custom repositories (only when generic cannot do the job)
IPharmacyRepository Pharmacies                    // SharedDbContext, tenant management
ISubscriptionRequestRepository SubscriptionRequests  // SharedDbContext
IUserRepository Users                             // Identity (UserManager integration)
IRoleRepository Roles                             // Identity (RoleManager integration)
```

**Rule: Never create a new `IXxxRepository` / `XxxRepository` without asking the user first.** Custom repositories only exist for `Pharmacies`, `SubscriptionRequests`, `Users`, and `Roles` because they require Identity `UserManager`/`RoleManager` integration or operate on the `SharedDbContext`. All other entities use the generic repository through `IUnitOfWork`.

**PK lookup pattern:** Always use `GetAsync([id])` (array syntax). Example:
```csharp
var category = await unitOfWork.Categories.GetAsync([id], cancellationToken);
```

---

## 4. Coding Style

### Fundamentals
- **File-scoped namespaces** -- always (`namespace Foo.Bar;`)
- **Primary constructors** -- used on handlers, services, controllers, repositories. Regular constructors used on `UnitOfWork` (multi-line initialization). Match whichever the class already uses.
- **`sealed` classes** -- all handlers and services are `sealed`. Entities are `sealed`. Configuration classes are `internal` (not sealed).
- **Records** -- used for commands, queries, DTOs (request and response), and error definitions.
- **Expression-bodied members** -- used wherever the body is a single expression:
  ```csharp
  public static Category Create(string name) => new() { Name = name };
  public void Update(string name) => Name = name;
  public static Result Success() => new(true, Error.None);
  ```
- **Null checks** -- use `is not { }` pattern.
- **Collection expressions** -- use `[]` syntax: `string[] includes = [nameof(Invoice.Items), nameof(Invoice.Returns)]`

### Line-Wrapping Rule
Only wrap to a new line when the full line **genuinely exceeds ~140 characters**. Do not wrap "for readability." Example of correct single-line class declaration (from real handler):
```csharp
public sealed class AddCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<AddCategoryCommand, Result<CategoryResponse>>
```
This is correct -- do not split it just because it looks long.

### `nameof()` -- Mandatory for All String Property References
Never use a magic string where `nameof()` works. This includes `Include()` paths, sort columns, search columns, and `ExecuteUpdateAsync` property names:
```csharp
// Includes
string[] includes = [nameof(Product.Category), nameof(Product.DosageForm)];

// Nested navigation path
$"{nameof(ExpiryReport.Product)}.{nameof(Product.Category)}"

// Sort column
nameof(Shift.Id)
nameof(ExpiryReport.ReportDate)
```

### Consistency Above All
When in doubt between two equally valid styles, match the dominant pattern already in the codebase.

---

## 5. Entity Pattern (DDD-lite -- mandatory)

```csharp
public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;

    public static Category Create(string name) => new() { Name = name };
    public void Update(string name) => Name = name;
}
```

```csharp
public sealed class Customer
{
    public int Id { get; set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public decimal TotalSpend { get; private set; }

    public int? CurrentTierId { get; private set; }
    public LoyaltyTier? CurrentTier { get; private set; }

    public static Customer Create(string fullName, string phoneNumber)
        => new()
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            TotalSpend = 0,
            CurrentTierId = null
        };

    public void Update(string fullName, string phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void UpdateSpendAndTier(decimal totalSpend, int? currentTierId)
    {
        TotalSpend = totalSpend;
        CurrentTierId = currentTierId;
    }
}
```

**Mandatory rules:**
- `Id` is the **only** property with `public set`. Every other property is `private set`.
- No public constructors. Object creation **only** through a static `Create(...)` factory method.
- State changes **only** through named behavior methods (e.g. `Update`, `UpdateSpendAndTier`, `DeductStock`, `ToggleRare`).
- Navigation properties also use `private set`.

---

## 6. Entity Configuration

Each entity has its **own dedicated** configuration file implementing `IEntityTypeConfiguration<T>`, named `{Entity}Configuration.cs`. Classes are `internal`. Never group multiple entities in one file; never configure entities inline in `DbContext`.

`ApplicationDbContext.OnModelCreating` uses `builder.ApplyConfigurationsFromAssembly(...)` to load all configurations automatically.

```csharp
// PharmaSaaS.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs
namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}
```

---

## 7. Mapping

This codebase uses **Mapster** in one direction only: **Entity -> Response DTO**.

### Standard usage
```csharp
// In a handler -- entity to response DTO
return Result.Success(category.Adapt<CategoryResponse>());
```

### Custom mapping configurations
Non-trivial mappings (computed fields, navigation flattening) are registered in `PharmaSaaS.Application/Mapping/MappingConfiguration.cs`, which implements `IRegister`:

```csharp
public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Shift, ShiftResponse>()
            .Map(dest => dest.CurrentBalance, src => src.OpeningBalance + src.TotalSales - src.TotalReturns - src.TotalExpenses);

        config.NewConfig<Invoice, InvoiceResponse>()
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.FullName : null);

        config.NewConfig<InvoiceItem, InvoiceItemResponse>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.Subtotal, src => (decimal)src.Quantity * src.UnitPrice);
    }
}
```

### Critical rule -- never map Request DTO -> Entity with Mapster
Entities are created and mutated **only** through their own `Create(...)` factory method and named behavior methods. Never use `dto.Adapt<Entity>()` for the Request -> Entity direction. Example (correct):
```csharp
var category = Category.Create(command.Request.Name);
```

### Infrastructure manual conversion -- `ToDomain()` (not Mapster)
In the Infrastructure layer, `IdentityResult` is converted to the project's `Result` type via a manual extension method. This is not Mapster -- it is a plain conversion between an ASP.NET Identity type and this project's domain type:

```csharp
// PharmaSaaS.Infrastructure/Extensions/GlobalExtensions.cs
extension(IdentityResult result)
{
    public Result ToDomain()
    {
        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
```

Usage in `UserRepository`:
```csharp
var result = await userManager.ConfirmEmailAsync(applicationUser, token);
return result.ToDomain();
```

**Never invent a new mapping approach.** Use Mapster for Entity->Response DTO, use entity factory/behavior methods for DTO->Entity, and the `ToDomain()` pattern only for external SDK/library type conversions in Infrastructure.

---

## 8. Result Pattern

This codebase uses a **custom Result Pattern** for all expected failure paths. Do not throw exceptions for business rule violations.

### `Result` / `Result<T>` shape

```csharp
// PharmaSaaS.Domain/Abstraction/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; } = default!;

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);
}

public class Result<TValue>(TValue value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Failure results can not have a value");
}
```

### `Error` shape

```csharp
// PharmaSaaS.Domain/Abstraction/Error.cs
public record Error(string Code, string Description, int? StatusCode)
{
    public static readonly Error None = new(string.Empty, string.Empty, null);
}
```

### Error organization -- one file per feature
Each feature/entity has its own `{Entity}Errors.cs` file in `PharmaSaaS.Domain/Errors/`. All errors are `static readonly` fields. Use `StatusCodes` constants from `PharmaSaaS.Domain/Constants/StatusCodes.cs` (not ASP.NET's `StatusCodes`).

```csharp
// PharmaSaaS.Domain/Errors/CategoryErrors.cs
namespace PharmaSaaS.Domain.Errors;

public record CategoryErrors
{
    public static readonly Error NotFound =
        new("Category.NotFound", "No category found", StatusCodes.NotFound);

    public static readonly Error DuplicatedName =
        new("Category.DuplicatedName", "Another category with the same name is already exists", StatusCodes.Conflict);

    public static readonly Error IsUsed =
        new("Category.IsUsed", "Cannot delete category because it is associated with one or more products", StatusCodes.Conflict);
}
```

When implementing a new feature, new errors belong in a new `{Entity}Errors.cs` file. Never inline an `Error` ad hoc inside a handler.

### Collections / Pagination -- never wrap in `Result<T>`
An empty list is a valid, successful outcome, not a failure. Collection queries return `IEnumerable<T>` directly. Paginated queries return `IPaginatedList<T>` directly. Neither is wrapped in `Result<T>`.

For empty paginated results, use `EmptyPaginatedList.Create<T>()`:
```csharp
// PharmaSaaS.Domain/Abstraction/IPaginatedList.cs
public static class EmptyPaginatedList
{
    public static IPaginatedList<T> Create<T>() where T : class => new Empty<T>();
}
```

### `ToProblem()` -- translating failures to HTTP
In controllers, a failed `Result` is translated via `result.ToProblem()`. Defined in `PharmaSaaS.Presentation/GlobalExtensions.cs`.

Pattern in every controller action:
```csharp
return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
return result.IsSuccess ? NoContent() : result.ToProblem();
return result.IsSuccess ? CreatedAtAction(...) : result.ToProblem();
```

---

## 9. DTOs & Validators

Request DTO and its validator always live in the **same file**, with the validator in a `#region Validation` block:

```csharp
// PharmaSaaS.Application/Contracts/Categories/CategoryRequest.cs
namespace PharmaSaaS.Application.Contracts.Categories;

public record CategoryRequest(
    string Name
);

#region Validation

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100);
    }
}

#endregion
```

Response DTOs are separate files, always records:
```csharp
// PharmaSaaS.Application/Contracts/Categories/CategoryResponse.cs
public record CategoryResponse(
    int Id,
    string Name
);
```

---

## 10. Comments Policy

- **No inline or explanatory comments** anywhere in the code. If a comment is needed to explain "why," that is a signal to write clearer code instead.
- The **only** place explanation belongs is XML `<summary>` on Controller actions.
- **Exception:** if the user explicitly asks for a comment on a specific piece of code, add it there and only there.

---

## 11. Controller Documentation Template

XML doc comments are used **only on Controllers**. Every controller action must follow this exact structure. `[ProducesResponseType]` attributes and `<response code>` lines must **exactly match** the actual outcomes the handler can return (no fixed list -- add a 409 only if that action can actually conflict, etc.):

```csharp
/// <summary>
/// Manage product categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <remarks>
    /// Creates a new product category with the specified name.
    ///
    /// Sample request:
    ///
    ///     POST /api/Categories
    ///     {
    ///       "name": "Antibiotics"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The category creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created category.</returns>
    /// <response code="201">Returns the newly created category.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a category with the same name already exists.</response>
    [HttpPost("")]
    [HasPermission(Permissions.AddCategory)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddCategoryCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }
}
```

**Rules:**
- Handlers, commands, queries, DTOs, entities, repository interfaces get **no** XML docs and **no** summary.
- Include `Sample request:` only for POST/PUT/PATCH actions that have a body.
- `[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]` is present on all controllers.
- Authorization uses `[HasPermission(Permissions.X)]` per action.

---

## 12. Mandatory Workflow

**Follow every step, every time. No skipping.**

### Step 0 -- Context
Read `PharmaSaaS_SystemAnalysis.md` before doing anything else if this file exists (it does).

### Step 1 -- Implementation Plan
Before writing any code:
1. List every open question about the feature: business rules, edge cases, naming, whether it touches existing entities, whether it needs a new repository.
2. Do **not** make assumptions or take decisions. Wait for the user's answers.
3. For Query endpoints: explicitly ask "Pagination needed? Cache needed?"
4. For large/complex features (multiple entities, multiple endpoints, multi-step flow): propose a Phase breakdown and get user approval before writing any code. Execute one phase at a time. At the start of each phase, re-read sections 4, 5, 8, 9 of this document before writing code.

### Step 2 -- Skeleton
Show the exact list of files to create/modify with exact paths, following the "Where Every File Lives" table above. Do not invent new locations. Wait for approval.

### Step 3 -- Entity-first (for new entities only)
If the feature requires a **new** Entity that does not exist yet: implement **only** Entity + EF Configuration + DTOs, then STOP and wait for confirmation.
If the entity already exists, skip this stop and go straight to Step 4.

### Step 4 -- Core Logic
Implement Migration + Handler(s) + Service(s) + everything needed **except** the Controller.
Do **not** write the Controller in this step.
Do **not** write tests in this step.

Then STOP and say exactly:
> "Core logic done (Handler/Service/Migration). Ready to build the Controller -- this step can run on a lighter model to save tokens. Continue now, or start a new session for it?"

Wait for the user's response before proceeding.

### Step 5 -- Controller
Build **only** the Controller action(s) for the command/query completed in Step 4, strictly following the template in section 11. Do not introduce any new logic, validation, or decisions here.

### Step 6 -- No tests until asked
Never write unit tests unless the user explicitly asks for them.

### Step 7 -- Closing a feature
When the user asks for tests, treat this as the signal that the feature is complete:
1. Write the tests.
2. Compare what was implemented against `PharmaSaaS_SystemAnalysis.md`. If there is a deviation, show the user exactly what differs and wait for confirmation before editing the spec.
3. Once confirmed, update `PharmaSaaS_SystemAnalysis.md`.
4. Output a final summary in this exact format:
   - A table: Layer | Files changed/added
   - A "Differences vs SystemAnalysis" section (only if any exist)
   - A short, ready-to-use Git commit message

### Mid-work deviations
If at **any** point during implementation the work ends up differing from what is documented in `PharmaSaaS_SystemAnalysis.md`, immediately notify the user with the specific deviation and ask whether to update the spec now or log it for later. Do not wait until Step 7.

---

## 13. Behavior Rules

- Act as a **senior .NET backend engineer**: decisive, precise, no unnecessary hedging or exploratory rambling.
- Responses must be **short and direct**. No filler, no compliments, no "great question" commentary. This is a working project, not a tutorial.
- Do not deviate from the "Where Every File Lives" table. Common mistakes to avoid:
  - Putting a new DTO outside `Contracts/`
  - Putting a Service in the Infrastructure layer instead of Application
  - Creating a new Repository instead of using the existing generic one through `IUnitOfWork`
- **Consistency over personal preference:** always match the dominant existing pattern in the codebase.
- **Unjustified complexity is a rejection reason on its own.** If a solution introduces more complexity than the requirement needs, do not build it -- ask first.
- **DRY violations are bugs.** If you notice duplicated logic (even pre-existing duplication, not something you just wrote), flag it to the user instead of adding to it.
- **`nameof()` is mandatory** wherever a property name is referenced as a string. Any occurrence of a raw magic string for a property name is a bug.
