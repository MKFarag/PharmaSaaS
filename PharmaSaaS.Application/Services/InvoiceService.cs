namespace PharmaSaaS.Application.Services;

public sealed class InvoiceService : IInvoiceService
{
    public Result<decimal> CalculateTotalAndValidateStock(IEnumerable<InvoiceItemRequest> requestedItems, IEnumerable<Inventory> inventories)
    {
        var inventoryByProductId = inventories.ToDictionary(inv => inv.ProductId);
        decimal total = 0;

        foreach (var item in requestedItems)
        {
            if (!inventoryByProductId.TryGetValue(item.ProductId, out var inventory))
                return Result.Failure<decimal>(InvoiceErrors.ProductNotFound);

            if (inventory.Quantity < item.Quantity)
                return Result.Failure<decimal>(InvoiceErrors.InsufficientStock);

            total += (decimal)item.Quantity * inventory.SellingPrice;
        }

        return Result.Success(total);
    }

    public List<InvoiceItem> ProcessInvoiceItemsAndDeductStock(int invoiceId, IEnumerable<InvoiceItemRequest> requestedItems, IEnumerable<Inventory> inventories)
    {
        var inventoryByProductId = inventories.ToDictionary(inv => inv.ProductId);

        return [.. requestedItems.Select(item =>
        {
            var inventory = inventoryByProductId[item.ProductId];

            inventory.DeductStock(item.Quantity);
            inventory.SetLastSold();
            inventory.AddToSoldUnits(item.Quantity);

            return InvoiceItem.Create(invoiceId, item.ProductId, item.Quantity, inventory.SellingPrice);
        })];
    }
}
