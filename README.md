# 💊 PharmaSaaS — Enterprise Pharmacy Management System

A **fully implemented multi-tenant SaaS backend API** built for pharmacy chains and independent pharmacies. The system is production-ready and covers every phase of pharmacy operations — from point-of-sale and inventory management to procurement, CRM, financial reporting, and AI-powered prescription parsing.

Built on **.NET 10**, using **Clean Architecture**, **DDD-lite**, and a **custom CQRS dispatcher** (no MediatR).

---

## 📌 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Technology Stack](#technology-stack)
- [Multi-Tenancy Model](#multi-tenancy-model)
- [Authentication & Authorization](#authentication--authorization)
- [API Modules](#api-modules)
- [Cross-Cutting Concerns](#cross-cutting-concerns)
- [Background Jobs](#background-jobs)
- [Test Coverage](#test-coverage)
- [Domain Model](#domain-model)

---

## Architecture Overview

PharmaSaaS follows **Clean Architecture** with strict layer boundaries:

```
Domain ← Application ← Infrastructure ← Presentation
```

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `PharmaSaaS.Domain` | Entities, repository interfaces, error definitions, Result/Error types, enums, constants |
| Application | `PharmaSaaS.Application` | CQRS commands/queries/handlers, DTOs, validators, service interfaces & implementations, Mapster mapping |
| Infrastructure | `PharmaSaaS.Infrastructure` | EF Core DbContexts, entity configurations, repositories, UnitOfWork, Identity, JWT, email, caching, background jobs, tenant resolution |
| Presentation | `PharmaSaaS.Presentation` | Controllers, middleware, DI wiring, Swagger/ReDoc, rate limiting, health checks |
| Tests | `PharmaSaaS.Application.Test` | Unit tests for all handlers and services (xUnit + FakeItEasy) |

### Two-Database Architecture

The system runs on two physically separate databases:

**Shared Database** — platform-level, managed via `[SystemOwner]` JWT:
- `Pharmacies` — tenant registry with subscription dates and active status
- `SubscriptionRequests` — pharmacy onboarding intake
- `SubscriptionPayments` — renewal payment proofs submitted by owners

**Per-Pharmacy Database** — one isolated database per tenant, provisioned automatically at startup:
- All operational data: products, inventory, customers, invoices, shifts, expenses, purchase orders, suppliers, loyalty, AI usage logs, refresh tokens

Tenant database connection strings are stored in `AppSettings` (not in the shared DB). The correct tenant context is resolved at request time from the `X-Tenant-Id` HTTP header.

---

## Project Structure

```
PharmaSaaS - without microservices/
├── PharmaSaaS.Domain/
│   ├── Entities/                   # 20 domain entities (DDD-lite, private setters, factory methods)
│   │   ├── Shared/                 # Pharmacy, SubscriptionRequest, SubscriptionPayment
│   │   └── *.cs                    # All tenant-scoped entities
│   ├── Repositories/               # IGenericRepository<T>, IPaginatedRepository<T>, IUnitOfWork, custom repo interfaces
│   ├── Errors/                     # Per-entity error definitions (static readonly Error records)
│   ├── Abstraction/                # Result<T>, Error, IPaginatedList<T>, EmptyPaginatedList
│   ├── Enums/                      # Domain enumerations
│   └── Constants/                  # HTTP status codes, pagination defaults
│
├── PharmaSaaS.Application/
│   ├── Abstractions/Messaging/     # IRequest<T>, IRequestHandler<T>, ISender (custom CQRS)
│   ├── Features/                   # CQRS handlers organized by module
│   │   ├── AI/                     # ParsePrescription, SuggestMedications, SetApiKey
│   │   ├── Auth/                   # Login, Register, ConfirmEmail, ResendConfirm, RefreshToken, RevokeRefresh
│   │   ├── Categories/             # Add, Get, GetAll, Update, Delete
│   │   ├── Customers/              # Add, Get, GetAll, Update
│   │   ├── DosageForms/            # Add, Get, GetAll, Update, Delete
│   │   ├── Expenses/               # Log, GetAllByShift, Delete
│   │   ├── ExpiryReports/          # Log, GetAll, Revert
│   │   ├── Inventory/              # GetAll, Get, UpdateStock, AdjustStock, ToggleRare
│   │   ├── Invoices/               # Create, GetAll, GetById, ProcessReturn, CheckReturnImpact
│   │   ├── Loyalty/                # AddTier, UpdateTier, DeleteTier, GetAll, AddRule, UpdateRule, DeleteRule
│   │   ├── Payments/               # Upload, GetAll, Verify
│   │   ├── Pharmacies/             # GetAll, Get, Update, ToggleStatus, SetSubscription
│   │   ├── Products/               # Add, Get, GetAll, GetByBarcode, Update, Delete
│   │   ├── Profile/                # GetMyProfile, UpdateProfile, ChangePassword, ChangeEmail, ConfirmChangeEmail
│   │   ├── PurchaseOrders/         # Create, List, Get, GetItems, Update, Approve, Return, Delete
│   │   ├── Reports/                # Daily, NetProfit, Shortages, Expenses, TopProducts
│   │   ├── Roles/                  # Add, Get, GetAll, GetPermissions, Update, ToggleStatus
│   │   ├── Shifts/                 # Open, Close, GetAll, GetById
│   │   ├── SubscriptionRequests/   # Submit, GetAll, GetById, UpdateStatus
│   │   ├── SupplierPayments/       # Record, List
│   │   ├── Suppliers/              # Add, Get, GetAll, Update, Delete, ResetBalance
│   │   └── Users/                  # Add, Get, Update, ToggleStatus
│   ├── Contracts/                  # Request DTOs + FluentValidation validators (same file), Response DTOs
│   ├── Interfaces/
│   │   ├── Infrastructure/         # IUnitOfWork, ICacheService, INotificationService, IFileStorageService, ICurrentTenantGetter, ITenantService
│   │   └── Services/               # ICustomerLoyaltyService, IReturnService, IInventoryFilterService, IAIService
│   ├── Services/                   # InvoiceService, CustomerLoyaltyService, ReturnService, InventoryFilterService, SupplierPaymentService
│   └── Mapping/                    # MappingConfiguration.cs (single IRegister file, Mapster)
│
├── PharmaSaaS.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs             # Tenant-scoped EF Core context (ASP.NET Identity)
│   │   ├── SharedDbContext.cs                  # Platform-level context (Pharmacies, SubscriptionRequests, SubscriptionPayments)
│   │   ├── Configurations/                     # Per-entity IEntityTypeConfiguration<T> files
│   │   ├── Repositories/                       # GenericRepository, PaginatedRepository, UserRepository, RoleRepository, PharmacyRepository, SubscriptionRequestRepository
│   │   └── UnitOfWork.cs                       # Aggregates all repositories
│   ├── Factories/
│   │   └── TenantProvisioningService.cs        # Runs migrations and seeds owner account for each tenant at startup
│   ├── Tenants/
│   │   └── TenantService.cs                    # Resolves tenant from X-Tenant-Id header, validates subscription
│   ├── Services/
│   │   ├── CacheService.cs                     # HybridCache (in-memory + Redis)
│   │   ├── NotificationService.cs              # MailKit email delivery with HTML templates
│   │   ├── AIService.cs                        # OpenRouter integration (prescription parsing + medication suggestions)
│   │   ├── FileStorageService.cs               # Local disk storage for uploaded files
│   │   ├── RecurringJobService.cs              # Hangfire job implementations
│   │   └── CustomerLoyaltyService.cs           # Loyalty tier evaluation and discount calculation
│   ├── Messaging/
│   │   └── Sender.cs                           # Custom CQRS dispatcher (reflection-based handler resolution)
│   └── Templates/                              # HTML email templates (confirmation, password reset, status updates)
│
├── PharmaSaaS.Presentation/
│   ├── Controllers/                # 19 controllers (see API Modules section)
│   ├── Middleware/
│   │   └── TenantSubscriptionMiddleware.cs     # Validates X-Tenant-Id header and subscription expiry per request
│   ├── SwaggerProcessors/          # Custom NSwag operation processors (tenant header, idempotency key, API version)
│   ├── Settings/
│   │   └── RateLimitingOptions.cs  # Typed config for 4 rate limiting policies
│   ├── DependencyInjection.cs      # All service registrations wired here
│   ├── GlobalExtensions.cs         # result.ToProblem() — translates Result failures to RFC 7807 ProblemDetails
│   └── Program.cs                  # App bootstrap, middleware pipeline, Hangfire recurring jobs
│
└── PharmaSaaS.Application.Test/   # ~95 test files covering all handlers and services
```

---

## Technology Stack

| Concern | Library / Version |
|---|---|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 (SQL Server) |
| Identity | ASP.NET Core Identity + EF |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 10) |
| Mapping | Mapster 10 (entity → response DTO only) |
| Validation | FluentValidation 12 + `SharpGrip.FluentValidation.AutoValidation.Mvc` |
| Caching | `Microsoft.Extensions.Caching.Hybrid` (HybridCache — in-memory + Redis) |
| Distributed cache | StackExchange Redis |
| Background jobs | Hangfire 1.8 (SQL Server storage, basic auth dashboard) |
| Email | MailKit 4 |
| API docs | NSwag 14 (Swagger UI + ReDoc) |
| API versioning | `Asp.Versioning.Http` 10 |
| Rate limiting | ASP.NET Core built-in rate limiter (4 policies) |
| Health checks | `AspNetCore.HealthChecks.SqlServer` + `AspNetCore.HealthChecks.Hangfire` |
| Logging | Serilog 10 (structured request logging) |
| AI | OpenRouter API (HTTP client, `google/gemini-2.0-flash-001`) |
| Dynamic LINQ | `System.Linq.Dynamic.Core` (pagination sort/filter) |
| Unit tests | xUnit 2.9 + FakeItEasy 9 + coverlet |

---

## Multi-Tenancy Model

Every authenticated API request (except `[SystemOwner]` and `[NoTenantRequired]` endpoints) must include:

```
X-Tenant-Id: <tenantId>
```

The `TenantSubscriptionMiddleware` runs after authentication/authorization and:
1. Skips `[SystemOwner]` and `[NoTenantRequired]` endpoints
2. Rejects requests missing the header with `400`
3. Rejects requests with an unknown tenant ID with `404`
4. Rejects requests where `tenant.EndDate < today` with `403` (includes the `expiredAt` date)

Tenant databases are provisioned at application startup via `TenantProvisioningService`, which runs EF Core migrations and seeds a system owner account for each tenant defined in `TenantSettings`.

### Subscription Lifecycle

| Field | Purpose |
|---|---|
| `SubscriptionStart` | When the current subscription period began |
| `SubscriptionEnd` | Expiry date — enforced by middleware per request |
| `IsActive` | Manual override; also deactivated automatically by a daily Hangfire job |

Expired subscriptions block all write operations but preserve read access to historical data.

**Subscription Renewal Flow:**
1. Pharmacy owner uploads a payment proof image via `POST /api/subscription-payments/{pharmacyId}` (public endpoint, no auth required)
2. System owner reviews pending proofs via `GET /api/subscription-payments`
3. System owner verifies and specifies months to extend via `PUT /api/subscription-payments/{id}/verify` → `SubscriptionEnd` is updated automatically

---

## Authentication & Authorization

### Tenant Authentication (per-pharmacy)

- **Registration** — `POST /api/auth/register` creates an employee account. Email confirmation required before first login.
- **Login** — `POST /api/auth/login` validates credentials, checks subscription status, returns a JWT + refresh token.
- **Token refresh** — `POST /api/auth/refresh` issues a new JWT; `POST /api/auth/revoke-refresh` invalidates it.
- **Email flows** — confirm, resend-confirm, forget-password, reset-password.
- **JWT claims** — include `PharmacyId`, `UserId`, `RoleId`, and all granted permissions.

### Permission System

Authorization is enforced at the action level via `[HasPermission(Permissions.XYZ)]`. Permissions are stored as role claims. Roles are fully customizable per tenant — each role can be granted any subset of the defined permissions.

Built-in roles: Owner, Manager, Pharmacist, Cashier. Custom roles can be created, updated, enabled/disabled.

### Platform Authorization (`[SystemOwner]`)

Shared database management endpoints are protected by `[SystemOwner]`. Access requires a JWT from the platform's seeded system owner account.

---

## API Modules

All 19 controllers with their endpoints:

### Auth (`/api/auth`)
| Method | Path | Description |
|---|---|---|
| POST | `/api/auth` | Login — returns JWT + refresh token |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/revoke-refresh` | Revoke refresh token |
| POST | `/api/auth/register` | Register new employee account |
| POST | `/api/auth/confirm` | Confirm email with token |
| POST | `/api/auth/resend-confirm` | Resend confirmation email |
| POST | `/api/auth/forget-password` | Send password reset email |
| POST | `/api/auth/reset-password` | Reset password with token |

### Profile (`/api/profile`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/profile` | Get my profile details |
| PUT | `/api/profile` | Update my name |
| PUT | `/api/profile/change-password` | Change my password |
| PUT | `/api/profile/change-email` | Request email change (sends confirmation) |
| POST | `/api/profile/confirm-change-email` | Confirm the new email with token |

### Subscription Requests (`/api/SubscriptionRequests`) — public submit, `[SystemOwner]` manage
| Method | Path | Description |
|---|---|---|
| POST | `/api/SubscriptionRequests` | Submit onboarding request (public, IP rate-limited, idempotent) |
| GET | `/api/SubscriptionRequests` | List all requests (filterable by status) |
| GET | `/api/SubscriptionRequests/{id}` | Get request details |
| PUT | `/api/SubscriptionRequests/{id}/status` | Update status → Approved auto-creates Pharmacy |

### Subscription Payments (`/api/subscription-payments`)
| Method | Path | Description |
|---|---|---|
| POST | `/api/subscription-payments/{pharmacyId}` | Upload payment proof image (public, no auth) |
| GET | `/api/subscription-payments` | List pending proofs (`[SystemOwner]`) |
| PUT | `/api/subscription-payments/{id}/verify` | Verify payment + extend subscription (`[SystemOwner]`) |

### Pharmacies (`/api/Pharmacies`) — `[SystemOwner]` only
| Method | Path | Description |
|---|---|---|
| GET | `/api/Pharmacies` | List all tenants |
| GET | `/api/Pharmacies/{id}` | Get pharmacy details |
| PUT | `/api/Pharmacies/{id}` | Update pharmacy name |
| PUT | `/api/Pharmacies/toggle-status/{id}` | Toggle IsActive |
| PUT | `/api/Pharmacies/{id}/subscription` | Set subscription end date |

### Users / Employees (`/api/Users`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/Users` | List all employees |
| GET | `/api/Users/{id}` | Get employee details |
| POST | `/api/Users` | Add employee (idempotent) |
| PUT | `/api/Users/{id}` | Update employee details or role |
| PUT | `/api/Users/{id}/toggle-status` | Enable/disable employee account |

### Roles (`/api/Roles`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/Roles` | List all roles |
| GET | `/api/Roles/permissions` | List all available permission values |
| GET | `/api/Roles/{id}` | Get role details with permissions |
| POST | `/api/Roles` | Create role (idempotent) |
| PUT | `/api/Roles/{id}` | Update role name and permissions |
| PUT | `/api/Roles/toggle-status/{id}` | Enable/disable role |

### Products (`/api/products`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/products` | List products (filterable by search, categoryId, dosageFormId) — paginated |
| GET | `/api/products/{id}` | Get product details |
| GET | `/api/products/barcode/{barcode}` | Barcode lookup (POS scanner) |
| POST | `/api/products` | Create product (barcode unique + name+concentration+dosageForm composite unique) |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product (blocked if inventory or invoice history exists) |

### Inventory (`/api/inventory`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/inventory` | List current stock levels |
| GET | `/api/inventory/{productId}` | Get single inventory record |
| PUT | `/api/inventory/{productId}` | Update quantity, selling price, and target stock |
| PATCH | `/api/inventory/{productId}/rare` | Toggle `IsRare` flag (manual shortage marker) |
| PATCH | `/api/inventory/{productId}/stock` | Adjust stock quantity (increment/decrement) |

### Expiry Reports (`/api/expiry-report`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/expiry-report` | List all expiry reports |
| POST | `/api/expiry-report` | Log expired stock (auto-deducts quantity from inventory) |
| DELETE | `/api/expiry-report/{id}` | Revert expiry report (re-adds quantity to inventory) |

### Customers (`/api/customers`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/customers` | List customers (searchable by name or phone) — paginated |
| GET | `/api/customers/{id}` | Get customer profile with loyalty tier and invoice history |
| POST | `/api/customers` | Add customer (phone number unique per pharmacy) |
| PUT | `/api/customers/{id}` | Update customer details |

### Loyalty (`/api/loyalty`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/loyalty/tiers` | List all tiers with their discount rules |
| POST | `/api/loyalty/tiers` | Create loyalty tier (e.g. Silver at 2,000 EGP) |
| PUT | `/api/loyalty/tiers/{id}` | Update tier — re-evaluates all customers against new threshold |
| DELETE | `/api/loyalty/tiers/{id}` | Delete tier (blocked if any customers are assigned) |
| POST | `/api/loyalty/tiers/{tierId}/rules` | Add discount rule to tier (e.g. 2% off invoices above 500 EGP) |
| PUT | `/api/loyalty/rules/{ruleId}` | Update discount rule |
| DELETE | `/api/loyalty/rules/{ruleId}` | Delete discount rule |

### Shifts (`/api/shifts`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/shifts` | List shifts (filterable by date) — paginated |
| GET | `/api/shifts/{id}` | Get shift detail with full reconciliation summary |
| POST | `/api/shifts/open` | Open daily shift with opening cash balance |
| POST | `/api/shifts/{id}/close` | Close shift — calculates expected balance and cash discrepancy |

### Invoices (`/api/invoices`)
| Method | Path | Description |
|---|---|---|
| POST | `/api/invoices` | Create sales invoice — applies loyalty discount, deducts inventory, updates customer spend |
| POST | `/api/invoices/{id}/return` | Process return — re-adds inventory, returns refund amount |
| GET | `/api/invoices` | List invoices (filterable by shiftId, customerId, date) — paginated |
| GET | `/api/invoices/{id}` | Invoice detail with all items and returns |
| GET | `/api/invoices/{id}/return/preview` | Preview return impact before committing |

### Expenses (`/api/expenses`)
| Method | Path | Description |
|---|---|---|
| POST | `/api/expenses` | Log petty cash expense against active shift |
| GET | `/api/expenses/shift/{shiftId}` | List expenses for a specific shift |
| DELETE | `/api/expenses/{id}` | Delete expense (only if shift is still open) |

### Suppliers (`/api/suppliers`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/suppliers` | List all suppliers (cached) with outstanding balance and last order date |
| GET | `/api/suppliers/{id}` | Get supplier details |
| POST | `/api/suppliers` | Add supplier (name unique per pharmacy) |
| PUT | `/api/suppliers/{id}` | Update supplier info |
| DELETE | `/api/suppliers/{id}` | Delete supplier (blocked if purchase orders exist) |
| PATCH | `/api/suppliers/{id}/reset-balance` | Reset outstanding balance to zero |

### Purchase Orders (`/api/purchase-orders`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/purchase-orders` | List purchase orders (filterable by supplierId) — paginated |
| GET | `/api/purchase-orders/{id}` | Purchase order detail |
| GET | `/api/purchase-orders/{id}/items` | Get items filtered by status (Received / Missing) |
| POST | `/api/purchase-orders` | Create PO — does not immediately update inventory |
| PUT | `/api/purchase-orders/{id}` | Update pending PO details |
| PUT | `/api/purchase-orders/{id}/approve` | Approve / receive PO — adds quantities to inventory, supports partial receipt and missing items |
| PUT | `/api/purchase-orders/{id}/return` | Return received PO items — deducts from inventory |
| DELETE | `/api/purchase-orders/{id}` | Delete PO (blocked if already received) |

### Supplier Payments (`/api/supplier-payments`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/supplier-payments` | List payments (filterable by supplierId) |
| POST | `/api/supplier-payments` | Record payment — can reference one or multiple purchase orders |

### Reports (`/api/reports`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/reports/daily` | Daily summary: total sales, returns, expenses, discrepancy, net revenue, top 5 products |
| GET | `/api/reports/profit` | Net profit for date range: `Sales − Expenses − Cash Discrepancies` |
| GET | `/api/reports/shortages` | All shortage products: auto (≤25% of target stock) and/or manually flagged |
| GET | `/api/reports/expenses` | All expenses within date range |
| GET | `/api/reports/top-products` | Top-selling products by quantity (configurable limit, default 10) |

### Categories (`/api/Categories`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/Categories` | List all categories (cached) |
| GET | `/api/Categories/{id}` | Get category |
| POST | `/api/Categories` | Add category (name unique) |
| PUT | `/api/Categories/{id}` | Update category name |
| DELETE | `/api/Categories/{id}` | Delete category (blocked if products exist) |

### Dosage Forms (`/api/DosageForms`)
| Method | Path | Description |
|---|---|---|
| GET | `/api/DosageForms` | List all dosage forms (cached) |
| GET | `/api/DosageForms/{id}` | Get dosage form |
| POST | `/api/DosageForms` | Add dosage form (name unique) |
| PUT | `/api/DosageForms/{id}` | Update dosage form name |
| DELETE | `/api/DosageForms/{id}` | Delete dosage form (blocked if products exist) |

### AI (`/api/AI`)
| Method | Path | Description |
|---|---|---|
| POST | `/api/AI/parse-prescription` | Upload prescription image (JPG/PNG, max 5 MB) → returns detected drug names |
| POST | `/api/AI/suggest-medications` | Describe symptoms in Arabic or English → returns suggested medications |
| PUT | `/api/AI/api-key` | Set or clear a custom OpenRouter API key for the pharmacy |

**AI quota:** Each pharmacy gets **30 free calls/month** across both AI endpoints. When exhausted, the API responds with `429` and a `retryAfter` timestamp. Pharmacies can bring their own OpenRouter API key to bypass the quota.

---

## Cross-Cutting Concerns

### Rate Limiting (4 policies)
| Policy | Applied To |
|---|---|
| `IpLimit` | Public endpoints (subscription request submission, payment upload) |
| `UserLimit` | Per-user throttle on sensitive operations |
| `Concurrency` | AI and write-heavy endpoints |
| `Sliding` | Read/report endpoints |

### Idempotency
Endpoints decorated with `[Idempotent]` require an `Idempotency-Key` header. The key is surfaced in Swagger via a custom NSwag operation processor.

### Caching
- **HybridCache** (in-memory L1 + Redis L2) used for categories, dosage forms, supplier lists
- Cache is invalidated on mutations via `ICacheService`

### Error Handling
- All business failures use the **Result Pattern** — no exceptions for domain errors
- Failed results are translated to RFC 7807 `ProblemDetails` via `result.ToProblem()` in `GlobalExtensions.cs`
- Unhandled exceptions caught by `app.UseExceptionHandler()`

### Logging
- **Serilog** structured request logging via `UseSerilogRequestLogging()`
- Configurable sinks via `appsettings.json`

### Health Checks
Available at `GET /health`:
- SQL Server (shared DB)
- Hangfire server

---

## Background Jobs

Two recurring Hangfire jobs registered at startup:

| Job | Schedule | Action |
|---|---|---|
| `RemoveExpiredRefreshTokens` | Every N days (configured by `RefreshToken.AutoRemoveAfterDays`) | Purges expired refresh tokens from all tenant databases |
| `DeactivateExpiredPharmacySubscriptions` | Daily | Sets `IsActive = false` on pharmacies whose `SubscriptionEnd` has passed |

Hangfire dashboard available at `/jobs` (basic auth protected via `Hangfire:Username` and `Hangfire:Password` in config).

---

## Test Coverage

~95 unit test files in `PharmaSaaS.Application.Test` using **xUnit + FakeItEasy**. Every command handler, query handler, and domain service has dedicated tests.

| Module | Test Files |
|---|---|
| Categories | 5 (Add, Delete, GetAll, GetById, Update) |
| Customers | 5 (Add, GetAll, GetById, Update, validator) |
| Dosage Forms | 5 (Add, Delete, GetAll, GetById, Update) |
| Expenses | 3 (Log, GetAllByShift, Delete) |
| Expiry Reports | 3 (Log, GetAll, Revert) |
| Inventory | 4 (GetAll, Get, UpdateStock, AdjustStock, ToggleRare) |
| Invoices | 5 (Create, GetAll, GetById, ProcessReturn, CheckReturnImpact) |
| Loyalty | 7 (AddTier, UpdateTier, DeleteTier, GetAll, AddRule, UpdateRule, DeleteRule) |
| Subscription Payments | 3 (Upload, GetAll, Verify) |
| Pharmacies | 5 (GetAll, Get, Update, ToggleStatus, SetSubscription) |
| Products | 6 (Add, Delete, GetAll, GetById, GetByBarcode, Update) |
| Profile | 5 (GetMyProfile, UpdateProfile, ChangePassword, ChangeEmail, ConfirmChangeEmail) |
| Purchase Orders | 8 (Create, List, Get, GetItems, Update, Approve, ReturnItems, Delete) |
| Roles | 5 (Add, Get, GetAll, GetPermissions, Update, ToggleStatus) |
| Services | 5 (CustomerLoyalty, Invoice, PurchaseOrder, Return, SupplierPayment) |
| Shifts | 4 (Open, Close, GetAll, GetById) |
| Subscription Requests | 5 (Submit, GetAll, GetById, UpdateStatus, validator) |
| Supplier Payments | 2 (Record, List) |
| Suppliers | 6 (Add, GetAll, GetById, Update, Delete, ResetBalance) |
| Users | 4 (Add, Get, Update, ToggleStatus) |

---

## Domain Model

### Tenant-Scoped Entities (per-pharmacy database)

| Entity | Key Fields |
|---|---|
| `Category` | Id, Name |
| `DosageForm` | Id, Name |
| `Product` | Id, Name, Barcode, Concentration, MaxStorageTemperature, CategoryId, DosageFormId |
| `Inventory` | Id, ProductId, Quantity, SellingPrice, TargetStock, IsRare |
| `ExpiryReport` | Id, ProductId, ExpiredQuantity, Notes, ReportDate |
| `Customer` | Id, FullName, PhoneNumber, TotalSpend, CurrentTierId |
| `LoyaltyTier` | Id, Name, SpendThreshold |
| `DiscountRule` | Id, TierId, MinInvoiceAmount, DiscountPercent |
| `Shift` | Id, Date, Status, OpeningBalance, TotalSales, TotalReturns, TotalExpenses, ActualBalance |
| `Invoice` | Id, ShiftId, CustomerId, Total, DiscountAmount, FinalAmount |
| `InvoiceItem` | Id, InvoiceId, ProductId, Quantity, UnitPrice |
| `Return` | Id, InvoiceId, ProductId, Quantity, RefundAmount |
| `Expense` | Id, ShiftId, Amount, Description, Date |
| `Supplier` | Id, Name, ContactInfo, Balance, LastOrderDate |
| `PurchaseOrder` | Id, SupplierId, OrderDate, TotalAmount, Status |
| `PurchaseOrderItem` | Id, PurchaseOrderId, ProductId, Quantity, ReceivedQuantity, UnitCost, Status |
| `SupplierPayment` | Id, SupplierId, Amount, PaidAt, linked OrderIds |
| `AIUsageLog` | Id, PharmacyId, Month, UsageCount, CustomApiKey |
| `RefreshToken` | Id, UserId, Token, ExpiresAt |
| `Role` | Id, Name, IsActive, Permissions (claims) |
| `User` (ApplicationUser) | Id, FullName, Email, IsActive, RoleId |

### Shared Entities (shared database)

| Entity | Key Fields |
|---|---|
| `Pharmacy` | Id, TenantId, Name, SubscriptionStart, SubscriptionEnd, IsActive |
| `SubscriptionRequest` | Id, PharmacyName, OwnerName, PhoneNumber, Email, Status, CreatedAt |
| `SubscriptionPayment` | Id, PharmacyId, ImagePath, VerifiedAmount, ExtendedByMonths, IsVerified, UploadedAt |
