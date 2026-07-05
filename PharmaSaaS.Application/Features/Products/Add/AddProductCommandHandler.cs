namespace PharmaSaaS.Application.Features.Products.Add;

public sealed class AddProductCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(AddProductCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        if (request.Barcode is not null &&
            await unitOfWork.Products.AnyAsync(p => p.Barcode == request.Barcode, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.DuplicatedBarcode);

        if (await unitOfWork.Products.AnyAsync(
                p => p.Name == request.Name && p.Concentration == request.Concentration && p.DosageFormId == request.DosageFormId,
                cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.CompositeDuplicate);

        if (!await unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.CategoryNotFound);

        if (!await unitOfWork.DosageForms.AnyAsync(d => d.Id == request.DosageFormId, cancellationToken))
            return Result.Failure<ProductResponse>(ProductErrors.DosageFormNotFound);

        var product = Product
            .Create(request.Name, request.CategoryId, request.DosageFormId, request.Barcode, request.Concentration, request.MaxStorageTemperature);

        await unitOfWork.Products.AddAsync(product, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(product.Adapt<ProductResponse>());
    }
}
