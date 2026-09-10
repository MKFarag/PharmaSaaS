namespace PharmaSaaS.Application.Services;

public sealed class PurchaseOrderService : IPurchaseOrderService
{
    public async Task<Result> ApproveItemsAsync(PurchaseOrder purchaseOrder, IEnumerable<ApprovePurchaseOrderItemRequest> requestedItems, IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var receiveByProductId = requestedItems.ToDictionary(x => x.ProductId);
        var poProductIds = purchaseOrder.Items.Select(i => i.ProductId).ToHashSet();

        if (poProductIds.Any(pid => !receiveByProductId.ContainsKey(pid)))
            return Result.Failure(PurchaseOrderErrors.ItemNotFound);

        var productIds = receiveByProductId.Keys.ToHashSet();

        var inventories = await unitOfWork.Inventories
            .TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), cancellationToken);

        var inventoryByProductId = inventories.ToDictionary(inv => inv.ProductId);

        foreach (var receiveItem in receiveByProductId.Values)
        {
            var item = purchaseOrder.Items.FirstOrDefault(i => i.ProductId == receiveItem.ProductId);

            if (item is null)
            {
                item = PurchaseOrderItem.Create(receiveItem.ProductId, 0, receiveItem.UnitCost);
                purchaseOrder.Items.Add(item);
            }

            item.Approve(receiveItem.UnitCost, receiveItem.ReceivedQuantity, receiveItem.IsMissing);

            if (receiveItem.ReceivedQuantity > 0 && !receiveItem.IsMissing)
            {
                if (inventoryByProductId.TryGetValue(item.ProductId, out var inventory))
                {
                    inventory.AddStock(receiveItem.ReceivedQuantity);
                    inventory.SetLastOrdered();
                }
                else
                {
                    var newInventory = Inventory.Create(item.ProductId, receiveItem.ReceivedQuantity, 0);
                    await unitOfWork.Inventories.AddAsync(newInventory, cancellationToken);
                    newInventory.SetLastOrdered();
                    inventoryByProductId[item.ProductId] = newInventory;
                }
            }
        }

        return Result.Success();
    }

    public async Task<Result> ReturnItemsAsync(PurchaseOrder purchaseOrder, IEnumerable<ReturnPurchaseOrderItemRequest> requestedItems, IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var returnByProductId = requestedItems.ToDictionary(x => x.ProductId);
        decimal totalRefundValue = 0;

        var productIds = returnByProductId.Keys.ToHashSet();

        var inventories = await unitOfWork.Inventories
            .TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), cancellationToken);

        var inventoryByProductId = inventories.ToDictionary(inv => inv.ProductId);

        foreach (var item in purchaseOrder.Items)
        {
            if (returnByProductId.TryGetValue(item.ProductId, out var returnItem))
            {
                if (returnItem.QuantityToReturn > item.ReceivedQuantity)
                    return Result.Failure(PurchaseOrderErrors.InvalidReturnQuantity);

                item.Return(returnItem.QuantityToReturn, returnItem.ReturnReason ?? string.Empty);

                totalRefundValue += (decimal)returnItem.QuantityToReturn * item.UnitCost;

                if (inventoryByProductId.TryGetValue(item.ProductId, out var inventory))
                    inventory.DeductStock(returnItem.QuantityToReturn);
            }
        }

        purchaseOrder.AdjustAmount(purchaseOrder.TotalAmount - totalRefundValue);

        purchaseOrder.Supplier.RecordPayment(totalRefundValue);

        return Result.Success();
    }

    public void AddItems(PurchaseOrder purchaseOrder, IEnumerable<PurchaseOrderItemRequest> addedItems)
    {
        if (addedItems == null)
            return;

        var existingItemsDict = purchaseOrder.Items.ToDictionary(i => i.ProductId);

        foreach (var addedItem in addedItems)
        {
            if (existingItemsDict.TryGetValue(addedItem.ProductId, out var existingItem))
            {
                existingItem.AddQuantity(addedItem.Quantity);
            }
            else
            {
                var newItem = PurchaseOrderItem.Create(addedItem.ProductId, addedItem.Quantity);
                purchaseOrder.Items.Add(newItem);
            }
        }
    }
}
