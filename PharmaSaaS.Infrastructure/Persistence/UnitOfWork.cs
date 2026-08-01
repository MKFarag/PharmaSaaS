using Microsoft.EntityFrameworkCore.Storage;
using PharmaSaaS.Infrastructure.Persistence.Repositories;

namespace PharmaSaaS.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly SharedDbContext _sharedDbContext;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed = false;

    // Shared
    public IPharmacyRepository Pharmacies { get; private set; }
    public ISubscriptionRequestRepository SubscriptionRequests { get; private set; }

    public IUserRepository Users { get; private set; }
    public IRoleRepository Roles { get; private set; }

    public IGenericRepository<Category> Categories { get; private set; }
    public IGenericRepository<DosageForm> DosageForms { get; private set; }
    public IGenericRepositoryWithPagination<Product> Products { get; private set; }
    public IGenericRepositoryWithPagination<Inventory> Inventories { get; private set; }
    public IGenericRepositoryWithPagination<ExpiryReport> ExpiryReports { get; private set; }

    public IGenericRepositoryWithPagination<Customer> Customers { get; private set; }
    public IGenericRepository<LoyaltyTier> LoyaltyTiers { get; private set; }
    public IGenericRepository<DiscountRule> DiscountRules { get; private set; }

    public IGenericRepositoryWithPagination<Shift> Shifts { get; private set; }

    public IGenericRepositoryWithPagination<Invoice> Invoices { get; private set; }
    public IGenericRepository<InvoiceItem> InvoiceItems { get; private set; }
    public IGenericRepositoryWithPagination<Return> Returns { get; private set; }
    public IGenericRepositoryWithPagination<Expense> Expenses { get; private set; }

    public UnitOfWork(
        ApplicationDbContext context,
        SharedDbContext sharedDbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _sharedDbContext = sharedDbContext;

        // Shared
        Pharmacies = new PharmacyRepository(_sharedDbContext);
        SubscriptionRequests = new SubscriptionRequestRepository(_sharedDbContext);

        Users = new UserRepository(_context, userManager);
        Roles = new RoleRepository(_context, roleManager);

        Categories = new GenericRepository<Category>(_context);
        DosageForms = new GenericRepository<DosageForm>(_context);
        Products = new GenericRepositoryWithPagination<Product>(_context);
        Inventories = new GenericRepositoryWithPagination<Inventory>(_context);
        ExpiryReports = new GenericRepositoryWithPagination<ExpiryReport>(_context);

        Customers = new GenericRepositoryWithPagination<Customer>(_context);
        LoyaltyTiers = new GenericRepository<LoyaltyTier>(_context);
        DiscountRules = new GenericRepository<DiscountRule>(_context);

        Shifts = new GenericRepositoryWithPagination<Shift>(_context);

        Invoices = new GenericRepositoryWithPagination<Invoice>(_context);
        InvoiceItems = new GenericRepository<InvoiceItem>(_context);
        Returns = new GenericRepositoryWithPagination<Return>(_context);
        Expenses = new GenericRepositoryWithPagination<Expense>(_context);
    }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<int> SharedCompleteAsync(CancellationToken cancellationToken = default)
        => await _sharedDbContext.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return;

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.RollbackAsync(CancellationToken.None);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context?.Dispose();
                _sharedDbContext?.Dispose();
            }

            _disposed = true;
        }
    }
}
