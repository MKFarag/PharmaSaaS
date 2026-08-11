using PharmaSaaS.Application.Contracts.Products;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(ApplicationDbContext context)
    : GenericRepositoryWithPagination<Product>(context), IProductRepository
{
    private readonly DbSet<Product> _products = context.Set<Product>();

    public async Task<IPaginatedList<ProductResponse>> GetFilteredAsync(
        ProductRequestFilters filters, CancellationToken cancellationToken = default)
    {
        var query = _products
            .AsNoTracking()
            .Include(nameof(Product.Category))
            .Include(nameof(Product.DosageForm))
            .Include(nameof(Product.Inventory))
            .AsQueryable();

        if (filters.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filters.CategoryId.Value);

        if (filters.DosageFormId.HasValue)
            query = query.Where(p => p.DosageFormId == filters.DosageFormId.Value);

        if (filters.OutOfStock == true)
            query = query.Where(p => p.Inventory != null && p.Inventory.Quantity == 0);

        if (filters.BelowTarget == true)
            query = query.Where(p => p.Inventory != null
                && p.Inventory.TargetStock.HasValue
                && p.Inventory.Quantity < p.Inventory.TargetStock.Value);

        if (filters.NotReordered == true)
            query = query.Where(p => p.Inventory != null
                && p.Inventory.LastSoldAt.HasValue
                && (p.Inventory.LastOrderedAt == null || p.Inventory.LastOrderedAt < p.Inventory.LastSoldAt));

        if (filters.SoldSince.HasValue)
            query = query.Where(p => p.Inventory != null
                && p.Inventory.LastSoldAt >= filters.SoldSince.Value);

        if (filters.LastOrderedBefore.HasValue)
            query = query.Where(p => p.Inventory != null
                && (p.Inventory.LastOrderedAt == null || p.Inventory.LastOrderedAt < filters.LastOrderedBefore.Value));

        if (!string.IsNullOrWhiteSpace(filters.SearchValue))
            query = query.Where(p => p.Name.Contains(filters.SearchValue));

        var projected = filters.SortByMostSold == true
            ? query.OrderByDescending(p => p.Inventory!.TotalUnitsSold).ProjectToType<ProductResponse>()
            : query.OrderBy(p => p.Name).ProjectToType<ProductResponse>();

        return await PaginatedList<ProductResponse>.CreateAsync(projected, filters.PageNumber, filters.PageSize, cancellationToken);
    }
}
