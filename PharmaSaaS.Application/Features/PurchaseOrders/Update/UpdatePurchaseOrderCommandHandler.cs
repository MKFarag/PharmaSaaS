using Mapster;
using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Enums;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Update;

public sealed class UpdatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePurchaseOrderCommand, Result<PurchaseOrderResponse>>
{
    public async Task<Result<PurchaseOrderResponse>> Handle(UpdatePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes = [nameof(PurchaseOrder.Items), nameof(PurchaseOrder.Supplier)];

        if (await unitOfWork.PurchaseOrders.TrackedFindAsync(po => po.Id == command.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status != PurchaseOrderStatus.Pending)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.InvalidStatus);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (command.Request.ExpectedTotalAmount.HasValue)
            {
                // Note: Before the order is received, TotalAmount holds the ExpectedTotalAmount.
                // We update it directly here.
                purchaseOrder.AdjustAmount(command.Request.ExpectedTotalAmount.Value);
            }

            if (command.Request.AddedItems != null && command.Request.AddedItems.Count > 0)
            {
                // Add new items, making sure they don't already exist in the order.
                // If they exist, we just add the quantity.
                var existingItemsDict = purchaseOrder.Items.ToDictionary(i => i.ProductId);

                foreach (var addedItem in command.Request.AddedItems)
                {
                    if (existingItemsDict.TryGetValue(addedItem.ProductId, out var existingItem))
                    {
                        // Domain behavior to add quantity
                        existingItem.AddQuantity(addedItem.Quantity);
                    }
                    else
                    {
                        // Add as a new item
                        var newItem = PurchaseOrderItem.Create(addedItem.ProductId, addedItem.Quantity);
                        purchaseOrder.Items.Add(newItem);
                    }
                }
            }

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
