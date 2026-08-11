using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Approve;

public sealed class ApprovePurchaseOrderCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes =
        [
            nameof(PurchaseOrder.Items),
            nameof(PurchaseOrder.Supplier)
        ];

        if (await unitOfWork.PurchaseOrders.TrackedFindAsync(po => po.Id == command.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure(PurchaseOrderErrors.NotFound);

        if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            return Result.Failure(PurchaseOrderErrors.AlreadyReceived);

        var receiveByProductId = command.Request.Items.ToDictionary(x => x.ProductId);
        var poProductIds = purchaseOrder.Items.Select(i => i.ProductId).ToHashSet();

        // Ensure all originally ordered items are included in the approval request
        if (poProductIds.Any(pid => !receiveByProductId.ContainsKey(pid)))
            return Result.Failure(PurchaseOrderErrors.ItemNotFound); 

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var receiveItem in command.Request.Items)
            {
                var item = purchaseOrder.Items.FirstOrDefault(i => i.ProductId == receiveItem.ProductId);
                
                if (item == null)
                {
                    // Item was not originally ordered, but is being received
                    item = PurchaseOrderItem.Create(receiveItem.ProductId, 0, receiveItem.UnitCost);
                    purchaseOrder.Items.Add(item);
                }

                // Update item status and final cost
                item.Approve(receiveItem.UnitCost, receiveItem.ReceivedQuantity, receiveItem.IsMissing);

                // Only add to inventory if actually received
                if (receiveItem.ReceivedQuantity > 0 && !receiveItem.IsMissing)
                {
                    var inventory = await unitOfWork.Inventories.TrackedFindAsync(
                        inv => inv.ProductId == item.ProductId, cancellationToken: cancellationToken);

                    if (inventory is null)
                    {
                        inventory = Inventory.Create(item.ProductId, receiveItem.ReceivedQuantity, 0);
                        await unitOfWork.Inventories.AddAsync(inventory, cancellationToken);
                        inventory.SetLastOrdered(); // newly created
                    }
                    else
                    {
                        inventory.AddStock(receiveItem.ReceivedQuantity);
                        inventory.SetLastOrdered();
                    }
                }
            }

            purchaseOrder.AdjustAmount(command.Request.TotalAmount);
            purchaseOrder.Receive();

            // Debit supplier account for the final total invoice amount
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
