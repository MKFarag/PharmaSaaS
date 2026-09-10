namespace PharmaSaaS.Application.Features.PurchaseOrders.Update;

public sealed class UpdatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseOrderService purchaseOrderService)
    : IRequestHandler<UpdatePurchaseOrderCommand, Result<PurchaseOrderResponse>>
{
    public async Task<Result<PurchaseOrderResponse>> Handle(UpdatePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.PurchaseOrders
            .TrackedFindAsync
            (
                po => po.Id == command.Id,
                [nameof(PurchaseOrder.Items), nameof(PurchaseOrder.Supplier)],
                cancellationToken
            )
            is not { } purchaseOrder)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status != PurchaseOrderStatus.Pending)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.InvalidStatus);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (command.Request.ExpectedTotalAmount.HasValue)
                purchaseOrder.AdjustAmount(command.Request.ExpectedTotalAmount.Value);

            if (command.Request.AddedItems is { Count: > 0 })
                purchaseOrderService.AddItems(purchaseOrder, command.Request.AddedItems);

            await unitOfWork.CompleteAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(purchaseOrder.Adapt<PurchaseOrderResponse>());
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
