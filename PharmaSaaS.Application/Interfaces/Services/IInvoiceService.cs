namespace PharmaSaaS.Application.Interfaces.Services;

public interface IInvoiceService
{
    Result<decimal> CalculateTotalAndValidateStock(IEnumerable<InvoiceItemRequest> requestedItems, IEnumerable<Inventory> inventories);

    List<InvoiceItem> ProcessInvoiceItemsAndDeductStock(int invoiceId, IEnumerable<InvoiceItemRequest> requestedItems, IEnumerable<Inventory> inventories);
}
