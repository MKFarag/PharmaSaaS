namespace PharmaSaaS.Application.Features.PurchaseOrders.Approve;

public sealed class ApprovePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IPurchaseOrderService purchaseOrderService)
    : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseOrderCommand command, CancellationToken cancellationToken = default)
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

        if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.AlreadyReceived);

        var approveResult = await purchaseOrderService
            .ApproveItemsAsync(purchaseOrder, command.Request.Items, unitOfWork, cancellationToken);

        if (approveResult.IsFailure)
            return approveResult;

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            purchaseOrder.AdjustAmount(command.Request.TotalAmount);

            purchaseOrder.Receive();

            purchaseOrder.Supplier.AddOrder(command.Request.TotalAmount);

            await unitOfWork.CompleteAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
