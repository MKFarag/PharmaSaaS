namespace PharmaSaaS.Application.Features.Products.GetAll;

public sealed class GetAllProductsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllProductsQuery, IPaginatedList<ProductResponse>>
{
    public async Task<IPaginatedList<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken = default)
        => await unitOfWork.Products.GetFilteredAsync(request.Filters, cancellationToken);
}
