namespace PharmaSaaS.Application.Features.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllProductsQuery, IPaginatedList<ProductResponse>>
{
    public async Task<IPaginatedList<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken = default)
    {
        var f = request.Filters;
        Expression<Func<Product, bool>> predicate = p =>
            (!f.CategoryId.HasValue || p.CategoryId == f.CategoryId.Value) &&
            (!f.DosageFormId.HasValue || p.DosageFormId == f.DosageFormId.Value);

        return await unitOfWork.Products.FindPaginatedListAsync<ProductResponse>(
            predicate,
            f.PageNumber, f.PageSize,
            f.SearchValue, nameof(Product.Name),
            nameof(Product.Name), OrderBy.Ascending,
            ColumnType.String,
            [nameof(Product.Category), nameof(Product.DosageForm)],
            cancellationToken);
    }
}
