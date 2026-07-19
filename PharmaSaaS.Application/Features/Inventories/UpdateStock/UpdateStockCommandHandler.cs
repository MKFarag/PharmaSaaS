namespace PharmaSaaS.Application.Features.Inventories.UpdateStock;

public sealed class UpdateStockCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateStockCommand, Result>
{
    public async Task<Result> Handle(UpdateStockCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.FindAsync(i => i.ProductId == command.ProductId, cancellationToken) is not { } inventory)
            return Result.Failure(InventoryErrors.NotFound);

        inventory.SetQuantity(command.Request.Quantity);
        inventory.SetPurchasePrice(command.Request.PurchasePrice);
        inventory.SetTargetStock(command.Request.TargetStock);
        inventory.SetRare(command.Request.IsRare);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
