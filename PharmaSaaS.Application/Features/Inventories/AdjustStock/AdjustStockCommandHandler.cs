namespace PharmaSaaS.Application.Features.Inventories.AdjustStock;

public sealed class AdjustStockCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(AdjustStockCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.TrackedFindAsync(i => i.ProductId == command.ProductId, cancellationToken) is not { } inventory)
            return Result.Failure(InventoryErrors.NotFound);

        if (!inventory.TryAdjustStock(command.Request.Amount, out _))
            return Result.Failure(InventoryErrors.InsufficientStock);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
