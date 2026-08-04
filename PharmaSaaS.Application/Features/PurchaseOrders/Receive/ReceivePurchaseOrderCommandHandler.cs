using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Errors;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Receive;

public sealed class ReceivePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ReceivePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ReceivePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes = [nameof(PurchaseOrder.Items)];
        
        if (await unitOfWork.PurchaseOrders.TrackedFindAsync(po => po.Id == command.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.AlreadyReceived);

        foreach (var item in purchaseOrder.Items)
        {
            var inventory = await unitOfWork.Inventories.TrackedFindAsync(i => i.ProductId == item.ProductId, cancellationToken);
            
            if (inventory is null)
            {
                inventory = Inventory.Create(item.ProductId, item.Quantity, 0);
                await unitOfWork.Inventories.AddAsync(inventory, cancellationToken);
            }
            else
            {
                inventory.AddStock(item.Quantity);
            }
        }

        purchaseOrder.Receive();

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
