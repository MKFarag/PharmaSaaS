namespace PharmaSaaS.Application.Features.Products.Get;

public sealed class GetProductQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProductQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products
            .FindAsync(p => p.Id == request.Id, [nameof(Product.Category), nameof(Product.DosageForm)], cancellationToken);

        if (product is not { })
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
