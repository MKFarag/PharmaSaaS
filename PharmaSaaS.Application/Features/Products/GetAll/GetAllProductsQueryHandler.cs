namespace PharmaSaaS.Application.Features.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllProductsQuery, IPaginatedList<ProductResponse>>
{
    public async Task<IPaginatedList<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken = default)
    {
        Expression<Func<Product, bool>> predicate = p =>
            (request.Filters.Name == null || p.Name.Contains(request.Filters.Name)) &&
            (!request.Filters.CategoryId.HasValue || p.CategoryId == request.Filters.CategoryId.Value) &&
            (!request.Filters.DosageFormId.HasValue || p.DosageFormId == request.Filters.DosageFormId.Value);

        return await unitOfWork.Products
            .FindPaginatedListAsync<ProductResponse>
            (
                predicate,
                request.Filters.PageNumber, request.Filters.PageSize,
                nameof(Product.Name),
                [nameof(Product.Category), nameof(Product.DosageForm)],
                cancellationToken
            );
    }
}
