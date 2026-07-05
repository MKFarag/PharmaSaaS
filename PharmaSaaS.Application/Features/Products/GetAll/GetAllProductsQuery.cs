namespace PharmaSaaS.Application.Features.Products.GetAll;

public record GetAllProductsQuery(ProductRequestFilters Filters) : IRequest<IPaginatedList<ProductResponse>>;
