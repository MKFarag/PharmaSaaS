namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    // Shared
    IPharmacyRepository Pharmacies { get; }
    ISubscriptionRequestRepository SubscriptionRequests { get; }

    IUserRepository Users { get; }
    IRoleRepository Roles { get; }

    IGenericRepository<Category> Categories { get; }
    IGenericRepository<DosageForm> DosageForms { get; }
    IGenericRepositoryWithPagination<Product> Products { get; }
    IGenericRepositoryWithPagination<Inventory> Inventories { get; }
    IGenericRepositoryWithPagination<ExpiryReport> ExpiryReports { get; }

    IGenericRepositoryWithPagination<Customer> Customers { get; }
    IGenericRepository<LoyaltyTier> LoyaltyTiers { get; }
    IGenericRepository<DiscountRule> DiscountRules { get; }

    IGenericRepositoryWithPagination<Shift> Shifts { get; }

    IGenericRepositoryWithPagination<Invoice> Invoices { get; }
    IGenericRepository<InvoiceItem> InvoiceItems { get; }
    IGenericRepositoryWithPagination<Return> Returns { get; }
    IGenericRepositoryWithPagination<Expense> Expenses { get; }
    IGenericRepository<AIUsageLog> AIUsageLogs { get; }

    IGenericRepositoryWithPagination<Supplier> Suppliers { get; }
    IGenericRepositoryWithPagination<PurchaseOrder> PurchaseOrders { get; }
    IGenericRepository<PurchaseOrderItem> PurchaseOrderItems { get; }
    IGenericRepositoryWithPagination<SupplierPayment> SupplierPayments { get; }

    /// <summary>Save changes to the database</summary>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>Save changes to the shared database</summary>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SharedCompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a database transaction if one is not already active</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current active database transaction</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current active database transaction</summary>
    Task RollbackTransactionAsync();
}
