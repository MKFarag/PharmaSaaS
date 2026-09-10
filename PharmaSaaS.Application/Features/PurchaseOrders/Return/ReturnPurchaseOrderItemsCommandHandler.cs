namespace PharmaSaaS.Application.Features.PurchaseOrders.Return;

public sealed class ReturnPurchaseOrderItemsCommandHandler(IUnitOfWork unitOfWork, IPurchaseOrderService purchaseOrderService)
    : IRequestHandler<ReturnPurchaseOrderItemsCommand, Result>
{
    public async Task<Result> Handle(ReturnPurchaseOrderItemsCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.PurchaseOrders
            .TrackedFindAsync
            (
                po => po.Id == command.Id,
                [nameof(PurchaseOrder.Items), nameof(PurchaseOrder.Supplier)],
                cancellationToken
            )
            is not { } purchaseOrder)
            return Result.Failure(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status != PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.InvalidStatus);

        var returnByProductId = command.Request.Items.ToDictionary(x => x.ProductId);

        var poProductIds = purchaseOrder.Items.Select(i => i.ProductId).ToHashSet();

        if (returnByProductId.Keys.Any(pid => !poProductIds.Contains(pid)))
            return Result.Failure(PurchaseOrderErrors.ItemNotFound);

        var returnResult = await purchaseOrderService
            .ReturnItemsAsync(purchaseOrder, command.Request.Items, unitOfWork, cancellationToken);

        if (returnResult.IsFailure)
            return returnResult;

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
