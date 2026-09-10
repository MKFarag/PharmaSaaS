namespace PharmaSaaS.Application.Features.Products.Add;

public sealed class AddProductCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(AddProductCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Request.Barcode is not null &&
            await unitOfWork.Products.AnyAsync(p => p.Barcode == command.Request.Barcode, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.DuplicatedBarcode);

        if (await unitOfWork.Products
            .AnyAsync
            (
                p => p.Name == command.Request.Name && p.Concentration == command.Request.Concentration && p.DosageFormId == command.Request.DosageFormId,
                cancellationToken
            ))
            return Result.Failure<ProductResponse>(ProductErrors.CompositeDuplicate);

        if (!await unitOfWork.Categories.AnyAsync(c => c.Id == command.Request.CategoryId, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.CategoryNotFound);

        if (!await unitOfWork.DosageForms.AnyAsync(d => d.Id == command.Request.DosageFormId, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.DosageFormNotFound);

        var product = Product.Create
            (
                command.Request.Name,
                command.Request.CategoryId,
                command.Request.DosageFormId,
                command.Request.Barcode,
                command.Request.Concentration,
                command.Request.MaxStorageTemperature
            );

        await unitOfWork.Products.AddAsync(product, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
