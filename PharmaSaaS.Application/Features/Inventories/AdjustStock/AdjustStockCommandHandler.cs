namespace PharmaSaaS.Application.Features.Inventories.AdjustStock;

public sealed class AdjustStockCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(AdjustStockCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.FindAsync(i => i.ProductId == command.ProductId, cancellationToken) is not { } inventory)
            return Result.Failure(InventoryErrors.NotFound);

        var newQuantity = inventory.Quantity + command.Request.Amount;

        if (newQuantity < 0)
            return Result.Failure(InventoryErrors.InsufficientStock);

        await unitOfWork.Inventories.ExecuteUpdateAsync(
            i => i.ProductId == command.ProductId,
            nameof(Inventory.Quantity), newQuantity,
            cancellationToken);

        return Result.Success();
    }
}
