namespace PharmaSaaS.Application.Features.Products.GetByBarcode;

public sealed class GetProductByBarcodeQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProductByBarcodeQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByBarcodeQuery query, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Products
            .FindAsync
            (
                p => p.Barcode == query.Barcode,
                [nameof(Product.Category), nameof(Product.DosageForm)],
                cancellationToken
            )
            is not { } product)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
