namespace PharmaSaaS.Application.Features.Products.Update;

public sealed class UpdateProductCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {

        if (await unitOfWork.Products.GetAsync([command.Id], cancellationToken) is not { } product)
            return Result.Failure(ProductErrors.NotFound);

        if (command.Request.Barcode is not null && await unitOfWork.Products
            .AnyAsync
            (
                p => p.Barcode == command.Request.Barcode &&
                p.Id != command.Id,
                cancellationToken
            ))
            return Result.Failure(ProductErrors.DuplicatedBarcode);

        if (await unitOfWork.Products
            .AnyAsync
            (
                p => p.Name == command.Request.Name &&
                p.Concentration == command.Request.Concentration &&
                p.DosageFormId == command.Request.DosageFormId &&
                p.Id != command.Id,
                cancellationToken
            ))
            return Result.Failure(ProductErrors.CompositeDuplicate);

        if (!await unitOfWork.Categories.AnyAsync(c => c.Id == command.Request.CategoryId, cancellationToken))
            return Result.Failure(ProductErrors.CategoryNotFound);

        if (!await unitOfWork.DosageForms.AnyAsync(d => d.Id == command.Request.DosageFormId, cancellationToken))
            return Result.Failure(ProductErrors.DosageFormNotFound);

        product.Update
            (
                command.Request.Name,
                command.Request.CategoryId,
                command.Request.DosageFormId,
                command.Request.Barcode,
                command.Request.Concentration,
                command.Request.MaxStorageTemperature,
                command.Request.StripsPerBox
            );

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
