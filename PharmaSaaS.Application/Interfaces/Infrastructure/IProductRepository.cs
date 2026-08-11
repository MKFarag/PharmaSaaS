using PharmaSaaS.Application.Contracts.Products;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Repositories;

namespace PharmaSaaS.Application.Interfaces.Infrastructure;

/// <summary>Specialized repository for Product entity with complex filtering support.</summary>
public interface IProductRepository : IGenericRepositoryWithPagination<Product>
{
    /// <summary>
    /// Returns a paginated, filtered, and sorted list of products.
    /// All filtering, sorting, and projection logic lives in Infrastructure.
    /// </summary>
    Task<IPaginatedList<ProductResponse>> GetFilteredAsync(ProductRequestFilters filters, CancellationToken cancellationToken = default);
}
