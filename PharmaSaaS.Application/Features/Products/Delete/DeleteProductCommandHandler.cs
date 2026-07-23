namespace PharmaSaaS.Application.Features.Products.Delete;

public sealed class DeleteProductCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Products.GetAsync([request.Id], cancellationToken) is not { } product)
            return Result.Failure(ProductErrors.NotFound);

        unitOfWork.Products.Delete(product);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
