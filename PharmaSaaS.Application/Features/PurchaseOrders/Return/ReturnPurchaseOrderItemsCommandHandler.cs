using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Return;

public sealed class ReturnPurchaseOrderItemsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ReturnPurchaseOrderItemsCommand, Result>
{
    public async Task<Result> Handle(ReturnPurchaseOrderItemsCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes =
        [
            nameof(PurchaseOrder.Items),
            nameof(PurchaseOrder.Supplier)
        ];

        if (await unitOfWork.PurchaseOrders.TrackedFindAsync(po => po.Id == command.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure(PurchaseOrderErrors.NotFound);

        // Can only return from a received order
        if (purchaseOrder.Status != PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.InvalidStatus);

        var returnByProductId = command.Request.Items.ToDictionary(x => x.ProductId);
        var poProductIds = purchaseOrder.Items.Select(i => i.ProductId).ToHashSet();

        if (returnByProductId.Keys.Any(pid => !poProductIds.Contains(pid)))
            return Result.Failure(PurchaseOrderErrors.ItemNotFound);

        decimal totalRefundValue = 0;

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var item in purchaseOrder.Items)
            {
                if (returnByProductId.TryGetValue(item.ProductId, out var returnItem))
                {
                    if (returnItem.QuantityToReturn > item.ReceivedQuantity)
                        return Result.Failure(PurchaseOrderErrors.InvalidReturnQuantity);

                    // 1. Mark item as returned (updates Status and quantities)
                    item.Return(returnItem.QuantityToReturn, returnItem.ReturnReason);

                    // 2. Value of returned goods
                    totalRefundValue += (decimal)returnItem.QuantityToReturn * item.UnitCost;

                    // 3. Deduct from inventory
                    var inventory = await unitOfWork.Inventories.TrackedFindAsync(
                        inv => inv.ProductId == item.ProductId, cancellationToken: cancellationToken);

                    if (inventory is not null)
                    {
                        inventory.DeductStock(returnItem.QuantityToReturn);
                        // Optional: if it reaches 0, it might automatically become OutOfStock, 
                        // handled by entity behavior
                    }
                }
            }

            // Adjust the PO total downward by the returned amount
            purchaseOrder.AdjustAmount(purchaseOrder.TotalAmount - totalRefundValue);

            // Credit the supplier balance by the returned value 
            // (Because we previously debited them for the full invoice, returning goods reduces our debt to them)
            purchaseOrder.Supplier.RecordPayment(totalRefundValue);

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
