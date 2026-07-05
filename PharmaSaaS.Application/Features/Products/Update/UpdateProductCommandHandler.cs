namespace PharmaSaaS.Application.Features.Products.Update;

public sealed class UpdateProductCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        if (await unitOfWork.Products.GetAsync([command.Id], cancellationToken) is not { } product)
            return Result.Failure(ProductErrors.NotFound);

        if (request.Barcode is not null &&
            await unitOfWork.Products.AnyAsync(p => p.Barcode == request.Barcode && p.Id != command.Id, cancellationToken))
            return Result.Failure(ProductErrors.DuplicatedBarcode);

        if (await unitOfWork.Products.AnyAsync(
                p => p.Name == request.Name && p.Concentration == request.Concentration && p.DosageFormId == request.DosageFormId && p.Id != command.Id,
                cancellationToken))
            return Result.Failure(ProductErrors.CompositeDuplicate);

        if (!await unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
            return Result.Failure(ProductErrors.CategoryNotFound);

        if (!await unitOfWork.DosageForms.AnyAsync(d => d.Id == request.DosageFormId, cancellationToken))
            return Result.Failure(ProductErrors.DosageFormNotFound);

        product.Update(request.Name, request.CategoryId, request.DosageFormId,
            request.Barcode, request.Concentration, request.MaxStorageTemperature);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
