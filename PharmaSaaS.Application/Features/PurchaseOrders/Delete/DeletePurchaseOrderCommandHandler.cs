using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Delete;

public sealed class DeletePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(DeletePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes = [nameof(PurchaseOrder.Supplier)];
        if (await unitOfWork.PurchaseOrders.TrackedFindAsync(po => po.Id == command.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.AlreadyReceived);

        purchaseOrder.Supplier.RemoveOrder(purchaseOrder.TotalAmount);

        unitOfWork.PurchaseOrders.Delete(purchaseOrder);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
